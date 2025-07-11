Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Web.UI.WebControls
Imports System.Drawing

Partial Class Attendance
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx")

        If Not IsPostBack Then
            Dim roleID As Integer = Convert.ToInt32(Session("RoleID"))
            Dim userID As Integer = Convert.ToInt32(Session("UserID"))

            If roleID = 1 OrElse roleID = 2 Then
                pnlAdminView.Visible = True
                pnlEmployeeManagerView.Visible = False
                BindAllAttendance()
            Else
                pnlAdminView.Visible = False
                pnlEmployeeManagerView.Visible = True
                BindMyAttendance(userID)

                If roleID = 3 Then
                    BindTeamRequests(userID)
                End If
            End If
        End If
    End Sub

    ' =============================== ATTENDANCE METHODS ================================

    Private Sub BindMyAttendance(employeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT TOP 30 LogDate, PunchInTime, PunchOutTime, Status FROM AttendanceLog WHERE EmployeeID = @EmployeeID ORDER BY LogDate DESC"

        Using con As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                Dim dt As New DataTable()
                con.Open()
                dt.Load(cmd.ExecuteReader())

                dt.Columns.Add("FormattedDate", GetType(String))
                dt.Columns.Add("FormattedPunchIn", GetType(String))
                dt.Columns.Add("FormattedPunchOut", GetType(String))
                dt.Columns.Add("StatusCssClass", GetType(String))

                For Each row As DataRow In dt.Rows
                    row("FormattedDate") = Convert.ToDateTime(row("LogDate")).ToString("dd-MMM-yyyy")
                    row("FormattedPunchIn") = FormatTimeSpan(row("PunchInTime"))
                    row("FormattedPunchOut") = FormatTimeSpan(row("PunchOutTime"))
                    row("StatusCssClass") = GetStatusCssClass(row("Status"))
                Next

                rptMyAttendance.DataSource = dt
                rptMyAttendance.DataBind()
                lblNoMyAttendance.Visible = (dt.Rows.Count = 0)
            End Using
        End Using
    End Sub

    Private Sub BindAllAttendance()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT LogID, EmployeeID, LogDate, PunchInTime, PunchOutTime, Status FROM AttendanceLog ORDER BY LogDate DESC"

        Using con As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, con)
                Dim dt As New DataTable()
                con.Open()
                dt.Load(cmd.ExecuteReader())

                dt.Columns.Add("FormattedDate", GetType(String))
                dt.Columns.Add("FormattedPunchIn", GetType(String))
                dt.Columns.Add("FormattedPunchOut", GetType(String))
                dt.Columns.Add("StatusCssClass", GetType(String))

                For Each row As DataRow In dt.Rows
                    row("FormattedDate") = Convert.ToDateTime(row("LogDate")).ToString("dd-MMM-yyyy")
                    row("FormattedPunchIn") = FormatTimeSpan(row("PunchInTime"))
                    row("FormattedPunchOut") = FormatTimeSpan(row("PunchOutTime"))
                    row("StatusCssClass") = GetStatusCssClass(row("Status"))
                Next

                rptAllAttendance.DataSource = dt
                rptAllAttendance.DataBind()
                lblNoAllAttendance.Visible = (dt.Rows.Count = 0)
            End Using
        End Using
    End Sub

    Private Sub BindTeamRequests(managerID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "
            SELECT r.RequestID, u.Name AS EmployeeName, r.DateToCorrect, r.NewPunchIn, r.NewPunchOut, r.Reason 
            FROM RegularizationRequests r 
            JOIN Users u ON r.EmployeeID = u.UserID 
            WHERE r.ManagerID = @ManagerID AND r.Status = 'Pending' 
            ORDER BY r.RequestedOn DESC"

        Using con As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@ManagerID", managerID)
                Dim dt As New DataTable()
                con.Open()
                dt.Load(cmd.ExecuteReader())

                dt.Columns.Add("FormattedDate", GetType(String))
                dt.Columns.Add("FormattedTime", GetType(String))

                For Each row As DataRow In dt.Rows
                    row("FormattedDate") = Convert.ToDateTime(row("DateToCorrect")).ToString("dd-MMM-yyyy")
                    row("FormattedTime") = $"{FormatTimeSpan(row("NewPunchIn"))} - {FormatTimeSpan(row("NewPunchOut"))}"
                Next

                rptTeamRequests.DataSource = dt
                rptTeamRequests.DataBind()
                lblNoTeamRequests.Visible = (dt.Rows.Count = 0)
            End Using
        End Using
    End Sub

    Private Function GetStatusCssClass(status As Object) As String
        If status Is Nothing OrElse status Is DBNull.Value Then Return ""
        Select Case status.ToString().ToLower()
            Case "present" : Return "status-present-text"
            Case "absent" : Return "status-absent-text"
            Case "on leave" : Return "status-on-leave-text"
            Case Else : Return ""
        End Select
    End Function

    Private Function FormatTimeSpan(value As Object) As String
        If value Is DBNull.Value OrElse value Is Nothing Then Return "N/A"
        Try
            Return New DateTime(CType(value, TimeSpan).Ticks).ToString("hh:mm tt")
        Catch ex As Exception
            Return "N/A"
        End Try
    End Function

End Class
