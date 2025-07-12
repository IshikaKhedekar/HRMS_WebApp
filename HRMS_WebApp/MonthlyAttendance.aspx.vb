Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Globalization

Public Class MonthlyAttendance
    Inherits System.Web.UI.Page

    ' Helper class to hold data for each day, now with Overtime
    Public Class DailyStatus
        Public Property [Date] As Date
        Public Property PunchIn As String = "--:--"
        Public Property PunchOut As String = "--:--"
        Public Property TotalHours As String = "-"
        Public Property Overtime As String = "-" ' <-- NEW PROPERTY
        Public Property Status As String
        Public Property StatusCssClass As String
    End Class

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            PopulateDropDowns()
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            If userRoleID <= 3 Then ' Manager, HR, SuperAdmin
                divEmployeeFilter.Visible = True
                PopulateEmployeesDropDown(CInt(Session("UserID")), userRoleID)
            End If
        End If
    End Sub

    Private Sub PopulateDropDowns()
        ' Year Dropdown
        For i As Integer = 0 To 2
            ddlYear.Items.Add(New ListItem(DateTime.Now.AddYears(-i).Year.ToString()))
        Next
        ' Month Dropdown
        For i As Integer = 1 To 12
            ddlMonth.Items.Add(New ListItem(DateTimeFormatInfo.CurrentInfo.GetMonthName(i), i.ToString()))
        Next
        ' Set default to last month
        Dim lastMonth As DateTime = DateTime.Now.AddMonths(-1)
        ddlMonth.SelectedValue = lastMonth.Month.ToString()
        If ddlYear.Items.FindByValue(lastMonth.Year.ToString()) IsNot Nothing Then
            ddlYear.SelectedValue = lastMonth.Year.ToString()
        End If
    End Sub

    Private Sub PopulateEmployeesDropDown(ByVal currentUserID As Integer, ByVal currentUserRoleID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT UserID, Name FROM Users WHERE RoleID <> 1"
        If currentUserRoleID = 3 Then ' Manager sees only their team
            query &= " AND ManagerID = @ManagerID"
        End If
        query &= " ORDER BY Name"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentUserRoleID = 3 Then
                    cmd.Parameters.AddWithValue("@ManagerID", currentUserID)
                End If
                conn.Open()
                ddlEmployees.DataSource = cmd.ExecuteReader()
                ddlEmployees.DataTextField = "Name"
                ddlEmployees.DataValueField = "UserID"
                ddlEmployees.DataBind()
            End Using
        End Using
        ddlEmployees.Items.Insert(0, New ListItem("-- Select Employee --", "0"))
    End Sub

    Protected Sub btnGenerateReport_Click(sender As Object, e As EventArgs) Handles btnGenerateReport.Click
        Dim selectedEmployeeID As Integer
        If divEmployeeFilter.Visible Then
            If ddlEmployees.SelectedValue = "0" Then
                ' You can add a label to show an error message here
                Return
            End If
            selectedEmployeeID = CInt(ddlEmployees.SelectedValue)
        Else
            selectedEmployeeID = CInt(Session("UserID"))
        End If

        GenerateReportForEmployee(selectedEmployeeID)
    End Sub

    Private Sub GenerateReportForEmployee(ByVal employeeID As Integer)
        pnlReportContent.Visible = True
        Dim selectedMonth As Integer = CInt(ddlMonth.SelectedValue)
        Dim selectedYear As Integer = CInt(ddlYear.SelectedValue)

        Dim startDate As New Date(selectedYear, selectedMonth, 1)
        Dim endDate As Date = startDate.AddMonths(1).AddDays(-1)
        Dim totalDaysInMonth As Integer = endDate.Day

        Dim dailyStatusList As New List(Of DailyStatus)

        ' Fetch all data for the month in one go
        Dim attendanceData As DataTable = GetAttendanceData(employeeID, startDate, endDate)
        Dim leaveData As DataTable = GetLeaveData(employeeID, startDate, endDate)
        Dim holidayData As DataTable = GetHolidayData(startDate, endDate)

        Dim presentCount, absentCount, leaveCount, holidayCount, weekendCount As Integer

        For i As Integer = 1 To totalDaysInMonth
            Dim currentDate As New Date(selectedYear, selectedMonth, i)
            Dim dayStatus As New DailyStatus With {.Date = currentDate}

            ' 1. Check for Holiday
            Dim holidayRow = holidayData.Select("HolidayDate = #" & currentDate.ToString("MM/dd/yyyy") & "#")
            If holidayRow.Length > 0 Then
                dayStatus.Status = holidayRow(0)("HolidayName").ToString()
                dayStatus.StatusCssClass = "status-holiday"
                holidayCount += 1
                ' 2. Check for Weekend (Saturday/Sunday)
            ElseIf currentDate.DayOfWeek = DayOfWeek.Sunday OrElse currentDate.DayOfWeek = DayOfWeek.Saturday Then
                dayStatus.Status = "Weekend"
                dayStatus.StatusCssClass = "status-weekend"
                weekendCount += 1
                ' 3. Check for Leave
            Else
                Dim onLeave As Boolean = False
                For Each row As DataRow In leaveData.Rows
                    If currentDate.Date >= CType(row("StartDate"), Date).Date AndAlso currentDate.Date <= CType(row("EndDate"), Date).Date Then
                        dayStatus.Status = row("LeaveType").ToString()
                        dayStatus.StatusCssClass = "status-leave"
                        leaveCount += 1
                        onLeave = True
                        Exit For
                    End If
                Next
                If onLeave Then
                    dailyStatusList.Add(dayStatus)
                    Continue For
                End If

                ' 4. Check for Attendance Log
                Dim attendanceRow = attendanceData.Select("LogDate = #" & currentDate.ToString("MM/dd/yyyy") & "#")
                If attendanceRow.Length > 0 Then
                    Dim punchIn As Object = attendanceRow(0)("PunchInTime")
                    Dim punchOut As Object = attendanceRow(0)("PunchOutTime")
                    Dim overtime As Object = attendanceRow(0)("OvertimeHours") ' <-- GET OT DATA

                    dayStatus.PunchIn = If(IsDBNull(punchIn), "--:--", (Date.Today + CType(punchIn, TimeSpan)).ToString("hh:mm tt"))
                    dayStatus.PunchOut = If(IsDBNull(punchOut), "--:--", (Date.Today + CType(punchOut, TimeSpan)).ToString("hh:mm tt"))

                    ' <-- ADDED LOGIC TO SET OT & TOTAL HOURS -->
                    If Not IsDBNull(punchIn) AndAlso Not IsDBNull(punchOut) Then
                        Dim totalWork As TimeSpan = CType(punchOut, TimeSpan) - CType(punchIn, TimeSpan)
                        dayStatus.TotalHours = $"{totalWork.Hours}h {totalWork.Minutes}m"
                    End If
                    If Not IsDBNull(overtime) Then
                        dayStatus.Overtime = Convert.ToDecimal(overtime).ToString("F2") & " hrs"
                    End If

                    dayStatus.Status = "Present"
                    dayStatus.StatusCssClass = "status-present"
                    presentCount += 1
                Else
                    ' 5. If nothing else, it's an Absent day
                    dayStatus.Status = "Absent"
                    dayStatus.StatusCssClass = "status-absent"
                    absentCount += 1
                End If
            End If

            dailyStatusList.Add(dayStatus)
        Next

        ' Bind the final list to the repeater
        rptMonthlyReport.DataSource = dailyStatusList
        rptMonthlyReport.DataBind()

        ' Update summary cards
        litPresentDays.Text = presentCount.ToString()
        litAbsentDays.Text = absentCount.ToString()
        litLeaveDays.Text = leaveCount.ToString()
        litHolidays.Text = holidayCount.ToString()
        litWeekends.Text = weekendCount.ToString()
    End Sub

#Region "Data Fetching Helper Functions"
    Private Function GetAttendanceData(ByVal employeeID As Integer, ByVal startDate As Date, ByVal endDate As Date) As DataTable
        ' --- QUERY UPDATED TO FETCH OVERTIMEHOURS ---
        Return GetDataTable("SELECT LogDate, PunchInTime, PunchOutTime, OvertimeHours FROM AttendanceLog WHERE EmployeeID = @EID AND LogDate BETWEEN @Start AND @End", employeeID, startDate, endDate)
    End Function

    Private Function GetLeaveData(ByVal employeeID As Integer, ByVal startDate As Date, ByVal endDate As Date) As DataTable
        Return GetDataTable("SELECT StartDate, EndDate, LeaveType FROM Leaves WHERE EmployeeID = @EID AND Status LIKE 'Approved%' AND StartDate <= @End AND EndDate >= @Start", employeeID, startDate, endDate)
    End Function

    Private Function GetHolidayData(ByVal startDate As Date, ByVal endDate As Date) As DataTable
        Return GetDataTable("SELECT HolidayName, HolidayDate FROM tblCompanyHolidays WHERE HolidayDate BETWEEN @Start AND @End", 0, startDate, endDate)
    End Function

    Private Function GetDataTable(ByVal query As String, ByVal employeeID As Integer, ByVal startDate As Date, ByVal endDate As Date) As DataTable
        Dim dt As New DataTable()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If employeeID > 0 Then cmd.Parameters.AddWithValue("@EID", employeeID)
                cmd.Parameters.AddWithValue("@Start", startDate)
                cmd.Parameters.AddWithValue("@End", endDate)
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)
            End Using
        End Using
        Return dt
    End Function
#End Region

End Class