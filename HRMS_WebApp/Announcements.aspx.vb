Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls ' Repeater ke liye

Public Class Announcements
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            ' Check user role to show/hide the create panel
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            If userRoleID = 1 OrElse userRoleID = 2 Then ' SuperAdmin or HR
                pnlCreateAnnouncement.Visible = True
            End If

            ' Load all announcements for everyone
            BindAnnouncements()
        End If
    End Sub

    ' Database se saari announcements laane ka function
    Private Sub BindAnnouncements()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Hum Users table se join karke PostedBy ka naam bhi nikalenge
        Dim query As String = "SELECT a.Title, a.Details, a.PostedOn, u.Name as PostedByUserName FROM Announcements a LEFT JOIN Users u ON a.PostedByUserID = u.UserID ORDER BY a.PostedOn DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptAnnouncements.DataSource = dt
                    rptAnnouncements.DataBind()
                    rptAnnouncements.Visible = True
                    lblNoAnnouncements.Visible = False
                Else
                    rptAnnouncements.Visible = False
                    lblNoAnnouncements.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' Publish button ka click event
    Protected Sub btnPublish_Click(sender As Object, e As EventArgs) Handles btnPublish.Click
        ' Form se values nikalna
        Dim title As String = txtTitle.Text.Trim()
        Dim details As String = txtDetails.Text.Trim()
        Dim postedByUserID As Integer = CInt(Session("UserID"))

        ' Check karna ki fields khaali to nahi hain
        If String.IsNullOrEmpty(title) OrElse String.IsNullOrEmpty(details) Then
            lblMessage.Text = "Title and Details cannot be empty."
            lblMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        ' Database me nayi announcement insert karna
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "INSERT INTO Announcements (Title, Details, PostedByUserID) VALUES (@Title, @Details, @PostedByUserID)"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Title", title)
                cmd.Parameters.AddWithValue("@Details", details)
                cmd.Parameters.AddWithValue("@PostedByUserID", postedByUserID)

                conn.Open()
                Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                If rowsAffected > 0 Then
                    lblMessage.Text = "Announcement published successfully!"
                    lblMessage.ForeColor = Drawing.Color.Green
                    ' Form ko clear karna
                    txtTitle.Text = ""
                    txtDetails.Text = ""
                    ' List ko refresh karna
                    BindAnnouncements()
                Else
                    lblMessage.Text = "Failed to publish announcement."
                    lblMessage.ForeColor = Drawing.Color.Red
                End If
            End Using
        End Using
    End Sub

End Class