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
        
        /* Styles for the new KPI sections */
        .kpi-item {
            border-bottom: 1px solid #e9ecef;
            padding-bottom: 1rem;
            margin-bottom: 1rem;
        }
        .kpi-item:last-child {
            border-bottom: none;
            margin-bottom: 0;
        }
        .kpi-title {
            font-weight: 600;
            color: #343a40;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #6f42c1; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Performance Management</h2>
        <p>Complete your self-appraisal and manage team reviews.</p>
    </div>

    <!-- ===== "MY PERFORMANCE REVIEW" SECTION (Sabke Liye) - UPDATED ===== -->
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
                    <label for="<%= txtEmployeeComments.ClientID %>" class="form-label">Your Overall Self-Appraisal (Achievements, Challenges, Goals):</label>
                    <asp:TextBox ID="txtEmployeeComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5"></asp:TextBox>
                </div>

                <!-- === NEW SECTION FOR EMPLOYEE'S KPI SELF-REVIEW === -->
                <hr />
                <div class="form-group mb-3">
                    <label class="form-label h5">Your KPI-wise Self-Appraisal:</label>
                    <div class="review-panel p-3 bg-light">
                        <asp:Repeater ID="rptMyKPIs" runat="server" OnItemDataBound="rptMyKPIs_ItemDataBound">
                            <ItemTemplate>
                                <div class="kpi-item">
                                    <asp:HiddenField ID="hdnMyEmployeeKpiID" runat="server" Value='<%# Eval("EmployeeKpiID") %>' />
                                    <div class="kpi-title mb-2"><%# Eval("KpiTitle") %></div>
                                    <asp:TextBox ID="txtMyKpiComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" placeholder="Your comments for this KPI..."></asp:TextBox>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Label ID="lblNoMyKPIs" runat="server" Text="No KPIs have been assigned for this review cycle." Visible="false" CssClass="text-muted"></asp:Label>
                    </div>
                </div>
                <!-- === END OF NEW SECTION === -->

                <div class="form-group mb-3 mt-4">
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
                                    Enabled='<%# Eval("Status").ToString() = "Pending Manager Review" OrElse Eval("Status").ToString() = "Completed" OrElse Eval("Status").ToString() = "Finalized" OrElse Eval("Status").ToString() = "Published" %>'
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
    
    <!-- ===== "MANAGE PERFORMANCE CYCLES" SECTION (Sirf HR/Manager ke liye) ===== -->
    <asp:Panel ID="pnlCycleManagement" runat="server" Visible="false" CssClass="mt-5">
        <div class="section-card">
            <div class="section-title">Manage Performance Cycles</div>
            <div class="section-body">
                <div class="row">
                    <!-- Left Side: Existing Cycles -->
                    <div class="col-md-7">
                        <asp:Repeater ID="rptCycles" runat="server" OnItemCommand="rptCycles_ItemCommand">
                            <HeaderTemplate>
                                <table class="table table-sm table-striped">
                                    <thead><tr><th>Cycle Name</th><th>Dates</th><th>Status</th><th>Actions</th></tr></thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("CycleName") %></td>
                                    <td><%# Eval("StartDate", "{0:dd-MMM}") %> to <%# Eval("EndDate", "{0:dd-MMM-yyyy}") %></td>
                                    <td><span class='badge <%# If(Eval("Status").ToString() = "Active", "bg-success", "bg-secondary") %>'><%# Eval("Status") %></span></td>
                                    <td>
                                        <asp:Button ID="btnActivateCycle" runat="server" Text="Activate" 
                                            CommandName="ActivateCycle" CommandArgument='<%# Eval("CycleID") %>' 
                                            Enabled='<%# Eval("Status").ToString() <> "Active" %>'
                                            OnClientClick="return confirm('Are you sure you want to activate this cycle? This will generate review records for all employees.');" 
                                            CssClass="btn btn-sm btn-success" />
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate></tbody></table></FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <!-- Right Side: Add New Cycle -->
                    <div class="col-md-5">
                        <div class="p-3 border bg-light rounded">
                            <h5>Create New Cycle</h5>
                            <div class="mb-2">
                                <label class="form-label">Cycle Name:</label>
                                <asp:TextBox ID="txtCycleName" runat="server" CssClass="form-control" placeholder="e.g., Q3 2025 Review"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label">Start Date:</label>
                                <asp:TextBox ID="txtCycleStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label">End Date:</label>
                                <asp:TextBox ID="txtCycleEndDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                            </div>
                            <asp:Button ID="btnAddCycle" runat="server" Text="Create Cycle" CssClass="btn btn-primary w-100" OnClick="btnAddCycle_Click" />
                            <asp:Label ID="lblCycleMessage" runat="server" CssClass="d-block mt-2"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- ===== "ASSIGN KPIs TO EMPLOYEES" SECTION (Sirf HR/Manager ke liye) ===== -->
    <asp:Panel ID="pnlAssignKPIs" runat="server" Visible="false" CssClass="mt-5">
        <div class="section-card">
            <div class="section-title">
                Assign KPIs for Active Cycle: <asp:Label ID="lblAssignKpiCycleName" runat="server" Text="(No Active Cycle)"></asp:Label>
            </div>
            <div class="section-body">
                <div class="row">
                    <!-- Column 1: Select Employee -->
                    <div class="col-md-4">
                        <label class="form-label">Select Employee to Assign KPIs:</label>
                        <asp:DropDownList ID="ddlEmployeesForKPI" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEmployeesForKPI_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                    <!-- Column 2: Available Master KPIs -->
                    <div class="col-md-4">
                         <asp:Panel ID="pnlAvailableKPIs" runat="server" Visible="false">
                            <label class="form-label">Available Master KPIs:</label>
                            <div class="border p-2" style="height: 200px; overflow-y: auto;">
                                <asp:CheckBoxList ID="cblMasterKPIs" runat="server" DataTextField="KpiTitle" DataValueField="KpiID">
                                </asp:CheckBoxList>
                            </div>
                            <asp:Button ID="btnAssignSelectedKPIs" runat="server" Text="Assign Selected KPIs →" CssClass="btn btn-primary mt-2" OnClick="btnAssignSelectedKPIs_Click" />
                        </asp:Panel>
                    </div>
                     <!-- Column 3: KPIs Assigned to this Employee -->
                    <div class="col-md-4">
                        <asp:Panel ID="pnlAssignedKPIs" runat="server" Visible="false">
                            <label class="form-label">KPIs Currently Assigned:</label>
                             <div class="border p-2 bg-light" style="height: 200px; overflow-y: auto;">
                                <asp:Repeater ID="rptAssignedKPIs" runat="server" OnItemCommand="rptAssignedKPIs_ItemCommand">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between align-items-center mb-1">
                                            <span><%# Eval("KpiTitle") %></span>
                                            <asp:Button ID="btnRemoveKPI" runat="server" Text="x" CommandName="RemoveKPI" CommandArgument='<%# Eval("EmployeeKpiID") %>' CssClass="btn btn-sm btn-danger py-0 px-1" />
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

</asp:Content>