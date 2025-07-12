Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.HtmlControls

Public Class ReviewDetail
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            If Request.QueryString("ReviewID") IsNot Nothing Then
                Dim reviewID As Integer = CInt(Request.QueryString("ReviewID"))
                hdnReviewID.Value = reviewID.ToString()
                LoadReviewDetails(reviewID)
            Else
                Response.Redirect("Performance.aspx")
            End If
        End If
    End Sub

    Private Sub LoadReviewDetails(ByVal reviewID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Query mein EmployeeID aur IsPublished bhi fetch karenge
        Dim query As String = "SELECT u.Name as EmployeeName, pr.EmployeeID, pr.EmployeeComments, pr.ManagerComments, pr.ManagerRating, pr.Status, pr.IsPublished FROM PerformanceReviews pr JOIN Users u ON pr.EmployeeID = u.UserID WHERE pr.ReviewID = @ReviewID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Dim forEmployeeID As Integer = CInt(reader("EmployeeID"))
                        lblEmployeeName.Text = reader("EmployeeName").ToString()
                        litEmployeeComments.Text = If(reader("EmployeeComments") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(reader("EmployeeComments").ToString()), Server.HtmlEncode(reader("EmployeeComments").ToString()).Replace(vbCrLf, "<br/>"), "No overall comments provided by employee.")

                        ' Load all related data for the page
                        BindKpiRepeater(reviewID)
                        BindPeerFeedback(reviewID)

                        BindPeerFeedback(reviewID, forEmployeeID)

                        ' Load overall manager comments and rating from DB
                        If reader("ManagerComments") IsNot DBNull.Value Then txtManagerComments.Text = reader("ManagerComments").ToString()
                        hdnOverallRatingValue.Value = If(reader("ManagerRating") IsNot DBNull.Value, reader("ManagerRating").ToString(), "0")

                        ' Control form editability based on review status
                        Dim isPublished As Boolean = Convert.ToBoolean(reader("IsPublished"))
                        Dim reviewStatus As String = reader("Status").ToString()

                        If isPublished Then
                            LockForm("This review has been finalized and published.", Drawing.Color.Green)
                        ElseIf reviewStatus = "Pending Self-Appraisal" OrElse reviewStatus = "Not Started" Then
                            LockForm("Employee self-appraisal is pending. You cannot review yet.", Drawing.Color.Red)
                        End If
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' Helper function to lock the entire form
    Private Sub LockForm(ByVal message As String, ByVal color As System.Drawing.Color)
        btnSaveDraft.Enabled = False
        btnFinalizeAndPublish.Enabled = False
        btnFinalizeAndPublish.Text = "Review Finalized"
        lblMessage.Text = message
        lblMessage.ForeColor = color

        txtManagerComments.ReadOnly = True
        divOverallStars.Attributes.Add("class", "star-rating disabled")
        ' The ItemDataBound event will handle disabling individual KPI controls
    End Sub

    Private Sub BindKpiRepeater(ByVal reviewID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' We need employee comments as well for the manager's view
        Dim query As String = "SELECT ek.EmployeeKpiID, k.KpiTitle, ek.ManagerComments, ek.ManagerRating, ek.EmployeeComments FROM EmployeeKPIs ek JOIN KPIs k ON ek.KpiID = k.KpiID WHERE ek.ReviewID = @ReviewID ORDER BY k.KpiTitle"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptEmployeeKPIs.DataSource = dt
                    rptEmployeeKPIs.DataBind()
                    lblNoKPIsAssigned.Visible = False
                Else
                    rptEmployeeKPIs.DataSource = Nothing
                    rptEmployeeKPIs.DataBind()
                    lblNoKPIsAssigned.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub rptEmployeeKPIs_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptEmployeeKPIs.ItemDataBound
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim txtComments As TextBox = CType(e.Item.FindControl("txtKpiManagerComments"), TextBox)
            Dim hdnRating As HiddenField = CType(e.Item.FindControl("hdnKpiRatingValue"), HiddenField)
            Dim divStars As HtmlGenericControl = CType(e.Item.FindControl("divKpiStars"), HtmlGenericControl)

            Dim managerComments As Object = DataBinder.Eval(e.Item.DataItem, "ManagerComments")
            Dim managerRating As Object = DataBinder.Eval(e.Item.DataItem, "ManagerRating")

            If Not IsDBNull(managerComments) Then txtComments.Text = managerComments.ToString()
            hdnRating.Value = If(Not IsDBNull(managerRating), managerRating.ToString(), "0")

            ' Lock controls if the main form is locked
            If Not btnFinalizeAndPublish.Enabled Then
                txtComments.ReadOnly = True
                divStars.Attributes.Add("class", "star-rating disabled")
            End If
        End If
    End Sub

    Private Sub BindPeerFeedback(ByVal reviewID As Integer, ByVal forEmployeeID As Integer)
        pnlPeerFeedback.Visible = True
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pf.FeedbackComments, u.Name as FromEmployeeName FROM PeerFeedback pf JOIN Users u ON pf.FromEmployeeID = u.UserID WHERE pf.ReviewID = @ReviewID AND pf.ForEmployeeID = @ForEmployeeID AND pf.Status = 'Completed'"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                cmd.Parameters.AddWithValue("@ForEmployeeID", forEmployeeID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptPeerFeedback.DataSource = dt
                    rptPeerFeedback.DataBind()
                    lblNoPeerFeedback.Visible = False
                Else
                    rptPeerFeedback.DataSource = Nothing
                    rptPeerFeedback.DataBind()
                    lblNoPeerFeedback.Visible = True
                End If
            End Using
        End Using
    End Sub
    Private Sub BindPeerFeedback(ByVal reviewID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Hum woh saara feedback layenge jo 'Completed' status mein hai
        Dim query As String = "SELECT pf.FeedbackComments, u.Name AS FromEmployeeName FROM PeerFeedback pf " &
                          "JOIN Users u ON pf.FromEmployeeID = u.UserID " &
                          "WHERE pf.ReviewID = @ReviewID AND pf.Status = 'Completed'"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptPeerFeedback.DataSource = dt
                    rptPeerFeedback.DataBind()
                    lblNoPeerFeedback.Visible = False
                Else
                    rptPeerFeedback.DataSource = Nothing
                    rptPeerFeedback.DataBind()
                    lblNoPeerFeedback.Visible = True
                End If
            End Using
        End Using
    End Sub
    ' Universal Save Function (for both Draft and Finalize)
    Private Sub SaveReviewData(ByVal isFinalizing As Boolean)
        Dim reviewID As Integer = CInt(hdnReviewID.Value)
        Dim overallManagerComments As String = txtManagerComments.Text.Trim()
        Dim overallRating As Integer = CInt(hdnOverallRatingValue.Value)

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            conn.Open()
            ' Step 1: Save KPI data
            For Each item As RepeaterItem In rptEmployeeKPIs.Items
                If item.ItemType = ListItemType.Item OrElse item.ItemType = ListItemType.AlternatingItem Then
                    Dim hdnEmployeeKpiID As HiddenField = CType(item.FindControl("hdnEmployeeKpiID"), HiddenField)
                    Dim txtKpiManagerComments As TextBox = CType(item.FindControl("txtKpiManagerComments"), TextBox)
                    Dim hdnKpiRatingValue As HiddenField = CType(item.FindControl("hdnKpiRatingValue"), HiddenField)

                    Dim employeeKpiID As Integer = CInt(hdnEmployeeKpiID.Value)
                    Dim kpiComments As String = txtKpiManagerComments.Text.Trim()
                    Dim kpiRating As Integer = CInt(hdnKpiRatingValue.Value)

                    Dim kpiUpdateQuery As String = "UPDATE EmployeeKPIs SET ManagerComments = @Comments, ManagerRating = @Rating WHERE EmployeeKpiID = @EmployeeKpiID"
                    Using cmdKpi As New SqlCommand(kpiUpdateQuery, conn)
                        cmdKpi.Parameters.AddWithValue("@EmployeeKpiID", employeeKpiID)
                        cmdKpi.Parameters.AddWithValue("@Comments", If(String.IsNullOrEmpty(kpiComments), DBNull.Value, kpiComments))
                        cmdKpi.Parameters.AddWithValue("@Rating", If(kpiRating = 0, DBNull.Value, kpiRating))
                        cmdKpi.ExecuteNonQuery()
                    End Using
                End If
            Next

            ' Step 2: Save Overall Review data
            Dim reviewUpdateQuery As String = "UPDATE PerformanceReviews SET ManagerComments = @ManagerComments, ManagerRating = @Rating"
            If isFinalizing Then
                ' When finalizing, set status to 'Completed' and IsPublished to 1
                reviewUpdateQuery &= ", Status = 'Completed', IsPublished = 1"
            End If
            reviewUpdateQuery &= " WHERE ReviewID = @ReviewID"

            Using cmdReview As New SqlCommand(reviewUpdateQuery, conn)
                cmdReview.Parameters.AddWithValue("@ReviewID", reviewID)
                cmdReview.Parameters.AddWithValue("@ManagerComments", overallManagerComments)
                cmdReview.Parameters.AddWithValue("@Rating", If(overallRating = 0, DBNull.Value, overallRating))
                cmdReview.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Protected Sub btnSaveDraft_Click(sender As Object, e As EventArgs) Handles btnSaveDraft.Click
        SaveReviewData(isFinalizing:=False)
        lblMessage.Text = "Draft saved successfully!"
        lblMessage.ForeColor = Drawing.Color.Blue
    End Sub

    Protected Sub btnFinalizeAndPublish_Click(sender As Object, e As EventArgs) Handles btnFinalizeAndPublish.Click
        If CInt(hdnOverallRatingValue.Value) = 0 Then
            lblMessage.Text = "Please provide an overall final rating before finalizing."
            lblMessage.ForeColor = Drawing.Color.Red
            Return
        End If
        SaveReviewData(isFinalizing:=True)
        Response.Redirect("Performance.aspx")
    End Sub

End Class