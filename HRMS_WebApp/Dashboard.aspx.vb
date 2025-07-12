Imports System.Configuration
Imports System.Data.SqlClient

Public Class Dashboard
    Inherits System.Web.UI.Page

    ' Class-level variables for database connection and user details
    Private ReadOnly connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
    Private userID As Integer
    Private userRoleID As Integer

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Security check at the beginning
        If Session("UserID") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        ' Initialize user details
        userID = CInt(Session("UserID"))
        userRoleID = CInt(Session("RoleID"))

        If Not IsPostBack Then
            ' A single, clean method to load all dashboard components
            LoadAllDashboardData()
        End If
    End Sub

    ''' <summary>
    ''' Main method to orchestrate the loading of all dashboard data by calling individual helper functions.
    ''' </summary>
    Private Sub LoadAllDashboardData()
        Using conn As New SqlConnection(connStr)
            conn.Open()
            ' Call each function to populate a specific card
            SetTotalEmployeeCount(conn)
            SetPresentTodayCount(conn)
            SetOnLeaveCount(conn)
            SetUpcomingHoliday(conn)
            SetUpcomingBirthday(conn)
        End Using

        ' Apply role-based permissions after loading data
        ApplyCardPermissions()
    End Sub

    ' =========================================================================
    ' == INDIVIDUAL DATA FETCHING FUNCTIONS (Professional & Separated Logic) ==
    ' =========================================================================

    ''' <summary>
    ''' Fetches and displays the total number of active employees.
    ''' </summary>
    Private Sub SetTotalEmployeeCount(ByVal conn As SqlConnection)
        Dim query As String = "SELECT COUNT(*) FROM Users WHERE RoleID <> 1;" ' Exclude SuperAdmin
        Using cmd As New SqlCommand(query, conn)
            litTotalEmployees.Text = cmd.ExecuteScalar().ToString()
        End Using
    End Sub

    ''' <summary>
    ''' Fetches and displays the count of employees who are present today.
    ''' </summary>
    Private Sub SetPresentTodayCount(ByVal conn As SqlConnection)
        Dim query As String = "SELECT COUNT(*) FROM AttendanceLog WHERE LogDate = @Today"
        Using cmd As New SqlCommand(query, conn)
            cmd.Parameters.AddWithValue("@Today", Date.Today)
            litPresentToday.Text = cmd.ExecuteScalar().ToString()
        End Using
    End Sub

    ''' <summary>
    ''' Fetches and displays the count of employees on leave today, based on the user's role.
    ''' </summary>
    Private Sub SetOnLeaveCount(ByVal conn As SqlConnection)
        Dim query As String = "SELECT COUNT(DISTINCT EmployeeID) FROM Leaves WHERE @Today BETWEEN StartDate AND EndDate AND Status LIKE 'Approved%'"
        ' For Managers, restrict the query to their team
        If userRoleID = 3 Then
            query &= " AND ManagerID = @UserID"
        End If

        Using cmd As New SqlCommand(query, conn)
            cmd.Parameters.AddWithValue("@Today", Date.Today)
            If userRoleID = 3 Then
                cmd.Parameters.AddWithValue("@UserID", userID)
            End If
            litOnLeave.Text = cmd.ExecuteScalar().ToString()
        End Using
    End Sub

    ''' <summary>
    ''' Finds and displays the next upcoming company holiday.
    ''' </summary>
    Private Sub SetUpcomingHoliday(ByVal conn As SqlConnection)
        Dim query As String = "SELECT TOP 1 HolidayName, HolidayDate FROM tblCompanyHolidays WHERE HolidayDate >= @Today ORDER BY HolidayDate ASC"
        Using cmd As New SqlCommand(query, conn)
            cmd.Parameters.AddWithValue("@Today", Date.Today)
            Using reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    litUpcomingHolidays.Text = $"{reader("HolidayName")} - {Convert.ToDateTime(reader("HolidayDate")):dd MMM}"
                Else
                    litUpcomingHolidays.Text = "No upcoming holidays"
                End If
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Finds and displays the next upcoming employee birthday.
    ''' </summary>
    Private Sub SetUpcomingBirthday(ByVal conn As SqlConnection)
        ' This complex query finds the next birthday, handling year-end rollovers.
        Dim query As String = "SELECT TOP 1 Name, DateOfBirth FROM Users " &
                              "WHERE RoleID <> 1 AND " &
                              "DATEADD(year, DATEDIFF(year, DateOfBirth, GETDATE()), DateOfBirth) >= CAST(GETDATE() AS DATE) " &
                              "ORDER BY DATEADD(year, DATEDIFF(year, DateOfBirth, GETDATE()), DateOfBirth) ASC"

        Using cmd As New SqlCommand(query, conn)
            Using reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    Dim empName As String = reader("Name").ToString().Split(" "c)(0) ' Get first name
                    Dim dob As Date = Convert.ToDateTime(reader("DateOfBirth"))
                    litUpcomingBirthdays.Text = $"{empName} - {dob:dd MMM}"
                Else
                    ' If no more birthdays this year, find the first birthday of next year.
                    Dim nextYearQuery As String = "SELECT TOP 1 Name, DateOfBirth FROM Users WHERE RoleID <> 1 " &
                                                  "ORDER BY MONTH(DateOfBirth), DAY(DateOfBirth) ASC"
                    Using cmdNextYear As New SqlCommand(nextYearQuery, conn)
                        Using readerNext As SqlDataReader = cmdNextYear.ExecuteReader()
                            If readerNext.Read() Then
                                Dim empName As String = readerNext("Name").ToString().Split(" "c)(0)
                                Dim dob As Date = Convert.ToDateTime(readerNext("DateOfBirth"))
                                litUpcomingBirthdays.Text = $"{empName} - {dob:dd MMM}"
                            End If
                        End Using
                    End Using
                End If
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Applies role-based permissions to the dashboard cards after data is loaded.
    ''' </summary>
    Private Sub ApplyCardPermissions()
        ' By default, all links are visible. We only hide what's necessary.
        If userRoleID = 4 Then ' Employee
            ' An employee cannot see attendance approvals.
            linkPresentToday.Visible = False
        End If
    End Sub

End Class