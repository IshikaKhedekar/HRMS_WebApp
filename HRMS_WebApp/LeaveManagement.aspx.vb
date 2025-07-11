Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient

Public Class LeaveManagement
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            Dim userID As Integer = CInt(Session("UserID"))

            LoadLeaveDashboardData(userID)
            PopulateReportToManagerDropDown(userID)
            BindMyLeaveHistory(userID)

            If userRoleID <= 3 Then ' Manager, HR, SuperAdmin
                liTeamApprovalsTab.Visible = True
                BindTeamApprovals(userID, userRoleID)
            Else
                liTeamApprovalsTab.Visible = False
            End If
        End If
    End Sub

    ' === MY LEAVE DASHBOARD ===
    Private Sub LoadLeaveDashboardData(ByVal employeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()

        ' Update used leave balances
        lblCasualLeaveBalanceUsed.Text = GetUsedLeaveDays(employeeID, "Casual Leave").ToString()
        lblSickLeaveBalanceUsed.Text = GetUsedLeaveDays(employeeID, "Sick Leave").ToString()
        lblPaidLeaveBalanceUsed.Text = GetUsedLeaveDays(employeeID, "Paid Leave").ToString()

        ' (Optional: Set total leave limits)
        lblCasualLeaveTotal.Text = "12"
        lblSickLeaveTotal.Text = "12"
        lblPaidLeaveTotal.Text = "12"

        ' Upcoming Holidays
        Dim holidays As New List(Of Object)()
        holidays.Add(New With {.HolidayName = "Independence Day", .HolidayDate = New DateTime(DateTime.Now.Year, 8, 15)})
        holidays.Add(New With {.HolidayName = "Diwali", .HolidayDate = New DateTime(DateTime.Now.Year, 11, 1)})

        If holidays.Count > 0 Then
            rptCompanyHolidays.DataSource = holidays
            rptCompanyHolidays.DataBind()
            lblNoCompanyHolidays.Visible = False
        Else
            lblNoCompanyHolidays.Visible = True
        End If

        ' Upcoming Leaves
        Dim queryUpcoming As String = "SELECT LeaveID, StartDate, EndDate, LeaveType, Status, Reason FROM Leaves WHERE EmployeeID = @EmployeeID AND EndDate >= GETDATE() ORDER BY StartDate ASC"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(queryUpcoming, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim dt As New DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptUpcomingMyLeaves.DataSource = dt
                    rptUpcomingMyLeaves.DataBind()
                    lblNoUpcomingMyLeaves.Visible = False
                Else
                    lblNoUpcomingMyLeaves.Visible = True
                End If
            End Using
        End Using
    End Sub

    Private Function GetUsedLeaveDays(ByVal employeeID As Integer, ByVal leaveType As String) As Integer
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT ISNULL(SUM(DATEDIFF(day, StartDate, EndDate) + 1), 0) FROM Leaves WHERE EmployeeID = @EmployeeID AND LeaveType = @LeaveType AND Status IN ('Approved by Manager', 'Approved by HR')"
        Dim usedDays As Integer = 0

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                cmd.Parameters.AddWithValue("@LeaveType", leaveType)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot DBNull.Value Then usedDays = CInt(result)
            End Using
        End Using
        Return usedDays
    End Function

    ' === APPLY FOR LEAVE ===
    Private Sub PopulateReportToManagerDropDown(ByVal currentEmployeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim defaultManagerID As Integer = GetManagerID(currentEmployeeID)

        Dim query As String = "SELECT UserID, Name FROM Users WHERE RoleID IN (1, 2, 3) ORDER BY Name"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                ddlReportToManager.DataSource = cmd.ExecuteReader()
                ddlReportToManager.DataBind()
            End Using
        End Using

        ddlReportToManager.Items.Insert(0, New ListItem("-- Select Manager --", "0"))

        If defaultManagerID > 0 AndAlso ddlReportToManager.Items.FindByValue(defaultManagerID.ToString()) IsNot Nothing Then
            ddlReportToManager.SelectedValue = defaultManagerID.ToString()
        Else
            ddlReportToManager.SelectedValue = "0"
        End If
    End Sub

    Protected Sub btnSubmitLeave_Click(sender As Object, e As EventArgs) Handles btnSubmitLeave.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim reportToManagerID As Integer = CInt(ddlReportToManager.SelectedValue)

        If reportToManagerID = 0 Then
            lblLeaveMessage.Text = "Please select a manager to report to."
            lblLeaveMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        Dim leaveType As String = ddlLeaveType.SelectedValue
        Dim startDate As Date
        Dim endDate As Date
        Dim reason As String = txtReason.Text.Trim()

        If Not Date.TryParse(txtStartDate.Text, startDate) OrElse Not Date.TryParse(txtEndDate.Text, endDate) Then
            lblLeaveMessage.Text = "Invalid date format."
            lblLeaveMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        If startDate > endDate Then
            lblLeaveMessage.Text = "Start Date cannot be after End Date."
            lblLeaveMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        If String.IsNullOrEmpty(reason) Then
            lblLeaveMessage.Text = "Reason for leave is required."
            lblLeaveMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "INSERT INTO Leaves (EmployeeID, ManagerID, LeaveType, StartDate, EndDate, Reason, Status) VALUES (@EmployeeID, @ReportToManagerID, @LeaveType, @StartDate, @EndDate, @Reason, 'Pending')"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                cmd.Parameters.AddWithValue("@ReportToManagerID", reportToManagerID)
                cmd.Parameters.AddWithValue("@LeaveType", leaveType)
                cmd.Parameters.AddWithValue("@StartDate", startDate)
                cmd.Parameters.AddWithValue("@EndDate", endDate)
                cmd.Parameters.AddWithValue("@Reason", reason)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        lblLeaveMessage.Text = "Leave request submitted successfully!"
        lblLeaveMessage.ForeColor = Drawing.Color.Green
        txtStartDate.Text = ""
        txtEndDate.Text = ""
        txtReason.Text = ""
        LoadLeaveDashboardData(employeeID)
        BindMyLeaveHistory(employeeID)
        ddlReportToManager.SelectedValue = "0"
    End Sub

    ' === MY LEAVE HISTORY ===
    Private Sub BindMyLeaveHistory(ByVal employeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT StartDate, EndDate, LeaveType, Status FROM Leaves WHERE EmployeeID = @EmployeeID ORDER BY AppliedOn DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim dt As New DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptMyLeaves.DataSource = dt
                    rptMyLeaves.DataBind()
                    lblNoMyLeavesHistory.Visible = False
                Else
                    rptMyLeaves.DataSource = Nothing
                    rptMyLeaves.DataBind()
                    lblNoMyLeavesHistory.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' === TEAM APPROVALS ===
    Private Sub BindTeamApprovals(ByVal currentUserID As Integer, ByVal currentUserRoleID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT l.LeaveID, u.Name as EmployeeName, l.LeaveType, l.StartDate, l.EndDate, l.Reason, l.Status " &
                              "FROM Leaves l JOIN Users u ON l.EmployeeID = u.UserID WHERE l.Status = 'Pending'"

        If currentUserRoleID = 3 Then
            query &= " AND l.ManagerID = @ManagerID"
        End If

        query &= " ORDER BY l.AppliedOn ASC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentUserRoleID = 3 Then
                    cmd.Parameters.AddWithValue("@ManagerID", currentUserID)
                End If
                conn.Open()
                Dim dt As New DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptTeamLeaves.DataSource = dt
                    rptTeamLeaves.DataBind()
                    lblNoTeamLeaves.Visible = False
                Else
                    rptTeamLeaves.DataSource = Nothing
                    rptTeamLeaves.DataBind()
                    lblNoTeamLeaves.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub rptTeamLeaves_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptTeamLeaves.ItemCommand
        Dim leaveID As Integer = CInt(e.CommandArgument)
        Dim newStatus As String = If(e.CommandName = "Approve", "Approved by Manager", "Rejected")

        UpdateLeaveStatus(leaveID, newStatus)
        BindTeamApprovals(CInt(Session("UserID")), CInt(Session("RoleID")))
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Refresh", "window.location.reload();", True)
    End Sub

    Private Sub UpdateLeaveStatus(ByVal leaveID As Integer, ByVal status As String)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "UPDATE Leaves SET Status = @Status WHERE LeaveID = @LeaveID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Status", status)
                cmd.Parameters.AddWithValue("@LeaveID", leaveID)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    ' === HELPER ===
    Private Function GetManagerID(ByVal employeeID As Integer) As Integer
        Dim managerID As Integer = 0
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT ManagerID FROM Users WHERE UserID = @EmployeeID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                    managerID = CInt(result)
                End If
            End Using
        End Using
        Return managerID
    End Function
End Class
