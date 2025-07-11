<%@ Page Title="Admin Panel" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Settings.aspx.vb" Inherits="HRMS_WebApp.Settings" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-header { background-color: #f2f2f2; padding: 15px; border-bottom: 1px solid #ddd; margin-bottom: 20px; font-size: 1.2em; font-weight: bold; color: #333; }
        .data-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
        .data-table th, .data-table td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        .data-table th { background-color: #dc3545; color: white; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #dc3545; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>System Administration</h2>
        <p>Manage users, roles, and permissions from this control center.</p>
    </div>

    <!-- Bootstrap Tabs -->
    <ul class="nav nav-tabs" id="adminTabs" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" id="users-tab" data-bs-toggle="tab" data-bs-target="#users" type="button" role="tab">User Management</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="roles-tab" data-bs-toggle="tab" data-bs-target="#roles" type="button" role="tab">Role Management</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="permissions-tab" data-bs-toggle="tab" data-bs-target="#permissions" type="button" role="tab">Permission Management</button>
        </li>
    </ul>

    <!-- Tab Content -->
    <div class="tab-content border border-top-0 p-3 bg-white">
        <!-- ===== USER MANAGEMENT TAB ===== -->
        <div class="tab-pane fade show active" id="users" role="tabpanel">
            <div class="section-header d-flex justify-content-between align-items-center">
                <span>All System Users</span>
                <asp:Button ID="btnAddUser" runat="server" Text="Add New User" CssClass="btn btn-primary" OnClick="btnAddUser_Click" />
            </div>
            <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
                <HeaderTemplate>
                    <table class="data-table table-striped">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Manager</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("Name") %></td>
                        <td><%# Eval("Email") %></td>
                        <td><%# Eval("RoleName") %></td>
                        <td><%# Eval("ManagerName") %></td>
                        <td>
                            <asp:Button ID="btnEditUser" runat="server" Text="Edit" 
                                CommandName="EditUser"
                                CommandArgument='<%# Eval("UserID") %>' 
                                CssClass="btn btn-sm btn-info" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoUsers" runat="server" Text="No users found." Visible="false" CssClass="text-muted"></asp:Label>
        </div>

        <!-- ===== ROLE MANAGEMENT TAB (Coming Soon) ===== -->
<div class="tab-pane fade" id="roles" role="tabpanel">
    <div class="row">
        <div class="col-md-7">
            <div class="section-header">Existing Roles</div>
            <asp:Repeater ID="rptRoles" runat="server" OnItemCommand="rptRoles_ItemCommand">
                <HeaderTemplate>
                    <table class="data-table table-striped">
                        <thead><tr><th>Role Name</th><th>Actions</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("RoleName") %></td>
                        <td>
                            <asp:Button ID="btnDeleteRole" runat="server" Text="Delete" 
                                CommandName="DeleteRole" CommandArgument='<%# Eval("RoleID") %>' 
                                OnClientClick="return confirm('Are you sure you want to delete this role?');" 
                                CssClass="btn btn-sm btn-danger" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody></table>
                </FooterTemplate>
            </asp:Repeater>
        </div>
        <div class="col-md-5">
            <div class="section-header">Add New Role</div>
            <div class="p-3 border bg-light">
                <div class="mb-3">
                    <label class="form-label">New Role Name:</label>
                    <asp:TextBox ID="txtNewRoleName" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <asp:Button ID="btnAddRole" runat="server" Text="Add Role" CssClass="btn btn-primary" OnClick="btnAddRole_Click" />
                <asp:Label ID="lblRoleMessage" runat="server" CssClass="ms-2"></asp:Label>
            </div>
        </div>
    </div>
</div>
        <!-- ===== PERMISSION MANAGEMENT TAB (Coming Soon) ===== -->
<div class="tab-pane fade" id="permissions" role="tabpanel">
    <div class="section-header">Role-Permission Matrix</div>
    <div class="table-responsive">
        <asp:Repeater ID="rptPermissions" runat="server" OnItemDataBound="rptPermissions_ItemDataBound">
            <HeaderTemplate>
                <table class="data-table table-bordered">
                    <thead>
                        <tr>
                            <th>Permission</th>
                            <!-- Role names as headers will be added dynamically from code-behind -->
                            <asp:PlaceHolder ID="phRoleHeaders" runat="server"></asp:PlaceHolder>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Eval("PermissionName") %></td>
<%-- Find this Repeater in your Settings.aspx file --%>
<asp:Repeater ID="rptPermissionRoles" runat="server">
    <ItemTemplate>
        <td class="text-center">
            <%-- We will use HiddenFields to store the IDs --%>
            <asp:HiddenField ID="hdnRoleID" runat="server" Value='<%# Eval("RoleID") %>' />
            <asp:HiddenField ID="hdnPermissionID" runat="server" Value='<%# CType(Container.Parent.Parent, RepeaterItem).DataItem("PermissionID") %>' />
            
            <asp:CheckBox ID="chkPermission" runat="server" Checked='<%# Eval("HasPermission") %>' />
        </td>
    </ItemTemplate>
</asp:Repeater>                </tr>
            </ItemTemplate>
            <FooterTemplate>
                    </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>
    <div class="text-end mt-3">
        <asp:Button ID="btnSavePermissions" runat="server" Text="Save All Permissions" CssClass="btn btn-success" OnClick="btnSavePermissions_Click" />
        <asp:Label ID="lblPermissionMessage" runat="server" CssClass="ms-2"></asp:Label>
    </div>
</div>    </div>

    <!-- ===== USER ADD/EDIT MODAL ===== -->
    <div class="modal fade" id="userModal" tabindex="-1" aria-labelledby="userModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="userModalLabel">Add/Edit User</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnUserID" runat="server" Value="0" />
                    <div class="mb-3">
                        <label class="form-label">Full Name</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" Required="true"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="form-control" Required="true"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" placeholder="Leave empty to not change"></asp:TextBox>
                    </div>
                     <div class="mb-3">
                        <label class="form-label">Role</label>
                        <asp:DropDownList ID="ddlRoles" runat="server" CssClass="form-select" DataTextField="RoleName" DataValueField="RoleID"></asp:DropDownList>
                    </div>
                     <div class="mb-3">
                        <label class="form-label">Reporting Manager</label>
                        <asp:DropDownList ID="ddlManagers" runat="server" CssClass="form-select" DataTextField="Name" DataValueField="UserID"></asp:DropDownList>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveUser" runat="server" Text="Save User" CssClass="btn btn-primary" OnClick="btnSaveUser_Click" />
                </div>
            </div>
        </div>
    </div>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>