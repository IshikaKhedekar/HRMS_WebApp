<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="PeerFeedback.aspx.vb" Inherits="HRMS_WebApp.PeerFeedback" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-card { background-color: #fff; border: 1px solid #dee2e6; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); margin-bottom: 20px; }
        .section-title { font-size: 1.25rem; font-weight: 600; padding: 15px 20px; border-bottom: 1px solid #dee2e6; }
        .section-body { padding: 20px; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #198754; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>360° Peer Feedback</h2>
        <p>Request and provide constructive feedback for team members.</p>
    </div>

    <!-- Bootstrap Tabs -->
    <ul class="nav nav-tabs" id="feedbackTabs" role="tablist">
        <li class="nav-item" role="presentation"><button class="nav-link active" id="requests-for-me-tab" data-bs-toggle="tab" data-bs-target="#requests-for-me" type="button">Feedback Requests for Me</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" id="requests-sent-tab" data-bs-toggle="tab" data-bs-target="#requests-sent" type="button">Feedback Requests I've Sent</button></li>
    </ul>

    <div class="tab-content border border-top-0 p-3 bg-white">
        <!-- ===== TAB 1: Requests for Me ===== -->
        <div class="tab-pane fade show active" id="requests-for-me" role="tabpanel">
            <asp:Repeater ID="rptRequestsForMe" runat="server" OnItemCommand="rptRequestsForMe_ItemCommand">
                <HeaderTemplate><table class="table table-striped"><thead><tr><th>Feedback For</th><th>Requested By</th><th>Status</th><th>Actions</th></tr></thead><tbody></HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("ForEmployeeName") %></td>
                        <td><%# Eval("RequestedByName") %></td>
                        <td><span class="badge <%# If(Eval("Status").ToString() = "Pending", "bg-warning", "bg-success") %>"><%# Eval("Status") %></span></td>
                        <td>
                            <asp:Button ID="btnProvideFeedback" runat="server" Text="Provide Feedback" CommandName="ProvideFeedback" CommandArgument='<%# Eval("PeerFeedbackID") %>' CssClass="btn btn-sm btn-primary" Enabled='<%# Eval("Status").ToString() = "Pending" %>' />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoRequestsForMe" runat="server" Text="You have no pending feedback requests." Visible="false" CssClass="text-muted"></asp:Label>
        </div>

        <!-- ===== TAB 2: Requests I've Sent ===== -->
        <div class="tab-pane fade" id="requests-sent" role="tabpanel">
            <div class="text-end mb-3">
                <asp:Button ID="btnShowRequestModal" runat="server" Text="Request New Feedback" CssClass="btn btn-info" OnClick="btnShowRequestModal_Click" />
            </div>
            <asp:Repeater ID="rptRequestsSent" runat="server">
                <HeaderTemplate><table class="table table-striped"><thead><tr><th>Feedback For</th><th>Feedback From</th><th>Status</th><th>Requested On</th></tr></thead><tbody></HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("ForEmployeeName") %></td>
                        <td><%# Eval("FromEmployeeName") %></td>
                        <td><span class="badge <%# If(Eval("Status").ToString() = "Pending", "bg-warning", "bg-success") %>"><%# Eval("Status") %></span></td>
                        <td><%# Eval("RequestedOn", "{0:dd-MMM-yyyy}") %></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
             <asp:Label ID="lblNoRequestsSent" runat="server" Text="You have not sent any feedback requests." Visible="false" CssClass="text-muted"></asp:Label>
        </div>
    </div>

    <!-- ===== "Request New Feedback" MODAL ===== -->
    <div class="modal fade" id="requestModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Request Peer Feedback</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Get feedback FOR which employee?</label>
                        <asp:DropDownList ID="ddlForEmployee" runat="server" CssClass="form-select" DataTextField="Name" DataValueField="UserID"></asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Request feedback FROM which employee(s)?</label>
                        <asp:CheckBoxList ID="cblFromEmployees" runat="server" CssClass="form-control" DataTextField="Name" DataValueField="UserID"></asp:CheckBoxList>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSendRequests" runat="server" Text="Send Requests" CssClass="btn btn-primary" OnClick="btnSendRequests_Click" />
                </div>
            </div>
        </div>
    </div>

     <!-- ===== "Provide Feedback" MODAL ===== -->
    <div class="modal fade" id="provideFeedbackModal" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Provide Feedback for <asp:Label ID="lblProvideFeedbackFor" runat="server"></asp:Label></h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnPeerFeedbackID" runat="server" />
                    <p>Please provide your honest and constructive feedback. This will be shared confidentially with the manager.</p>
                    <div class="mb-3">
                        <asp:TextBox ID="txtFeedbackComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSubmitFeedback" runat="server" Text="Submit Feedback" CssClass="btn btn-primary" OnClick="btnSubmitFeedback_Click" />
                </div>
            </div>
        </div>
    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>