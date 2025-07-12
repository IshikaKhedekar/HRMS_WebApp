Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports System.Web.UI

Public Class Site
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("IsLoggedIn") Is Nothing OrElse Not CBool(Session("IsLoggedIn")) Then
            Response.Redirect("~/Login.aspx")
            Return
        Else
            lblWelcomeMessage.Text = "Hello, " & Session("UserName").ToString()

            ApplyMenuPermissions()

            If Not IsPostBack Then
                ' Notifications ko load karo
                LoadNotifications()
            End If
        End If
    End Sub

    ' === NOTIFICATION LOGIC ===
    Private Sub LoadNotifications()
        Dim userRoleID As Integer = CInt(Session("RoleID"))
        Dim userID As Integer = CInt(Session("UserID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString

        Dim pendingCount As Integer = 0
        Dim notifications As New List(Of Object)()

        ' Sirf Manager aur HR/Admin ko leave approval notifications dikhengi
        If userRoleID = 3 OrElse userRoleID = 2 OrElse userRoleID = 1 Then
            Dim query As String = "SELECT l.LeaveID, u.Name as EmployeeName, l.LeaveType, l.StartDate, l.EndDate, l.Reason FROM Leaves l JOIN Users u ON l.EmployeeID = u.UserID WHERE l.Status = 'Pending'"
            If userRoleID = 3 Then ' Manager ke liye sirf uski team ki pending requests
                query &= " AND l.ManagerID = @UserID"
            End If
            query &= " ORDER BY l.AppliedOn DESC"

            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    If userRoleID = 3 Then cmd.Parameters.AddWithValue("@UserID", userID)
                    conn.Open()
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        pendingCount += 1
                        notifications.Add(New With {
                            .LeaveID = CInt(reader("LeaveID")),
                            .EmployeeName = reader("EmployeeName").ToString(),
                            .LeaveType = reader("LeaveType").ToString(),
                            .StartDate = Convert.ToDateTime(reader("StartDate")),
                            .EndDate = Convert.ToDateTime(reader("EndDate")),
                            .Reason = reader("Reason").ToString()
                        })
                    End While
                End Using
            End Using
        End If

        ' Badge me count dikhana
        If pendingCount > 0 Then
            litNotificationBadge.Visible = True
            litNotificationBadge.Text = $"<span class='notification-badge'>{pendingCount}</span>"
            rptNotifications.DataSource = notifications
            rptNotifications.DataBind()
            lblNoNotifications.Visible = False
        Else
            litNotificationBadge.Visible = False
            lblNoNotifications.Visible = True
            rptNotifications.DataBind()
            lblNoNotifications.Visible = True
        End If
    End Sub
    ' ==========================

    ' === Smart Lock system (ApplyMenuPermissions) ===
    Private Sub ApplyMenuPermissions()
        Dim userRoleID As Integer = CInt(Session("RoleID"))

        ' Reset classes first
        liRecruitment.Attributes("class") = ""
        liPerformance.Attributes("class") = "has-submenu"
        liPayroll.Attributes("class") = ""
        liPolicies.Attributes("class") = ""
        liAdminPanel.Visible = False

        Select Case userRoleID
            Case 4 ' Employee Role
                liRecruitment.Attributes("class") = "menu-locked"
                liPerformance.Attributes("class") = "has-submenu menu-locked"  '        liPerformance.Attributes("class") = "menu-locked"
                liPayroll.Attributes("class") = "menu-locked"
                liLeaveReportsSubMenu.Visible = False
                liAttendanceApprovals.Visible = False' Admin ko dikhega
            Case 3 ' Manager Role
                liRecruitment.Attributes("class") = "menu-locked"
                liPayroll.Attributes("class") = "menu-locked"
                liAttendanceApprovals.Visible = True ' Admin ko dikhega
            Case 2 ' HR Role
                ' All active except Admin Panel
                liPerformanceAnalytics.Visible = True
                liAttendanceApprovals.Visible = True ' Admin ko dikhega
                liAttendanceAnalytics.Visible = True
            Case 1 ' SuperAdmin Role
                liAdminPanel.Visible = True
                liPerformanceAnalytics.Visible = True
                liAttendanceApprovals.Visible = True ' Admin ko dikhega
                liAttendanceAnalytics.Visible = True
        End Select
    End Sub
    ' ===============================================

    Protected Sub btnLogout_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnLogout.Click
        Session.Clear()
        Session.Abandon()
        Response.Redirect("~/Login.aspx")
    End Sub

End Class