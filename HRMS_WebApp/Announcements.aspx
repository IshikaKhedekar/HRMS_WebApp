<%@ Page Title="Announcements" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Announcements.aspx.vb" Inherits="HRMS_WebApp.Announcements" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .announcement-card {
            background-color: #fff;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }
        .announcement-title {
            font-size: 1.25rem;
            font-weight: 600;
            color: #007bff;
        }
        .announcement-body {
            padding: 15px;
        }
        .announcement-footer {
            background-color: #f8f9fa;
            font-size: 0.8rem;
            color: #6c757d;
            padding: 10px 15px;
            border-top: 1px solid #dee2e6;
        }
        .form-section {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #fd7e14; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Company Announcements</h2>
        <p>Stay updated with the latest news and announcements.</p>
    </div>

    <!-- ===== YE SECTION SIRF HR/ADMIN KE LIYE HAI ===== -->
    <asp:Panel ID="pnlCreateAnnouncement" runat="server" Visible="false" CssClass="form-section">
        <h4>Create New Announcement</h4>
        <div class="mb-3">
            <label for="<%= txtTitle.ClientID %>" class="form-label">Title</label>
            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" placeholder="Enter a catchy title"></asp:TextBox>
        </div>
        <div class="mb-3">
            <label for="<%= txtDetails.ClientID %>" class="form-label">Details</label>
            <asp:TextBox ID="txtDetails" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" placeholder="Enter the full announcement details here..."></asp:TextBox>
        </div>
        <asp:Button ID="btnPublish" runat="server" Text="Publish Announcement" CssClass="btn btn-primary" OnClick="btnPublish_Click" />
        <asp:Label ID="lblMessage" runat="server" Text="" CssClass="ms-3"></asp:Label>
    </asp:Panel>

    <!-- ===== YE SECTION SABKE LIYE HAI ===== -->
    <h3 class="mt-4">Recent Announcements</h3>
    <hr />
    <asp:Repeater ID="rptAnnouncements" runat="server">
        <ItemTemplate>
            <div class="announcement-card">
                <div class="announcement-body">
                    <h5 class="announcement-title"><%# Eval("Title") %></h5>
                    <p><%# Eval("Details") %></p>
                </div>
                <div class="announcement-footer">
                    Posted by: <%# Eval("PostedByUserName") %> on <%# Eval("PostedOn", "{0:dd-MMM-yyyy hh:mm tt}") %>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Label ID="lblNoAnnouncements" runat="server" Text="No announcements found." Visible="false" CssClass="text-muted"></asp:Label>

</asp:Content>