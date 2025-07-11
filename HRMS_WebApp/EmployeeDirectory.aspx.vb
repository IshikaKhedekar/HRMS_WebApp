Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls ' HtmlTableCell ke liye zaroori

Public Class EmployeeDirectory
    Inherits System.Web.UI.Page

    ' Page Load Event
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            ' Page pehli baar load hone par saara data bind karo
            BindEmployeeData()
        End If
    End Sub

    ' Database se employees ka data laane aur Repeater se bind karne ka function
    Private Sub BindEmployeeData()
        Dim searchTerm As String = txtSearch.Text.Trim()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Hum Users table se saara data nikal rahe hain, RoleID 1 (SuperAdmin) ko chhod kar
        Dim query As String = "SELECT UserID, Name, Email, Designation, Department, Mobile FROM Users WHERE RoleID <> 1"

        ' Agar search box me kuch likha hai, to query me filter jodo
        If Not String.IsNullOrEmpty(searchTerm) Then
            query &= " AND (Name LIKE @SearchTerm OR Department LIKE @SearchTerm)"
        End If

        query &= " ORDER BY Name ASC" ' Alphabetical order me dikhao

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                ' Parameterized query ka istemal SQL Injection se bachne ke liye
                If Not String.IsNullOrEmpty(searchTerm) Then
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" & searchTerm & "%")
                End If

                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptEmployees.DataSource = dt
                    rptEmployees.DataBind()
                    rptEmployees.Visible = True
                    lblNoEmployees.Visible = False
                Else
                    rptEmployees.Visible = False
                    lblNoEmployees.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' Ye event har row ke banne par chalta hai. Yahan hum columns ko hide/show karenge.
    Protected Sub rptEmployees_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptEmployees.ItemDataBound
        ' Logged-in user ka role nikalna
        Dim userRoleID As Integer = CInt(Session("RoleID"))

        ' Agar user HR ya SuperAdmin hai, to extra columns dikhao
        If userRoleID = 1 OrElse userRoleID = 2 Then
            ' Header row ke liye
            If e.Item.ItemType = ListItemType.Header Then
                Dim thMobile As HtmlTableCell = CType(e.Item.FindControl("thMobileHeader"), HtmlTableCell)
                Dim thActions As HtmlTableCell = CType(e.Item.FindControl("thActionsHeader"), HtmlTableCell)
                thMobile.Visible = True
                thActions.Visible = True
            End If
            ' Data rows ke liye
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim tdMobile As HtmlTableCell = CType(e.Item.FindControl("tdMobile"), HtmlTableCell)
                Dim tdActions As HtmlTableCell = CType(e.Item.FindControl("tdActions"), HtmlTableCell)
                tdMobile.Visible = True
                tdActions.Visible = True
            End If
        End If
    End Sub

    ' Search button ka click event
    Protected Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        ' Data ko search term ke hisaab se dobara bind karo
        BindEmployeeData()
    End Sub

    ' Clear button ka click event
    Protected Sub btnClearSearch_Click(sender As Object, e As EventArgs) Handles btnClearSearch.Click
        ' Search box khaali karo aur poori list dobara dikhao
        txtSearch.Text = ""
        BindEmployeeData()
    End Sub

    ' Grid ke andar "Edit" button ka click event
    Protected Sub btnEdit_Click(sender As Object, e As EventArgs)
        Dim btn As Button = CType(sender, Button)
        Dim userIDToEdit As String = btn.CommandArgument.ToString()

        ' HR/Admin ko us user ke profile page par bhej do
        ' Hum QueryString ka istemal karke UserID pass karenge
        Response.Redirect("MyProfile.aspx?UserID=" & userIDToEdit)
    End Sub

End Class