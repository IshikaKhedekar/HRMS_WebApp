Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO ' For file operations
Imports System.Linq ' For OrderBy() in dropdowns (e.g., in PopulateJobFilterDropDown)
Imports System.Drawing ' For System.Drawing.Color

Public Class Recruitment
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            ' Populate dropdowns for Job Vacancies modal and public application form
            PopulateDepartmentDropDown(ddlDepartmentModal)
            PopulateHiringManagerDropDown(ddlHiringManager)
            PopulateExperienceLevelDropDown(ddlExperienceLevel)
            PopulateEducationLevelDropDown(ddlEducationLevel)
            PopulateLocationTypeDropDown(ddlLocationType)
            PopulateEmploymentTypeDropDown(ddlEmploymentType)
            PopulateCurrencyDropDown(ddlCurrency)
            PopulateJobStatusDropDown(ddlJobStatus)

            ' Populate dropdowns for Applications Tracking filter
            PopulateJobFilterDropDown()

            ' Populate dropdown for Candidate Details modal
            PopulateCandidateStatusDropDown(ddlCandidateStatus)

            ' Populate dropdown for Apply for Job tab
            PopulateAvailableJobsDropDown(ddlAvailableJobs)

            ' Set initial visibility and bind data based on user role (HR/Manager/SuperAdmin)
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            If userRoleID <= 3 Then ' Assuming Manager, HR, SuperAdmin can manage recruitment
                BindJobVacancies() ' Show active job openings
                BindCandidates()   ' Show all candidate applications
            Else
                ' For standard employees/applicants, redirect to Apply for Job tab
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "SwitchTab", "document.getElementById('apply-job-tab').click();", True)
            End If

            ' Check if we're redirected from a resignation to create a vacancy
            If Request.QueryString("ResignationID") IsNot Nothing AndAlso Integer.TryParse(Request.QueryString("ResignationID"), New Integer()) Then
                Dim resignationIDToLoad As Integer = CInt(Request.QueryString("ResignationID"))
                LoadJobOpeningFromResignation(resignationIDToLoad)
                ' Programmatically switch to Job Vacancies tab and show modal
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ShowJobModalFromResignation", "document.getElementById('job-vacancies-tab').click(); var myModal = new bootstrap.Modal(document.getElementById('jobModal')); myModal.show();", True)
            End If
        End If
    End Sub

    ' --- Helper Methods for Dropdown Populating ---
    Private Sub PopulateDepartmentDropDown(ddl As DropDownList)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
                ddl.DataSource = dt
                ddl.DataTextField = "DepartmentName"
                ddl.DataValueField = "DepartmentID"
                ddl.DataBind()
            End Using
        End Using
        ddl.Items.Insert(0, New ListItem("-- Select Department --", "0"))
    End Sub

    Private Sub PopulateHiringManagerDropDown(ddl As DropDownList)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT UserID, Name FROM Users WHERE RoleID IN (1, 2, 3) ORDER BY Name" ' Managers, HR, SuperAdmins
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
                ddl.DataSource = dt
                ddl.DataTextField = "Name"
                ddl.DataValueField = "UserID"
                ddl.DataBind()
            End Using
        End Using
        ddl.Items.Insert(0, New ListItem("-- Select Hiring Manager --", "0"))
    End Sub

    Private Sub PopulateExperienceLevelDropDown(ddl As DropDownList)
        Dim levels As New List(Of String) From {"Entry Level", "Mid Level", "Senior Level", "Lead/Principal"}
        ddl.DataSource = levels
        ddl.DataBind()
    End Sub

    Private Sub PopulateEducationLevelDropDown(ddl As DropDownList)
        Dim levels As New List(Of String) From {"High School", "Vocational Training", "Bachelor's Degree", "Master's Degree", "PhD"}
        ddl.DataSource = levels
        ddl.DataBind()
    End Sub

    Private Sub PopulateLocationTypeDropDown(ddl As DropDownList)
        Dim types As New List(Of String) From {"On-site", "Remote", "Hybrid", "Specific City"}
        ddl.DataSource = types
        ddl.DataBind()
    End Sub

    Private Sub PopulateEmploymentTypeDropDown(ddl As DropDownList)
        Dim types As New List(Of String) From {"Full-time", "Part-time", "Contract", "Internship"}
        ddl.DataSource = types
        ddl.DataBind()
    End Sub

    Private Sub PopulateCurrencyDropDown(ddl As DropDownList)
        Dim currencies As New List(Of String) From {"INR", "USD", "EUR", "GBP"}
        ddl.DataSource = currencies
        ddl.DataBind()
    End Sub

    Private Sub PopulateJobStatusDropDown(ddl As DropDownList)
        Dim statuses As New List(Of String) From {"Draft", "Open", "On Hold", "Closed", "Pending Approval"}
        ddl.DataSource = statuses
        ddl.DataBind()
    End Sub

    Private Sub PopulateJobFilterDropDown()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT OpeningID, JobTitle FROM JobOpenings ORDER BY JobTitle"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
                If dt.Rows.Count > 0 Then
                    ddlJobFilter.DataSource = dt
                    ddlJobFilter.DataTextField = "JobTitle"
                    ddlJobFilter.DataValueField = "OpeningID"
                    ddlJobFilter.DataBind()
                Else
                    ddlJobFilter.DataSource = Nothing
                    ' No DataBind if DataSource is Nothing to avoid Eval errors
                End If
            End Using
        End Using
        ddlJobFilter.Items.Insert(0, New ListItem("-- All Job Titles --", "0"))
    End Sub

    Private Sub PopulateAvailableJobsDropDown(ddl As DropDownList)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT OpeningID, JobTitle FROM JobOpenings WHERE JobStatus = 'Open' AND ApplicationDeadline >= GETDATE() OR ApplicationDeadline IS NULL ORDER BY JobTitle"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
                If dt.Rows.Count > 0 Then
                    ddl.DataSource = dt
                    ddl.DataTextField = "JobTitle"
                    ddl.DataValueField = "OpeningID"
                    ddl.DataBind()
                Else
                    ddl.DataSource = Nothing
                    ' No DataBind if DataSource is Nothing
                End If
            End Using
        End Using
        ddl.Items.Insert(0, New ListItem("-- Select a Job --", "0"))
    End Sub

    Private Sub PopulateCandidateStatusDropDown(ddl As DropDownList)
        Dim statuses As New List(Of String) From {
            "Applied", "Screened", "Interview-1", "Interview-2",
            "Offered", "Offer Accepted", "Background Check", "Reference Check", "Hired", "Rejected"
        }
        ddl.DataSource = statuses
        ddl.DataBind()
    End Sub

    ' --- JOB VACANCIES TAB LOGIC (HR/ADMIN) ---
    Private Sub BindJobVacancies()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "
    SELECT 
        jo.OpeningID,
        jo.JobTitle,
        d.DepartmentName,
        jo.LocationType,
        jo.SpecificLocation,
        jo.EmploymentType,
        jo.ExperienceLevel,
        jo.EducationLevel,
        jo.KeyResponsibilities,
        jo.RequiredSkills,
        jo.JobDescription,
        jo.MinSalary,
        jo.MaxSalary,
        jo.Currency,
        jo.BenefitsDescription,
        jo.ApplicationDeadline,
        jo.HiringManagerID,
        jo.NumberOfPositions,
        jo.JobStatus,
        jo.DatePosted,
        jo.LastUpdated
    FROM JobOpenings jo
    INNER JOIN Departments d ON jo.DepartmentID = d.DepartmentID
    ORDER BY jo.DatePosted DESC"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using

                If dt.Rows.Count > 0 Then
                    rptJobVacancies.DataSource = dt
                    rptJobVacancies.DataBind()
                    lblNoJobs.Visible = False
                Else
                    rptJobVacancies.DataSource = Nothing
                    lblNoJobs.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub btnShowJobModal_Click(sender As Object, e As EventArgs) Handles btnShowJobModal.Click
        ' Reset modal for new job creation
        hdnCurrentJobID.Value = ""
        txtJobTitleModal.Text = ""
        ddlDepartmentModal.SelectedValue = "0"
        ddlLocationType.SelectedValue = "On-site"
        txtSpecificLocation.Text = ""
        ddlEmploymentType.SelectedValue = "Full-time"
        ddlExperienceLevel.SelectedValue = "Entry Level"
        ddlEducationLevel.SelectedValue = "High School"
        txtMinSalary.Text = ""
        txtMaxSalary.Text = ""
        ddlCurrency.SelectedValue = "INR"
        txtBenefitsDescription.Text = ""
        txtKeyResponsibilities.Text = ""
        txtRequiredSkills.Text = ""
        txtJobDescriptionModal.Text = ""
        txtApplicationDeadlineModal.Text = ""
        ddlHiringManager.SelectedValue = "0"
        txtNumberOfPositions.Text = "1"
        ddlJobStatus.SelectedValue = "Draft"
        lblJobModalMessage.Text = ""

        ' If using RTE, ensure it's reset
        ' ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ResetRTE", "if (typeof CKEDITOR !== 'undefined') { CKEDITOR.instances['txtJobDescriptionModal'].setData(''); CKEDITOR.instances['txtKeyResponsibilities'].setData(''); CKEDITOR.instances['txtBenefitsDescription'].setData(''); }", True)

        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenJobModal", "var myModal = new bootstrap.Modal(document.getElementById('jobModal')); myModal.show();", True)
    End Sub

    Protected Sub btnSaveJobOpening_Click(sender As Object, e As EventArgs) Handles btnSaveJobOpening.Click
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim openingID As Integer = 0
        If Not String.IsNullOrEmpty(hdnCurrentJobID.Value) Then
            openingID = CInt(hdnCurrentJobID.Value)
        End If

        ' Collect all field values
        Dim jobTitle As String = txtJobTitleModal.Text.Trim()
        Dim departmentID As Integer = CInt(ddlDepartmentModal.SelectedValue)
        Dim departmentName As String = ddlDepartmentModal.SelectedItem.Text
        Dim locationType As String = ddlLocationType.SelectedValue
        Dim specificLocation As String = txtSpecificLocation.Text.Trim()
        Dim employmentType As String = ddlEmploymentType.SelectedValue
        Dim experienceLevel As String = ddlExperienceLevel.SelectedValue
        Dim educationLevel As String = ddlEducationLevel.SelectedValue
        Dim keyResponsibilities As String = txtKeyResponsibilities.Text.Trim()
        Dim requiredSkills As String = txtRequiredSkills.Text.Trim()
        Dim jobDescription As String = txtJobDescriptionModal.Text.Trim()
        Dim minSalary As Nullable(Of Decimal) = Nothing
        If Not String.IsNullOrEmpty(txtMinSalary.Text) Then minSalary = CDec(txtMinSalary.Text)
        Dim maxSalary As Nullable(Of Decimal) = Nothing
        If Not String.IsNullOrEmpty(txtMaxSalary.Text) Then maxSalary = CDec(txtMaxSalary.Text)
        Dim currency As String = ddlCurrency.SelectedValue
        Dim benefitsDescription As String = txtBenefitsDescription.Text.Trim()
        Dim applicationDeadline As Nullable(Of Date) = Nothing
        If Not String.IsNullOrWhiteSpace(txtApplicationDeadlineModal.Text) Then
            Dim parsedDate As DateTime
            If Date.TryParse(txtApplicationDeadlineModal.Text.Trim(), parsedDate) Then
                applicationDeadline = parsedDate
            Else
                lblJobModalMessage.Text = "Invalid date format for Application Deadline."
                lblJobModalMessage.ForeColor = Color.Red
                Exit Sub
            End If
        End If
        Dim hiringManagerID As Integer = CInt(ddlHiringManager.SelectedValue)
        Dim numberOfPositions As Integer
        If Not Integer.TryParse(txtNumberOfPositions.Text, numberOfPositions) Then numberOfPositions = 1
        Dim jobStatus As String = ddlJobStatus.SelectedValue

        If departmentID = 0 OrElse hiringManagerID = 0 Then
            lblJobModalMessage.Text = "Please select a valid Department and Hiring Manager."
            lblJobModalMessage.ForeColor = Color.Red
            Return
        End If
        If String.IsNullOrEmpty(jobTitle) OrElse String.IsNullOrEmpty(jobDescription) Then
            lblJobModalMessage.Text = "Job Title and Job Description are required."
            lblJobModalMessage.ForeColor = Color.Red
            Return
        End If

        Using conn As New SqlConnection(connStr)
            conn.Open()
            Dim query As String
            Dim cmd As SqlCommand

            If openingID = 0 Then
                ' INSERT
                query = "INSERT INTO JobOpenings (JobTitle, DepartmentID, Department, LocationType, SpecificLocation, EmploymentType, ExperienceLevel, EducationLevel, KeyResponsibilities, RequiredSkills, JobDescription, MinSalary, MaxSalary, Currency, BenefitsDescription, ApplicationDeadline, HiringManagerID, NumberOfPositions, JobStatus, DatePosted, LastUpdated) " &
                    "VALUES (@JobTitle, @DepartmentID, @Department, @LocationType, @SpecificLocation, @EmploymentType, @ExperienceLevel, @EducationLevel, @KeyResponsibilities, @RequiredSkills, @JobDescription, @MinSalary, @MaxSalary, @Currency, @BenefitsDescription, @ApplicationDeadline, @HiringManagerID, @NumberOfPositions, @JobStatus, GETDATE(), GETDATE());"
                cmd = New SqlCommand(query, conn)
            Else
                ' UPDATE
                query = "UPDATE JobOpenings SET JobTitle=@JobTitle, DepartmentID=@DepartmentID, Department=@Department, LocationType=@LocationType, SpecificLocation=@SpecificLocation, EmploymentType=@EmploymentType, ExperienceLevel=@ExperienceLevel, EducationLevel=@EducationLevel, KeyResponsibilities=@KeyResponsibilities, RequiredSkills=@RequiredSkills, JobDescription=@JobDescription, MinSalary=@MinSalary, MaxSalary=@MaxSalary, Currency=@Currency, BenefitsDescription=@BenefitsDescription, ApplicationDeadline=@ApplicationDeadline, HiringManagerID=@HiringManagerID, NumberOfPositions=@NumberOfPositions, JobStatus=@JobStatus, LastUpdated=GETDATE() WHERE OpeningID=@OpeningID"
                cmd = New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@OpeningID", openingID)
            End If

            ' Common parameters for both INSERT and UPDATE
            cmd.Parameters.AddWithValue("@JobTitle", jobTitle)
            cmd.Parameters.AddWithValue("@DepartmentID", departmentID)
            cmd.Parameters.AddWithValue("@Department", departmentName)
            cmd.Parameters.AddWithValue("@LocationType", locationType)
            cmd.Parameters.AddWithValue("@SpecificLocation", If(String.IsNullOrEmpty(specificLocation), DBNull.Value, CType(specificLocation, Object)))
            cmd.Parameters.AddWithValue("@EmploymentType", employmentType)
            cmd.Parameters.AddWithValue("@ExperienceLevel", experienceLevel)
            cmd.Parameters.AddWithValue("@EducationLevel", educationLevel)
            cmd.Parameters.AddWithValue("@KeyResponsibilities", If(String.IsNullOrEmpty(keyResponsibilities), DBNull.Value, CType(keyResponsibilities, Object)))
            cmd.Parameters.AddWithValue("@RequiredSkills", If(String.IsNullOrEmpty(requiredSkills), DBNull.Value, CType(requiredSkills, Object)))
            cmd.Parameters.AddWithValue("@JobDescription", jobDescription)
            cmd.Parameters.AddWithValue("@MinSalary", If(minSalary.HasValue, CType(minSalary.Value, Object), DBNull.Value))
            cmd.Parameters.AddWithValue("@MaxSalary", If(maxSalary.HasValue, CType(maxSalary.Value, Object), DBNull.Value))
            cmd.Parameters.AddWithValue("@Currency", currency)
            cmd.Parameters.AddWithValue("@BenefitsDescription", If(String.IsNullOrEmpty(benefitsDescription), DBNull.Value, CType(benefitsDescription, Object)))
            cmd.Parameters.AddWithValue("@ApplicationDeadline", If(applicationDeadline.HasValue, CType(applicationDeadline.Value, Object), DBNull.Value))
            cmd.Parameters.AddWithValue("@HiringManagerID", hiringManagerID)
            cmd.Parameters.AddWithValue("@NumberOfPositions", numberOfPositions)
            cmd.Parameters.AddWithValue("@JobStatus", jobStatus)

            cmd.ExecuteNonQuery()

            lblJobModalMessage.Text = If(openingID = 0, "Job opening added successfully!", "Job opening updated successfully!")
            lblJobModalMessage.ForeColor = Color.Green
        End Using

        ' Refresh all data
        BindJobVacancies()
        PopulateJobFilterDropDown()
        PopulateAvailableJobsDropDown(ddlAvailableJobs)
    End Sub

    Protected Sub rptJobVacancies_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptJobVacancies.ItemCommand
        Dim openingID As Integer = CInt(e.CommandArgument)
        If e.CommandName = "EditJob" Then
            LoadJobForEdit(openingID)
        ElseIf e.CommandName = "DeleteJob" Then
            DeleteJob(openingID)
            BindJobVacancies() ' Refresh after delete
            PopulateJobFilterDropDown() ' Update job filter
            PopulateAvailableJobsDropDown(ddlAvailableJobs) ' Update public jobs list
        ElseIf e.CommandName = "ShareJob" Then
            ' Example for LinkedIn share. Replace with your actual public job details page URL
            Dim publicJobURL As String = ResolveUrl($"~/PublicJobDetails.aspx?JobID={openingID}")
            Dim linkedInShareURL As String = $"https://www.linkedin.com/sharing/share-offsite/?url={Server.UrlEncode(publicJobURL)}"
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ShareLinkedIn", $"window.open('{linkedInShareURL}', '_blank');", True)
        End If
    End Sub

    Private Sub LoadJobForEdit(openingID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT * FROM JobOpenings WHERE OpeningID = @OpeningID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@OpeningID", openingID)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    hdnCurrentJobID.Value = openingID.ToString()
                    txtJobTitleModal.Text = reader("JobTitle").ToString()
                    If ddlDepartmentModal.Items.FindByValue(reader("DepartmentID").ToString()) IsNot Nothing Then
                        ddlDepartmentModal.SelectedValue = reader("DepartmentID").ToString()
                    Else
                        ddlDepartmentModal.SelectedValue = "0"
                    End If
                    ddlLocationType.SelectedValue = reader("LocationType").ToString()
                    txtSpecificLocation.Text = If(reader("SpecificLocation") Is DBNull.Value, "", reader("SpecificLocation").ToString())
                    ddlEmploymentType.SelectedValue = reader("EmploymentType").ToString()
                    ddlExperienceLevel.SelectedValue = reader("ExperienceLevel").ToString()
                    ddlEducationLevel.SelectedValue = reader("EducationLevel").ToString()
                    txtMinSalary.Text = If(reader("MinSalary") Is DBNull.Value, "", reader("MinSalary").ToString())
                    txtMaxSalary.Text = If(reader("MaxSalary") Is DBNull.Value, "", reader("MaxSalary").ToString())
                    ddlCurrency.SelectedValue = If(reader("Currency") Is DBNull.Value, "INR", reader("Currency").ToString())
                    txtBenefitsDescription.Text = If(reader("BenefitsDescription") Is DBNull.Value, "", reader("BenefitsDescription").ToString())
                    txtKeyResponsibilities.Text = If(reader("KeyResponsibilities") Is DBNull.Value, "", reader("KeyResponsibilities").ToString())
                    txtRequiredSkills.Text = If(reader("RequiredSkills") Is DBNull.Value, "", reader("RequiredSkills").ToString())
                    txtJobDescriptionModal.Text = reader("JobDescription").ToString()
                    If reader("ApplicationDeadline") IsNot DBNull.Value Then
                        txtApplicationDeadlineModal.Text = CDate(reader("ApplicationDeadline")).ToString("yyyy-MM-dd")
                    Else
                        txtApplicationDeadlineModal.Text = ""
                    End If
                    If ddlHiringManager.Items.FindByValue(reader("HiringManagerID").ToString()) IsNot Nothing Then
                        ddlHiringManager.SelectedValue = reader("HiringManagerID").ToString()
                    Else
                        ddlHiringManager.SelectedValue = "0"
                    End If
                    txtNumberOfPositions.Text = reader("NumberOfPositions").ToString()
                    ddlJobStatus.SelectedValue = reader("JobStatus").ToString()

                    lblJobModalMessage.Text = ""
                    ' If using RTE, ensure content is loaded
                    ' ScriptManager.RegisterStartupScript(Me, Me.GetType(), "LoadRTE", $"if (typeof CKEDITOR !== 'undefined') {{ CKEDITOR.instances['txtJobDescriptionModal'].setData('{HttpUtility.JavaScriptStringEncode(reader("JobDescription").ToString())}'); }}", True)

                    ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenJobModal", "var myModal = new bootstrap.Modal(document.getElementById('jobModal')); myModal.show();", True)
                End If
            End Using
        End Using
    End Sub

    Private Sub DeleteJob(openingID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "DELETE FROM JobOpenings WHERE OpeningID = @OpeningID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@OpeningID", openingID)
                conn.Open()
                cmd.ExecuteNonQuery()
                lblJobModalMessage.Text = "Job opening deleted successfully."
                lblJobModalMessage.ForeColor = System.Drawing.Color.Green
            End Using
        End Using
    End Sub

    ' --- Method to pre-fill job vacancy from resignation (Called from Resignation.aspx.vb) ---
    Private Sub LoadJobOpeningFromResignation(resignationID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT R.*, U.Name AS EmployeeName, U.DepartmentID AS EmployeeDeptID, U.JobTitle AS EmployeeJobTitle " &
                              "FROM Resignations R JOIN Users U ON R.EmployeeID = U.UserID WHERE R.ResignationID = @ResignationID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ResignationID", resignationID)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    hdnCurrentJobID.Value = "" ' Ensure it's for new job creation
                    txtJobTitleModal.Text = reader("EmployeeJobTitle").ToString()
                    If reader("EmployeeDeptID") IsNot DBNull.Value AndAlso ddlDepartmentModal.Items.FindByValue(reader("EmployeeDeptID").ToString()) IsNot Nothing Then
                        ddlDepartmentModal.SelectedValue = reader("EmployeeDeptID").ToString()
                    Else
                        ddlDepartmentModal.SelectedValue = "0"
                    End If

                    ddlLocationType.SelectedValue = "On-site"
                    txtSpecificLocation.Text = ""
                    ddlEmploymentType.SelectedValue = "Full-time"
                    ddlExperienceLevel.SelectedValue = "Mid Level"
                    ddlEducationLevel.SelectedValue = "Bachelor's Degree"
                    txtMinSalary.Text = ""
                    txtMaxSalary.Text = ""
                    ddlCurrency.SelectedValue = "INR"
                    txtBenefitsDescription.Text = ""
                    txtKeyResponsibilities.Text = "Responsibilities for " & reader("EmployeeJobTitle").ToString() & " position (replacing " & reader("EmployeeName").ToString() & ")."
                    txtRequiredSkills.Text = "Skills required for " & reader("EmployeeJobTitle").ToString() & " position."
                    txtJobDescriptionModal.Text = "This position is open due to the resignation of " & reader("EmployeeName").ToString() & ". Details to be finalized by HR."

                    txtApplicationDeadlineModal.Text = ""
                    If Session("UserID") IsNot Nothing Then
                        If ddlHiringManager.Items.FindByValue(CInt(Session("UserID")).ToString()) IsNot Nothing Then
                            ddlHiringManager.SelectedValue = CInt(Session("UserID")).ToString()
                        Else
                            ddlHiringManager.SelectedValue = "0"
                        End If
                    Else
                        ddlHiringManager.SelectedValue = "0"
                    End If

                    txtNumberOfPositions.Text = "1"
                    ddlJobStatus.SelectedValue = "Draft"

                    lblJobModalMessage.Text = "Job Vacancy form pre-filled based on resignation of " & reader("EmployeeName").ToString() & ". Review and Save."
                    lblJobModalMessage.ForeColor = System.Drawing.Color.Blue
                    btnSaveJobOpening.Text = "Create Job Opening"

                    ViewState("SourceResignationID") = resignationID ' Store resignation ID to link it back
                End If
            End Using
        End Using
    End Sub

    ' --- APPLICATIONS TRACKING TAB LOGIC (HR/ADMIN) ---
    Private Sub BindCandidates(Optional openingID As Integer = 0)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT c.CandidateID, c.FullName, c.Email, c.Phone, c.AppliedOn, c.Status, jo.JobTitle " &
                              "FROM Candidates c JOIN JobOpenings jo ON c.OpeningID = jo.OpeningID "

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
                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using

                If dt.Rows.Count > 0 Then
                    rptCandidates.DataSource = dt
                    rptCandidates.DataBind()
                    lblNoCandidates.Visible = False
                Else
                    rptCandidates.DataSource = Nothing
                    lblNoCandidates.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub ddlJobFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlJobFilter.SelectedIndexChanged
        Dim selectedOpeningID As Integer = CInt(ddlJobFilter.SelectedValue)
        BindCandidates(selectedOpeningID)
    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click
        ddlJobFilter.SelectedValue = "0"
        BindCandidates()
    End Sub

    Protected Sub rptCandidates_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "ViewDetails" Then
            Dim candidateID As Integer = Convert.ToInt32(e.CommandArgument)
            hdnCandidateID.Value = candidateID.ToString()

            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
            Dim query As String = "SELECT c.*, jo.JobTitle FROM Candidates c JOIN JobOpenings jo ON c.OpeningID = jo.OpeningID WHERE c.CandidateID = @CandidateID"

            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@CandidateID", candidateID)
                    conn.Open()
                    Dim reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        lblName.Text = reader("FullName").ToString()
                        lblCandidateJobTitle.Text = reader("JobTitle").ToString()
                        lblEmail.Text = reader("Email").ToString()
                        lblPhone.Text = reader("Phone").ToString()
                        lblAddress.Text = If(reader("CurrentAddress") Is DBNull.Value, "N/A", reader("CurrentAddress").ToString())
                        hlLinkedIn.Text = If(reader("LinkedInProfile") Is DBNull.Value, "N/A", reader("LinkedInProfile").ToString())
                        hlLinkedIn.NavigateUrl = If(reader("LinkedInProfile") Is DBNull.Value, "", reader("LinkedInProfile").ToString())
                        hlLinkedIn.Visible = If(reader("LinkedInProfile") Is DBNull.Value, False, Not String.IsNullOrEmpty(reader("LinkedInProfile").ToString()))
                        lblDesiredSalary.Text = If(reader("DesiredSalary") Is DBNull.Value, "N/A", reader("DesiredSalary").ToString())
                        lblEarliestStart.Text = If(reader("EarliestStartDate") Is DBNull.Value, "N/A", CDate(reader("EarliestStartDate")).ToString("dd-MMM-yyyy"))
                        lblApplicationSource.Text = If(reader("ApplicationSource") Is DBNull.Value, "N/A", reader("ApplicationSource").ToString())
                        lblAppliedOn.Text = CDate(reader("AppliedOn")).ToString("dd-MMM-yyyy")
                        hlResume.NavigateUrl = If(reader("ResumeFilePath") Is DBNull.Value, "", reader("ResumeFilePath").ToString())
                        hlResume.Visible = If(reader("ResumeFilePath") Is DBNull.Value, False, Not String.IsNullOrEmpty(reader("ResumeFilePath").ToString()))
                        hlCoverLetter.NavigateUrl = If(reader("CoverLetterFilePath") Is DBNull.Value, "", reader("CoverLetterFilePath").ToString())
                        hlCoverLetter.Visible = If(reader("CoverLetterFilePath") Is DBNull.Value, False, Not String.IsNullOrEmpty(reader("CoverLetterFilePath").ToString()))
                        ddlCandidateStatus.SelectedValue = reader("Status").ToString()
                        txtInterviewNotes.Text = If(reader("InterviewNotes") Is DBNull.Value, "", reader("InterviewNotes").ToString())
                        txtAssessmentScores.Text = If(reader("AssessmentScores") Is DBNull.Value, "", reader("AssessmentScores").ToString())
                        txtHRRemarks.Text = If(reader("HRRemarks") Is DBNull.Value, "", reader("HRRemarks").ToString())
                        ' Onboarding Checklist
                        Dim collectedDocs As String = If(reader("DocumentsCollected") Is DBNull.Value, "", reader("DocumentsCollected").ToString())
                        chkIDProof.Checked = collectedDocs.Contains("ID Proof")
                        chkAddressProof.Checked = collectedDocs.Contains("Address Proof")
                        chkEduCerts.Checked = collectedDocs.Contains("Educational Certificates")
                        chkPrevEmpLetter.Checked = collectedDocs.Contains("Previous Employment Letters")
                        chkBackgroundCheck.Checked = (reader("BackgroundCheckStatus").ToString() = "Cleared")
                        chkReferenceCheck.Checked = (reader("ReferenceCheckStatus").ToString() = "Completed")
                    End If
                End Using
            End Using

            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenCandidateModal", "var myModal = new bootstrap.Modal(document.getElementById('candidateModal')); myModal.show();", True)
        End If
    End Sub
    Protected Sub btnUpdateCandidate_Click(sender As Object, e As EventArgs) Handles btnUpdateCandidate.Click
        Dim candidateID As Integer = CInt(hdnCandidateID.Value)
        Dim newStatus As String = ddlCandidateStatus.SelectedValue
        Dim interviewNotes As String = txtInterviewNotes.Text.Trim()
        Dim assessmentScores As String = txtAssessmentScores.Text.Trim()
        Dim hrRemarks As String = txtHRRemarks.Text.Trim()

        Dim documentsCollectedList As New List(Of String)
        If chkIDProof.Checked Then documentsCollectedList.Add("ID Proof")
        If chkAddressProof.Checked Then documentsCollectedList.Add("Address Proof")
        If chkEduCerts.Checked Then documentsCollectedList.Add("Educational Certificates")
        If chkPrevEmpLetter.Checked Then documentsCollectedList.Add("Previous Employment Letters")
        Dim documentsCollected As String = String.Join(", ", documentsCollectedList)

        Dim backgroundCheckStatus As String = If(chkBackgroundCheck.Checked, "Cleared", "Pending")
        Dim referenceCheckStatus As String = If(chkReferenceCheck.Checked, "Completed", "Pending")

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "UPDATE Candidates SET Status = @Status, InterviewNotes = @InterviewNotes, " &
                              "AssessmentScores = @AssessmentScores, HRRemarks = @HRRemarks, DocumentsCollected = @DocumentsCollected, " &
                              "BackgroundCheckStatus = @BackgroundCheckStatus, ReferenceCheckStatus = @ReferenceCheckStatus " &
                              "WHERE CandidateID = @CandidateID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CandidateID", candidateID)
                cmd.Parameters.AddWithValue("@Status", newStatus)
                cmd.Parameters.AddWithValue("@InterviewNotes", If(String.IsNullOrEmpty(interviewNotes), CObj(DBNull.Value), interviewNotes))
                cmd.Parameters.AddWithValue("@AssessmentScores", If(String.IsNullOrEmpty(assessmentScores), CObj(DBNull.Value), assessmentScores))
                cmd.Parameters.AddWithValue("@HRRemarks", If(String.IsNullOrEmpty(hrRemarks), CObj(DBNull.Value), hrRemarks))
                cmd.Parameters.AddWithValue("@DocumentsCollected", If(String.IsNullOrEmpty(documentsCollected), CObj(DBNull.Value), documentsCollected))
                cmd.Parameters.AddWithValue("@BackgroundCheckStatus", backgroundCheckStatus)
                cmd.Parameters.AddWithValue("@ReferenceCheckStatus", referenceCheckStatus)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        lblCandidateModalMessage.Text = "Candidate details updated successfully!"
        lblCandidateModalMessage.ForeColor = System.Drawing.Color.Green
        BindCandidates(CInt(ddlJobFilter.SelectedValue))
        ' ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseCandidateModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('candidateModal')); if(myModal) myModal.hide();", True)
    End Sub
    ' This binds the LinkedIn share icon hyperlink
    Protected Sub rptJobVacancies_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptJobVacancies.ItemDataBound
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim hlink As HyperLink = CType(e.Item.FindControl("lnkLinkedIn"), HyperLink)
            Dim row As DataRowView = CType(e.Item.DataItem, DataRowView)

            Dim jobId As String = row("OpeningID").ToString()
            Dim jobTitle As String = row("JobTitle").ToString()

            ' 🔁 Replace below URL with your actual job details page link
            Dim jobUrl As String = "https://yourdomain.com/JobDetails.aspx?JobID=" & jobId

            ' LinkedIn share URL format
            Dim linkedInUrl As String = "https://www.linkedin.com/sharing/share-offsite/?url=" & HttpUtility.UrlEncode(jobUrl)
            hlink.NavigateUrl = linkedInUrl
        End If
    End Sub

    ' --- APPLY FOR JOB TAB LOGIC (PUBLIC) ---
    Protected Sub btnSubmitApplication_Click(sender As Object, e As EventArgs) Handles btnSubmitApplication.Click
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()

        Dim openingID As Integer = CInt(ddlAvailableJobs.SelectedValue)
        If openingID = 0 Then
            lblApplicationFormMessage.Text = "Please select a job to apply for."
            lblApplicationFormMessage.ForeColor = System.Drawing.Color.Red
            Return
        End If

        Dim fullName As String = txtApplicantName.Text.Trim()
        Dim email As String = txtApplicantEmail.Text.Trim()
        Dim phone As String = txtApplicantPhone.Text.Trim()
        Dim currentAddress As String = txtApplicantAddress.Text.Trim()
        Dim linkedInProfile As String = txtLinkedInProfile.Text.Trim()
        Dim desiredSalary As String = txtDesiredSalary.Text.Trim()
        Dim earliestStartDate As Nullable(Of Date) = Nothing
        If Not String.IsNullOrEmpty(txtEarliestStartDate.Text) Then
            If Not Date.TryParse(txtEarliestStartDate.Text, earliestStartDate) Then
                lblApplicationFormMessage.Text = "Invalid date format for Earliest Start Date."
                lblApplicationFormMessage.ForeColor = System.Drawing.Color.Red
                Return
            End If
        End If
        Dim applicationSource As String = ddlApplicationSource.SelectedValue

        ' File Uploads
        Dim resumeFilePath As String = ""
        Dim coverLetterFilePath As String = ""
        Dim uploadFolder As String = Server.MapPath("~/App_Data/Resumes/") ' Create this folder manually in your project

        If Not Directory.Exists(uploadFolder) Then
            Directory.CreateDirectory(uploadFolder)
        End If

        Try
            If fuResume.HasFile Then
                Dim resumeFileName As String = Path.GetFileName(fuResume.FileName)
                Dim resumeExtension As String = Path.GetExtension(resumeFileName).ToLower()
                If resumeExtension = ".pdf" OrElse resumeExtension = ".docx" Then
                    Dim uniqueFileName As String = Guid.NewGuid().ToString() & resumeExtension
                    resumeFilePath = Path.Combine(uploadFolder, uniqueFileName)
                    fuResume.SaveAs(resumeFilePath)
                    resumeFilePath = "~/App_Data/Resumes/" & uniqueFileName ' Store relative path in DB
                Else
                    lblApplicationFormMessage.Text = "Only PDF and DOCX files are allowed for Resume."
                    lblApplicationFormMessage.ForeColor = System.Drawing.Color.Red
                    Return
                End If
            Else
                lblApplicationFormMessage.Text = "Please upload your Resume."
                lblApplicationFormMessage.ForeColor = System.Drawing.Color.Red
                Return
            End If

            If fuCoverLetter.HasFile Then
                Dim coverLetterFileName As String = Path.GetFileName(fuCoverLetter.FileName)
                Dim coverLetterExtension As String = Path.GetExtension(coverLetterFileName).ToLower()
                If coverLetterExtension = ".pdf" OrElse coverLetterExtension = ".docx" Then
                    Dim uniqueFileName As String = Guid.NewGuid().ToString() & coverLetterExtension
                    coverLetterFilePath = Path.Combine(uploadFolder, uniqueFileName)
                    fuCoverLetter.SaveAs(coverLetterFilePath)
                    coverLetterFilePath = "~/App_Data/Resumes/" & uniqueFileName
                Else
                    lblApplicationFormMessage.Text = "Only PDF and DOCX files are allowed for Cover Letter."
                    lblApplicationFormMessage.ForeColor = System.Drawing.Color.Red
                    Return
                End If
            End If

            Dim query As String = "INSERT INTO Candidates (OpeningID, FullName, Email, Phone, CurrentAddress, LinkedInProfile, " &
                                  "DesiredSalary, EarliestStartDate, ResumeFilePath, CoverLetterFilePath, ApplicationSource, AppliedOn, Status) " &
                                  "VALUES (@OpeningID, @FullName, @Email, @Phone, @CurrentAddress, @LinkedInProfile, " &
                                  "@DesiredSalary, @EarliestStartDate, @ResumeFilePath, @CoverLetterFilePath, @ApplicationSource, GETDATE(), 'Applied')"

            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@OpeningID", openingID)
                    cmd.Parameters.AddWithValue("@FullName", fullName)
                    cmd.Parameters.AddWithValue("@Email", email)
                    cmd.Parameters.AddWithValue("@Phone", phone)
                    cmd.Parameters.AddWithValue("@CurrentAddress", If(String.IsNullOrEmpty(currentAddress), CObj(DBNull.Value), currentAddress))
                    cmd.Parameters.AddWithValue("@LinkedInProfile", If(String.IsNullOrEmpty(linkedInProfile), CObj(DBNull.Value), linkedInProfile))
                    cmd.Parameters.AddWithValue("@DesiredSalary", If(String.IsNullOrEmpty(desiredSalary), CObj(DBNull.Value), desiredSalary))
                    cmd.Parameters.AddWithValue("@EarliestStartDate", If(earliestStartDate.HasValue, CObj(earliestStartDate.Value), CObj(DBNull.Value)))
                    cmd.Parameters.AddWithValue("@ResumeFilePath", resumeFilePath)
                    cmd.Parameters.AddWithValue("@CoverLetterFilePath", If(String.IsNullOrEmpty(coverLetterFilePath), CObj(DBNull.Value), coverLetterFilePath))
                    cmd.Parameters.AddWithValue("@ApplicationSource", applicationSource)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            lblApplicationFormMessage.Text = "Your application has been submitted successfully! We will get back to you shortly."
            lblApplicationFormMessage.ForeColor = System.Drawing.Color.Green
            ' Clear form fields
            txtApplicantName.Text = ""
            txtApplicantEmail.Text = ""
            txtApplicantPhone.Text = ""
            txtApplicantAddress.Text = ""
            txtLinkedInProfile.Text = ""
            txtDesiredSalary.Text = ""
            txtEarliestStartDate.Text = ""
            ddlAvailableJobs.SelectedValue = "0"
            ddlApplicationSource.SelectedValue = "LinkedIn" ' Reset to default
            ' FileUpload controls cannot be cleared directly in code for security reasons, user has to re-select

            BindCandidates(openingID) ' Show the candidate under the job they applied for
        Catch ex As Exception
            lblApplicationFormMessage.Text = "An error occurred: " & ex.Message
            lblApplicationFormMessage.ForeColor = System.Drawing.Color.Red
            ' Optional: Log the exception details for debugging
            ' System.Diagnostics.Trace.TraceError("Error submitting application: " & ex.ToString())
        End Try
    End Sub

    Private Sub DownloadApplicationDocument(candidateID As Integer, docType As String)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim filePathColumn As String = If(docType = "Resume", "ResumeFilePath", "CoverLetterFilePath")
        Dim query As String = $"SELECT {filePathColumn} FROM Candidates WHERE CandidateID = @CandidateID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CandidateID", candidateID)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                    Dim relativeFilePath As String = result.ToString()
                    If relativeFilePath.StartsWith("~/") Then
                        relativeFilePath = relativeFilePath.Replace("~/", "")
                    End If
                    Dim filePath As String = Server.MapPath("~/" & relativeFilePath) ' Ensure correct mapping for App_Data

                    If File.Exists(filePath) Then
                        Response.ContentType = "application/octet-stream" ' General binary file type
                        Response.AppendHeader("Content-Disposition", "attachment; filename=" & Path.GetFileName(filePath))
                        Response.TransmitFile(filePath)
                        Response.End()
                    Else
                        lblCandidateModalMessage.Text = $"{docType} file not found on server: {Path.GetFileName(filePath)}"
                        lblCandidateModalMessage.ForeColor = System.Drawing.Color.Red
                    End If
                Else
                    lblCandidateModalMessage.Text = $"{docType} not uploaded for this application."
                    lblCandidateModalMessage.ForeColor = System.Drawing.Color.Orange
                End If
            End Using
        End Using
    End Sub

End Class