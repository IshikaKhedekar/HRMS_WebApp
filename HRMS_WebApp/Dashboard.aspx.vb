Imports System.Configuration
Imports System.Data.SqlClient

Public Class Dashboard
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Security Check pehle hi Master Page me ho raha hai,
        ' lekin yahan bhi rakhna ek acchi practice hai.
        If Session("IsLoggedIn") Is Nothing OrElse Not CBool(Session("IsLoggedIn")) Then
            Response.Redirect("Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            ' Dashboard par data load karna
            LoadDashboardData()

            ' Role ke hisaab se access control lagana
            ApplyPermissions()
        End If
    End Sub

    Private Sub LoadDashboardData()
        ' In functions se hum database se live data layenge
        litTotalEmployees.Text = GetTotalEmployeeCount().ToString()
        ' Abhi ke liye Present aur OnLeave ko 0 rakhte hain. Inke liye hum alag se Leaves table banayenge.
        litPresentToday.Text = "145" ' Dummy Data
        litOnLeave.Text = "7"    ' Dummy Data
    End Sub

    Private Sub ApplyPermissions()
        ' Session se RoleID nikalna
        Dim userRoleID As Integer = CInt(Session("RoleID"))
        ' Is Role ke saare permissions nikalna
        ' Dim userPermissions As List(Of String) = FetchPermissionsForRole(userRoleID) ' Iski zaroorat abhi nahi hai

        ' -- SMART LOCK SYSTEM --
        ' Attendance Cards (Present/OnLeave) sirf Manager/HR/Admin ko dikhenge
        If userRoleID = 3 OrElse userRoleID = 2 OrElse userRoleID = 1 Then ' 3=Manager, 2=HR, 1=SuperAdmin
            ' Active hai, kuch nahi karna
        Else
            pnlPresentToday.CssClass &= " widget-locked"
            pnlOnLeave.CssClass &= " widget-locked"
        End If
    End Sub

    ' --- DATABASE HELPER FUNCTIONS ---

    Private Function GetTotalEmployeeCount() As Integer
        Dim count As Integer = 0
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT COUNT(*) FROM Users WHERE RoleID <> 1;" ' SuperAdmin ko count na karein

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                Try
                    conn.Open()
                    count = CInt(cmd.ExecuteScalar())
                Catch ex As Exception
                    count = 0
                End Try
            End Using
        End Using
        Return count
    End Function

    ' Is function ki abhi is page par zaroorat nahi hai,
    ' lekin future ke liye rakh sakte hain ya hata sakte hain.
    ' Private Function FetchPermissionsForRole(ByVal roleID As Integer) As List(Of String)
    '     ...
    ' End Function

End Class