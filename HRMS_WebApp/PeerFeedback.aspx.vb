Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class PeerFeedback
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            BindRequestsForMe()
            BindRequestsSent()
        End If
    End Sub

    ' =====================================================
    ' == DATA BINDING
    ' =====================================================

    Private Sub BindRequestsForMe()
        Dim currentUserID As Integer = CInt(Session("UserID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pf.PeerFeedbackID, uFor.Name AS ForEmployeeName, uReq.Name AS RequestedByName, pf.Status " &
                              "FROM PeerFeedback pf " &
                              "JOIN Users uFor ON pf.ForEmployeeID = uFor.UserID " &
                              "JOIN Users uReq ON pf.RequestedByUserID = uReq.UserID " & ' Assuming you add RequestedByUserID
                              "WHERE pf.FromEmployeeID = @CurrentUserID ORDER BY pf.RequestedOn DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CurrentUserID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                rptRequestsForMe.DataSource = dt
                rptRequestsForMe.DataBind()
                lblNoRequestsForMe.Visible = (dt.Rows.Count = 0)
            End Using
        End Using
    End Sub

    Private Sub BindRequestsSent()
        Dim currentUserID As Integer = CInt(Session("UserID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pf.PeerFeedbackID, uFor.Name AS ForEmployeeName, uFrom.Name AS FromEmployeeName, pf.Status, pf.RequestedOn " &
                              "FROM PeerFeedback pf " &
                              "JOIN Users uFor ON pf.ForEmployeeID = uFor.UserID " &
                              "JOIN Users uFrom ON pf.FromEmployeeID = uFrom.UserID " &
                              "WHERE pf.RequestedByUserID = @CurrentUserID ORDER BY pf.RequestedOn DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CurrentUserID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                rptRequestsSent.DataSource = dt
                rptRequestsSent.DataBind()
                lblNoRequestsSent.Visible = (dt.Rows.Count = 0)
            End Using
        End Using
    End Sub

    ' =====================================================
    ' == MODAL HANDLING & REQUEST CREATION
    ' =====================================================

    Protected Sub btnShowRequestModal_Click(sender As Object, e As EventArgs)
        PopulateModalDropdowns()
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenRequestModal", "var myModal = new bootstrap.Modal(document.getElementById('requestModal')); myModal.show();", True)
    End Sub

    Private Sub PopulateModalDropdowns()
        Dim currentUserID As Integer = CInt(Session("UserID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Dropdown me saare employees dikhao, siwaye khud ke
        Dim query As String = "SELECT UserID, Name FROM Users WHERE UserID <> @CurrentUserID AND RoleID <> 1 ORDER BY Name"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CurrentUserID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                ddlForEmployee.DataSource = dt
                ddlForEmployee.DataBind()
                ddlForEmployee.Items.Insert(0, New ListItem("-- Select Employee --", "0"))

                cblFromEmployees.DataSource = dt
                cblFromEmployees.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub btnSendRequests_Click(sender As Object, e As EventArgs) Handles btnSendRequests.Click
        Dim forEmployeeID As Integer = CInt(ddlForEmployee.SelectedValue)
        Dim requestedByUserID As Integer = CInt(Session("UserID"))
        Dim reviewID As Integer = GetActiveReviewIDForEmployee(forEmployeeID) ' Helper function needed

        If forEmployeeID = 0 OrElse reviewID = 0 Then
            ' Show error message
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            conn.Open()
            For Each item As ListItem In cblFromEmployees.Items
                If item.Selected Then
                    Dim fromEmployeeID As Integer = CInt(item.Value)
                    Dim query As String = "INSERT INTO PeerFeedback (ReviewID, ForEmployeeID, FromEmployeeID, RequestedByUserID) VALUES (@ReviewID, @ForEmployeeID, @FromEmployeeID, @RequestedByUserID)"
                    Using cmd As New SqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                        cmd.Parameters.AddWithValue("@ForEmployeeID", forEmployeeID)
                        cmd.Parameters.AddWithValue("@FromEmployeeID", fromEmployeeID)
                        cmd.Parameters.AddWithValue("@RequestedByUserID", requestedByUserID)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
            Next
        End Using

        BindRequestsSent() ' Refresh the grid
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseRequestModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('requestModal')); if(myModal) myModal.hide();", True)
    End Sub

    Private Function GetActiveReviewIDForEmployee(ByVal employeeID As Integer) As Integer
        ' Yeh function humne Performance.aspx.vb me bhi banaya tha. Best practice is to move it to a BLL/Helper class.
        ' Abhi ke liye hum ise yahan dobara likh rahe hain.
        Dim reviewID As Integer = 0
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT TOP 1 pr.ReviewID FROM PerformanceReviews pr JOIN PerformanceCycles pc ON pr.CycleID = pc.CycleID WHERE pr.EmployeeID = @EmployeeID AND pc.Status = 'Active'"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot DBNull.Value AndAlso result IsNot Nothing Then reviewID = CInt(result)
            End Using
        End Using
        Return reviewID
    End Function

    ' =====================================================
    ' == PROVIDING FEEDBACK
    ' =====================================================

    Protected Sub rptRequestsForMe_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "ProvideFeedback" Then
            Dim peerFeedbackID As Integer = CInt(e.CommandArgument)
            hdnPeerFeedbackID.Value = peerFeedbackID.ToString()

            ' Fetch existing feedback and employee name to show in modal
            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Dim query As String = "SELECT u.Name, pf.FeedbackComments FROM PeerFeedback pf JOIN Users u ON pf.ForEmployeeID = u.UserID WHERE pf.PeerFeedbackID = @ID"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ID", peerFeedbackID)
                    conn.Open()
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        lblProvideFeedbackFor.Text = reader("Name").ToString()
                        txtFeedbackComments.Text = If(IsDBNull(reader("FeedbackComments")), "", reader("FeedbackComments").ToString())
                    End If
                End Using
            End Using

            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenProvideFeedbackModal", "var myModal = new bootstrap.Modal(document.getElementById('provideFeedbackModal')); myModal.show();", True)
        End If
    End Sub

    Protected Sub btnSubmitFeedback_Click(sender As Object, e As EventArgs) Handles btnSubmitFeedback.Click
        Dim peerFeedbackID As Integer = CInt(hdnPeerFeedbackID.Value)
        Dim comments As String = txtFeedbackComments.Text.Trim()

        If String.IsNullOrEmpty(comments) Then
            ' Show error
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "UPDATE PeerFeedback SET FeedbackComments = @Comments, Status = 'Completed' WHERE PeerFeedbackID = @ID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ID", peerFeedbackID)
                cmd.Parameters.AddWithValue("@Comments", comments)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        BindRequestsForMe()
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseProvideFeedbackModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('provideFeedbackModal')); if(myModal) myModal.hide();", True)
    End Sub
End Class