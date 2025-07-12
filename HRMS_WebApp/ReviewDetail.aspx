<%@ Page Title="Review Detail" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="ReviewDetail.aspx.vb" Inherits="HRMS_WebApp.ReviewDetail" %>



<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/css/all.min.css" />
    <style>
        .review-panel { background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 20px; border-radius: 8px; }
        .kpi-item { border-bottom: 1px solid #e0e0e0; padding-bottom: 15px; margin-bottom: 15px; }
        .kpi-item:last-child { border-bottom: none; margin-bottom: 0; }
        .kpi-title { font-weight: 600; color: #333; }
        .employee-kpi-comment { background-color: #e9ecef; border-left: 4px solid #0d6efd; padding: 10px; border-radius: 4px; font-style: italic; }
        
        .star-rating { font-size: 1.8rem; color: #ccc; display: inline-block; }
        .star-rating .star { cursor: pointer; transition: color 0.2s; }
        .star-rating .star:hover, .star-rating .star.hover { color: #ffc107; }
        .star-rating .star.selected { color: #ffc107; }
        .star-rating.disabled .star { cursor: not-allowed; }

        .peer-feedback-item { border-bottom: 1px dashed #ccc; padding: 10px 0; }
        .peer-feedback-item:last-child { border-bottom: none; }
        .peer-feedback-from { font-weight: bold; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hdnReviewID" runat="server" />

    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Performance Review for <asp:Label ID="lblEmployeeName" runat="server"></asp:Label></h2>
        <p>Review employee's self-appraisal, provide feedback, and finalize the review.</p>
    </div>

    <div class="row">
        <!-- LEFT COLUMN: Employee's & Peer's Feedback -->
        <div class="col-lg-5">
            <!-- Employee's Overall Comments -->
            <h4 class="mb-3">Employee's Self-Appraisal</h4>
            <div class="review-panel mb-4">
                <h6>Overall Comments:</h6>
                <asp:Literal ID="litEmployeeComments" runat="server">No comments provided.</asp:Literal>
            </div>

            <!-- Peer Feedback (360-Degree) -->
            <asp:Panel ID="pnlPeerFeedback" runat="server" Visible="false">
                 <h4 class="mb-3">Peer Feedback (360°)</h4>
                 <div class="review-panel">
                     <asp:Repeater ID="rptPeerFeedback" runat="server">
                         <ItemTemplate>
                             <div class="peer-feedback-item">
                                 <div class="peer-feedback-from">Feedback from: <%# Eval("FromEmployeeName") %></div>
                                 <div class="peer-feedback-content">"<%# Eval("FeedbackComments") %>"</div>
                             </div>
                         </ItemTemplate>
                     </asp:Repeater>
                     <asp:Label ID="lblNoPeerFeedback" runat="server" CssClass="text-muted" Text="No peer feedback submitted for this review." Visible="false"></asp:Label>
                 </div>
            </asp:Panel>
        </div>

        <!-- RIGHT COLUMN: Manager's Evaluation -->
        <div class="col-lg-7">
            <h4 class="mb-3">Manager's Evaluation</h4>

            <!-- KPI-wise Evaluation -->
            <div class="review-panel mb-4">
                <h5 class="mb-3">KPI-wise Evaluation</h5>
                <asp:Repeater ID="rptEmployeeKPIs" runat="server" OnItemDataBound="rptEmployeeKPIs_ItemDataBound">
                    <ItemTemplate>
                        <div class="kpi-item">
                            <asp:HiddenField ID="hdnEmployeeKpiID" runat="server" Value='<%# Eval("EmployeeKpiID") %>' />
                            <div class="kpi-title mb-2"><%# Eval("KpiTitle") %></div>
                            
                            <!-- Employee's comment on this KPI -->
                            <div class="employee-kpi-comment mb-2">
                                <strong>Employee's Comment:</strong>
                                <em><%# If(Eval("EmployeeComments") IsNot DBNull.Value AndAlso Not String.IsNullOrEmpty(Eval("EmployeeComments").ToString()), Eval("EmployeeComments"), "No specific comment.") %></em>
                            </div>
                            
                            <!-- Manager's rating for this KPI -->
                            <div class="row">
                                <div class="col-md-8">
                                    <asp:TextBox ID="txtKpiManagerComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" placeholder="Manager's comments for this KPI..."></asp:TextBox>
                                </div>
                                <div class="col-md-4">
                                    <div class="star-rating" id="divKpiStars" runat="server">
                                        <asp:HiddenField ID="hdnKpiRatingValue" runat="server" Value="0" />
                                        <span class="star fas fa-star" data-value="1"></span>
                                        <span class="star fas fa-star" data-value="2"></span>
                                        <span class="star fas fa-star" data-value="3"></span>
                                        <span class="star fas fa-star" data-value="4"></span>
                                        <span class="star fas fa-star" data-value="5"></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Label ID="lblNoKPIsAssigned" runat="server" Text="No KPIs assigned." Visible="false"></asp:Label>
            </div>
            <!-- ===== PEER FEEDBACK DISPLAY SECTION (NEW) ===== -->
<div class="mt-4">
    <h3 class="mb-3">
        <a class="text-decoration-none" data-bs-toggle="collapse" href="#peerFeedbackCollapse" role="button" aria-expanded="false" aria-controls="peerFeedbackCollapse">
            Peer Feedback Summary <i class="fas fa-chevron-down fa-xs"></i>
        </a>
    </h3>
    <div class="collapse" id="peerFeedbackCollapse">
        <div class="review-panel">
            <asp:Repeater ID="Repeater1" runat="server">
                <ItemTemplate>
                    <div class="kpi-review-item">
                        <div class="kpi-title mb-2">Feedback from: <%# Eval("FromEmployeeName") %></div>
                        <div class="employee-comment-box">
                            <%# Eval("FeedbackComments") %>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <asp:Label ID="Label1" runat="server" Text="No peer feedback has been provided for this review cycle." Visible="false" CssClass="text-muted"></asp:Label>
        </div>
    </div>
</div>
<!-- ===== END OF PEER FEEDBACK SECTION ===== -->
            
            <!-- Overall Feedback & Finalization -->
            <div class="review-panel">
                 <h5 class="mb-3">Overall Feedback & Finalization</h5>
                 <div class="form-group mb-3">
                    <label class="form-label">Your Overall Comments:</label>
                    <asp:TextBox ID="txtManagerComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"></asp:TextBox>
                 </div>
                 <div class="form-group mb-4">
                    <label class="form-label">Overall Final Rating (1-5):</label>
                    <div class="star-rating" id="divOverallStars" runat="server">
                        <asp:HiddenField ID="hdnOverallRatingValue" runat="server" Value="0" />
                        <span class="star fas fa-star" data-value="1"></span>
                        <span class="star fas fa-star" data-value="2"></span>
                        <span class="star fas fa-star" data-value="3"></span>
                        <span class="star fas fa-star" data-value="4"></span>
                        <span class="star fas fa-star" data-value="5"></span>
                    </div>
                </div>
                <hr />
                <div class="d-flex justify-content-end align-items-center">
                    <asp:Label ID="lblMessage" runat="server" CssClass="me-auto"></asp:Label>
                    <asp:Button ID="btnSaveDraft" runat="server" Text="Save as Draft" CssClass="btn btn-secondary me-2" OnClick="btnSaveDraft_Click" />
                    <asp:Button ID="btnFinalizeAndPublish" runat="server" Text="Finalize & Publish to Employee" CssClass="btn btn-success" OnClick="btnFinalizeAndPublish_Click" OnClientClick="return confirm('Are you sure you want to finalize and publish this review? After this, no more changes can be made.');" />
                </div>
            </div>
        </div>
    </div>
    
    <!-- JavaScript for Star Ratings (from previous step) -->
    <script type="text/javascript">
        function initializeStarRating(container) {
            if (!container) return;
            const stars = container.querySelectorAll('.star');
            const hiddenInput = container.querySelector('input[type=hidden]');
            let currentRating = parseInt(hiddenInput.value) || 0;
            function setStars(rating) {
                stars.forEach(star => {
                    star.classList.toggle('selected', star.dataset.value <= rating);
                });
            }
            setStars(currentRating);
            if (container.classList.contains('disabled')) {
                return;
            }
            stars.forEach(star => {
                star.addEventListener('mouseover', () => {
                    stars.forEach(s => s.classList.remove('hover'));
                    const rating = star.dataset.value;
                    for (let i = 0; i < rating; i++) {
                        stars[i].classList.add('hover');
                    }
                });
                star.addEventListener('mouseout', () => {
                    stars.forEach(s => s.classList.remove('hover'));
                });
                star.addEventListener('click', () => {
                    currentRating = star.dataset.value;
                    hiddenInput.value = currentRating;
                    setStars(currentRating);
                });
            });
            container.addEventListener('mouseout', () => {
                setStars(currentRating);
            });
        }
        document.addEventListener('DOMContentLoaded', function () {
            document.querySelectorAll('.star-rating').forEach(initializeStarRating);
        });
    </script>
</asp:Content>