Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class AttendanceAnalytics
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Restrict access
        Dim userRoleID As Integer = CInt(Session("RoleID"))
        If userRoleID > 2 Then Response.Redirect("~/Dashboard.aspx") : Return

        If Not IsPostBack Then
            LoadAnalytics()
        End If
    End Sub

    ' REPLACE THE ENTIRE LoadAnalytics function with this new version

    Private Sub LoadAnalytics()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim last30Days As Date = Date.Today.AddDays(-30)
        Dim serializer As New JavaScriptSerializer()

        ' --- 1. Top 5 Late Comers ---
        Dim dtLateComers As New System.Data.DataTable()
        Dim lateComersQuery As String = "SELECT TOP 5 u.Name, COUNT(al.LogID) AS LateCount " &
                                    "FROM AttendanceLog al JOIN Users u ON al.EmployeeID = u.UserID " &
                                    "WHERE al.LogDate >= @StartDate AND al.PunchInTime > '09:30:00' " &
                                    "GROUP BY u.Name ORDER BY LateCount DESC"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(lateComersQuery, conn)
                cmd.Parameters.AddWithValue("@StartDate", last30Days)
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dtLateComers) ' Load data into DataTable
            End Using
        End Using
        rptLateComers.DataSource = dtLateComers ' Bind from DataTable
        rptLateComers.DataBind()


        ' --- 2. Top 5 Overtime Employees ---
        Dim dtOvertime As New System.Data.DataTable()
        Dim overtimeQuery As String = "SELECT TOP 5 u.Name, SUM(al.OvertimeHours) as TotalOT " &
                                  "FROM AttendanceLog al JOIN Users u ON al.EmployeeID = u.UserID " &
                                  "WHERE al.LogDate >= @StartDate AND al.OvertimeHours > 0 " &
                                  "GROUP BY u.Name ORDER BY TotalOT DESC"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(overtimeQuery, conn)
                cmd.Parameters.AddWithValue("@StartDate", last30Days)
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dtOvertime) ' Load data into DataTable
            End Using
        End Using
        rptOvertime.DataSource = dtOvertime ' Bind from DataTable
        rptOvertime.DataBind()


        ' --- 3. Data for Daily Trend Chart ---
        Dim dailyData As New List(Of Object)
        Dim allLogsQuery As String = "SELECT LogDate, Status FROM AttendanceLog WHERE LogDate >= @StartDate"
        Dim allLeavesQuery As String = "SELECT StartDate, EndDate FROM Leaves WHERE Status LIKE 'Approved%' AND StartDate >= @StartDate"
        Dim dtLogs As New System.Data.DataTable()
        Dim dtLeaves As New System.Data.DataTable()
        Dim totalEmployees As Integer

        Using conn As New SqlConnection(connStr)
            Using adapter As New SqlDataAdapter(allLogsQuery, conn)
                adapter.SelectCommand.Parameters.AddWithValue("@StartDate", last30Days)
                adapter.Fill(dtLogs)
            End Using
            Using adapter As New SqlDataAdapter(allLeavesQuery, conn)
                adapter.SelectCommand.Parameters.AddWithValue("@StartDate", last30Days)
                adapter.Fill(dtLeaves)
            End Using
            Using cmd As New SqlCommand("SELECT COUNT(*) FROM Users WHERE RoleID <> 1", conn)
                conn.Open()
                totalEmployees = CInt(cmd.ExecuteScalar())
                conn.Close()
            End Using
        End Using

        For i As Integer = 0 To 29
            Dim aDate As Date = Date.Today.AddDays(-i)
            Dim presentCount = dtLogs.Select("LogDate = #" & aDate.ToString("MM/dd/yyyy") & "# AND Status = 'Present'").Length
            Dim onLeaveCount = dtLeaves.AsEnumerable().Count(Function(row) aDate.Date >= row.Field(Of Date)("StartDate").Date AndAlso aDate.Date <= row.Field(Of Date)("EndDate").Date)
            Dim absentCount = totalEmployees - presentCount - onLeaveCount

            dailyData.Add(New With {
            .Date = aDate.ToString("yyyy-MM-dd"),
            .Present = presentCount,
            .Absent = If(absentCount < 0, 0, absentCount),
            .OnLeave = onLeaveCount
        })
        Next
        hdnDailyTrendData.Value = serializer.Serialize(dailyData.OrderBy(Function(x) x.Date))

        ' --- 4. Data for Status Distribution Pie Chart ---
        Dim dtStatus As New System.Data.DataTable()
        Dim statusQuery As String = "SELECT Status, COUNT(*) as StatusCount FROM AttendanceLog WHERE LogDate >= @StartDate GROUP BY Status"
        Dim statusData As New List(Of Object)

        Using conn As New SqlConnection(connStr)
            Using adapter As New SqlDataAdapter(statusQuery, conn)
                adapter.SelectCommand.Parameters.AddWithValue("@StartDate", last30Days)
                adapter.Fill(dtStatus) ' Load data into DataTable
            End Using
        End Using

        For Each row As DataRow In dtStatus.Rows
            statusData.Add(New With {
            .Status = row("Status").ToString(),
            .Count = CInt(row("StatusCount"))
        })
        Next
        hdnStatusDistributionData.Value = serializer.Serialize(statusData)
    End Sub

End Class