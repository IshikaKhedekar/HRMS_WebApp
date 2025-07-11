<%@ Page Title="Performance" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Performance.aspx.vb" Inherits="HRMS_WebApp.Performance" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .section-card {
            background-color: #fff;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }
        .section-title {
            font-size: 1.25rem;
            font-weight: 600;
            color: #0d6efd;
            padding: 15px 20px;
            border-bottom: 1px solid #dee2e6;
        }
        .section-body {
            padding: 20px;
        }
        .form-label {
            font-weight: 600;
        }
        .review-status {
            font-size: 1.1rem;
            font-weight: bold;
        }
        .status-pending { color: #ffc107; } /* Yellow */
        .status-completed { color: #198754; } /* Green */
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #6f42c1; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Performance Management</h2>
        <p>Complete your self-appraisal and manage team reviews.</p>
    </div>

    <!-- ===== "MY PERFORMANCE REVIEW" SECTION (Sabke Liye) ===== -->
    <asp:Panel ID="pnlMyReview" runat="server">
        <div class="section-card">
            <div class="section-title">
                My Performance Review for: <asp:Label ID="lblCycleName" runat="server" Text="..."></asp:Label>
            </div>
            <div class="section-body">
                <div class="mb-3">
                    <span class="form-label">Current Status:</span>
                    <asp:Label ID="lblReviewStatus" runat="server" CssClass="review-status"></asp:Label>
                </div>
                <hr />
                <div class="form-group mb-3">
                    <label for="<%= txtEmployeeComments.ClientID %>" class="form-label">Your Self-Appraisal (Achievements, Challenges, Goals):</label>
                    <asp:TextBox ID="txtEmployeeComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"></asp:TextBox>
                </div>
                <div class="form-group mb-3">
                    <label class="form-label">Manager's Feedback & Comments:</label>
                    <asp:TextBox ID="txtManagerComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8" ReadOnly="true" BackColor="#e9ecef"></asp:TextBox>
                </div>
                <div class="form-group mb-4">
                     <label class="form-label">Overall Rating by Manager:</label>
                     <asp:Label ID="lblManagerRating" runat="server" Text="Not yet rated" CssClass="h4"></asp:Label>
                </div>
                <asp:HiddenField ID="hdnReviewID" runat="server" />
                <asp:Button ID="btnSubmitToManager" runat="server" Text="Submit to Manager" CssClass="btn btn-primary" OnClick="btnSubmitToManager_Click" />
                <asp:Label ID="lblMessage" runat="server" CssClass="ms-3"></asp:Label>
            </div>
        </div>
    </asp:Panel>

    <!-- ===== "TEAM PERFORMANCE REVIEW" SECTION (Sirf Manager/HR ke liye) ===== -->
    <asp:Panel ID="pnlTeamView" runat="server" Visible="false" CssClass="mt-5">
        <div class="section-card">
            <div class="section-title">
                Team Performance Reviews
            </div>
            <div class="section-body">
                <asp:Repeater ID="rptTeamReviews" runat="server" OnItemCommand="rptTeamReviews_ItemCommand">
                    <HeaderTemplate>
                        <table class="table table-striped">
                            <thead>
                                <tr>
                                    <th>Employee Name</th>
                                    <th>Review Status</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("EmployeeName") %></td>
                            <td><span class='<%# GetStatusCssClass(Eval("Status")) %>'><%# Eval("Status") %></span></td>
                            <td>
                                <asp:Button ID="btnStartReview" runat="server" Text="Start/View Review" 
                                    CommandName="StartReview"
                                    CommandArgument='<%# Eval("ReviewID") %>' 
                                    Enabled='<%# Eval("Status").ToString() = "Pending Manager Review" OrElse Eval("Status").ToString() = "Completed" %>'
                                    CssClass="btn btn-sm btn-info" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Label ID="lblNoTeamReviews" runat="server" Text="No team reviews found for the active cycle." Visible="false" CssClass="text-muted"></asp:Label>
            </div>
        </div>
    </asp:Panel>

</asp:Content>