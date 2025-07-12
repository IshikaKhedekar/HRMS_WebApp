<%@ Page Title="Resignation Management" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Resignation.aspx.vb" Inherits="HRMS_WebApp.Resignation" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .page-header { background-color: #fd7e14; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px; }
        .section-card { border: 1px solid #e9ecef; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); background-color: #fff; margin-bottom: 20px; }
        .section-card-header { padding: 15px 20px; background-color: #f8f9fa; border-bottom: 1px solid #e9ecef; font-weight: bold; }
        .section-card-body { padding: 20px; }
        .data-table th { background-color: #fd7e14; color: white; }
        
        /* Status styles for Resignations - UPDATED to match inline generation */
        /* These class names are derived from Eval("Status").ToString().Replace(" ", "") */
        .status-Submitted { color: #0d6efd; font-weight: bold; } /* Blue */
        .status-Accepted { color: #6610f2; font-weight: bold; } /* Indigo */
        .status-OnNoticePeriod { color: #fd7e14; font-weight: bold; } /* Orange */
        .status-ExitInterviewScheduled { color: #8729cc; font-weight: bold;} /* Purple */
        .status-Cleared { color: #28a745; font-weight: bold; } /* Green */
        .status-Rejected { color: #dc3545; font-weight: bold; } /* Red */
        .status-Withdrawn { color: #6c757d; font-weight: bold; } /* Gray */
        .countdown-timer { font-size: 1.5rem; font-weight: bold; color: #007bff; }
        .status-default, .status-unknown { color: gray; } /* Fallback for unknown statuses */

        /* Added for pleasing display of employee info */
        .employee-info-card { background-color: #f8f9fa; padding: 15px 20px; border-radius: 8px; margin-bottom: 20px; border: 1px solid #e9ecef; }
        .employee-info-card h5 { margin-bottom: 10px; color: #343a40; font-size: 1.1em; }
        .employee-info-card p { margin-bottom: 5px; font-size: 0.95em; }
        .employee-info-card strong { color: #495057; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #6610f2; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Resignation Management</h2>
        <p>Submit your resignation and track its status.</p>
    </div>

    <!-- Hidden Field to pass User Role (Not directly used in this code, but useful for JS/client-side) -->
    <asp:HiddenField ID="hdnUserRoleID" runat="server" />

    <!-- Tabs for Employee View / HR/Manager View -->
    <ul class="nav nav-tabs" id="resignationTabs" role="tablist">
        <li class="nav-item" role="presentation"><button class="nav-link active" id="my-resignation-tab" data-bs-toggle="tab" data-bs-target="#my-resignation" type="button">My Resignation</button></li>
        <li class="nav-item" role="presentation" runat="server" id="liManageResignationsTab"><button class="nav-link" id="manage-resignations-tab" data-bs-toggle="tab" data-bs-target="#manage-resignations" type="button">Manage Resignations</button></li>
    </ul>

    <div class="tab-content border border-top-0 p-3 bg-white">
        <!-- ===== MY RESIGNATION TAB (EMPLOYEE VIEW) ===== -->
        <div class="tab-pane fade show active" id="my-resignation" role="tabpanel">
            <asp:Panel ID="pnlResignationForm" runat="server">
                <div class="section-header">Submit Your Resignation</div>
                <div class="resignation-card">
                    <%-- Added employee info card for pleasing display --%>
                    <div class="employee-info-card">
                        <h5>Your Details</h5>
                        <p><strong>Name:</strong> <asp:Label ID="lblEmployeeName" runat="server"></asp:Label></p>
                        <p><strong>Current Job Title:</strong> <asp:Label ID="lblCurrentJobTitle" runat="server"></asp:Label></p>
                        <p><strong>Date of Joining:</strong> <asp:Label ID="lblDateOfJoining" runat="server"></asp:Label></p>
                        <p><strong>Tenure:</strong> <asp:Label ID="lblTenure" runat="server"></asp:Label></p>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Resignation Date:</label>
                        <asp:Label ID="lblResignationDate" runat="server" CssClass="form-control-plaintext"></asp:Label>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Proposed Last Working Date:</label>
                        <asp:TextBox ID="txtProposedLastWorkingDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        <small class="form-text text-muted">Please propose your last working day, considering your notice period.</small>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Reason for Resignation:</label>
                        <asp:TextBox ID="txtResignationReason" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnSubmitResignation" runat="server" Text="Submit Resignation" CssClass="btn btn-primary" OnClick="btnSubmitResignation_Click" />
                    <asp:Label ID="lblResignationMessage" runat="server" CssClass="d-block mt-2"></asp:Label>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlResignationStatus" runat="server" Visible="false">
                <div class="section-header">Your Current Resignation Status</div>
                <div class="resignation-card">
                    <p><strong>Status:</strong> <asp:Label ID="lblCurrentStatus" runat="server"></asp:Label></p>
                    <p><strong>Submitted On:</strong> <asp:Label ID="lblSubmittedOn" runat="server"></asp:Label></p>
                    <p><strong>Reason:</strong> <asp:Literal ID="litResignationReason" runat="server"></asp:Literal></p>
                    <p><strong>Requested Last Working Date:</strong> <asp:Label ID="lblRequestedLastWorkingDate" runat="server"></asp:Label></p>
                    <p><strong>Approved Last Working Date:</strong> <asp:Label ID="lblApprovedLastWorkingDate" runat="server"></asp:Label></p>
                    <p><strong>Notice Period Left:</strong> <asp:Label ID="lblNoticePeriodLeft" runat="server" CssClass="countdown-timer"></asp:Label></p>
                    <asp:Button ID="btnWithdrawResignation" runat="server" Text="Withdraw Resignation" CssClass="btn btn-warning mt-2" OnClick="btnWithdrawResignation_Click" />
                </div>
            </asp:Panel>
        </div>

        <!-- ===== MANAGE RESIGNATIONS TAB (HR/MANAGER VIEW) ===== -->
        <div class="tab-pane fade" id="manage-resignations" role="tabpanel">
            <div class="section-header">Pending Resignation Requests</div>
            <asp:Repeater ID="rptPendingResignations" runat="server" OnItemCommand="rptPendingResignations_ItemCommand">
                <HeaderTemplate>
                    <table class="table table-striped data-table">
                        <thead><tr><th>Employee Name</th><th>Submitted On</th><th>Requested Last Day</th><th>Reason</th><th>Actions</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("EmployeeName") %></td>
                        <td><%# Eval("ResignationDate", "{0:dd-MMM-yyyy}") %></td> 
                        <td><%# Eval("RequestedLastWorkingDate", "{0:dd-MMM-yyyy}") %></td>
                        <td><%# Eval("ResignationReason") %></td>
                        <td><asp:Button ID="btnViewDetails" runat="server" Text="View Details" CommandName="ViewDetails" CommandArgument='<%# Eval("ResignationID") %>' CssClass="btn btn-sm btn-info" /></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoPendingResignations" runat="server" Text="No pending resignation requests." Visible="false" CssClass="text-muted p-3"></asp:Label>

            <div class="section-header mt-4">Active & Completed Resignations</div>
            <asp:Repeater ID="rptOtherResignations" runat="server" OnItemCommand="rptOtherResignations_ItemCommand">
                <HeaderTemplate>
                    <table class="table table-striped data-table">
                        <thead><tr><th>Employee Name</th><th>Status</th><th>Approved Last Day</th><th>Notice Period</th><th>Actions</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("EmployeeName") %></td>
                        <td class="status-<%# Eval("Status").ToString().Replace(" ", "") %>"><%# Eval("Status") %></td>
                        <td><%# Eval("ApprovedLastWorkingDate", "{0:dd-MMM-yyyy}") %></td>
                        <td><%# Eval("NoticePeriodDays") %> days</td>
                        <td>
                            <asp:Button ID="btnViewDetailsOther" runat="server" Text="View Details" CommandName="ViewDetailsOther" CommandArgument='<%# Eval("ResignationID") %>' CssClass="btn btn-sm btn-secondary" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
             <asp:Label ID="lblNoOtherResignations" runat="server" Text="No other resignation records." Visible="false" CssClass="text-muted p-3"></asp:Label>
        </div>
    </div>

    <!-- ===== RESIGNATION DETAIL MODAL (HR/MANAGER) ===== -->
    <div class="modal fade" id="resignationDetailModal" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="resignationDetailModalLabel">Resignation Details for <asp:Label ID="lblModalEmpName" runat="server"></asp:Label></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnResignationID" runat="server" />
                    <p><strong>Status:</strong> <asp:Label ID="lblModalStatus" runat="server"></asp:Label></p>
                    <p><strong>Submitted On:</strong> <asp:Label ID="lblModalSubmittedOn" runat="server"></asp:Label></p>
                    <p><strong>Reason:</strong> <asp:Literal ID="litModalReason" runat="server"></asp:Literal></p>
                    <p><strong>Requested Last Working Date:</strong> <asp:Label ID="lblModalRequestedDate" runat="server"></asp:Label></p>
                    
                    <hr />
                    <h6 class="mb-3">HR/Manager Actions:</h6>
                    <div class="mb-3">
                        <label class="form-label">Approve Last Working Date:</label>
                        <asp:TextBox ID="txtApprovedLastWorkingDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Notice Period (Days):</label>
                        <asp:TextBox ID="txtNoticePeriod" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Update Status:</label>
                        <asp:DropDownList ID="ddlUpdateStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Accepted">Accepted</asp:ListItem>
                            <asp:ListItem Value="On Notice Period">On Notice Period</asp:ListItem>
                            <asp:ListItem Value="Exit Interview Scheduled">Exit Interview Scheduled</asp:ListItem>
                            <asp:ListItem Value="Cleared">Cleared</asp:ListItem>
                            <asp:ListItem Value="Rejected">Rejected</asp:ListItem>
                            <asp:ListItem Value="Withdrawn">Withdrawn</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Internal Notes:</label>
                        <asp:TextBox ID="txtInternalNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Label ID="lblModalMessage" runat="server" CssClass="me-auto"></asp:Label>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveResignationDetails" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSaveResignationDetails_Click" />
                    <asp:Button ID="btnModalApprove" runat="server" Text="Approve" CssClass="btn btn-success" OnClick="btnModalApprove_Click" Visible="false"/>
                    <asp:Button ID="btnModalReject" runat="server" Text="Reject" CssClass="btn btn-danger" OnClick="btnModalReject_Click" Visible="false"/>
                    <asp:Button ID="btnModalCreateVacancy" runat="server" Text="Create Vacancy" CssClass="btn btn-warning" OnClick="btnModalCreateVacancy_Click" Visible="false"/>
                </div>
            </div>
        </div>
    </div>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>