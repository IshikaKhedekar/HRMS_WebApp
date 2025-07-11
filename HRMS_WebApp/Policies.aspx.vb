Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class Policies
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            If userRoleID = 1 OrElse userRoleID = 2 Then ' SuperAdmin or HR
                btnAddPolicy.Visible = True
            End If
            BindPolicies()
        End If
    End Sub

    ' ===============================================
    ' == DATA BINDING FUNCTIONS
    ' ===============================================

    Private Sub BindPolicies()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim selectedCategory As String = ddlPolicyCategory.SelectedValue

        Dim query As String = "SELECT p.PolicyID, p.Title, p.Category, p.Content, p.LastUpdatedOn, u.Name as LastUpdatedByUserName " &
                          "FROM Policies p LEFT JOIN Users u ON p.LastUpdatedByUserID = u.UserID "

        If selectedCategory <> "0" Then
            query &= "WHERE p.Category = @Category "
        End If

        query &= "ORDER BY p.LastUpdatedOn DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If selectedCategory <> "0" Then
                    cmd.Parameters.AddWithValue("@Category", selectedCategory)
                End If
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd) ' <<< YAHAN BADLAAV HAI >>>
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptPolicies.DataSource = dt
                    rptPolicies.DataBind()
                    lblNoPolicies.Visible = False
                Else
                    rptPolicies.DataSource = Nothing
                    rptPolicies.DataBind()
                    lblNoPolicies.Visible = True
                End If
            End Using
        End Using
    End Sub    ' Repeater ItemDataBound event (Edit/Delete buttons ke visibility ke liye)
    Protected Sub rptPolicies_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptPolicies.ItemDataBound
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            If userRoleID = 1 OrElse userRoleID = 2 Then ' SuperAdmin or HR
                CType(e.Item.FindControl("btnEditPolicy"), Button).Visible = True
                CType(e.Item.FindControl("btnDeletePolicy"), Button).Visible = True
            End If
        End If
    End Sub

    ' Filter dropdown change hone par policies ko dobara bind karna
    Protected Sub ddlPolicyCategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlPolicyCategory.SelectedIndexChanged
        BindPolicies()
    End Sub

    ' ===============================================
    ' == BUTTON CLICK EVENTS (ADD/EDIT/SAVE/DELETE)
    ' ===============================================

    Protected Sub btnAddPolicy_Click(sender As Object, e As EventArgs) Handles btnAddPolicy.Click
        ' Modal ko "Add" mode me kholne ke liye fields clear karein
        hdnPolicyID.Value = "0"
        txtPolicyTitle.Text = ""
        ddlModalCategory.ClearSelection()
        txtPolicyContent.Text = ""
        lblModalMessage.Text = "" ' Message clear karna
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenPolicyModal", "var myModal = new bootstrap.Modal(document.getElementById('policyModal')); myModal.show();", True)
    End Sub

    ' Repeater ke andar ke buttons (Edit/Delete) ke click ko handle karna
    Protected Sub rptPolicies_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptPolicies.ItemCommand
        Dim policyID As Integer = CInt(e.CommandArgument)

        If e.CommandName = "EditPolicy" Then
            hdnPolicyID.Value = policyID.ToString()
            lblModalMessage.Text = "" ' Message clear karna

            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Dim query As String = "SELECT Title, Category, Content FROM Policies WHERE PolicyID = @PolicyID"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PolicyID", policyID)
                    conn.Open()
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        txtPolicyTitle.Text = reader("Title").ToString()
                        ddlModalCategory.SelectedValue = reader("Category").ToString()
                        txtPolicyContent.Text = reader("Content").ToString()
                    End If
                End Using
            End Using
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenPolicyModal", "var myModal = new bootstrap.Modal(document.getElementById('policyModal')); myModal.show();", True)

        ElseIf e.CommandName = "DeletePolicy" Then
            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Dim query As String = "DELETE FROM Policies WHERE PolicyID = @PolicyID"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PolicyID", policyID)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            BindPolicies() ' List ko refresh karo
            ' Yahan aap page level message bhi dikha sakti hain
        End If
    End Sub

    Protected Sub btnSavePolicy_Click(sender As Object, e As EventArgs) Handles btnSavePolicy.Click
        Dim policyID As Integer = CInt(hdnPolicyID.Value)
        Dim title As String = txtPolicyTitle.Text.Trim()
        Dim category As String = ddlModalCategory.SelectedValue
        Dim content As String = txtPolicyContent.Text.Trim()
        Dim lastUpdatedByUserID As Integer = CInt(Session("UserID"))

        If String.IsNullOrEmpty(title) OrElse String.IsNullOrEmpty(content) Then
            lblModalMessage.Text = "Title and Content cannot be empty."
            lblModalMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            Using cmd As SqlCommand = New SqlCommand()
                cmd.Connection = conn
                conn.Open()
                If policyID = 0 Then ' Insert new policy
                    cmd.CommandText = "INSERT INTO Policies (Title, Category, Content, LastUpdatedByUserID) VALUES (@Title, @Category, @Content, @LastUpdatedByUserID)"
                Else ' Update existing policy
                    cmd.CommandText = "UPDATE Policies SET Title = @Title, Category = @Category, Content = @Content, LastUpdatedByUserID = @LastUpdatedByUserID, LastUpdatedOn = GETDATE() WHERE PolicyID = @PolicyID"
                    cmd.Parameters.AddWithValue("@PolicyID", policyID)
                End If

                cmd.Parameters.AddWithValue("@Title", title)
                cmd.Parameters.AddWithValue("@Category", category)
                cmd.Parameters.AddWithValue("@Content", content)
                cmd.Parameters.AddWithValue("@LastUpdatedByUserID", lastUpdatedByUserID)

                cmd.ExecuteNonQuery()
            End Using
        End Using

        BindPolicies() ' List ko refresh karo
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ClosePolicyModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('policyModal')); if(myModal) myModal.hide();", True)
        ' Page level message dikhana (optional)
        ' lblMessage.Text = "Policy saved successfully!"
        ' lblMessage.ForeColor = Drawing.Color.Green
    End Sub

End Class