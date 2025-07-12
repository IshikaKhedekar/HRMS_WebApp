Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlControls
Imports System.Drawing ' IMPORTANT: Added this import for System.Drawing.Color

Public Class Resignation
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        Dim userRoleID As Integer = CInt(Session("RoleID"))
        Dim userID As Integer = CInt(Session("UserID"))

        If userRoleID <= 3 Then ' 1=SuperAdmin, 2=HR, 3=Manager
            liManageResignationsTab.Visible = True
        Else
            liManageResignationsTab.Visible = False
        End If

        If Not IsPostBack Then
            BindMyResignation(userID)
            If userRoleID <= 3 Then
                BindPendingResignations(userRoleID, userID)
                BindOtherResignations(userRoleID, userID)
            End If
        End If
    End Sub

    Private Function GetCssClassForStatus(ByVal status As String) As String
        If String.IsNullOrEmpty(status) Then Return "status-unknown"
        Select Case status.ToLower().Replace(" ", "").Trim()
            Case "submitted" : Return "status-Submitted"
            Case "accepted" : Return "status-Accepted"
            Case "onnoticeperiod" : Return "status-OnNoticePeriod"
            Case "exitinterviewscheduled" : Return "status-ExitInterviewScheduled"
            Case "cleared" : Return "status-Cleared"
            Case "rejected" : Return "status-Rejected"
            Case "withdrawn" : Return "status-Withdrawn"
            Case Else : Return "status-default"
        End Select
    End Function

    Private Sub BindMyResignation(ByVal employeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim employeeDetailsQuery As String = "SELECT Name, JobTitle, DateOfJoining FROM Users WHERE UserID = @EmployeeID"
        Dim employeeName As String = ""
        Dim employeeJobTitle As String = ""
        Dim employeeDOJ As Nullable(Of Date) = Nothing

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(employeeDetailsQuery, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    employeeName = reader("Name").ToString()
                    employeeJobTitle = reader("JobTitle").ToString()
                    If reader("DateOfJoining") IsNot DBNull.Value Then
                        employeeDOJ = Convert.ToDateTime(reader("DateOfJoining"))
                    End If
                End If
            End Using
        End Using

        lblEmployeeName.Text = employeeName
        lblCurrentJobTitle.Text = employeeJobTitle
        lblResignationDate.Text = Date.Today.ToString("dd-MMM-yyyy")

        ' Declare tenureString at a wider scope
        Dim tenureString As String = ""

        ' Display Date of Joining and calculate Tenure
        If employeeDOJ.HasValue Then
            lblDateOfJoining.Text = employeeDOJ.Value.ToString("dd-MMM-yyyy")

            Dim today As Date = Date.Today
            Dim doj As Date = employeeDOJ.Value

            Dim years As Integer = today.Year - doj.Year
            Dim months As Integer = today.Month - doj.Month
            Dim days As Integer = today.Day - doj.Day

            If days < 0 Then
                months -= 1
                days += DateTime.DaysInMonth(today.Year, If(today.Month = 1, 12, today.Month - 1))
            End If

            If months < 0 Then
                years -= 1
                months += 12
            End If

            ' Construct tenure string
            If years > 0 Then
                tenureString += $"{years} year(s)"
            End If
            If months > 0 Then
                If tenureString <> "" Then tenureString += " and "
                tenureString += $"{months} month(s)"
            End If

            If tenureString = "" Then
                tenureString = "Less than a month"
            End If

            lblTenure.Text = tenureString
        Else
            lblDateOfJoining.Text = "N/A"
            lblTenure.Text = "N/A"
        End If

        If Not IsPostBack AndAlso String.IsNullOrEmpty(txtProposedLastWorkingDate.Text) Then
            txtProposedLastWorkingDate.Text = Date.Today.AddDays(90).ToString("yyyy-MM-dd")
        End If

        Dim query As String = "SELECT ResignationID, ResignationReason, ResignationDate, RequestedLastWorkingDate, ApprovedLastWorkingDate, NoticePeriodDays, Status FROM Resignations WHERE EmployeeID = @EmployeeID ORDER BY ResignationDate DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    ' Check if the most recent resignation is Withdrawn
                    Dim latestStatus As String = reader("Status").ToString()
                    If latestStatus = "Withdrawn" Then
                        ' If withdrawn, show the form again
                        pnlResignationForm.Visible = True
                        pnlResignationStatus.Visible = False
                        lblResignationMessage.Text = "Your previous resignation was withdrawn. You can submit a new one if needed."
                        lblResignationMessage.ForeColor = System.Drawing.Color.Green ' Fixed: Fully qualified Color
                        txtResignationReason.Text = "" ' Clear form for new submission
                        txtProposedLastWorkingDate.Text = Date.Today.AddDays(90).ToString("yyyy-MM-dd") ' Reset default proposed date
                        Return ' Exit early, form is visible
                    End If

                    pnlResignationForm.Visible = False
                    pnlResignationStatus.Visible = True

                    lblCurrentStatus.Text = latestStatus
                    lblCurrentStatus.CssClass = GetCssClassForStatus(latestStatus)
                    lblSubmittedOn.Text = Convert.ToDateTime(reader("ResignationDate")).ToString("dd-MMM-yyyy")
                    litResignationReason.Text = Server.HtmlEncode(reader("ResignationReason").ToString()).Replace(vbCrLf, "<br/>")
                    lblRequestedLastWorkingDate.Text = Convert.ToDateTime(reader("RequestedLastWorkingDate")).ToString("dd-MMM-yyyy")

                    ' --- MODIFIED LOGIC FOR NOTICE PERIOD LEFT (Addressing "N/A" for pending) ---
                    If latestStatus = "Submitted" Then
                        ' If pending, show countdown from the PROPOSED last working date
                        lblApprovedLastWorkingDate.Text = "Pending HR Approval"
                        Dim proposedDate As DateTime = Convert.ToDateTime(reader("RequestedLastWorkingDate"))
                        Dim daysLeftProposed As TimeSpan = proposedDate - DateTime.Today
                        If daysLeftProposed.Days >= 0 Then
                            lblNoticePeriodLeft.Text = $"Proposed: {daysLeftProposed.Days} days left"
                            lblNoticePeriodLeft.CssClass = "countdown-timer text-info"
                        Else
                            lblNoticePeriodLeft.Text = "Proposed: Past due (Contact HR)"
                            lblNoticePeriodLeft.CssClass = "countdown-timer text-danger"
                        End If
                    ElseIf reader("ApprovedLastWorkingDate") IsNot DBNull.Value Then
                        ' If approved date is set (for Accepted, On Notice Period, Cleared statuses)
                        Dim approvedDate As DateTime = Convert.ToDateTime(reader("ApprovedLastWorkingDate"))
                        lblApprovedLastWorkingDate.Text = approvedDate.ToString("dd-MMM-yyyy")

                        If approvedDate >= DateTime.Today Then
                            Dim daysLeft As TimeSpan = approvedDate - DateTime.Today
                            lblNoticePeriodLeft.Text = $"{daysLeft.Days} days left"
                            lblNoticePeriodLeft.CssClass = "countdown-timer text-info"
                        Else
                            lblNoticePeriodLeft.Text = "Completed"
                            lblNoticePeriodLeft.CssClass = "countdown-timer text-success"
                        End If
                    Else
                        ' Fallback for other statuses where ApprovedLastWorkingDate is unexpectedly null or irrelevant
                        lblApprovedLastWorkingDate.Text = "N/A (No approved date)"
                        lblNoticePeriodLeft.Text = "N/A"
                        lblNoticePeriodLeft.CssClass = "countdown-timer text-muted"
                    End If
                    ' --- END MODIFIED LOGIC ---

                    If latestStatus = "Submitted" OrElse latestStatus = "Accepted" Then
                        btnWithdrawResignation.Visible = True
                    Else
                        btnWithdrawResignation.Visible = False
                    End If

                Else
                    ' No resignation record found at all, show the form
                    pnlResignationForm.Visible = True
                    pnlResignationStatus.Visible = False
                    lblResignationMessage.Text = ""
                    txtResignationReason.Text = ""
                    txtProposedLastWorkingDate.Text = Date.Today.AddDays(90).ToString("yyyy-MM-dd") ' Reset for new submission
                End If
            End Using
        End Using
    End Sub

    Protected Sub btnSubmitResignation_Click(sender As Object, e As EventArgs) Handles btnSubmitResignation.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim reason As String = txtResignationReason.Text.Trim()
        Dim requestedDate As Date
        If Not Date.TryParse(txtProposedLastWorkingDate.Text, requestedDate) Then
            lblResignationMessage.Text = "Invalid date format for proposed last working date."
            lblResignationMessage.ForeColor = System.Drawing.Color.Red ' Fixed: Fully qualified Color
            Return
        End If

        If String.IsNullOrEmpty(reason) Then
            lblResignationMessage.Text = "Please provide a reason for resignation."
            lblResignationMessage.ForeColor = System.Drawing.Color.Red ' Fixed: Fully qualified Color
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()

        Dim checkQuery As String = "SELECT COUNT(*) FROM Resignations WHERE EmployeeID = @EmployeeID AND Status IN ('Submitted', 'Accepted', 'On Notice Period', 'Exit Interview Scheduled')"
        Using conn As New SqlConnection(connStr)
            Using cmdCheck As New SqlCommand(checkQuery, conn)
                cmdCheck.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim pendingCount As Integer = CInt(cmdCheck.ExecuteScalar())
                If pendingCount > 0 Then
                    lblResignationMessage.Text = "You already have an active or pending resignation request. Please contact HR if you wish to modify or withdraw it."
                    lblResignationMessage.ForeColor = System.Drawing.Color.Orange ' Fixed: Fully qualified Color
                    Return
                End If
            End Using
        End Using

        Dim insertQuery As String = "INSERT INTO Resignations (EmployeeID, ResignationDate, RequestedLastWorkingDate, ResignationReason, Status) " &
                                     "VALUES (@EmployeeID, @ResignationDate, @RequestedLastWorkingDate, @Reason, 'Submitted')"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(insertQuery, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                cmd.Parameters.AddWithValue("@ResignationDate", Date.Today)
                cmd.Parameters.AddWithValue("@RequestedLastWorkingDate", requestedDate)
                cmd.Parameters.AddWithValue("@Reason", reason)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        Dim updateStatusQuery As String = "UPDATE Users SET EmploymentStatus = 'Notice Period', NoticePeriodEndDate = @NoticePeriodEndDate WHERE UserID = @UserID"
        Using conn As New SqlConnection(connStr)
            Using cmdUpdate As New SqlCommand(updateStatusQuery, conn)
                cmdUpdate.Parameters.AddWithValue("@NoticePeriodEndDate", requestedDate)
                cmdUpdate.Parameters.AddWithValue("@UserID", employeeID)
                conn.Open()
                cmdUpdate.ExecuteNonQuery()
            End Using
        End Using

        lblResignationMessage.Text = "Resignation submitted successfully! Awaiting HR/Manager approval."
        lblResignationMessage.ForeColor = System.Drawing.Color.Green ' Fixed: Fully qualified Color
        txtResignationReason.Text = ""
        txtProposedLastWorkingDate.Text = ""
        BindMyResignation(employeeID)
    End Sub

    Protected Sub btnWithdrawResignation_Click(sender As Object, e As EventArgs) Handles btnWithdrawResignation.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()

        Dim resignationIDToWithdraw As Integer = 0
        Dim getResignationIDQuery As String = "SELECT TOP 1 ResignationID FROM Resignations WHERE EmployeeID = @EmployeeID AND Status IN ('Submitted', 'Accepted') ORDER BY ResignationDate DESC"
        Using connGetID As New SqlConnection(connStr)
            Using cmdGetID As New SqlCommand(getResignationIDQuery, connGetID)
                cmdGetID.Parameters.AddWithValue("@EmployeeID", employeeID)
                connGetID.Open()
                Dim result As Object = cmdGetID.ExecuteScalar()
                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                    resignationIDToWithdraw = CInt(result)
                End If
            End Using
        End Using

        If resignationIDToWithdraw > 0 Then
            Dim query As String = "UPDATE Resignations SET Status = 'Withdrawn', HRNotes = 'Withdrawn by employee.', AcknowledgedByUserID = @AcknowledgedByUserID, AcknowledgementDate = GETDATE() WHERE ResignationID = @ResignationID"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ResignationID", resignationIDToWithdraw)
                    cmd.Parameters.AddWithValue("@AcknowledgedByUserID", employeeID) ' Employee themselves withdrew
                    conn.Open()
                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    If rowsAffected > 0 Then
                        lblResignationMessage.Text = "Resignation withdrawn successfully."
                        lblResignationMessage.ForeColor = System.Drawing.Color.Green ' Fixed: Fully qualified Color

                        Dim checkActiveQuery As String = "SELECT COUNT(*) FROM Resignations WHERE EmployeeID = @EmployeeID AND Status IN ('Submitted', 'Accepted', 'On Notice Period', 'Exit Interview Scheduled')"
                        Using connCheckActive As New SqlConnection(connStr)
                            Using cmdCheckActive As New SqlCommand(checkActiveQuery, connCheckActive)
                                cmdCheckActive.Parameters.AddWithValue("@EmployeeID", employeeID)
                                connCheckActive.Open()
                                If CInt(cmdCheckActive.ExecuteScalar()) = 0 Then
                                    Dim revertUserStatusQuery As String = "UPDATE Users SET EmploymentStatus = 'Active', NoticePeriodEndDate = NULL WHERE UserID = @UserID"
                                    Using connRevert As New SqlConnection(connStr)
                                        Using cmdRevert As New SqlCommand(revertUserStatusQuery, connRevert)
                                            cmdRevert.Parameters.AddWithValue("@UserID", employeeID)
                                            connRevert.Open()
                                            cmdRevert.ExecuteNonQuery()
                                        End Using
                                    End Using
                                End If
                            End Using
                        End Using
                    Else
                        lblResignationMessage.Text = "Could not withdraw resignation (Status might have changed or no withdrawable resignation found)."
                        lblResignationMessage.ForeColor = System.Drawing.Color.Red ' Fixed: Fully qualified Color
                    End If
                End Using
            End Using
        Else
            lblResignationMessage.Text = "No active or submitted resignation found to withdraw."
            lblResignationMessage.ForeColor = System.Drawing.Color.Red ' Fixed: Fully qualified Color
        End If
        BindMyResignation(employeeID) ' Rebind to show updated status or form
    End Sub

    Private Sub BindPendingResignations(ByVal currentRoleID As Integer, ByVal currentUserID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT r.ResignationID, u.Name as EmployeeName, u.JobTitle AS EmployeeJobTitle, r.ResignationDate, r.RequestedLastWorkingDate, r.ResignationReason FROM Resignations r JOIN Users u ON r.EmployeeID = u.UserID WHERE r.Status = 'Submitted'"
        If currentRoleID = 3 Then
            query &= " AND u.ManagerID = @UserID"
        End If
        query &= " ORDER BY r.ResignationDate ASC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentRoleID = 3 Then cmd.Parameters.AddWithValue("@UserID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
                If dt.Rows.Count > 0 Then
                    rptPendingResignations.DataSource = dt
                    rptPendingResignations.DataBind()
                    lblNoPendingResignations.Visible = False
                Else
                    rptPendingResignations.DataSource = Nothing
                    lblNoPendingResignations.Visible = True
                End If
            End Using
        End Using
    End Sub

    Private Sub BindOtherResignations(ByVal currentRoleID As Integer, ByVal currentUserID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT r.ResignationID, u.Name as EmployeeName, r.Status, r.ApprovedLastWorkingDate, r.NoticePeriodDays, r.HRNotes, ISNULL(U_Ack.Name, 'N/A') AS AcknowledgedByName, r.AcknowledgementDate, r.VacancyAutoCreated " &
                              "FROM Resignations r JOIN Users u ON r.EmployeeID = u.UserID " &
                              "LEFT JOIN Users U_Ack ON r.AcknowledgedByUserID = U_Ack.UserID " &
                              "WHERE r.Status <> 'Submitted' AND r.Status <> 'Withdrawn'"
        If currentRoleID = 3 Then
            query &= " AND u.ManagerID = @UserID"
        End If
        query &= " ORDER BY r.ApprovedLastWorkingDate DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentRoleID = 3 Then cmd.Parameters.AddWithValue("@UserID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
                If dt.Rows.Count > 0 Then
                    rptOtherResignations.DataSource = dt
                    rptOtherResignations.DataBind()
                    lblNoOtherResignations.Visible = False
                Else
                    rptOtherResignations.DataSource = Nothing
                    lblNoOtherResignations.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub rptPendingResignations_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptPendingResignations.ItemCommand
        If e.CommandName = "ViewDetails" Then
            Dim resignationID As Integer = CInt(e.CommandArgument)
            hdnResignationID.Value = resignationID.ToString()
            LoadResignationDetailsForModal(resignationID)
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenResignationModal", "var myModal = new bootstrap.Modal(document.getElementById('resignationDetailModal')); myModal.show();", True)
        End If
    End Sub

    Protected Sub rptOtherResignations_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptOtherResignations.ItemCommand
        If e.CommandName = "ViewDetailsOther" Then
            Dim resignationID As Integer = CInt(e.CommandArgument)
            hdnResignationID.Value = resignationID.ToString()
            LoadResignationDetailsForModal(resignationID)
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenResignationModal", "var myModal = new bootstrap.Modal(document.getElementById('resignationDetailModal')); myModal.show();", True)
        End If
    End Sub

    Private Sub LoadResignationDetailsForModal(ByVal resignationID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT r.EmployeeID, u.Name as EmployeeName, r.ResignationDate, r.ResignationReason, r.RequestedLastWorkingDate, r.ApprovedLastWorkingDate, r.NoticePeriodDays, r.Status, r.HRNotes, r.VacancyAutoCreated, u.JobTitle AS EmployeeJobTitle, u.DepartmentID AS EmployeeDepartmentID FROM Resignations r JOIN Users u ON r.EmployeeID = u.UserID WHERE r.ResignationID = @ResignationID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ResignationID", resignationID)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    lblModalEmpName.Text = reader("EmployeeName").ToString()
                    lblModalStatus.Text = reader("Status").ToString()
                    lblModalStatus.CssClass = GetCssClassForStatus(reader("Status").ToString())
                    lblModalSubmittedOn.Text = Convert.ToDateTime(reader("ResignationDate")).ToString("dd-MMM-yyyy")
                    litModalReason.Text = Server.HtmlEncode(reader("ResignationReason").ToString()).Replace(vbCrLf, "<br/>")
                    lblModalRequestedDate.Text = Convert.ToDateTime(reader("RequestedLastWorkingDate")).ToString("dd-MMM-yyyy")

                    If reader("ApprovedLastWorkingDate") IsNot DBNull.Value Then
                        txtApprovedLastWorkingDate.Text = Convert.ToDateTime(reader("ApprovedLastWorkingDate")).ToString("yyyy-MM-dd")
                    Else
                        txtApprovedLastWorkingDate.Text = ""
                    End If
                    If reader("NoticePeriodDays") IsNot DBNull.Value Then
                        txtNoticePeriod.Text = reader("NoticePeriodDays").ToString()
                    Else
                        txtNoticePeriod.Text = ""
                    End If

                    ddlUpdateStatus.SelectedValue = reader("Status").ToString()
                    txtInternalNotes.Text = reader("HRNotes").ToString()


                    Dim currentStatus As String = reader("Status").ToString()
                    If currentStatus = "Submitted" Then
                        btnModalApprove.Visible = True
                        btnModalReject.Visible = True
                        btnModalCreateVacancy.Visible = False
                    ElseIf currentStatus = "Accepted" AndAlso Not CBool(reader("VacancyAutoCreated")) Then
                        btnModalApprove.Visible = False
                        btnModalReject.Visible = False
                        btnModalCreateVacancy.Visible = True
                    Else
                        btnModalApprove.Visible = False
                        btnModalReject.Visible = False
                        btnModalCreateVacancy.Visible = False
                    End If

                    ViewState("ResigningEmployeeJobTitle") = reader("EmployeeJobTitle").ToString()
                    ViewState("ResigningEmployeeDepartmentID") = reader("EmployeeDepartmentID").ToString()
                    ViewState("ResigningEmployeeName") = reader("EmployeeName").ToString()
                End If
            End Using
        End Using
    End Sub

    Protected Sub btnSaveResignationDetails_Click(sender As Object, e As EventArgs) Handles btnSaveResignationDetails.Click
        Dim resignationID As Integer = CInt(hdnResignationID.Value)
        Dim approvedLastWorkingDate As Date = Nothing
        Dim noticePeriodDays As Integer = 0
        Dim status As String = ddlUpdateStatus.SelectedValue
        Dim notes As String = txtInternalNotes.Text.Trim()
        Dim acknowledgedByUserID As Integer = CInt(Session("UserID"))

        If Not String.IsNullOrEmpty(txtApprovedLastWorkingDate.Text) Then
            If Not Date.TryParse(txtApprovedLastWorkingDate.Text, approvedLastWorkingDate) Then
                lblModalMessage.Text = "Invalid Approved Last Working Date format."
                lblModalMessage.ForeColor = System.Drawing.Color.Red ' Fixed: Fully qualified Color
                Return
            End If
        End If

        If Not String.IsNullOrEmpty(txtNoticePeriod.Text) Then
            If Not Integer.TryParse(txtNoticePeriod.Text, noticePeriodDays) Then
                lblModalMessage.Text = "Invalid Notice Period (must be a number)."
                lblModalMessage.ForeColor = System.Drawing.Color.Red ' Fixed: Fully qualified Color
                Return
            End If
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "UPDATE Resignations SET ApprovedLastWorkingDate = @ApprovedDate, NoticePeriodDays = @NoticePeriod, Status = @Status, HRNotes = @Notes, AcknowledgedByUserID = @AcknowledgedBy, AcknowledgementDate = GETDATE() WHERE ResignationID = @ResignationID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ResignationID", resignationID)
                cmd.Parameters.AddWithValue("@ApprovedDate", If(approvedLastWorkingDate.Equals(Nothing), CObj(DBNull.Value), approvedLastWorkingDate))
                cmd.Parameters.AddWithValue("@NoticePeriod", If(noticePeriodDays = 0, CObj(DBNull.Value), noticePeriodDays))
                cmd.Parameters.AddWithValue("@Status", status)
                cmd.Parameters.AddWithValue("@Notes", notes)
                cmd.Parameters.AddWithValue("@AcknowledgedBy", acknowledgedByUserID)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        Dim employeeIDAffected As Integer = GetEmployeeIDFromResignation(resignationID)
        If employeeIDAffected > 0 Then
            Dim userUpdateQuery As String = "UPDATE Users SET EmploymentStatus = @NewEmploymentStatus, NoticePeriodEndDate = @EndDate WHERE UserID = @EmployeeID"
            Dim newEmpStatus As String = ""
            Dim finalDate As Object = DBNull.Value

            Select Case status
                Case "Accepted", "On Notice Period", "Exit Interview Scheduled"
                    newEmpStatus = "Notice Period"
                    finalDate = approvedLastWorkingDate
                Case "Cleared"
                    newEmpStatus = "Resigned"
                    finalDate = approvedLastWorkingDate
                Case "Rejected", "Withdrawn"
                    newEmpStatus = "Active"
                    finalDate = DBNull.Value
                Case Else
                    newEmpStatus = "Active"
            End Select

            Using connUpdate As New SqlConnection(connStr)
                Using cmdUpdate As New SqlCommand(userUpdateQuery, connUpdate)
                    cmdUpdate.Parameters.AddWithValue("@NewEmploymentStatus", newEmpStatus)
                    cmdUpdate.Parameters.AddWithValue("@EndDate", If(finalDate.Equals(DBNull.Value), finalDate, finalDate))
                    cmdUpdate.Parameters.AddWithValue("@EmployeeID", employeeIDAffected)
                    connUpdate.Open()
                    cmdUpdate.ExecuteNonQuery()
                End Using
            End Using
        End If

        lblModalMessage.Text = "Resignation updated successfully!"
        lblModalMessage.ForeColor = System.Drawing.Color.Green ' Fixed: Fully qualified Color

        BindPendingResignations(CInt(Session("RoleID")), CInt(Session("UserID")))
        BindOtherResignations(CInt(Session("RoleID")), CInt(Session("UserID")))
        BindMyResignation(employeeIDAffected)
    End Sub

    Protected Sub btnModalApprove_Click(sender As Object, e As EventArgs) Handles btnModalApprove.Click
        ddlUpdateStatus.SelectedValue = "Accepted"
        If String.IsNullOrEmpty(txtApprovedLastWorkingDate.Text) Then
            txtApprovedLastWorkingDate.Text = lblModalRequestedDate.Text
        End If
        If String.IsNullOrEmpty(txtNoticePeriod.Text) AndAlso Not String.IsNullOrEmpty(txtApprovedLastWorkingDate.Text) Then
            Dim requestedDate As Date = Convert.ToDateTime(lblModalRequestedDate.Text)
            Dim approvedDate As Date = Convert.ToDateTime(txtApprovedLastWorkingDate.Text)
            txtNoticePeriod.Text = (approvedDate - requestedDate).Days.ToString()
        End If
        btnSaveResignationDetails_Click(sender, e)
    End Sub

    Protected Sub btnModalReject_Click(sender As Object, e As EventArgs) Handles btnModalReject.Click
        ddlUpdateStatus.SelectedValue = "Rejected"
        btnSaveResignationDetails_Click(sender, e)
    End Sub

    Protected Sub btnModalCreateVacancy_Click(sender As Object, e As EventArgs) Handles btnModalCreateVacancy.Click
        Dim resignationID As Integer = CInt(hdnResignationID.Value)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()

        Dim updateResignationFlagQuery As String = "UPDATE Resignations SET VacancyAutoCreated = 1, AcknowledgedByUserID = @AcknowledgedByUserID, AcknowledgementDate = GETDATE() WHERE ResignationID = @ResignationID"
        Using connUpdate As New SqlConnection(connStr)
            Using cmdUpdate As New SqlCommand(updateResignationFlagQuery, connUpdate)
                cmdUpdate.Parameters.AddWithValue("@ResignationID", resignationID)
                cmdUpdate.Parameters.AddWithValue("@AcknowledgedByUserID", CInt(Session("UserID")))
                connUpdate.Open()
                cmdUpdate.ExecuteNonQuery()
            End Using
        End Using

        Response.Redirect("~/Recruitment.aspx?ResignationID=" & resignationID.ToString())
    End Sub

    Private Function GetEmployeeIDFromResignation(resignationID As Integer) As Integer
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim query As String = "SELECT EmployeeID FROM Resignations WHERE ResignationID = @ResignationID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ResignationID", resignationID)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                    Return CInt(result)
                End If
            End Using
        End Using
        Return 0
    End Function

End Class