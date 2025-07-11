Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class Performance
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            Dim userID As Integer = CInt(Session("UserID"))

            LoadMyPerformanceReview(userID)

            If userRoleID <= 3 Then
                pnlTeamView.Visible = True
                BindTeamReviews(userID, userRoleID)
            End If
        End If
    End Sub

    ' === YAHAN PAR WO MISSING FUNCTION ADD KIYA GAYA HAI ===
    Public Function GetStatusCssClass(ByVal status As Object) As String
        If status Is DBNull.Value OrElse status Is Nothing Then
            Return ""
        End If
        Select Case status.ToString()
            Case "Pending Self-Appraisal", "Pending Manager Review"
                Return "status-pending"
            Case "Completed"
                Return "status-completed"
            Case Else
                Return ""
        End Select
    End Function
    ' =========================================================

    Private Sub LoadMyPerformanceReview(ByVal employeeID As Integer)
        ' ... (Ye function ainvayi rahega jaisa pehle tha) ...
        ' ... Isme koi badlaav nahi ...
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pr.ReviewID, pr.EmployeeComments, pr.ManagerComments, pr.ManagerRating, pr.Status, pc.CycleName FROM PerformanceReviews pr JOIN PerformanceCycles pc ON pr.CycleID = pc.CycleID WHERE pr.EmployeeID = @EmployeeID AND pc.Status = 'Active'"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        pnlMyReview.Visible = True
                        hdnReviewID.Value = reader("ReviewID").ToString()
                        lblCycleName.Text = reader("CycleName").ToString()
                        Dim reviewStatus As String = reader("Status").ToString()
                        lblReviewStatus.Text = reviewStatus
                        If reader("EmployeeComments") IsNot DBNull.Value Then txtEmployeeComments.Text = reader("EmployeeComments").ToString()
                        If reader("ManagerComments") IsNot DBNull.Value Then txtManagerComments.Text = reader("ManagerComments").ToString()
                        If reader("ManagerRating") IsNot DBNull.Value Then lblManagerRating.Text = reader("ManagerRating").ToString() & "/5" Else lblManagerRating.Text = "Not Yet Rated"
                        Select Case reviewStatus
                            Case "Pending Self-Appraisal"
                                txtEmployeeComments.ReadOnly = False
                                btnSubmitToManager.Enabled = True
                                btnSubmitToManager.Text = "Submit to Manager"
                            Case "Pending Manager Review"
                                txtEmployeeComments.ReadOnly = True
                                btnSubmitToManager.Enabled = False
                                btnSubmitToManager.Text = "Submitted"
                            Case "Completed"
                                txtEmployeeComments.ReadOnly = True
                                btnSubmitToManager.Enabled = False
                                btnSubmitToManager.Text = "Review Completed"
                            Case Else
                                txtEmployeeComments.ReadOnly = True
                                btnSubmitToManager.Enabled = False
                        End Select
                    Else
                        pnlMyReview.Visible = False
                    End If
                End Using
            End Using
        End Using
    End Sub

    Protected Sub btnSubmitToManager_Click(sender As Object, e As EventArgs) Handles btnSubmitToManager.Click
        ' ... (Ye function ainvayi rahega) ...
        Dim reviewID As Integer = CInt(hdnReviewID.Value)
        Dim employeeComments As String = txtEmployeeComments.Text.Trim()
        If String.IsNullOrEmpty(employeeComments) Then
            lblMessage.Text = "Please provide your comments before submitting."
            lblMessage.ForeColor = Drawing.Color.Red
            Return
        End If
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "UPDATE PerformanceReviews SET EmployeeComments = @EmployeeComments, Status = 'Pending Manager Review' WHERE ReviewID = @ReviewID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                cmd.Parameters.AddWithValue("@EmployeeComments", employeeComments)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Response.Redirect(Request.RawUrl)
    End Sub

    Private Sub BindTeamReviews(ByVal currentUserID As Integer, ByVal currentUserRoleID As Integer)
        ' ... (Ye function ainvayi rahega) ...
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pr.ReviewID, u.Name as EmployeeName, pr.Status FROM PerformanceReviews pr JOIN Users u ON pr.EmployeeID = u.UserID JOIN PerformanceCycles pc ON pr.CycleID = pc.CycleID WHERE pc.Status = 'Active'"
        If currentUserRoleID = 3 Then query &= " AND pr.ManagerID = @ManagerID"
        query &= " ORDER BY u.Name"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentUserRoleID = 3 Then cmd.Parameters.AddWithValue("@ManagerID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)
                If dt.Rows.Count > 0 Then
                    rptTeamReviews.DataSource = dt
                    rptTeamReviews.DataBind()
                    lblNoTeamReviews.Visible = False
                Else
                    lblNoTeamReviews.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub rptTeamReviews_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        ' ... (Ye function ainvayi rahega) ...
        If e.CommandName = "StartReview" Then
            Dim reviewID As String = e.CommandArgument.ToString()
            Response.Redirect("ReviewDetail.aspx?ReviewID=" & reviewID)
        End If
    End Sub

End Class