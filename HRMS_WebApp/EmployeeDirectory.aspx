<%@ Page Title="Employee Directory" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="EmployeeDirectory.aspx.vb" Inherits="HRMS_WebApp.EmployeeDirectory" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-header {
            background-color: #f2f2f2;
            padding: 15px;
            border-bottom: 1px solid #ddd;
            margin-bottom: 20px;
            font-size: 1.2em;
            font-weight: bold;
            color: #333;
        }
        .employee-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        .employee-table th, .employee-table td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
            vertical-align: middle;
        }
        .employee-table th {
            background-color: #17a2b8; /* Teal theme for directory */
            color: white;
        }
        .employee-photo-thumb {
            width: 50px;
            height: 50px;
            border-radius: 50%;
            object-fit: cover;
        }
        .action-buttons button {
            margin-right: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #17a2b8; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Employee Directory</h2>
        <p>Browse and search for employees within the organization.</p>
    </div>

    <!-- Search and Filter Section -->
    <div class="mb-4 p-3 border rounded bg-light">
        <div class="row g-3 align-items-end">
            <div class="col-md-8">
                <label for="<%= txtSearch.ClientID %>" class="form-label">Search by Name or Department:</label>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="e.g., Rohan Sharma, IT Department"></asp:TextBox>
            </div>
            <div class="col-md-2">
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary w-100" OnClick="btnSearch_Click" />
            </div>
            <div class="col-md-2">
                <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-outline-secondary w-100" OnClick="btnClearSearch_Click" />
            </div>
        </div>
    </div>

    <!-- Employee List Section -->
    <div class="section-header">Company Employees</div>
    <asp:Repeater ID="rptEmployees" runat="server" OnItemDataBound="rptEmployees_ItemDataBound">
        <HeaderTemplate>
            <table class="employee-table table-striped">
                <thead>
                    <tr>
                        <th>Photo</th>
                        <th>Name</th>
                        <th>Designation</th>
                        <th>Department</th>
                        <th>Official Email</th>
                        <th runat="server" id="thMobileHeader" visible="false">Mobile</th>
                        <th runat="server" id="thActionsHeader" visible="false">Actions</th>
                    </tr>
                </thead>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td><img src='Images/default-user.png' class="employee-photo-thumb" alt="Photo" /></td>
                <td><%# Eval("Name") %></td>
                <td><%# Eval("Designation") %></td>
                <td><%# Eval("Department") %></td>
                <td><a href='mailto:<%# Eval("Email") %>'><%# Eval("Email") %></a></td>
                <td runat="server" id="tdMobile" visible="false"><%# Eval("Mobile") %></td>
                <td runat="server" id="tdActions" visible="false" class="action-buttons">
                    <asp:Button ID="btnEdit" runat="server" Text="Edit" CommandArgument='<%# Eval("UserID") %>' OnClick="btnEdit_Click" CssClass="btn btn-sm btn-info" />
                    <%-- Note: Delete button can be risky. We can add it later if needed. --%>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:Label ID="lblNoEmployees" runat="server" Text="No employees found." Visible="false" CssClass="text-muted"></asp:Label>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>