<%@ Page Title="Payroll" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Payroll.aspx.vb" Inherits="HRMS_WebApp.Payroll" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .payslip-container {
            border: 1px solid #dee2e6;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.05);
        }
        .payslip-header {
            background-color: #0d6efd;
            color: white;
            padding: 1.5rem;
            border-top-left-radius: 8px;
            border-top-right-radius: 8px;
        }
        .payslip-body {
            padding: 1.5rem;
        }
        .payslip-section-title {
            font-weight: 600;
            color: #0d6efd;
            border-bottom: 2px solid #0d6efd;
            padding-bottom: 5px;
            margin-bottom: 15px;
        }
        .payslip-row {
            display: flex;
            justify-content: space-between;
            padding: 8px 0;
            border-bottom: 1px solid #f0f0f0;
        }
        .payslip-label { color: #6c757d; }
        .payslip-value { font-weight: 500; }
        .payslip-total {
            font-weight: bold;
            font-size: 1.1rem;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>My Payslips</h2>
        <p>View and download your monthly salary slips.</p>
    </div>

    <!-- ===== EMPLOYEE VIEW PANEL ===== -->
    <asp:Panel ID="pnlEmployeeView" runat="server">
        <div class="row justify-content-center">
            <div class="col-lg-8">
                <!-- Filter Section -->
                <div class="d-flex justify-content-end align-items-center mb-3">
                    <label class="me-2">Select Period:</label>
                    <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-select me-2" Style="width: auto;"></asp:DropDownList>
                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select me-2" Style="width: auto;"></asp:DropDownList>
                    <asp:Button ID="btnShowPayslip" runat="server" Text="Show Payslip" CssClass="btn btn-primary" OnClick="btnShowPayslip_Click" />
                </div>

                <!-- Payslip Display Panel -->
                <asp:Panel ID="pnlPayslip" runat="server" CssClass="payslip-container" Visible="false">
                    <div class="payslip-header text-center">
                        <h4>Payslip for <asp:Literal ID="litMonthYear" runat="server"></asp:Literal></h4>
                        <h5><asp:Literal ID="litEmpName" runat="server"></asp:Literal></h5>
                    </div>
                    <div class="payslip-body">
                        <div class="row">
                            <!-- Earnings -->
                            <div class="col-md-6 mb-4">
                                <h5 class="payslip-section-title">Earnings</h5>
                                <div class="payslip-row"><span class="payslip-label">Basic Salary</span><span class="payslip-value"><asp:Literal ID="litBasic" runat="server"></asp:Literal></span></div>
                                <div class="payslip-row"><span class="payslip-label">House Rent Allowance (HRA)</span><span class="payslip-value"><asp:Literal ID="litHRA" runat="server"></asp:Literal></span></div>
                                <div class="payslip-row"><span class="payslip-label">Other Allowances</span><span class="payslip-value"><asp:Literal ID="litAllowances" runat="server"></asp:Literal></span></div>
                                <div class="payslip-row payslip-total bg-light"><span class="payslip-label">Gross Earnings</span><span class="payslip-value"><asp:Literal ID="litGross" runat="server"></asp:Literal></span></div>
                            </div>
                            <!-- Deductions -->
                            <div class="col-md-6 mb-4">
                                <h5 class="payslip-section-title">Deductions</h5>
                                <div class="payslip-row"><span class="payslip-label">Provident Fund (PF)</span><span class="payslip-value"><asp:Literal ID="litPF" runat="server"></asp:Literal></span></div>
                                <div class="payslip-row"><span class="payslip-label">Professional Tax</span><span class="payslip-value"><asp:Literal ID="litProfTax" runat="server"></asp:Literal></span></div>
                                <div class="payslip-row"><span class="payslip-label">TDS</span><span class="payslip-value"><asp:Literal ID="litTDS" runat="server"></asp:Literal></span></div>
                                <div class="payslip-row payslip-total bg-light"><span class="payslip-label">Total Deductions</span><span class="payslip-value"><asp:Literal ID="litDeductions" runat="server"></asp:Literal></span></div>
                            </div>
                        </div>
                        <div class="text-center mt-3">
                            <h4>Net Salary: <span class="text-success"><asp:Literal ID="litNetSalary" runat="server"></asp:Literal></span></h4>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Label ID="lblNoPayslip" runat="server" CssClass="alert alert-warning d-block text-center" Visible="false">No payslip found for the selected period.</asp:Label>
            </div>
        </div>
    </asp:Panel>

    <!-- ===== HR/ADMIN VIEW PANEL (Coming Soon) ===== -->
<asp:Panel ID="pnlAdminView" runat="server" Visible="false">
    <div class="section-header d-flex justify-content-between align-items-center">
        <span>Process Payroll for All Employees</span>
        <div>
            <label class="me-2">For Period:</label>
            <asp:DropDownList ID="ddlProcessMonth" runat="server" CssClass="form-select d-inline-block" Style="width: auto;"></asp:DropDownList>
            <asp:DropDownList ID="ddlProcessYear" runat="server" CssClass="form-select d-inline-block" Style="width: auto;"></asp:DropDownList>
            <asp:Button ID="btnRunPayroll" runat="server" Text="Run & Generate Slips" CssClass="btn btn-success ms-2" OnClick="btnRunPayroll_Click" OnClientClick="return confirm('Are you sure you want to run payroll for the selected period? This will generate payslips for all employees.');" />
        </div>
    </div>
    <asp:Label ID="lblAdminMessage" runat="server" CssClass="d-block mb-3"></asp:Label>
    
    <asp:Repeater ID="rptAllSalaries" runat="server">
        <HeaderTemplate>
            <table class="table table-bordered table-striped data-table">
                <thead>
                    <tr>
                        <th>Employee Name</th>
                        <th>Basic Salary</th>
                        <th>HRA</th>
                        <th>Allowances</th>
                        <th>Gross Earnings</th>
                        <th>PF</th>
                        <th>Prof. Tax</th>
                        <th>Net Salary</th>
                    </tr>
                </thead>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><%# Eval("Name") %></td>
                <td><%# Eval("BasicSalary", "{0:N2}") %></td>
                <td><%# Eval("HRA", "{0:N2}") %></td>
                <td><%# Eval("OtherAllowances", "{0:N2}") %></td>
                <td><strong><%# Eval("GrossSalary", "{0:N2}") %></strong></td>
                <td><%# Eval("PF_Deduction", "{0:N2}") %></td>
                <td><%# Eval("ProfessionalTax_Deduction", "{0:N2}") %></td>
                <td><strong><%# Eval("NetSalary", "{0:N2}") %></strong></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:Repeater>
</asp:Panel>
</asp:Content>