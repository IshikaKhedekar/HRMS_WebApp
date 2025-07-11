<%@ Page Title="Company Policies" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Policies.aspx.vb" Inherits="HRMS_WebApp.Policies" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-header { background-color: #f8f9fa; padding: 15px; border-bottom: 1px solid #dee2e6; margin-bottom: 20px; font-size: 1.2rem; font-weight: 600; }
        .policy-card { background-color: #fff; border: 1px solid #dee2e6; border-radius: .375rem; margin-bottom: 1.5rem; }
        .policy-card-header { padding: 1rem 1.25rem; background-color: #f8f9fa; border-bottom: 1px solid #dee2e6; font-size: 1.1rem; font-weight: 600; display: flex; justify-content: space-between; align-items: center; }
        .policy-card-body { padding: 1.25rem; }
        .policy-title { font-weight: bold; color: #007bff; margin-bottom: 0.5rem; }
        .policy-meta { font-size: 0.85rem; color: #6c757d; margin-bottom: 1rem; }
        .policy-content { white-space: pre-wrap; } /* Naye lines ko dikhane ke liye */
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #6c757d; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Company Policies & Guidelines</h2>
        <p>Your guide to the rules, regulations, and benefits at MyCompany.</p>
    </div>

    <!-- Filter by Category -->
    <div class="mb-4 d-flex align-items-center">
        <label for="<%= ddlPolicyCategory.ClientID %>" class="form-label me-3 mb-0">Filter by Category:</label>
        <asp:DropDownList ID="ddlPolicyCategory" runat="server" CssClass="form-select w-auto" AutoPostBack="true" OnSelectedIndexChanged="ddlPolicyCategory_SelectedIndexChanged">
            <asp:ListItem Value="0" Text="All Categories"></asp:ListItem>
            <asp:ListItem Value="General" Text="General Rules"></asp:ListItem>
            <asp:ListItem Value="Leave" Text="Leave Policy"></asp:ListItem>
            <asp:ListItem Value="Payment" Text="Payment Policy"></asp:ListItem>
            <asp:ListItem Value="IT" Text="IT Policy"></asp:ListItem>
        </asp:DropDownList>
        
        <!-- HR/Admin button to add new policy -->
        <asp:Button ID="btnAddPolicy" runat="server" Text="Add New Policy" CssClass="btn btn-primary ms-auto" Visible="false" OnClick="btnAddPolicy_Click" />
    </div>

    <!-- Policies List -->
    <asp:Repeater ID="rptPolicies" runat="server" OnItemCommand="rptPolicies_ItemCommand"> <%-- OnItemCommand added --%>
        <ItemTemplate>
            <div class="policy-card">
                <div class="policy-card-header">
                    <div>
                        <h5 class="policy-title"><%# Eval("Title") %></h5>
                        <div class="policy-meta">Category: <%# Eval("Category") %> | Last Updated: <%# Eval("LastUpdatedOn", "{0:dd-MMM-yyyy hh:mm tt}") %> by <%# Eval("LastUpdatedByUserName") %></div>
                    </div>
                    <div class="actions">
                        <asp:Button ID="btnEditPolicy" runat="server" Text="Edit" CommandName="EditPolicy" CommandArgument='<%# Eval("PolicyID") %>' CssClass="btn btn-sm btn-info" Visible="false" /> <%-- OnClick removed, CommandName added --%>
                        <asp:Button ID="btnDeletePolicy" runat="server" Text="Delete" CommandName="DeletePolicy" CommandArgument='<%# Eval("PolicyID") %>' OnClientClick="return confirm('Are you sure you want to delete this policy?');" CssClass="btn btn-sm btn-danger ms-2" Visible="false" /> <%-- OnClick removed, CommandName added --%>
                    </div>
                </div>
                <div class="policy-card-body">
                    <p class="policy-content"><%# Eval("Content") %></p>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Label ID="lblNoPolicies" runat="server" Text="No policies found in this category." Visible="false" CssClass="text-muted p-3"></asp:Label>

    <!-- ===== POLICY ADD/EDIT MODAL ===== -->
    <div class="modal fade" id="policyModal" tabindex="-1" aria-labelledby="policyModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="policyModalLabel">Add/Edit Policy</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnPolicyID" runat="server" Value="0" />
                    <div class="mb-3">
                        <label class="form-label">Title</label>
                        <asp:TextBox ID="txtPolicyTitle" runat="server" CssClass="form-control" Required="true"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Category</label>
                        <asp:DropDownList ID="ddlModalCategory" runat="server" CssClass="form-select">
                            <asp:ListItem Value="General">General Rules</asp:ListItem>
                            <asp:ListItem Value="Leave">Leave Policy</asp:ListItem>
                            <asp:ListItem Value="Payment">Payment Policy</asp:ListItem>
                            <asp:ListItem Value="IT">IT Policy</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Content</label>
                        <asp:TextBox ID="txtPolicyContent" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="10" Required="true"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Label ID="lblModalMessage" runat="server" CssClass="me-auto"></asp:Label> <%-- Naya Label --%>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSavePolicy" runat="server" Text="Save Policy" CssClass="btn btn-primary" OnClick="btnSavePolicy_Click" />
                </div>
            </div>
        </div>
    </div>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>