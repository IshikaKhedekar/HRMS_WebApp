Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

Public Class Performance
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            Dim userID As Integer = CInt(Session("UserID"))

            ' Step 1: My Personal Review ko load karo (ye sabke liye hai)
            LoadMyPerformanceReview(userID)

            ' Step 2: Agar user Manager/HR/Admin hai, to management panels dikhao
            If userRoleID <= 3 Then
                ' Pehle Cycle Management section dikhao aur data bharo
                pnlCycleManagement.Visible = True
                BindCyclesRepeater()

                ' Phir Team Reviews section dikhao aur ACTIVE cycle ke hisaab se data bharo
                pnlTeamView.Visible = True
                BindTeamReviews(userID, userRoleID)

                ' Aakhir mein KPI Assignment section dikhao aur data bharo
                pnlAssignKPIs.Visible = True
                SetActiveCycleName()
                PopulateEmployeesForKPIDropdown(userID, userRoleID)
                PopulateMasterKPIsCheckboxList()
            End If
        End If
    End Sub

    Public Function GetStatusCssClass(ByVal status As Object) As String
        If status Is DBNull.Value OrElse status Is Nothing Then
            Return ""
        End If
        Select Case status.ToString()
            Case "Pending Self-Appraisal", "Pending Manager Review"
                Return "status-pending"
            Case "Completed", "Published", "Finalized"
                Return "status-completed"
            Case Else
                Return ""
        End Select
    End Function

    Private Sub LoadMyPerformanceReview(ByVal employeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pr.ReviewID, pr.EmployeeComments, pr.ManagerComments, pr.ManagerRating, pr.Status, pc.CycleName, pr.IsPublished FROM PerformanceReviews pr JOIN PerformanceCycles pc ON pr.CycleID = pc.CycleID WHERE pr.EmployeeID = @EmployeeID AND pc.Status = 'Active'"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        pnlMyReview.Visible = True
                        Dim reviewID As Integer = CInt(reader("ReviewID"))
                        hdnReviewID.Value = reviewID.ToString()
                        lblCycleName.Text = reader("CycleName").ToString()

                        ' Bind the employee's KPI repeater
                        BindMyKpiRepeater(reviewID)

                        Dim reviewStatus As String = reader("Status").ToString()
                        Dim isPublished As Boolean = Convert.ToBoolean(reader("IsPublished"))

                        lblReviewStatus.Text = reviewStatus

                        If reader("EmployeeComments") IsNot DBNull.Value Then txtEmployeeComments.Text = reader("EmployeeComments").ToString()

                        ' Manager ke comments aur rating sirf tabhi dikhao jab review published ho
                        If isPublished Then
                            txtManagerComments.Text = If(reader("ManagerComments") IsNot DBNull.Value, reader("ManagerComments").ToString(), "N/A")
                            lblManagerRating.Text = If(reader("ManagerRating") IsNot DBNull.Value, reader("ManagerRating").ToString() & "/5", "Not Yet Rated")
                        Else
                            txtManagerComments.Text = "Your manager's feedback will be visible here once the review is finalized."
                            lblManagerRating.Text = "Pending"
                        End If

                        Select Case reviewStatus
                            Case "Pending Self-Appraisal"
                                txtEmployeeComments.ReadOnly = False
                                btnSubmitToManager.Enabled = True
                                btnSubmitToManager.Text = "Submit to Manager"
                            Case Else
                                txtEmployeeComments.ReadOnly = True
                                btnSubmitToManager.Enabled = False
                                btnSubmitToManager.Text = "Submitted"
                        End Select
                    Else
                        pnlMyReview.Visible = False
                    End If
                End Using
            End Using
        End Using
    End Sub

    Protected Sub btnSubmitToManager_Click(sender As Object, e As EventArgs) Handles btnSubmitToManager.Click
        Dim reviewID As Integer = CInt(hdnReviewID.Value)
        Dim employeeOverallComments As String = txtEmployeeComments.Text.Trim()

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            conn.Open()

            ' Step 1: Har KPI ke liye employee ke comments ko save/update karo
            For Each item As RepeaterItem In rptMyKPIs.Items
                If item.ItemType = ListItemType.Item OrElse item.ItemType = ListItemType.AlternatingItem Then
                    Dim hdnEmployeeKpiID As HiddenField = CType(item.FindControl("hdnMyEmployeeKpiID"), HiddenField)
                    Dim txtMyKpiComments As TextBox = CType(item.FindControl("txtMyKpiComments"), TextBox)

                    Dim employeeKpiID As Integer = CInt(hdnEmployeeKpiID.Value)
                    Dim kpiComments As String = txtMyKpiComments.Text.Trim()

                    Dim kpiUpdateQuery As String = "UPDATE EmployeeKPIs SET EmployeeComments = @Comments WHERE EmployeeKpiID = @EmployeeKpiID"
                    Using cmdKpi As New SqlCommand(kpiUpdateQuery, conn)
                        cmdKpi.Parameters.AddWithValue("@EmployeeKpiID", employeeKpiID)
                        cmdKpi.Parameters.AddWithValue("@Comments", If(String.IsNullOrEmpty(kpiComments), DBNull.Value, kpiComments))
                        cmdKpi.ExecuteNonQuery()
                    End Using
                End If
            Next

            ' Step 2: Overall review ko update karo aur status 'Pending Manager Review' set karo
            Dim query As String = "UPDATE PerformanceReviews SET EmployeeComments = @EmployeeComments, Status = 'Pending Manager Review' WHERE ReviewID = @ReviewID"
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                cmd.Parameters.AddWithValue("@EmployeeComments", employeeOverallComments)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Response.Redirect(Request.RawUrl)
    End Sub

    Private Sub BindTeamReviews(ByVal currentUserID As Integer, ByVal currentUserRoleID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT pr.ReviewID, u.Name as EmployeeName, pr.Status FROM PerformanceReviews pr JOIN Users u ON pr.EmployeeID = u.UserID JOIN PerformanceCycles pc ON pr.CycleID = pc.CycleID WHERE pc.Status = 'Active'"
        If currentUserRoleID = 3 Then query &= " AND pr.ManagerID = @ManagerID"
        query &= " ORDER BY u.Name"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentUserRoleID = 3 Then cmd.Parameters.AddWithValue("@ManagerID", currentUserID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)
                If dt.Rows.Count > 0 Then
                    rptTeamReviews.DataSource = dt
                    rptTeamReviews.DataBind()
                    lblNoTeamReviews.Visible = False
                Else
                    lblNoTeamReviews.Visible = True
                End If
            End Using
        End Using
    End Sub

    Protected Sub rptTeamReviews_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "StartReview" Then
            Dim reviewID As String = e.CommandArgument.ToString()
            Response.Redirect("ReviewDetail.aspx?ReviewID=" & reviewID)
        End If
    End Sub

    ' ===============================================
    ' == CYCLE MANAGEMENT LOGIC
    ' ===============================================
    Private Sub BindCyclesRepeater()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT CycleID, CycleName, StartDate, EndDate, Status FROM PerformanceCycles ORDER BY StartDate DESC"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                rptCycles.DataSource = cmd.ExecuteReader()
                rptCycles.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub btnAddCycle_Click(sender As Object, e As EventArgs) Handles btnAddCycle.Click
        Dim cycleName As String = txtCycleName.Text.Trim()
        Dim startDate As Date
        Dim endDate As Date
        If String.IsNullOrEmpty(cycleName) OrElse Not Date.TryParse(txtCycleStartDate.Text, startDate) OrElse Not Date.TryParse(txtCycleEndDate.Text, endDate) Then
            lblCycleMessage.Text = "All fields are required and dates must be valid."
            lblCycleMessage.ForeColor = Drawing.Color.Red
            Return
        End If
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "INSERT INTO PerformanceCycles (CycleName, StartDate, EndDate) VALUES (@Name, @Start, @End)"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Name", cycleName)
                cmd.Parameters.AddWithValue("@Start", startDate)
                cmd.Parameters.AddWithValue("@End", endDate)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
        lblCycleMessage.Text = "Cycle created successfully!"
        lblCycleMessage.ForeColor = Drawing.Color.Green
        txtCycleName.Text = ""
        txtCycleStartDate.Text = ""
        txtCycleEndDate.Text = ""
        BindCyclesRepeater()
    End Sub

    Protected Sub rptCycles_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptCycles.ItemCommand
        If e.CommandName = "ActivateCycle" Then
            Dim cycleIDToActivate As Integer = CInt(e.CommandArgument)
            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Using conn As New SqlConnection(connStr)
                conn.Open()
                Using cmdDeactivate As New SqlCommand("UPDATE PerformanceCycles SET Status = 'Inactive'", conn)
                    cmdDeactivate.ExecuteNonQuery()
                End Using
                Using cmdActivate As New SqlCommand("UPDATE PerformanceCycles SET Status = 'Active' WHERE CycleID = @CycleID", conn)
                    cmdActivate.Parameters.AddWithValue("@CycleID", cycleIDToActivate)
                    cmdActivate.ExecuteNonQuery()
                End Using
                Dim usersList As New List(Of Tuple(Of Integer, Integer))()
                Dim usersQuery As String = "SELECT UserID, ManagerID FROM Users WHERE RoleID <> 1"
                Using cmdUsers As New SqlCommand(usersQuery, conn)
                    Using reader As SqlDataReader = cmdUsers.ExecuteReader()
                        While reader.Read()
                            Dim userID As Integer = CInt(reader("UserID"))
                            Dim managerID As Integer = If(reader("ManagerID") IsNot DBNull.Value, CInt(reader("ManagerID")), 0)
                            usersList.Add(Tuple.Create(userID, managerID))
                        End While
                    End Using
                End Using
                For Each userTuple In usersList
                    Dim checkQuery As String = "SELECT COUNT(*) FROM PerformanceReviews WHERE CycleID = @CycleID AND EmployeeID = @EmployeeID"
                    Dim recordExists As Boolean = False
                    Using checkCmd As New SqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@CycleID", cycleIDToActivate)
                        checkCmd.Parameters.AddWithValue("@EmployeeID", userTuple.Item1)
                        recordExists = (CInt(checkCmd.ExecuteScalar()) > 0)
                    End Using
                    If Not recordExists Then
                        Dim insertQuery As String = "INSERT INTO PerformanceReviews (CycleID, EmployeeID, ManagerID, Status) VALUES (@CycleID, @EmployeeID, @ManagerID, 'Pending Self-Appraisal')"
                        Using insertCmd As New SqlCommand(insertQuery, conn)
                            insertCmd.Parameters.AddWithValue("@CycleID", cycleIDToActivate)
                            insertCmd.Parameters.AddWithValue("@EmployeeID", userTuple.Item1)
                            If userTuple.Item2 = 0 Then
                                insertCmd.Parameters.AddWithValue("@ManagerID", DBNull.Value)
                            Else
                                insertCmd.Parameters.AddWithValue("@ManagerID", userTuple.Item2)
                            End If
                            insertCmd.ExecuteNonQuery()
                        End Using
                    End If
                Next
            End Using
            Response.Redirect(Request.RawUrl)
        End If
    End Sub

    ' ===============================================
    ' == KPI ASSIGNMENT LOGIC
    ' ===============================================
    Private Sub SetActiveCycleName()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT TOP 1 CycleName FROM PerformanceCycles WHERE Status = 'Active'"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim cycleName As Object = cmd.ExecuteScalar()
                If cycleName IsNot Nothing AndAlso cycleName IsNot DBNull.Value Then
                    lblAssignKpiCycleName.Text = cycleName.ToString()
                End If
            End Using
        End Using
    End Sub

    Private Sub PopulateEmployeesForKPIDropdown(ByVal currentUserID As Integer, ByVal currentUserRoleID As Integer)
        ddlEmployeesForKPI.Items.Clear()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT UserID, Name FROM Users WHERE RoleID <> 1"
        If currentUserRoleID = 3 Then query &= " AND ManagerID = @ManagerID"
        query &= " ORDER BY Name"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                If currentUserRoleID = 3 Then cmd.Parameters.AddWithValue("@ManagerID", currentUserID)
                conn.Open()
                ddlEmployeesForKPI.DataSource = cmd.ExecuteReader()
                ddlEmployeesForKPI.DataTextField = "Name"
                ddlEmployeesForKPI.DataValueField = "UserID"
                ddlEmployeesForKPI.DataBind()
            End Using
        End Using
        ddlEmployeesForKPI.Items.Insert(0, New ListItem("-- Select an Employee --", "0"))
    End Sub

    Private Sub PopulateMasterKPIsCheckboxList()
        cblMasterKPIs.Items.Clear()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT KpiID, KpiTitle FROM KPIs WHERE IsActive = 1 ORDER BY KpiTitle"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                cblMasterKPIs.DataSource = cmd.ExecuteReader()
                cblMasterKPIs.DataBind()
            End Using
        End Using
    End Sub

    Private Sub BindAssignedKPIs(ByVal reviewID As Integer)
        rptAssignedKPIs.DataSource = Nothing
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT ek.EmployeeKpiID, k.KpiTitle FROM EmployeeKPIs ek JOIN KPIs k ON ek.KpiID = k.KpiID WHERE ek.ReviewID = @ReviewID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                conn.Open()
                rptAssignedKPIs.DataSource = cmd.ExecuteReader()
                rptAssignedKPIs.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub ddlEmployeesForKPI_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlEmployeesForKPI.SelectedIndexChanged
        Dim selectedEmployeeID As Integer = CInt(ddlEmployeesForKPI.SelectedValue)
        If selectedEmployeeID > 0 Then
            Dim reviewID As Integer = GetActiveReviewIDForEmployee(selectedEmployeeID)
            If reviewID > 0 Then
                pnlAvailableKPIs.Visible = True
                pnlAssignedKPIs.Visible = True
                BindAssignedKPIs(reviewID)
            Else
                pnlAvailableKPIs.Visible = False
                pnlAssignedKPIs.Visible = False
            End If
        Else
            pnlAvailableKPIs.Visible = False
            pnlAssignedKPIs.Visible = False
        End If
    End Sub

    Private Function GetActiveReviewIDForEmployee(ByVal employeeID As Integer) As Integer
        Dim reviewID As Integer = 0
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT TOP 1 pr.ReviewID FROM PerformanceReviews pr JOIN PerformanceCycles pc ON pr.CycleID = pc.CycleID WHERE pr.EmployeeID = @EmployeeID AND pc.Status = 'Active'"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                    reviewID = CInt(result)
                End If
            End Using
        End Using
        Return reviewID
    End Function

    Protected Sub btnAssignSelectedKPIs_Click(sender As Object, e As EventArgs) Handles btnAssignSelectedKPIs.Click
        Dim selectedEmployeeID As Integer = CInt(ddlEmployeesForKPI.SelectedValue)
        Dim reviewID As Integer = GetActiveReviewIDForEmployee(selectedEmployeeID)
        If reviewID = 0 Then Return
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using conn As New SqlConnection(connStr)
            conn.Open()
            For Each item As ListItem In cblMasterKPIs.Items
                If item.Selected Then
                    Dim kpiID As Integer = CInt(item.Value)
                    Dim checkQuery As String = "SELECT COUNT(*) FROM EmployeeKPIs WHERE ReviewID = @ReviewID AND KpiID = @KpiID"
                    Using checkCmd As New SqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@ReviewID", reviewID)
                        checkCmd.Parameters.AddWithValue("@KpiID", kpiID)
                        Dim count As Integer = CInt(checkCmd.ExecuteScalar())
                        If count = 0 Then
                            Dim insertQuery As String = "INSERT INTO EmployeeKPIs (ReviewID, KpiID) VALUES (@ReviewID, @KpiID)"
                            Using insertCmd As New SqlCommand(insertQuery, conn)
                                insertCmd.Parameters.AddWithValue("@ReviewID", reviewID)
                                insertCmd.Parameters.AddWithValue("@KpiID", kpiID)
                                insertCmd.ExecuteNonQuery()
                            End Using
                        End If
                    End Using
                End If
            Next
        End Using
        BindAssignedKPIs(reviewID)
        cblMasterKPIs.ClearSelection()
    End Sub

    Protected Sub rptAssignedKPIs_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "RemoveKPI" Then
            Dim employeeKpiID As Integer = CInt(e.CommandArgument)
            Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
            Dim query As String = "DELETE FROM EmployeeKPIs WHERE EmployeeKpiID = @EmployeeKpiID"
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@EmployeeKpiID", employeeKpiID)
                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            Dim selectedEmployeeID As Integer = CInt(ddlEmployeesForKPI.SelectedValue)
            Dim reviewID As Integer = GetActiveReviewIDForEmployee(selectedEmployeeID)
            BindAssignedKPIs(reviewID)
        End If
    End Sub

    ' ===============================================
    ' == EMPLOYEE KPI SELF-REVIEW LOGIC
    ' ===============================================
    Protected Sub rptMyKPIs_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptMyKPIs.ItemDataBound
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim txtComments As TextBox = CType(e.Item.FindControl("txtMyKpiComments"), TextBox)

            Dim employeeComments As Object = DataBinder.Eval(e.Item.DataItem, "EmployeeComments")

            If Not IsDBNull(employeeComments) Then
                txtComments.Text = employeeComments.ToString()
            End If

            If Not btnSubmitToManager.Enabled Then
                txtComments.ReadOnly = True
            End If
        End If
    End Sub

    Private Sub BindMyKpiRepeater(ByVal reviewID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT ek.EmployeeKpiID, k.KpiTitle, ek.EmployeeComments FROM EmployeeKPIs ek JOIN KPIs k ON ek.KpiID = k.KpiID WHERE ek.ReviewID = @ReviewID ORDER BY k.KpiTitle"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptMyKPIs.DataSource = dt
                    rptMyKPIs.DataBind()
                    lblNoMyKPIs.Visible = False
                Else
                    rptMyKPIs.DataSource = Nothing
                    rptMyKPIs.DataBind()
                    lblNoMyKPIs.Visible = True
                End If
            End Using
        End Using
    End Sub

End Class