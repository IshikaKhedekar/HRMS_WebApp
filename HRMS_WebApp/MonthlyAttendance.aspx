<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="MonthlyAttendance.aspx.vb" Inherits="HRMS_WebApp.MonthlyAttendance" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .report-header {
            background-color: #f8f9fa;
            padding: 1.5rem;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        .report-table th {
            background-color: #0d6efd;
            color: white;
        }
        .summary-card {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 15px;
            text-align: center;
            border-radius: 8px;
        }
        .summary-card .count {
            font-size: 2rem;
            font-weight: 700;
        }
        .summary-card .label {
            color: #6c757d;
        }
        .status-present { background-color: #d1e7dd; color: #0f5132; } /* Green */
        .status-absent { background-color: #f8d7da; color: #842029; } /* Red */
        .status-leave { background-color: #fff3cd; color: #664d03; } /* Yellow */
        .status-holiday { background-color: #cff4fc; color: #055160; } /* Blue */
        .status-weekend { background-color: #e2e3e5; color: #41464b; } /* Grey */
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Monthly Attendance Report</h2>
        <p>View your detailed monthly attendance summary.</p>
    </div>

    <!-- Filter Section -->
    <div class="report-header">
        <div class="row g-3 align-items-end">
            <!-- Employee Dropdown (for Manager/HR) -->
            <div class="col-md-4" runat="server" id="divEmployeeFilter" visible="false">
                <label class="form-label">Select Employee:</label>
                <asp:DropDownList ID="ddlEmployees" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>
            <!-- Month/Year Filters -->
            <div class="col-md-3">
                <label class="form-label">Select Month:</label>
                <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>
            <div class="col-md-3">
                <label class="form-label">Select Year:</label>
                <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select"></asp:DropDownList>
            </div>
            <div class="col-md-2">
                <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report" CssClass="btn btn-primary w-100" OnClick="btnGenerateReport_Click" />
            </div>
        </div>
    </div>
    
    <!-- Report Content Panel -->
    <asp:Panel ID="pnlReportContent" runat="server" Visible="false">
        <!-- Summary Cards -->
        <div class="row text-center mb-4">
            <div class="col"><div class="summary-card"><div class="count text-success"><asp:Literal ID="litPresentDays" runat="server">0</asp:Literal></div><div class="label">Present</div></div></div>
            <div class="col"><div class="summary-card"><div class="count text-danger"><asp:Literal ID="litAbsentDays" runat="server">0</asp:Literal></div><div class="label">Absent</div></div></div>
            <div class="col"><div class="summary-card"><div class="count text-warning"><asp:Literal ID="litLeaveDays" runat="server">0</asp:Literal></div><div class="label">On Leave</div></div></div>
            <div class="col"><div class="summary-card"><div class="count text-info"><asp:Literal ID="litHolidays" runat="server">0</asp:Literal></div><div class="label">Holidays</div></div></div>
            <div class="col"><div class="summary-card"><div class="count text-secondary"><asp:Literal ID="litWeekends" runat="server">0</asp:Literal></div><div class="label">Weekends</div></div></div>
        </div>

        <!-- Detailed Report Table -->
        <div class="table-responsive">
            <asp:Repeater ID="rptMonthlyReport" runat="server">
                <HeaderTemplate><table class="table table-bordered report-table"><thead><tr><th>Date</th><th>Day</th><th>Punch In</th><th>Punch Out</th><th>Total Hours</th>  <th>Overtime</th><th>Status</th></tr></thead><tbody></HeaderTemplate>
                <ItemTemplate>
                    <tr class='<%# Eval("StatusCssClass") %>'>
                        <td><%# Eval("Date", "{0:dd-MMM-yyyy}") %></td>
                        <td><%# Eval("Date", "{0:dddd}") %></td>
                        <td><%# Eval("PunchIn") %></td>
                        <td><%# Eval("PunchOut") %></td>
                        <td><%# Eval("TotalHours") %></td>
        <td><strong><%# Eval("Overtime") %></strong></td> <!-- <<< NEW CELL -->

                        <td><strong><%# Eval("Status") %></strong></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>
</asp:Content>