Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization


Public Class SalaryData
    Public Property UserID As Integer
    Public Property BasicSalary As Decimal
    Public Property HRA As Decimal
    Public Property OtherAllowances As Decimal
    Public Property PF_Deduction As Decimal
    Public Property ProfessionalTax_Deduction As Decimal
End Class

Public Class Payroll
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            Dim userRoleID As Integer = CInt(Session("RoleID"))
            If userRoleID = 1 OrElse userRoleID = 2 Then ' HR/Admin
                pnlAdminView.Visible = True
                pnlEmployeeView.Visible = False
                PopulateProcessPeriodDropDowns()
                BindAllSalaries()
            Else ' Employee/Manager
                pnlAdminView.Visible = False
                pnlEmployeeView.Visible = True
                PopulatePeriodDropDowns()
            End If
        End If
    End Sub

    ' ===============================================
    ' == EMPLOYEE VIEW LOGIC
    ' ===============================================
    Private Sub PopulatePeriodDropDowns()
        ' ... (ye function pehle jaisa hi hai) ...
        For i As Integer = 0 To 2
            ddlYear.Items.Add(New ListItem(DateTime.Now.AddYears(-i).Year.ToString()))
        Next
        For i As Integer = 1 To 12
            ddlMonth.Items.Add(New ListItem(DateTimeFormatInfo.CurrentInfo.GetMonthName(i), i.ToString()))
        Next
        ddlMonth.SelectedValue = DateTime.Now.AddMonths(-1).Month.ToString()
    End Sub

    Protected Sub btnShowPayslip_Click(sender As Object, e As EventArgs) Handles btnShowPayslip.Click
        ' ... (ye function pehle jaisa hi hai) ...
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim selectedMonth As Integer = CInt(ddlMonth.SelectedValue)
        Dim selectedYear As Integer = CInt(ddlYear.SelectedValue)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT s.*, u.Name FROM SalarySlips s JOIN Users u ON s.EmployeeID = u.UserID WHERE s.EmployeeID = @EmployeeID AND s.ForMonth = @Month AND s.ForYear = @Year"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                cmd.Parameters.AddWithValue("@Month", selectedMonth)
                cmd.Parameters.AddWithValue("@Year", selectedYear)
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()
                If reader.Read() Then
                    pnlPayslip.Visible = True
                    lblNoPayslip.Visible = False
                    litMonthYear.Text = $"{ddlMonth.SelectedItem.Text}, {selectedYear}"
                    litEmpName.Text = reader("Name").ToString()
                    litBasic.Text = String.Format("{0:N2}", reader("BasicSalary"))
                    litHRA.Text = String.Format("{0:N2}", reader("HRA"))
                    litAllowances.Text = String.Format("{0:N2}", reader("OtherAllowances"))
                    litGross.Text = String.Format("{0:N2}", reader("GrossEarnings"))
                    litPF.Text = String.Format("{0:N2}", reader("PF_Deduction"))
                    litProfTax.Text = String.Format("{0:N2}", reader("ProfessionalTax_Deduction"))
                    litTDS.Text = String.Format("{0:N2}", reader("TDS_Deduction"))
                    litDeductions.Text = String.Format("{0:N2}", reader("TotalDeductions"))
                    litNetSalary.Text = "₹ " & String.Format("{0:N2}", reader("NetSalary"))
                Else
                    pnlPayslip.Visible = False
                    lblNoPayslip.Visible = True
                End If
            End Using
        End Using
    End Sub

    ' ===============================================
    ' == HR/ADMIN VIEW LOGIC (Corrected)
    ' ===============================================
    Private Sub PopulateProcessPeriodDropDowns()
        ' ... (ye function pehle jaisa hi hai) ...
        For i As Integer = 0 To 1
            ddlProcessYear.Items.Add(New ListItem(DateTime.Now.AddYears(-i).Year.ToString()))
        Next
        For i As Integer = 1 To 12
            ddlProcessMonth.Items.Add(New ListItem(DateTimeFormatInfo.CurrentInfo.GetMonthName(i), i.ToString()))
        Next
        ddlProcessMonth.SelectedValue = DateTime.Now.Month.ToString()
    End Sub

    Private Sub BindAllSalaries()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT UserID, Name, BasicSalary, HRA, OtherAllowances, PF_Deduction, ProfessionalTax_Deduction FROM Users WHERE RoleID <> 1"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim dt As New DataTable()
                Dim adapter As New SqlDataAdapter(cmd)
                adapter.Fill(dt)

                dt.Columns.Add("GrossSalary", GetType(Decimal))
                dt.Columns.Add("NetSalary", GetType(Decimal))

                For Each row As DataRow In dt.Rows
                    Dim basic As Decimal = If(IsDBNull(row("BasicSalary")), 0D, Convert.ToDecimal(row("BasicSalary")))
                    Dim hra As Decimal = If(IsDBNull(row("HRA")), 0D, Convert.ToDecimal(row("HRA")))
                    Dim allowances As Decimal = If(IsDBNull(row("OtherAllowances")), 0D, Convert.ToDecimal(row("OtherAllowances")))
                    Dim pf As Decimal = If(IsDBNull(row("PF_Deduction")), 0D, Convert.ToDecimal(row("PF_Deduction")))
                    Dim ptax As Decimal = If(IsDBNull(row("ProfessionalTax_Deduction")), 0D, Convert.ToDecimal(row("ProfessionalTax_Deduction")))

                    Dim gross As Decimal = basic + hra + allowances
                    Dim deductions As Decimal = pf + ptax
                    Dim net As Decimal = gross - deductions

                    row("GrossSalary") = gross
                    row("NetSalary") = net
                Next

                rptAllSalaries.DataSource = dt
                rptAllSalaries.DataBind()
            End Using
        End Using
    End Sub
    Protected Sub btnRunPayroll_Click(sender As Object, e As EventArgs) Handles btnRunPayroll.Click
        ' ... (ye function pehle jaisa hi hai) ...
        Dim selectedMonth As Integer = CInt(ddlProcessMonth.SelectedValue)
        Dim selectedYear As Integer = CInt(ddlProcessYear.SelectedValue)
        If IsPayrollAlreadyRun(selectedMonth, selectedYear) Then
            lblAdminMessage.Text = "Payroll for this period has already been processed."
            lblAdminMessage.ForeColor = Drawing.Color.Red
            Return
        End If
        GenerateAndSaveSlips(selectedMonth, selectedYear)
        lblAdminMessage.Text = "Payroll processed successfully for all employees!"
        lblAdminMessage.ForeColor = Drawing.Color.Green
    End Sub

    Private Function IsPayrollAlreadyRun(ByVal month As Integer, ByVal year As Integer) As Boolean
        ' ... (ye function pehle jaisa hi hai) ...
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT COUNT(*) FROM SalarySlips WHERE ForMonth = @Month AND ForYear = @Year"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Month", month)
                cmd.Parameters.AddWithValue("@Year", year)
                conn.Open()
                Return CInt(cmd.ExecuteScalar()) > 0
            End Using
        End Using
    End Function

    Private Sub GenerateAndSaveSlips(ByVal month As Integer, ByVal year As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim usersQuery As String = "SELECT UserID, BasicSalary, HRA, OtherAllowances, PF_Deduction, ProfessionalTax_Deduction FROM Users WHERE RoleID <> 1"
        Dim insertQuery As String = "INSERT INTO SalarySlips (EmployeeID, ForMonth, ForYear, BasicSalary, HRA, OtherAllowances, GrossEarnings, PF_Deduction, ProfessionalTax_Deduction, TotalDeductions, NetSalary, TDS_Deduction) VALUES (@EmpID, @Month, @Year, @Basic, @HRA, @Allowances, @Gross, @PF, @PTax, @TotalDeduct, @Net, @TDS)"

        Dim userSalaries As New List(Of SalaryData)()

        Using conn As New SqlConnection(connStr)
            Using cmdUsers As New SqlCommand(usersQuery, conn)
                conn.Open()
                Using reader As SqlDataReader = cmdUsers.ExecuteReader()
                    While reader.Read()
                        userSalaries.Add(New SalaryData With {
                        .UserID = CInt(reader("UserID")),
                        .BasicSalary = If(Not IsDBNull(reader("BasicSalary")), CDec(reader("BasicSalary")), 0D),
                        .HRA = If(Not IsDBNull(reader("HRA")), CDec(reader("HRA")), 0D),
                        .OtherAllowances = If(Not IsDBNull(reader("OtherAllowances")), CDec(reader("OtherAllowances")), 0D),
                        .PF_Deduction = If(Not IsDBNull(reader("PF_Deduction")), CDec(reader("PF_Deduction")), 0D),
                        .ProfessionalTax_Deduction = If(Not IsDBNull(reader("ProfessionalTax_Deduction")), CDec(reader("ProfessionalTax_Deduction")), 0D)
                    })
                    End While
                End Using
            End Using
        End Using

        Using conn As New SqlConnection(connStr)
            conn.Open()
            For Each userSalary As SalaryData In userSalaries
                Dim gross As Decimal = userSalary.BasicSalary + userSalary.HRA + userSalary.OtherAllowances
                Dim tds As Decimal = 0D
                Dim deductions As Decimal = userSalary.PF_Deduction + userSalary.ProfessionalTax_Deduction + tds
                Dim net As Decimal = gross - deductions

                Using cmdInsert As New SqlCommand(insertQuery, conn)
                    cmdInsert.Parameters.AddWithValue("@EmpID", userSalary.UserID)
                    cmdInsert.Parameters.AddWithValue("@Month", month)
                    cmdInsert.Parameters.AddWithValue("@Year", year)
                    cmdInsert.Parameters.AddWithValue("@Basic", userSalary.BasicSalary)
                    cmdInsert.Parameters.AddWithValue("@HRA", userSalary.HRA)
                    cmdInsert.Parameters.AddWithValue("@Allowances", userSalary.OtherAllowances)
                    cmdInsert.Parameters.AddWithValue("@Gross", gross)
                    cmdInsert.Parameters.AddWithValue("@PF", userSalary.PF_Deduction)
                    cmdInsert.Parameters.AddWithValue("@PTax", userSalary.ProfessionalTax_Deduction)
                    cmdInsert.Parameters.AddWithValue("@TDS", tds)
                    cmdInsert.Parameters.AddWithValue("@TotalDeduct", deductions)
                    cmdInsert.Parameters.AddWithValue("@Net", net)
                    cmdInsert.ExecuteNonQuery()
                End Using
            Next
        End Using
    End Sub
End Class