Imports System.Configuration
Imports System.Data.SqlClient

Public Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Page load par kuch nahi karna
    End Sub

    Protected Sub btnLogin_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim email As String = txtEmail.Text.Trim()
        Dim password As String = txtPassword.Text.Trim()

        If String.IsNullOrEmpty(email) Or String.IsNullOrEmpty(password) Then
            lblError.Text = "Email and Password are required."
            Return
        End If

        Dim conn As New SqlConnection(connStr)
        Try
            conn.Open()
            Dim cmd As New SqlCommand("SELECT UserID, Name, RoleID FROM Users WHERE Email = @Email AND Password = @Password", conn)
            cmd.Parameters.AddWithValue("@Email", email)
            cmd.Parameters.AddWithValue("@Password", password)
            Dim reader As SqlDataReader = cmd.ExecuteReader()

            If reader.Read() Then
                Session("IsLoggedIn") = True
                Session("UserID") = reader("UserID")
                Session("UserName") = reader("Name")
                Session("RoleID") = reader("RoleID")
                Response.Redirect("Dashboard.aspx")
            Else
                lblError.Text = "Invalid email or password."
            End If
        Catch ex As Exception
            lblError.Text = "An error occurred. Please check configuration."
        Finally
            conn.Close()
        End Try
    End Sub

End Class