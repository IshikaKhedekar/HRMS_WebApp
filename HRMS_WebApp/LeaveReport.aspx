<%@ Page Title="Leave Reports" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="LeaveReport.aspx.vb" Inherits="HRMS_WebApp.LeaveReport" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-card { border: 1px solid #e9ecef; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); background-color: #fff; margin-bottom: 20px; }
        .section-card-header { padding: 15px 20px; background-color: #f8f9fa; border-bottom: 1px solid #e9ecef; font-weight: bold; }
        .section-card-body { padding: 20px; }
        .data-table th { background-color: #fd7e14; color: white; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #fd7e14; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Leave Reports</h2>
        <p>Generate and view comprehensive leave reports for the company.</p>
    </div>

    <!-- Filter Section -->
    <div class="section-card">
        <div class="section-card-header">Report Filters</div>
        <div class="section-card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <label class="form-label me-2 mb-0">Filter Report:</label>
                <asp:DropDownList ID="ddlReportMonth" runat="server" CssClass="form-select me-2" Style="width: auto;"></asp:DropDownList>
                <asp:DropDownList ID="ddlReportYear" runat="server" CssClass="form-select me-2" Style="width: auto;"></asp:DropDownList>
                <asp:DropDownList ID="ddlReportStatusFilter" runat="server" CssClass="form-select me-2" Style="width: auto;"></asp:DropDownList>
                <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report" CssClass="btn btn-info" OnClick="btnGenerateReport_Click" />
            </div>
        </div>
    </div>

    <!-- Leave Report Table -->
    <div class="section-card">
        <div class="section-card-header">Generated Leave Report</div>
        <div class="section-card-body">
            <asp:Repeater ID="rptLeaveReport" runat="server">
                <HeaderTemplate><table class="table table-striped data-table"><thead><tr><th>Employee</th><th>Leave Type</th><th>Dates</th><th>Status</th><th>Reason</th></tr></thead><tbody></HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("EmployeeName") %></td>
                        <td><%# Eval("LeaveType") %></td>
                        <td><%# Eval("StartDate", "{0:dd-MMM-yyyy}") %> to <%# Eval("EndDate", "{0:dd-MMM-yyyy}") %></td>
                        <td><%# Eval("Status") %></td>
                        <td><%# Eval("Reason") %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoLeaveReport" runat="server" Text="No leave records found for the selected criteria." Visible="false" CssClass="text-muted"></asp:Label>
        </div>
    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>