Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Linq ' Required for OrderBy()

Public Class LeaveManagement
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Redirect to login if UserID is not in session
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            Dim userID As Integer = CInt(Session("UserID"))

            ' Load data for the dashboard, dropdowns, and history
            LoadLeaveDashboardData(userID)
            PopulateReportToManagerDropDown(userID)
            BindMyLeaveHistory(userID)

            ' Show/hide Team Approvals tab based on user role
            If userRoleID <= 3 Then ' Manager (3), HR (2), SuperAdmin (1)
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

        ' Define total leave entitlements
        Dim totalCasualLeave As Integer = 12
        Dim totalSickLeave As Integer = 8

        ' Get used leave days
        Dim usedCasualLeave As Integer = GetUsedLeaveDays(employeeID, "Casual Leave")
        Dim usedSickLeave As Integer = GetUsedLeaveDays(employeeID, "Sick Leave")
        Dim usedPaidLeave As Integer = GetUsedLeaveDays(employeeID, "Paid Leave")

        ' Calculate available leave
        Dim availableCasualLeave As Integer = totalCasualLeave - usedCasualLeave
        Dim availableSickLeave As Integer = totalSickLeave - usedSickLeave

        If availableCasualLeave < 0 Then availableCasualLeave = 0
        If availableSickLeave < 0 Then availableSickLeave = 0

        ' Update UI labels
        lblCasualLeaveBalanceAvailable.Text = availableCasualLeave.ToString()
        lblCasualLeaveTotal.Text = totalCasualLeave.ToString()
        lblSickLeaveBalanceAvailable.Text = availableSickLeave.ToString()
        lblSickLeaveTotal.Text = totalSickLeave.ToString()
        lblPaidLeaveCountTaken.Text = usedPaidLeave.ToString()

        ' === UPCOMING HOLIDAYS LOGIC ===
        Dim holidays As New List(Of Object)()

        ' 1. Add static holidays
        holidays.Add(New With {.HolidayName = "Independence Day", .HolidayDate = New DateTime(DateTime.Now.Year, 8, 15)})
        holidays.Add(New With {.HolidayName = "Diwali", .HolidayDate = New DateTime(DateTime.Now.Year, 11, 1)})

        ' 2. Add upcoming Sundays for next 6 months
        Dim startDateForSundays As Date = Date.Today
        Dim endDateForSundays As Date = Date.Today.AddMonths(6)

        Dim currentDay As Date = startDateForSundays
        Do While currentDay <= endDateForSundays
            If currentDay.DayOfWeek = DayOfWeek.Sunday Then
                holidays.Add(New With {.HolidayName = "Sunday Holiday", .HolidayDate = currentDay})
            End If
            currentDay = currentDay.AddDays(1)
        Loop

        ' 3. Sort holidays by date
        holidays = holidays.OrderBy(Function(h) h.HolidayDate).ToList()

        ' 4. FILTER to show only holidays in current month
        Dim currentMonth As Integer = DateTime.Now.Month
        Dim currentYear As Integer = DateTime.Now.Year

        Dim currentMonthHolidays = holidays.Where(Function(h) h.HolidayDate.Month = currentMonth AndAlso h.HolidayDate.Year = currentYear).ToList()

        ' 5. Bind to repeater
        If currentMonthHolidays.Count > 0 Then
            rptCompanyHolidays.DataSource = currentMonthHolidays
            rptCompanyHolidays.DataBind()
            lblNoCompanyHolidays.Visible = False
        Else
            rptCompanyHolidays.DataSource = Nothing
            rptCompanyHolidays.DataBind()
            lblNoCompanyHolidays.Visible = True
        End If

        ' === UPCOMING PERSONAL LEAVES ===
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
                    rptUpcomingMyLeaves.DataSource = Nothing
                    rptUpcomingMyLeaves.DataBind()
                    lblNoUpcomingMyLeaves.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' Helper function to get total days used for a specific leave type and employee
    Private Function GetUsedLeaveDays(ByVal employeeID As Integer, ByVal leaveType As String) As Integer
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        ' Query sums the number of days for leaves with 'Approved by Manager' or 'Approved by HR' status
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
    ' Populates the "Report To Manager" dropdown
    Private Sub PopulateReportToManagerDropDown(ByVal currentEmployeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim defaultManagerID As Integer = GetManagerID(currentEmployeeID) ' Get the employee's assigned manager

        ' Select users with Manager, HR, or SuperAdmin roles
        Dim query As String = "SELECT UserID, Name FROM Users WHERE RoleID IN (1, 2, 3) ORDER BY Name"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                ddlReportToManager.DataSource = cmd.ExecuteReader()
                ddlReportToManager.DataBind()
            End Using
        End Using

        ' Add a default "Select" item
        ddlReportToManager.Items.Insert(0, New ListItem("-- Select Manager --", "0"))

        ' Pre-select the employee's manager if found in the list
        If defaultManagerID > 0 AndAlso ddlReportToManager.Items.FindByValue(defaultManagerID.ToString()) IsNot Nothing Then
            ddlReportToManager.SelectedValue = defaultManagerID.ToString()
        Else
            ddlReportToManager.SelectedValue = "0"
        End If
    End Sub

    ' Handles the "Submit Leave Request" button click
    Protected Sub btnSubmitLeave_Click(sender As Object, e As EventArgs) Handles btnSubmitLeave.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim reportToManagerID As Integer = CInt(ddlReportToManager.SelectedValue)

        ' Basic validations
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

        ' Insert leave request into the database
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "INSERT INTO Leaves (EmployeeID, ManagerID, LeaveType, StartDate, EndDate, Reason, Status, AppliedOn) VALUES (@EmployeeID, @ReportToManagerID, @LeaveType, @StartDate, @EndDate, @Reason, 'Pending', GETDATE())"

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

        ' Provide feedback and clear form
        lblLeaveMessage.Text = "Leave request submitted successfully! Your balance will update upon approval."
        lblLeaveMessage.ForeColor = Drawing.Color.Green
        txtStartDate.Text = ""
        txtEndDate.Text = ""
        txtReason.Text = ""
        ddlReportToManager.SelectedValue = "0"

        ' Refresh data displays after submission (balances don't change until approval)
        LoadLeaveDashboardData(employeeID)
        BindMyLeaveHistory(employeeID)
    End Sub

    ' === MY LEAVE HISTORY ===
    ' Binds the employee's past leave applications to the repeater
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
                    lblNoMyLeavesHistory.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' === TEAM APPROVALS ===
    ' Binds pending leave requests for managers/HR/SuperAdmins
    Private Sub BindTeamApprovals(ByVal currentUserID As Integer, ByVal currentUserRoleID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT l.LeaveID, u.Name as EmployeeName, l.LeaveType, l.StartDate, l.EndDate, l.Reason, l.Status " &
                              "FROM Leaves l JOIN Users u ON l.EmployeeID = u.UserID WHERE l.Status = 'Pending'"

        ' If the current user is a Manager (RoleID = 3), show only leaves for their direct reports
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
                    lblNoTeamLeaves.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' Handles Approve/Reject commands from the Team Approvals Repeater
    Protected Sub rptTeamLeaves_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptTeamLeaves.ItemCommand
        Dim leaveID As Integer = CInt(e.CommandArgument)
        Dim newStatus As String = If(e.CommandName = "Approve", "Approved by Manager", "Rejected")

        UpdateLeaveStatus(leaveID, newStatus)

        ' Rebind Team Approvals after an action
        Dim currentUserID As Integer = CInt(Session("UserID"))
        Dim currentUserRoleID As Integer = CInt(Session("RoleID"))
        BindTeamApprovals(currentUserID, currentUserRoleID)

        ' This was removed in previous step to fix the infinite reload/blink issue.
        ' A full postback already occurs when the button is clicked,
        ' which will re-run Page_Load and refresh all dashboard data correctly.
        ' ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Refresh", "window.location.reload();", True) 
    End Sub

    ' Updates the status of a leave request in the database
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
    ' Gets the ManagerID for a given employee from the Users table
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

    ' GetStatusCssClass function has been removed as requested.

End Class