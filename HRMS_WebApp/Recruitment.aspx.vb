Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class Recruitment
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return
        ' Optional: Yahan aap "Smart Lock" laga sakti hain
        ' Dim userRoleID As Integer = CInt(Session("RoleID"))
        ' If userRoleID > 2 Then Response.Redirect("~/Dashboard.aspx") : Return

        If Not IsPostBack Then
            BindJobOpenings()
            BindCandidates()
            PopulateJobFilterDropDown()
        End If
    End Sub

    ' ===============================================
    ' == DATA BINDING FUNCTIONS
    ' ===============================================

    Private Sub BindJobOpenings()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT OpeningID, JobTitle, Department, PostedOn, Status FROM JobOpenings ORDER BY PostedOn DESC"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                rptJobOpenings.DataSource = cmd.ExecuteReader()
                rptJobOpenings.DataBind()
            End Using
        End Using
    End Sub

    Private Sub BindCandidates(Optional openingID As Integer = 0)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT c.CandidateID, c.FullName, c.AppliedOn, c.Status, j.JobTitle " &
                              "FROM Candidates c JOIN JobOpenings j ON c.OpeningID = j.OpeningID "

        If openingID > 0 Then
            query &= "WHERE c.OpeningID = @OpeningID "
        End If
        query &= "ORDER BY c.AppliedOn DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If openingID > 0 Then
                    cmd.Parameters.AddWithValue("@OpeningID", openingID)
                End If
                conn.Open()
                rptCandidates.DataSource = cmd.ExecuteReader()
                rptCandidates.DataBind()
            End Using
        End Using
    End Sub

    Private Sub PopulateJobFilterDropDown()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT OpeningID, JobTitle FROM JobOpenings WHERE Status = 'Open' ORDER BY JobTitle"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                ddlJobFilter.DataSource = cmd.ExecuteReader()
                ddlJobFilter.DataTextField = "JobTitle"
                ddlJobFilter.DataValueField = "OpeningID"
                ddlJobFilter.DataBind()
            End Using
        End Using
        ddlJobFilter.Items.Insert(0, New ListItem("All Jobs", "0"))
    End Sub

    ' ===============================================
    ' == JOB OPENINGS TAB - BUTTON CLICKS
    ' ===============================================

    Protected Sub btnShowOpeningModal_Click(sender As Object, e As EventArgs)
        ' Add mode ke liye modal ko reset karo
        txtJobTitle.Text = ""
        txtDepartment.Text = ""
        txtJobDescription.Text = ""
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenOpeningModal", "var myModal = new bootstrap.Modal(document.getElementById('openingModal')); myModal.show();", True)
    End Sub

    Protected Sub btnSaveOpening_Click(sender As Object, e As EventArgs)
        Dim title As String = txtJobTitle.Text.Trim()
        Dim dept As String = txtDepartment.Text.Trim()
        Dim desc As String = txtJobDescription.Text.Trim()

        If String.IsNullOrEmpty(title) OrElse String.IsNullOrEmpty(dept) OrElse String.IsNullOrEmpty(desc) Then
            ' Yahan aap ek error message dikha sakti hain
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "INSERT INTO JobOpenings (JobTitle, Department, JobDescription) VALUES (@Title, @Dept, @Desc)"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Title", title)
                cmd.Parameters.AddWithValue("@Dept", dept)
                cmd.Parameters.AddWithValue("@Desc", desc)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        ' Data refresh karo aur modal band karo
        BindJobOpenings()
        PopulateJobFilterDropDown()
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseOpeningModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('openingModal')); if(myModal) myModal.hide();", True)
    End Sub

    Protected Sub btnViewCandidates_Click(sender As Object, e As EventArgs)
        Dim btn As Button = CType(sender, Button)
        Dim openingID As Integer = CInt(btn.CommandArgument)

        ' Dropdown ko us job ke liye set karo
        ddlJobFilter.SelectedValue = openingID.ToString()
        ' Candidate list ko filter karo
        BindCandidates(openingID)
        ' User ko seedha doosre tab par le jao
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "SwitchToCandidatesTab", "var tab = new bootstrap.Tab(document.getElementById('candidates-tab')); tab.show();", True)
    End Sub

    ' ===============================================
    ' == CANDIDATE MANAGEMENT TAB - BUTTON CLICKS
    ' ===============================================

    Protected Sub ddlJobFilter_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim selectedOpeningID As Integer = CInt(ddlJobFilter.SelectedValue)
        BindCandidates(selectedOpeningID)
    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs)
        ddlJobFilter.SelectedValue = "0"
        BindCandidates() ' Saare candidates dobara load karo
    End Sub

    Protected Sub btnViewCandidateDetails_Click(sender As Object, e As EventArgs)
        Dim btn As Button = CType(sender, Button)
        Dim candidateID As Integer = CInt(btn.CommandArgument)
        hdnCandidateID.Value = candidateID.ToString()

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT FullName, Email, Phone, ResumePath, Status, Notes FROM Candidates WHERE CandidateID = @CandidateID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CandidateID", candidateID)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    lblName.Text = reader("FullName").ToString()
                    lblEmail.Text = reader("Email").ToString()
                    lblPhone.Text = reader("Phone").ToString()
                    hlResume.NavigateUrl = reader("ResumePath").ToString()
                    ddlCandidateStatus.SelectedValue = reader("Status").ToString()
                    txtNotes.Text = reader("Notes").ToString()
                End If
            End Using
        End Using
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenCandidateModal", "var myModal = new bootstrap.Modal(document.getElementById('candidateModal')); myModal.show();", True)
    End Sub

    Protected Sub btnUpdateCandidate_Click(sender As Object, e As EventArgs)
        Dim candidateID As Integer = CInt(hdnCandidateID.Value)
        Dim newStatus As String = ddlCandidateStatus.SelectedValue
        Dim newNotes As String = txtNotes.Text.Trim()

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "UPDATE Candidates SET Status = @Status, Notes = @Notes WHERE CandidateID = @CandidateID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CandidateID", candidateID)
                cmd.Parameters.AddWithValue("@Status", newStatus)
                cmd.Parameters.AddWithValue("@Notes", newNotes)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        ' Data refresh karo aur modal band karo
        Dim selectedOpeningID As Integer = CInt(ddlJobFilter.SelectedValue)
        BindCandidates(selectedOpeningID)
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseCandidateModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('candidateModal')); if(myModal) myModal.hide();", True)
    End Sub

End Class