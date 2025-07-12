<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="AttendanceApprovals.aspx.vb" Inherits="HRMS_WebApp.AttendanceApprovals" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .history-table th { background-color: #ffc107; color: #212529; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #ffc107; color: #212529; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Team Attendance Approvals</h2>
        <p>Review and approve/reject attendance regularization requests from your team.</p>
    </div>

    <div class="card">
        <div class="card-header">
            <h4>Pending Requests</h4>
        </div>
        <div class="card-body">
            <asp:Repeater ID="rptApprovalRequests" runat="server" OnItemCommand="rptApprovalRequests_ItemCommand">
                <HeaderTemplate><table class="table table-striped history-table"><thead><tr><th>Employee</th><th>Date to Correct</th><th>Requested Timings</th><th>Reason</th><th>Actions</th></tr></thead><tbody></HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("EmployeeName") %></td>
                        <td><%# Eval("DateToCorrect", "{0:dd-MMM-yyyy}") %></td>
                        <td>
                            In: <%# FormatTime(Eval("NewPunchIn")) %> <br/>
                            Out: <%# FormatTime(Eval("NewPunchOut")) %>
                        </td>
                        <td><%# Eval("Reason") %></td>
                        <td>
                            <asp:Button ID="btnApprove" runat="server" Text="Approve" CommandName="Approve" CommandArgument='<%# Eval("RequestID") %>' CssClass="btn btn-sm btn-success" />
                            <asp:Button ID="btnReject" runat="server" Text="Reject" CommandName="Reject" CommandArgument='<%# Eval("RequestID") %>' CssClass="btn btn-sm btn-danger ms-1" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
            <asp:Label ID="lblNoRequests" runat="server" Text="No pending requests found for your team." Visible="false" CssClass="text-muted"></asp:Label>
        </div>
    </div>
</asp:Content>