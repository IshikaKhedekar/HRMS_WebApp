Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class Settings
    Inherits System.Web.UI.Page

    ' Page-level variable for permission data
    Private AllRolePermissions As List(Of RolePermission)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("RoleID") Is Nothing OrElse CInt(Session("RoleID")) <> 1 Then
            Response.Redirect("~/Dashboard.aspx")
            Return
        End If

        If Not IsPostBack Then
            ' User Management
            BindUsersGrid()
            PopulateRolesDropDown()
            PopulateManagersDropDown()
            ' Role Management
            BindRolesRepeater()
            ' Permission Management
            LoadAllRolePermissions()
            BindPermissionMatrix()
        End If
    End Sub

    ' ===============================================
    ' == USER MANAGEMENT LOGIC (Aapka pehle ka code)
    ' ===============================================
    Private Sub BindUsersGrid() '... (ye function ainvayi rahega)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT u.UserID, u.Name, u.Email, r.RoleName, mu.Name as ManagerName FROM Users u LEFT JOIN Roles r ON u.RoleID = r.RoleID LEFT JOIN Users mu ON u.ManagerID = mu.UserID WHERE u.RoleID <> 1 ORDER BY u.Name"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                rptUsers.DataSource = cmd.ExecuteReader()
                rptUsers.DataBind()
            End Using
        End Using
    End Sub
    Private Sub PopulateRolesDropDown() '... (ye function ainvayi rahega)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT RoleID, RoleName FROM Roles WHERE RoleID <> 1"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                ddlRoles.DataSource = cmd.ExecuteReader()
                ddlRoles.DataBind()
            End Using
        End Using
        ddlRoles.Items.Insert(0, New ListItem("-- Select Role --", "0"))
    End Sub
    Private Sub PopulateManagersDropDown() '... (ye function ainvayi rahega)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT UserID, Name FROM Users WHERE RoleID IN (1, 2, 3)"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                ddlManagers.DataSource = cmd.ExecuteReader()
                ddlManagers.DataBind()
            End Using
        End Using
        ddlManagers.Items.Insert(0, New ListItem("-- No Manager --", "0"))
    End Sub
    Protected Sub btnAddUser_Click(sender As Object, e As EventArgs) Handles btnAddUser.Click '... (ye ainvayi rahega)
        hdnUserID.Value = "0"
        txtName.Text = ""
        txtEmail.Text = ""
        txtPassword.Attributes("placeholder") = "Enter a strong password"
        ddlRoles.ClearSelection()
        ddlManagers.ClearSelection()
        OpenModal()
    End Sub
    Protected Sub rptUsers_ItemCommand(source As Object, e As RepeaterCommandEventArgs) '... (ye ainvayi rahega)
        If e.CommandName = "EditUser" Then
            Dim userID As Integer = CInt(e.CommandArgument)
            hdnUserID.Value = userID.ToString()
            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Dim query As String = "SELECT Name, Email, RoleID, ManagerID FROM Users WHERE UserID = @UserID"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@UserID", userID)
                    conn.Open()
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        txtName.Text = reader("Name").ToString()
                        txtEmail.Text = reader("Email").ToString()
                        txtPassword.Attributes("placeholder") = "Leave empty to not change"
                        ddlRoles.SelectedValue = reader("RoleID").ToString()
                        If reader("ManagerID") IsNot DBNull.Value Then ddlManagers.SelectedValue = reader("ManagerID").ToString() Else ddlManagers.SelectedValue = "0"
                    End If
                End Using
            End Using
            OpenModal()
        End If
    End Sub
    Protected Sub btnSaveUser_Click(sender As Object, e As EventArgs) Handles btnSaveUser.Click '... (ye ainvayi rahega)
        Dim userID As Integer = CInt(hdnUserID.Value)
        Dim name As String = txtName.Text.Trim()
        Dim email As String = txtEmail.Text.Trim()
        Dim password As String = txtPassword.Text.Trim()
        Dim roleID As Integer = CInt(ddlRoles.SelectedValue)
        Dim managerID As Integer = CInt(ddlManagers.SelectedValue)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        If userID = 0 Then
            Dim query As String = "INSERT INTO Users (Name, Email, Password, RoleID, ManagerID) VALUES (@Name, @Email, @Password, @RoleID, @ManagerID)"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@Name", name)
                    cmd.Parameters.AddWithValue("@Email", email)
                    cmd.Parameters.AddWithValue("@Password", password)
                    cmd.Parameters.AddWithValue("@RoleID", roleID)
                    cmd.Parameters.AddWithValue("@ManagerID", If(managerID = 0, CObj(DBNull.Value), managerID))
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Else
            Dim query As String = If(String.IsNullOrEmpty(password), "UPDATE Users SET Name = @Name, Email = @Email, RoleID = @RoleID, ManagerID = @ManagerID WHERE UserID = @UserID", "UPDATE Users SET Name = @Name, Email = @Email, Password = @Password, RoleID = @RoleID, ManagerID = @ManagerID WHERE UserID = @UserID")
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@UserID", userID)
                    cmd.Parameters.AddWithValue("@Name", name)
                    cmd.Parameters.AddWithValue("@Email", email)
                    cmd.Parameters.AddWithValue("@RoleID", roleID)
                    cmd.Parameters.AddWithValue("@ManagerID", If(managerID = 0, CObj(DBNull.Value), managerID))
                    If Not String.IsNullOrEmpty(password) Then cmd.Parameters.AddWithValue("@Password", password)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End If
        BindUsersGrid()
        CloseModal()
    End Sub
    Private Sub OpenModal() '... (ye ainvayi rahega)
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenUserModal", "var myModal = new bootstrap.Modal(document.getElementById('userModal')); myModal.show();", True)
    End Sub
    Private Sub CloseModal() '... (ye ainvayi rahega)
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseUserModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('userModal')); if(myModal) myModal.hide();", True)
    End Sub


    ' ===============================================
    ' == ROLE MANAGEMENT LOGIC (Naya Code)
    ' ===============================================
    Private Sub BindRolesRepeater()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT RoleID, RoleName FROM Roles WHERE RoleID <> 1"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                rptRoles.DataSource = cmd.ExecuteReader()
                rptRoles.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub btnAddRole_Click(sender As Object, e As EventArgs) Handles btnAddRole.Click
        Dim roleName As String = txtNewRoleName.Text.Trim()
        If String.IsNullOrEmpty(roleName) Then
            lblRoleMessage.Text = "Role name cannot be empty."
            lblRoleMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Pehle ek naya RoleID nikalenge
        Dim newRoleID As Integer = 0
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand("SELECT MAX(RoleID) + 1 FROM Roles", conn)
                conn.Open()
                newRoleID = CInt(cmd.ExecuteScalar())
            End Using
            Using cmd As New SqlCommand("INSERT INTO Roles (RoleID, RoleName) VALUES (@RoleID, @RoleName)", conn)
                cmd.Parameters.AddWithValue("@RoleID", newRoleID)
                cmd.Parameters.AddWithValue("@RoleName", roleName)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        lblRoleMessage.Text = "Role added successfully!"
        lblRoleMessage.ForeColor = Drawing.Color.Green
        txtNewRoleName.Text = ""
        BindRolesRepeater() ' List refresh karo
        ' Permission matrix bhi refresh karna hoga
        LoadAllRolePermissions()
        BindPermissionMatrix()
    End Sub

    Protected Sub rptRoles_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "DeleteRole" Then
            Dim roleID As Integer = CInt(e.CommandArgument)
            ' Yahan par logic likhna hoga ki agar is role me users hain to delete na ho
            ' Abhi ke liye hum seedha delete kar rahe hain
            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Using conn As New SqlConnection(connStr)
                ' Pehle RolePermissions se delete karo
                Using cmd As New SqlCommand("DELETE FROM RolePermissions WHERE RoleID = @RoleID", conn)
                    cmd.Parameters.AddWithValue("@RoleID", roleID)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
                ' Phir Roles se delete karo
                Using cmd As New SqlCommand("DELETE FROM Roles WHERE RoleID = @RoleID", conn)
                    cmd.Parameters.AddWithValue("@RoleID", roleID)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            BindRolesRepeater()
            LoadAllRolePermissions()
            BindPermissionMatrix()
        End If
    End Sub

    ' ===============================================
    ' == PERMISSION MANAGEMENT LOGIC (Naya Code)
    ' ===============================================
    Private Sub LoadAllRolePermissions()
        AllRolePermissions = New List(Of RolePermission)()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT RoleID, PermissionID FROM RolePermissions"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        AllRolePermissions.Add(New RolePermission With {
                            .RoleID = CInt(reader("RoleID")),
                            .PermissionID = CInt(reader("PermissionID"))
                        })
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub BindPermissionMatrix()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT PermissionID, PermissionName FROM Permissions"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                rptPermissions.DataSource = cmd.ExecuteReader()
                rptPermissions.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub rptPermissions_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Header Then
            Dim ph As PlaceHolder = CType(e.Item.FindControl("phRoleHeaders"), PlaceHolder)
            Dim dt As DataTable = GetRolesForHeader()
            For Each row As DataRow In dt.Rows
                ph.Controls.Add(New Literal() With {.Text = "<th>" & row("RoleName").ToString() & "</th>"})
            Next
        End If

        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim rpt As Repeater = CType(e.Item.FindControl("rptPermissionRoles"), Repeater)
            Dim permissionID As Integer = CInt(DataBinder.Eval(e.Item.DataItem, "PermissionID"))
            rpt.DataSource = GetRolesForCheckbox(permissionID)
            rpt.DataBind()
        End If
    End Sub

    Private Function GetRolesForHeader() As DataTable
        Dim dt As New DataTable()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand("SELECT RoleID, RoleName FROM Roles WHERE RoleID <> 1", conn) ' Exclude SuperAdmin
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)
            End Using
        End Using
        Return dt
    End Function

    Public Function GetRolesForCheckbox(ByVal permissionID As Integer) As List(Of Object)
        Dim roleData As New List(Of Object)()
        Dim rolesTable As DataTable = GetRolesForHeader()
        For Each row As DataRow In rolesTable.Rows
            Dim roleID As Integer = CInt(row("RoleID"))
            roleData.Add(New With {
                .RoleID = roleID,
                .HasPermission = AllRolePermissions.Exists(Function(p) p.RoleID = roleID AndAlso p.PermissionID = permissionID)
            })
        Next
        Return roleData
    End Function

    ' Replace the old btnSavePermissions_Click with this new one
    Protected Sub btnSavePermissions_Click(sender As Object, e As EventArgs) Handles btnSavePermissions.Click
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            conn.Open()
            ' First, delete all existing permissions (except for SuperAdmin)
            Using cmd As New SqlCommand("DELETE FROM RolePermissions WHERE RoleID <> 1", conn)
                cmd.ExecuteNonQuery()
            End Using

            ' Now, loop through the repeaters and insert the new checked permissions
            For Each pItem As RepeaterItem In rptPermissions.Items
                If pItem.ItemType = ListItemType.Item OrElse pItem.ItemType = ListItemType.AlternatingItem Then
                    Dim rptRoles As Repeater = CType(pItem.FindControl("rptPermissionRoles"), Repeater)

                    For Each rItem As RepeaterItem In rptRoles.Items
                        If rItem.ItemType = ListItemType.Item OrElse rItem.ItemType = ListItemType.AlternatingItem Then

                            Dim chk As CheckBox = CType(rItem.FindControl("chkPermission"), CheckBox)

                            If chk.Checked Then
                                ' Read the IDs from the HiddenFields
                                Dim hdnRoleID As HiddenField = CType(rItem.FindControl("hdnRoleID"), HiddenField)
                                Dim hdnPermissionID As HiddenField = CType(rItem.FindControl("hdnPermissionID"), HiddenField)

                                Dim roleID As Integer = CInt(hdnRoleID.Value)
                                Dim permissionID As Integer = CInt(hdnPermissionID.Value)

                                ' Insert the new permission into the database
                                Using cmd As New SqlCommand("INSERT INTO RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)", conn)
                                    cmd.Parameters.AddWithValue("@RoleID", roleID)
                                    cmd.Parameters.AddWithValue("@PermissionID", permissionID)
                                    cmd.ExecuteNonQuery()
                                End Using
                            End If
                        End If
                    Next
                End If
            Next
        End Using

        lblPermissionMessage.Text = "Permissions saved successfully!"
        lblPermissionMessage.ForeColor = Drawing.Color.Green
        ' Refresh the data to show the new state
        LoadAllRolePermissions()
        BindPermissionMatrix()
    End Sub
End Class

' === HELPER CLASS for Permission Management ===
Public Class RolePermission
    Public Property RoleID As Integer
    Public Property PermissionID As Integer
End Class