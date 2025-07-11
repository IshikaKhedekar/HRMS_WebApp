<%@ Page Title="Recruitment" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Recruitment.aspx.vb" Inherits="HRMS_WebApp.Recruitment" %>

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
        .data-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        .data-table th, .data-table td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }
        .data-table th {
            background-color: #6f42c1; /* Purple theme for recruitment */
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #6f42c1; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Recruitment Management</h2>
        <p>Manage job openings and track candidates through the hiring process.</p>
    </div>

    <!-- Bootstrap Tabs -->
    <ul class="nav nav-tabs" id="recruitmentTabs" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" id="openings-tab" data-bs-toggle="tab" data-bs-target="#openings" type="button" role="tab">Job Openings</button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="candidates-tab" data-bs-toggle="tab" data-bs-target="#candidates" type="button" role="tab">Candidate Management</button>
        </li>
    </ul>

    <!-- Tab Content -->
    <div class="tab-content border border-top-0 p-3 bg-white">
        <!-- ===== JOB OPENINGS TAB ===== -->
        <div class="tab-pane fade show active" id="openings" role="tabpanel">
            <div class="section-header d-flex justify-content-between align-items-center">
                <span>Active Job Openings</span>
                <asp:Button ID="btnShowOpeningModal" runat="server" Text="Create New Opening" CssClass="btn btn-primary" OnClick="btnShowOpeningModal_Click" />
            </div>
            <asp:Repeater ID="rptJobOpenings" runat="server">
                <HeaderTemplate>
                    <table class="data-table table-striped">
                        <thead><tr><th>Job Title</th><th>Department</th><th>Posted On</th><th>Status</th><th>Actions</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("JobTitle") %></td>
                        <td><%# Eval("Department") %></td>
                        <td><%# Eval("PostedOn", "{0:dd-MMM-yyyy}") %></td>
                        <td><span class='badge <%# If(Eval("Status").ToString() = "Open", "bg-success", "bg-secondary") %>'><%# Eval("Status") %></span></td>
                        <td>
                            <asp:Button ID="btnViewCandidates" runat="server" Text="View Candidates" CommandArgument='<%# Eval("OpeningID") %>' OnClick="btnViewCandidates_Click" CssClass="btn btn-sm btn-info" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
        </div>

        <!-- ===== CANDIDATE MANAGEMENT TAB ===== -->
        <div class="tab-pane fade" id="candidates" role="tabpanel">
            <div class="section-header">All Candidates</div>
            <div class="mb-3">
                <label class="form-label">Filter by Job Opening:</label>
                <asp:DropDownList ID="ddlJobFilter" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlJobFilter_SelectedIndexChanged"></asp:DropDownList>
                <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filter" CssClass="btn btn-link" OnClick="btnClearFilter_Click" />
            </div>
             <asp:Repeater ID="rptCandidates" runat="server">
                <HeaderTemplate>
                    <table class="data-table table-striped">
                        <thead><tr><th>Candidate Name</th><th>Applied For</th><th>Applied On</th><th>Status</th><th>Actions</th></tr></thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td><%# Eval("FullName") %></td>
                        <td><%# Eval("JobTitle") %></td>
                        <td><%# Eval("AppliedOn", "{0:dd-MMM-yyyy}") %></td>
                        <td><%# Eval("Status") %></td>
                        <td>
                             <asp:Button ID="btnViewCandidateDetails" runat="server" Text="View/Update" CommandArgument='<%# Eval("CandidateID") %>' OnClick="btnViewCandidateDetails_Click" CssClass="btn btn-sm btn-info" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></table></FooterTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- ===== JOB OPENING ADD/EDIT MODAL ===== -->
    <div class="modal fade" id="openingModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Create Job Opening</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
                <div class="modal-body">
                    <div class="mb-3"><label class="form-label">Job Title</label><asp:TextBox ID="txtJobTitle" runat="server" CssClass="form-control"></asp:TextBox></div>
                    <div class="mb-3"><label class="form-label">Department</label><asp:TextBox ID="txtDepartment" runat="server" CssClass="form-control"></asp:TextBox></div>
                    <div class="mb-3"><label class="form-label">Job Description</label><asp:TextBox ID="txtJobDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6"></asp:TextBox></div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveOpening" runat="server" Text="Save Opening" CssClass="btn btn-primary" OnClick="btnSaveOpening_Click" />
                </div>
            </div>
        </div>
    </div>

     <!-- ===== CANDIDATE DETAILS MODAL ===== -->
    <div class="modal fade" id="candidateModal" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Candidate Details</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnCandidateID" runat="server" />
                    <p><strong>Name:</strong> <asp:Label ID="lblName" runat="server"></asp:Label></p>
                    <p><strong>Email:</strong> <asp:Label ID="lblEmail" runat="server"></asp:Label></p>
                    <p><strong>Phone:</strong> <asp:Label ID="lblPhone" runat="server"></asp:Label></p>
                    <p><strong>Resume:</strong> <asp:HyperLink ID="hlResume" runat="server" Target="_blank">Download Resume</asp:HyperLink></p>
                    <hr />
                    <div class="mb-3"><label class="form-label"><strong>Update Status:</strong></label>
                        <asp:DropDownList ID="ddlCandidateStatus" runat="server" CssClass="form-select">
                           <asp:ListItem>Applied</asp:ListItem>
                           <asp:ListItem>Shortlisted</asp:ListItem>
                           <asp:ListItem>Interview</asp:ListItem>
                           <asp:ListItem>Hired</asp:ListItem>
                           <asp:ListItem>Rejected</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3"><label class="form-label"><strong>Internal Notes:</strong></label>
                        <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnUpdateCandidate" runat="server" Text="Update Candidate" CssClass="btn btn-primary" OnClick="btnUpdateCandidate_Click" />
                </div>
            </div>
        </div>
    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>