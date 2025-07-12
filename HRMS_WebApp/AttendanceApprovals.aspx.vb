Imports System
Imports System.Configuration
Imports System.Data.SqlClient

Public Class AttendanceApprovals
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        ' Optional: Restrict access to this page
        Dim userRoleID As Integer = CInt(Session("RoleID"))
        If userRoleID = 4 Then ' Employee
            Response.Redirect("~/Dashboard.aspx")
            Return
        End If

        If Not IsPostBack Then
            BindApprovalRequests()
        End If
    End Sub

    ' Helper function to format TimeSpan
    Public Function FormatTime(ByVal timeData As Object) As String
        If IsDBNull(timeData) OrElse timeData Is Nothing Then
            Return "--:--"
        End If
        Dim timeSpan As TimeSpan = CType(timeData, TimeSpan)
        Return (Date.Today + timeSpan).ToString("hh:mm tt")
    End Function

    Private Sub BindApprovalRequests()
        Dim currentUserID As Integer = CInt(Session("UserID"))
        Dim currentUserRoleID As Integer = CInt(Session("RoleID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString

        Dim query As String = "SELECT rr.RequestID, u.Name AS EmployeeName, rr.DateToCorrect, rr.NewPunchIn, rr.NewPunchOut, rr.Reason " &
                              "FROM RegularizationRequests rr JOIN Users u ON rr.EmployeeID = u.UserID " &
                              "WHERE rr.Status = 'Pending' "

        ' If the user is a Manager (not HR/Admin), show only their team's requests
        If currentUserRoleID = 3 Then
            query &= "AND rr.ManagerID = @ManagerID "
        End If

        query &= "ORDER BY rr.RequestedOn ASC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentUserRoleID = 3 Then
                    cmd.Parameters.AddWithValue("@ManagerID", currentUserID)
                End If
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                rptApprovalRequests.DataSource = dt
                rptApprovalRequests.DataBind()
                lblNoRequests.Visible = (dt.Rows.Count = 0)
            End Using
        End Using
    End Sub

    Protected Sub rptApprovalRequests_ItemCommand(source As Object, e As System.Web.UI.WebControls.RepeaterCommandEventArgs)
        Dim requestID As Integer = CInt(e.CommandArgument)

        If e.CommandName = "Approve" Then
            ProcessRequest(requestID, "Approved")
        ElseIf e.CommandName = "Reject" Then
            ProcessRequest(requestID, "Rejected")
        End If

        ' Refresh the list
        BindApprovalRequests()
    End Sub

    Private Sub ProcessRequest(ByVal requestID As Integer, ByVal newStatus As String)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString

        Using conn As New SqlConnection(connStr)
            conn.Open()

            If newStatus = "Approved" Then
                ' Step 1: Get the details from the request
                Dim requestDetailsQuery As String = "SELECT EmployeeID, DateToCorrect, NewPunchIn, NewPunchOut FROM RegularizationRequests WHERE RequestID = @RequestID"
                Dim employeeID As Integer
                Dim dateToCorrect As Date
                Dim newPunchIn As Object = DBNull.Value
                Dim newPunchOut As Object = DBNull.Value

                Using cmdDetails As New SqlCommand(requestDetailsQuery, conn)
                    cmdDetails.Parameters.AddWithValue("@RequestID", requestID)
                    Using reader As SqlDataReader = cmdDetails.ExecuteReader()
                        If reader.Read() Then
                            employeeID = CInt(reader("EmployeeID"))
                            dateToCorrect = Convert.ToDateTime(reader("DateToCorrect"))
                            If Not IsDBNull(reader("NewPunchIn")) Then newPunchIn = CType(reader("NewPunchIn"), TimeSpan)
                            If Not IsDBNull(reader("NewPunchOut")) Then newPunchOut = CType(reader("NewPunchOut"), TimeSpan)
                        End If
                    End Using
                End Using

                ' Step 2: Update or Insert into AttendanceLog
                Dim attendanceUpdateQuery As String = "MERGE AttendanceLog AS target " &
                                                      "USING (SELECT @EmployeeID AS EmployeeID, @DateToCorrect AS LogDate) AS source " &
                                                      "ON (target.EmployeeID = source.EmployeeID AND target.LogDate = source.LogDate) " &
                                                      "WHEN MATCHED THEN UPDATE SET PunchInTime = ISNULL(@NewPunchIn, PunchInTime), PunchOutTime = ISNULL(@NewPunchOut, PunchOutTime) " &
                                                      "WHEN NOT MATCHED THEN INSERT (EmployeeID, LogDate, PunchInTime, PunchOutTime, Status) VALUES (@EmployeeID, @DateToCorrect, @NewPunchIn, @NewPunchOut, 'Present');"

                Using cmdUpdate As New SqlCommand(attendanceUpdateQuery, conn)
                    cmdUpdate.Parameters.AddWithValue("@EmployeeID", employeeID)
                    cmdUpdate.Parameters.AddWithValue("@DateToCorrect", dateToCorrect)
                    cmdUpdate.Parameters.AddWithValue("@NewPunchIn", If(TypeOf newPunchIn Is TimeSpan, newPunchIn, DBNull.Value))
                    cmdUpdate.Parameters.AddWithValue("@NewPunchOut", If(TypeOf newPunchOut Is TimeSpan, newPunchOut, DBNull.Value))
                    cmdUpdate.ExecuteNonQuery()
                End Using
            End If

            ' Step 3: Update the request's status
            Dim statusUpdateQuery As String = "UPDATE RegularizationRequests SET Status = @Status, ReviewedOn = GETDATE() WHERE RequestID = @RequestID"
            Using cmdStatus As New SqlCommand(statusUpdateQuery, conn)
                cmdStatus.Parameters.AddWithValue("@Status", newStatus)
                cmdStatus.Parameters.AddWithValue("@RequestID", requestID)
                cmdStatus.ExecuteNonQuery()
            End Using
        End Using
    End Sub

End Class