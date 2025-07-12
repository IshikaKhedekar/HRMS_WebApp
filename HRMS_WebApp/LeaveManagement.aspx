<%@ Page Title="Leave Management" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="LeaveManagement.aspx.vb" Inherits="HRMS_WebApp.LeaveManagement" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-header { background-color: #f2f2f2; padding: 15px; border-bottom: 1px solid #ddd; margin-bottom: 20px; font-size: 1.2em; font-weight: bold; }
        .data-table th { background-color: #fd7e14; color: white; }
        .leave-balance-card { border: 1px solid #e9ecef; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); background-color: #fff; margin-bottom: 20px; }
        .leave-balance-card-header { padding: 15px 20px; background-color: #f8f9fa; border-bottom: 1px solid #e9ecef; font-weight: bold; }
        .leave-balance-card-body { padding: 20px; }
        .leave-balance-item { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px dashed #eee; }
        .leave-balance-item:last-child { border-bottom: none; }
        .leave-balance-count { font-size: 1.2rem; font-weight: bold; color: #007bff; }
        .total-leave { color: #6c757d; }

        /* CSS for Leave Statuses - UPDATED to match inline generation */
        .status-Pending { color: orange; font-weight: bold; }
        .status-ApprovedbyManager, .status-ApprovedbyHR { color: green; font-weight: bold; } /* Combines "Approved by Manager" and "Approved by HR" */
        .status-Rejected { color: red; font-weight: bold; }
        .status-default, .status-unknown { color: gray; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #fd7e14; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Leave Management</h2>
        <p>Apply for leaves and manage approvals.</p>
    </div>

    <!-- Bootstrap Tabs -->
    <ul class="nav nav-tabs" id="leaveTabs" role="tablist">
        <li class="nav-item" role="presentation"><button class="nav-link active" id="my-dashboard-tab" data-bs-toggle="tab" data-bs-target="#my-dashboard" type="button">My Leave Dashboard</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" id="apply-leave-tab" data-bs-toggle="tab" data-bs-target="#apply-leave" type="button">Apply for Leave</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" id="my-history-tab" data-bs-toggle="tab" data-bs-target="#my-history" type="button">My Leave History</button></li>
        <li class="nav-item" role="presentation" runat="server" id="liTeamApprovalsTab"><button class="nav-link" id="team-approvals-tab" data-bs-toggle="tab" data-bs-target="#team-approvals" type="button">Team Approvals</button></li>
        <%-- liLeaveReportsTab yahan se hata diya gaya hai --%>
    </ul>

    <div class="tab-content border border-top-0 p-3 bg-white">
        <!-- ===== MY LEAVE DASHBOARD TAB ===== -->
        <div class="tab-pane fade show active" id="my-dashboard" role="tabpanel">
            <div class="row">
                <div class="col-md-5">
                    <div class="leave-balance-card">
                        <div class="leave-balance-card-header">Your Leave Balance</div>
                        <div class="section-card-body">
                            <div class="leave-balance-item"><span class="leave-type">Casual Leave</span><span class="leave-balance-count"><asp:Label ID="lblCasualLeaveBalanceAvailable" runat="server" Text="0"></asp:Label>/<asp:Label ID="lblCasualLeaveTotal" runat="server" Text="12"></asp:Label> <span class="total-leave">days</span></span></div>
                            <div class="leave-balance-item"><span class="leave-type">Sick Leave</span><span class="leave-balance-count"><asp:Label ID="lblSickLeaveBalanceAvailable" runat="server" Text="0"></asp:Label>/<asp:Label ID="lblSickLeaveTotal" runat="server" Text="8"></asp:Label> <span class="total-leave">days</span></span></div>
                            <%-- Paid Leave: Shows 'Taken' count as requested --%>
                            <div class="leave-balance-item"><span class="leave-type">Paid Leave Taken</span><span class="leave-balance-count"><asp:Label ID="lblPaidLeaveCountTaken" runat="server" Text="0"></asp:Label> <span class="total-leave">days</span></span></div>
                        </div>
                    </div>
                </div>
                <div class="col-md-7">
                    <div class="leave-balance-card">
                        <div class="leave-balance-card-header">Upcoming Company Holidays</div>
                        <div class="section-card-body">
                            <asp:Repeater ID="rptCompanyHolidays" runat="server">
                                <HeaderTemplate><ul class="list-unstyled"></HeaderTemplate>
                                <ItemTemplate><li><i class="fas fa-calendar-day me-2"></i><%# Eval("HolidayName") %>: <%# Eval("HolidayDate", "{0:MMM dd, yyyy}") %></li></ItemTemplate>
                                <FooterTemplate></ul></FooterTemplate>
                            </asp:Repeater>
                            <asp:Label ID="lblNoCompanyHolidays" runat="server" Text="No upcoming holidays." Visible="false" CssClass="text-muted"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="section-card">
                <div class="section-card-header">Your Upcoming Leaves</div>
                <div class="section-card-body">
                    <asp:Repeater ID="rptUpcomingMyLeaves" runat="server">
                        <HeaderTemplate><table class="table table-sm table-striped"><thead><tr><th>Start Date</th><th>End Date</th><th>Leave Type</th><th>Status</th><th>Reason</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("StartDate", "{0:dd-MMM-yyyy}") %></td>
                                <td><%# Eval("EndDate", "{0:dd-MMM-yyyy}") %></td>
                                <td><%# Eval("LeaveType") %></td>
                                <%-- UPDATED: Use inline expression for status class --%>
                                <td class="status-<%# Eval("Status").ToString().Replace(" ", "") %>"><%# Eval("Status") %></td>
                                <td><%# Eval("Reason") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoUpcomingMyLeaves" runat="server" Text="No upcoming leaves." Visible="false" CssClass="text-muted"></asp:Label>
                </div>
            </div>
        </div>

        <!-- ===== APPLY FOR LEAVE TAB ===== -->
        <div class="tab-pane fade" id="apply-leave" role="tabpanel">
            <div class="section-card">
                <div class="section-card-header">New Leave Application</div>
                <div class="section-card-body">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label">Leave Type</label><asp:DropDownList ID="ddlLeaveType" runat="server" CssClass="form-select"><asp:ListItem>Casual Leave</asp:ListItem><asp:ListItem>Sick Leave</asp:ListItem><asp:ListItem>Paid Leave</asp:ListItem></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Start Date</label><asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">End Date</label><asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label">Report To Manager</label><asp:DropDownList ID="ddlReportToManager" runat="server" CssClass="form-select" DataTextField="Name" DataValueField="UserID"></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Reason for Leave</label><asp:TextBox ID="txtReason" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5"></asp:TextBox></div>
                        </div>
                    </div>
                    <asp:Button ID="btnSubmitLeave" runat="server" Text="Submit Leave Request" CssClass="btn btn-primary" OnClick="btnSubmitLeave_Click" />
                    <asp:Label ID="lblLeaveMessage" runat="server" CssClass="ms-3"></asp:Label>
                </div>
            </div>
        </div>

        <!-- ===== MY LEAVE HISTORY TAB ===== -->
        <div class="tab-pane fade" id="my-history" role="tabpanel">
            <div class="section-card">
                <div class="section-card-header">Your Past Leave Applications</div>
                <div class="section-card-body">
                    <asp:Repeater ID="rptMyLeaves" runat="server">
                        <HeaderTemplate><table class="table table-striped data-table"><thead><tr><th>Start Date</th><th>End Date</th><th>Leave Type</th><th>Status</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("StartDate", "{0:dd-MMM-yyyy}") %></td>
                                <td><%# Eval("EndDate", "{0:dd-MMM-yyyy}") %></td>
                                <td><%# Eval("LeaveType") %></td>
                                <%-- UPDATED: Use inline expression for status class --%>
                                <td class="status-<%# Eval("Status").ToString().Replace(" ", "") %>"><%# Eval("Status") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoMyLeavesHistory" runat="server" Text="No leave history found." Visible="false" CssClass="text-muted"></asp:Label>
                </div>
            </div>
        </div>

        <!-- ===== TEAM APPROVALS TAB ===== -->
        <div class="tab-pane fade" id="team-approvals" role="tabpanel">
            <div class="section-card">
                <div class="section-card-header">Pending Leave Requests for Your Team</div>
                <div class="section-card-body">
                    <asp:Repeater ID="rptTeamLeaves" runat="server" OnItemCommand="rptTeamLeaves_ItemCommand">
                        <HeaderTemplate><table class="table table-striped data-table"><thead><tr><th>Employee Name</th><th>Leave Type</th><th>Dates</th><th>Reason</th><th>Actions</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("EmployeeName") %></td>
                                <td><%# Eval("LeaveType") %></td>
                                <td><%# Eval("StartDate", "{0:dd-MMM-yyyy}") %> to <%# Eval("EndDate", "{0:dd-MMM-yyyy}") %></td>
                                <td><%# Eval("Reason") %></td>
                                <td>
                                    <asp:Button ID="btnApprove" runat="server" Text="Approve" CommandName="Approve" CommandArgument='<%# Eval("LeaveID") %>' CssClass="btn btn-sm btn-success" />
                                    <asp:Button ID="btnReject" runat="server" Text="Reject" CommandName="Reject" CommandArgument='<%# Eval("LeaveID") %>' CssClass="btn btn-sm btn-danger" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoTeamLeaves" runat="server" Text="No pending leave requests for your team." Visible="false" CssClass="text-muted"></asp:Label>
                </div>
            </div>
        </div>

    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>