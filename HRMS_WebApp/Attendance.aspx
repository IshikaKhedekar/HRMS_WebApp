<%@ Page Title="Attendance" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Attendance.aspx.vb" Inherits="HRMS_WebApp.Attendance" %>
<%-- DevExtreme Registration (dx tagPrefix) ko hata diya gaya hai --%>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .info-card { background-color: #fff; border: 1px solid #dee2e6; border-radius: .375rem; margin-bottom: 1.5rem; }
        .info-card-header { padding: 1rem 1.25rem; background-color: #f8f9fa; border-bottom: 1px solid #dee2e6; font-size: 1.1rem; font-weight: 600; }
        .info-card-body { padding: 1.25rem; }
        .data-table { width: 100%; border-collapse: collapse; }
        .data-table th, .data-table td { border: 1px solid #e9ecef; padding: 12px; text-align: left; }
        .data-table th { background-color: #343a40; color: white; }
        /* Status text colors */
        .status-present-text { color: #198754; font-weight: bold; }
        .status-absent-text { color: #dc3545; font-weight: bold; }
        .status-on-leave-text { color: #ffc107; font-weight: bold; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #20c997; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Attendance Management</h2>
        <p>Your portal for attendance records and requests.</p>
    </div>

    <!-- Hidden field to pass user role from server to client -->
    <asp:HiddenField ID="hdnUserRoleID" runat="server" />

    <!-- ===== EMPLOYEE / MANAGER VIEW (BOOTSTRAP TABS & TABLES) ===== -->
    <asp:Panel ID="pnlEmployeeManagerView" runat="server" Visible="false">
        <ul class="nav nav-tabs" id="attendanceTabs" role="tablist">
            <li class="nav-item" role="presentation"><button class="nav-link active" id="my-history-tab" data-bs-toggle="tab" data-bs-target="#my-history" type="button">My Attendance History</button></li>
            <li class="nav-item" role="presentation"><button class="nav-link" id="regularize-tab" data-bs-toggle="tab" data-bs-target="#regularize" type="button">Regularize Attendance</button></li>
            <li class="nav-item" role="presentation" runat="server" id="liTeamTab"><button class="nav-link" id="team-approvals-tab" data-bs-toggle="tab" data-bs-target="#team-approvals" type="button">Team Approvals</button></li>
        </ul>
        <div class="tab-content border border-top-0 p-3 bg-white">
            <!-- My Attendance History Tab -->
            <div class="tab-pane fade show active" id="my-history" role="tabpanel">
                <div class="info-card">
                    <div class="info-card-header">Your Recent Attendance</div>
                    <div class="info-card-body">
                        <asp:Repeater ID="rptMyAttendance" runat="server" OnItemDataBound="rptMyAttendance_ItemDataBound">
                            <HeaderTemplate><table class="table table-sm table-striped"><thead><tr><th>Date</th><th>Status</th><th>In-Time</th><th>Out-Time</th></tr></thead><tbody></HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("FormattedDate") %></td>
                                    <td><span class='<%# Eval("StatusCssClass") %>'><%# Eval("Status") %></span></td>
                                    <td><%# Eval("FormattedPunchIn") %></td>
                                    <td><%# Eval("FormattedPunchOut") %></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate></tbody></table></FooterTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoMyAttendance" runat="server" Text="No attendance records found." Visible="false" CssClass="text-muted p-3"></asp:Label>
                    </div>
                </div>
            </div>
            <!-- Regularize Attendance Tab -->
            <div class="tab-pane fade" id="regularize" role="tabpanel">
                <div class="info-card">
                    <div class="info-card-header">Request Attendance Regularization</div>
                    <div class="info-card-body">
                        <div class="mb-3"><label class="form-label">Date to Correct:</label><asp:TextBox ID="txtRegDate" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox></div>
                        <div class="mb-3"><label class="form-label">Correct Punch In Time:</label><asp:TextBox ID="txtRegPunchIn" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox></div>
                        <div class="mb-3"><label class="form-label">Correct Punch Out Time:</label><asp:TextBox ID="txtRegPunchOut" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox></div>
                        <div class="mb-3"><label class="form-label">Reason:</label><asp:TextBox ID="txtRegReason" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox></div>
                        <asp:Button ID="btnSubmitRegularization" runat="server" Text="Submit Request" CssClass="btn btn-primary" OnClick="btnSubmitRegularization_Click" />
                        <asp:Label ID="lblRegMessage" runat="server" CssClass="d-block mt-2"></asp:Label>
                    </div>
                </div>
            </div>
            <!-- Team Approvals Tab -->
            <div class="tab-pane fade" id="team-approvals" role="tabpanel">
                 <div class="info-card">
                    <div class="info-card-header">Pending Regularization Requests for Your Team</div>
                    <div class="info-card-body">
                        <asp:Repeater ID="rptTeamRequests" runat="server" OnItemCommand="rptTeamRequests_ItemCommand" OnItemDataBound="rptTeamRequests_ItemDataBound">
                            <HeaderTemplate><table class="table table-sm table-striped"><thead><tr><th>Employee</th><th>Date</th><th>Requested In/Out</th><th>Reason</th><th>Actions</th></tr></thead><tbody></HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("EmployeeName") %></td>
                                    <td><%# Eval("FormattedDate") %></td>
                                    <td><%# Eval("FormattedTime") %></td>
                                    <td><%# Eval("Reason") %></td>
                                    <td>
                                        <asp:Button ID="btnApprove" runat="server" Text="Approve" CommandName="Approve" CommandArgument='<%# Eval("RequestID") %>' CssClass="btn btn-sm btn-success" />
                                        <asp:Button ID="btnReject" runat="server" Text="Reject" CommandName="Reject" CommandArgument='<%# Eval("RequestID") %>' CssClass="btn btn-sm btn-danger" />
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate></tbody></table></FooterTemplate>
                         </asp:Repeater>
                         <asp:Label ID="lblNoTeamRequests" runat="server" Text="No pending requests for your team." Visible="false" CssClass="text-muted p-3"></asp:Label>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- ===== ADMIN / HR VIEW (Table with Modal for Add/Edit) ===== -->
    <asp:Panel ID="pnlAdminView" runat="server" Visible="false">
         <div class="info-card">
            <div class="info-card-header d-flex justify-content-between align-items-center">
                <span>Manage All Company Attendance Records</span>
                <asp:Button ID="btnAddAdminRecord" runat="server" Text="Add New Record" CssClass="btn btn-primary" OnClick="btnAddAdminRecord_Click" />
            </div>
            <div class="info-card-body">
                <asp:Repeater ID="rptAllAttendance" runat="server" OnItemCommand="rptAllAttendance_ItemCommand" OnItemDataBound="rptAllAttendance_ItemDataBound">
                    <HeaderTemplate>
                        <table class="table table-bordered table-striped data-table">
                            <thead>
                                <tr>
                                    <th>Log ID</th>
                                    <th>Employee Name</th>
                                    <th>Date</th>
                                    <th>Punch In</th>
                                    <th>Punch Out</th>
                                    <th>Status</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("LogID") %></td>
                            <td><%# Eval("EmployeeName") %></td>
                            <td><%# Eval("FormattedDate") %></td>
                            <td><%# Eval("FormattedPunchIn") %></td>
                            <td><%# Eval("FormattedPunchOut") %></td>
                            <td><span class='<%# Eval("StatusCssClass") %>'><%# Eval("Status") %></span></td>
                            <td class="action-buttons">
                                <asp:Button ID="btnEditAdminRec" runat="server" Text="Edit" CommandName="EditAdmin" CommandArgument='<%# Eval("LogID") %>' CssClass="btn btn-sm btn-info" />
                                <asp:Button ID="btnDeleteAdminRec" runat="server" Text="Delete" CommandName="DeleteAdmin" CommandArgument='<%# Eval("LogID") %>' OnClientClick="return confirm('Are you sure you want to delete this record?');" CssClass="btn btn-sm btn-danger" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Label ID="lblNoAllAttendance" runat="server" Text="No attendance records found for the company." Visible="false" CssClass="text-muted"></asp:Label>
            </div>
        </div>
        
        <!-- Admin Add/Edit Modal (Bootstrap Modal) -->
        <div class="modal fade" id="adminRecordModal" tabindex="-1" aria-labelledby="adminRecordModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="adminRecordModalLabel">Add/Edit Attendance Record</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hdnAdminLogID" runat="server" Value="0" />
                        <div class="mb-3">
                            <label class="form-label">Employee ID</label>
                            <asp:TextBox ID="txtAdminEmployeeID" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Date</label>
                            <asp:TextBox ID="txtAdminLogDate" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Punch In Time</label>
                            <asp:TextBox ID="txtAdminPunchInTime" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Punch Out Time</label>
                            <asp:TextBox ID="txtAdminPunchOutTime" runat="server" TextMode="Time" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Status</label>
                            <asp:DropDownList ID="ddlAdminStatus" runat="server" CssClass="form-select">
                                <asp:ListItem>Present</asp:ListItem>
                                <asp:ListItem>Absent</asp:ListItem>
                                <asp:ListItem>On Leave</asp:ListItem>
                                <asp:ListItem>Holiday</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Label ID="lblAdminModalMessage" runat="server" CssClass="me-auto"></asp:Label>
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <asp:Button ID="btnSaveAdminRecord" runat="server" Text="Save Record" CssClass="btn btn-primary" OnClick="btnSaveAdminRecord_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>