Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls ' Repeater, DropDownList ke liye
Imports System.Globalization ' For month names

Public Class LeaveReport
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return
        ' Optional: Smart lock for HR/Admin only
        ' Dim userRoleID As Integer = CInt(Session("RoleID"))
        ' If userRoleID > 2 Then Response.Redirect("~/Dashboard.aspx") : Return

        If Not IsPostBack Then
            PopulateReportFilterDropDowns()
            BindLeaveReports() ' Page load par default report generate karna
        End If
    End Sub

    ' Dropdowns ko bharne ka function
    Private Sub PopulateReportFilterDropDowns()
        ' Year Dropdown
        For i As Integer = 0 To 2 ' Pichle 3 saal
            ddlReportYear.Items.Add(New ListItem(DateTime.Now.AddYears(-i).Year.ToString()))
        Next
        ' Month Dropdown
        For i As Integer = 1 To 12
            ddlReportMonth.Items.Add(New ListItem(Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(i), i.ToString()))
        Next
        ' Status Dropdown
        ddlReportStatusFilter.Items.Add(New ListItem("All Statuses", "0"))
        ddlReportStatusFilter.Items.Add(New ListItem("Pending", "Pending"))
        ddlReportStatusFilter.Items.Add(New ListItem("Approved by Manager", "Approved by Manager"))
        ddlReportStatusFilter.Items.Add(New ListItem("Approved by HR", "Approved by HR"))
        ddlReportStatusFilter.Items.Add(New ListItem("Rejected", "Rejected"))

        ' Default selection (optional)
        ddlReportMonth.SelectedValue = DateTime.Now.Month.ToString()
        ddlReportYear.SelectedValue = DateTime.Now.Year.ToString()
    End Sub

    ' Report Generate button click
    Protected Sub btnGenerateReport_Click(sender As Object, e As EventArgs) Handles btnGenerateReport.Click
        BindLeaveReports()
    End Sub

    ' Leave Reports ko bind karna
    Private Sub BindLeaveReports()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ToString()
        Dim selectedMonth As Integer = CInt(ddlReportMonth.SelectedValue)
        Dim selectedYear As Integer = CInt(ddlReportYear.SelectedValue)
        Dim selectedStatus As String = ddlReportStatusFilter.SelectedValue

        Dim query As String = "SELECT l.LeaveID, u.Name as EmployeeName, l.LeaveType, l.StartDate, l.EndDate, l.Status, l.Reason " &
                              "FROM Leaves l JOIN Users u ON l.EmployeeID = u.UserID " &
                              "WHERE MONTH(l.StartDate) = @Month AND YEAR(l.StartDate) = @Year "

        If selectedStatus <> "0" Then
            query &= " AND l.Status = @Status"
        End If

        query &= " ORDER BY u.Name, l.StartDate ASC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Month", selectedMonth)
                cmd.Parameters.AddWithValue("@Year", selectedYear)
                If selectedStatus <> "0" Then
                    cmd.Parameters.AddWithValue("@Status", selectedStatus)
                End If
                conn.Open()
                Dim dt As New System.Data.DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                If dt.Rows.Count > 0 Then
                    rptLeaveReport.DataSource = dt
                    rptLeaveReport.DataBind()
                    lblNoLeaveReport.Visible = False
                Else
                    rptLeaveReport.DataSource = Nothing
                    rptLeaveReport.DataBind()
                    lblNoLeaveReport.Visible = True
                End If
            End Using
        End Using
    End Sub

End Class