<%@ Page Title="Review Detail" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="ReviewDetail.aspx.vb" Inherits="HRMS_WebApp.ReviewDetail" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .review-panel {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 20px;
            border-radius: 8px;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Performance Review for <asp:Label ID="lblEmployeeName" runat="server"></asp:Label></h2>
        <p>Review employee's self-appraisal and provide your feedback and rating.</p>
    </div>

    <div class="row">
        <!-- Employee's Comments (Read-Only) -->
        <div class="col-md-6">
            <h4>Employee's Self-Appraisal</h4>
            <div class="review-panel">
                <asp:Literal ID="litEmployeeComments" runat="server"></asp:Literal>
            </div>
        </div>

        <!-- Manager's Feedback Form -->
        <div class="col-md-6">
            <h4>Your Feedback & Rating</h4>
            <div class="form-group mb-3">
                <label class="form-label">Your Comments:</label>
                <asp:TextBox ID="txtManagerComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"></asp:TextBox>
            </div>
            <div class="form-group mb-3">
                <label class="form-label">Overall Rating (1-5):</label>
                <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select">
                    <asp:ListItem Text="-- Select Rating --" Value="0"></asp:ListItem>
                    <asp:ListItem Text="1 - Needs Improvement" Value="1"></asp:ListItem>
                    <asp:ListItem Text="2 - Below Expectations" Value="2"></asp:ListItem>
                    <asp:ListItem Text="3 - Meets Expectations" Value="3"></asp:ListItem>
                    <asp:ListItem Text="4 - Exceeds Expectations" Value="4"></asp:ListItem>
                    <asp:ListItem Text="5 - Outstanding" Value="5"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <asp:HiddenField ID="hdnReviewID" runat="server" />
            <asp:Button ID="btnFinalizeReview" runat="server" Text="Finalize & Submit Review" CssClass="btn btn-success" OnClick="btnFinalizeReview_Click" />
            <asp:Label ID="lblMessage" runat="server" CssClass="ms-3"></asp:Label>
        </div>
    </div>
</asp:Content>