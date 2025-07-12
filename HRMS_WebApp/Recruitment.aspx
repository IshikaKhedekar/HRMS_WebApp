<%@ Page Title="Recruitment Management" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Recruitment.aspx.vb" Inherits="HRMS_WebApp.Recruitment" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <%-- Optional: Add Rich Text Editor library like CKEditor or TinyMCE here if you want --%>
    <%-- <script src="path/to/ckeditor.js"></script> --%>
    <style>
        .page-header { background-color: #fd7e14; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px; }
        .section-card { border: 1px solid #e9ecef; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); background-color: #fff; margin-bottom: 20px; }
        .section-card-header { padding: 15px 20px; background-color: #f8f9fa; border-bottom: 1px solid #e9ecef; font-weight: bold; }
        .section-card-body { padding: 20px; }
        .data-table th { background-color: #6f42c1; color: white; } /* Purple theme for recruitment */
        
        /* Status colors for Recruitment Process */
        .status-Open { color: #28a745; font-weight: bold; } /* Green */
        .status-Closed { color: #dc3545; font-weight: bold; } /* Red */
        .status-Draft, .status-OnHold { color: #ffc107; font-weight: bold; } /* Yellow/Orange */
        .status-Applied { color: #007bff; font-weight: bold; } /* Blue */
        .status-Screened { color: #6f42c1; font-weight: bold; } /* Purple */
        .status-Interview-1, .status-Interview-2 { color: #fd7e11; font-weight: bold; } /* Orange */
        .status-Offered { color: #20c997; font-weight: bold; } /* Teal */
        .status-OfferAccepted { color: #17a2b8; font-weight: bold; } /* Cyan */
        .status-BackgroundCheck, .status-ReferenceCheck { color: #6c757d; font-weight: bold; } /* Gray */
        .status-Hired { color: #008000; font-weight: bold; } /* Darker green */
        .status-default, .status-unknown { color: gray; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #6f42c1; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Recruitment Management</h2>
        <p>Manage job vacancies and track candidate applications.</p>
    </div>

    <!-- Bootstrap Tabs -->
    <ul class="nav nav-tabs" id="recruitmentTabs" role="tablist">
        <li class="nav-item" role="presentation"><button class="nav-link active" id="job-vacancies-tab" data-bs-toggle="tab" data-bs-target="#job-vacancies" type="button">Job Vacancies</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" id="applications-tracking-tab" data-bs-toggle="tab" data-bs-target="#applications-tracking" type="button">Applications Tracking</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" id="apply-job-tab" data-bs-toggle="tab" data-bs-target="#apply-job" type="button">Apply for Job</button></li>
    </ul>

    <div class="tab-content border border-top-0 p-3 bg-white">
        <!-- ===== JOB VACANCIES TAB (For HR/Admins) ===== -->
        <div class="tab-pane fade show active" id="job-vacancies" role="tabpanel">
            <div class="section-card mb-4">
                <div class="section-card-header d-flex justify-content-between align-items-center">
                    <span>Manage Job Openings</span>
                    <asp:Button ID="btnShowJobModal" runat="server" Text="Create New Opening" CssClass="btn btn-primary" OnClick="btnShowJobModal_Click" />
                </div>
                <div class="section-card-body">
                    <asp:Repeater ID="rptJobVacancies" runat="server" OnItemCommand="rptJobVacancies_ItemCommand">
                        <HeaderTemplate><table class="table table-striped data-table"><thead><tr><th>Title</th><th>Department</th><th>Location</th><th>Type</th><th>Salary</th><th>Deadline</th><th>Status</th><th>Actions</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("JobTitle") %></td>
                                <td><%# Eval("DepartmentName") %></td> 
                                <td><%# Eval("LocationType") %><%# If(Eval("SpecificLocation") IsNot DBNull.Value AndAlso Eval("SpecificLocation").ToString() <> "", " (" & Eval("SpecificLocation") & ")", "") %></td>
                                <td><%# Eval("EmploymentType") %></td>
                                <td><%# If(Eval("MinSalary") IsNot DBNull.Value, Eval("MinSalary", "{0:C0}"), "") %><%# If(Eval("MaxSalary") IsNot DBNull.Value, " - " & Eval("MaxSalary", "{0:C0}"), "") %><%# If(Eval("Currency") IsNot DBNull.Value, " " & Eval("Currency"), "") %></td>
                                <td><%# Eval("ApplicationDeadline", "{0:dd-MMM-yyyy}") %></td>
                                <td><span class="status-<%# Eval("JobStatus").ToString().Replace(" ", "") %>"><%# Eval("JobStatus") %></span></td>
                             <td>
    <asp:Button ID="btnEditJob" runat="server" Text="Edit" CommandName="EditJob" CommandArgument='<%# Eval("OpeningID") %>' CssClass="btn btn-sm btn-info me-1" />
    <asp:Button ID="btnDeleteJob" runat="server" Text="Delete" CommandName="DeleteJob" CommandArgument='<%# Eval("OpeningID") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Are you sure you want to delete this job opening?');" />

    <!-- LinkedIn Share Icon Button -->
    <asp:HyperLink ID="lnkLinkedIn" runat="server" CssClass="btn btn-sm btn-outline-primary ms-1" ToolTip="Share on LinkedIn" Target="_blank">
        <i class="fab fa-linkedin"></i>
    </asp:HyperLink>
</td>

                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoJobs" runat="server" Text="No job openings found." Visible="false" CssClass="text-muted"></asp:Label>
                </div>
            </div>
        </div>

        <!-- ===== APPLICATIONS TRACKING TAB (For HR/Admins) ===== -->
        <div class="tab-pane fade" id="applications-tracking" role="tabpanel">
            <div class="section-card">
                <div class="section-card-header">All Candidate Applications</div>
                <div class="section-card-body">
                    <div class="mb-3">
                        <label class="form-label">Filter by Job Opening:</label>
                        <asp:DropDownList ID="ddlJobFilter" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="ddlJobFilter_SelectedIndexChanged"></asp:DropDownList>
                        <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filter" CssClass="btn btn-link mt-1" OnClick="btnClearFilter_Click" />
                    </div>
                    <asp:Repeater ID="rptCandidates" runat="server" OnItemCommand="rptCandidates_ItemCommand">
                        <HeaderTemplate><table class="table table-striped data-table"><thead><tr><th>Candidate Name</th><th>Job Applied</th><th>Email</th><th>Phone</th><th>Applied On</th><th>Current Status</th><th>Actions</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("FullName") %></td>
                                <td><%# Eval("JobTitle") %></td>
                                <td><%# Eval("Email") %></td>
                                <td><%# Eval("Phone") %></td>
                                <td><%# Eval("AppliedOn", "{0:dd-MMM-yyyy}") %></td>
                                <td><span class="status-<%# Eval("Status").ToString().Replace(" ", "").Replace("-", "") %>"><%# Eval("Status") %></span></td> 
                                <td>
                                    <asp:Button ID="btnViewCandidateDetails" runat="server" Text="View/Update" CommandName="ViewDetails" CommandArgument='<%# Eval("CandidateID") %>' CssClass="btn btn-sm btn-primary me-1" />
                                    <asp:Button ID="btnDownloadResume" runat="server" Text="Resume" CommandName="DownloadResume" CommandArgument='<%# Eval("CandidateID") %>' CssClass="btn btn-sm btn-success" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoCandidates" runat="server" Text="No applications found." Visible="false" CssClass="text-muted"></asp:Label>
                </div>
            </div>
        </div>

        <!-- ===== APPLY FOR JOB TAB (For Applicants) ===== -->
        <div class="tab-pane fade" id="apply-job" role="tabpanel">
            <div class="section-card">
                <div class="section-card-header">Apply for a Job Opening</div>
                <div class="section-card-body">
                    <div class="mb-3">
                        <label class="form-label">Select Job to Apply For:</label>
                        <asp:DropDownList ID="ddlAvailableJobs" runat="server" CssClass="form-select" DataTextField="JobTitle" DataValueField="OpeningID"></asp:DropDownList>
                    </div>
                    <hr />
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label">Your Full Name</label><asp:TextBox ID="txtApplicantName" runat="server" CssClass="form-control"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Email</label><asp:TextBox ID="txtApplicantEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Phone Number</label><asp:TextBox ID="txtApplicantPhone" runat="server" CssClass="form-control" TextMode="Phone"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Current Address</label><asp:TextBox ID="txtApplicantAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">LinkedIn Profile URL (Optional)</label><asp:TextBox ID="txtLinkedInProfile" runat="server" CssClass="form-control" TextMode="Url"></asp:TextBox></div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label">Desired Salary (e.g., Annual, Monthly, Hourly)</label><asp:TextBox ID="txtDesiredSalary" runat="server" CssClass="form-control"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Earliest Start Date</label><asp:TextBox ID="txtEarliestStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Upload Resume (< 5MB, PDF/DOCX)</label><asp:FileUpload ID="fuResume" runat="server" CssClass="form-control" /></div>
                            <div class="mb-3"><label class="form-label">Upload Cover Letter (Optional, < 2MB, PDF/DOCX)</label><asp:FileUpload ID="fuCoverLetter" runat="server" CssClass="form-control" /></div>
                            <div class="mb-3"><label class="form-label">How did you hear about this job?</label><asp:DropDownList ID="ddlApplicationSource" runat="server" CssClass="form-select">
                                <asp:ListItem>LinkedIn</asp:ListItem>
                                <asp:ListItem>Indeed</asp:ListItem>
                                <asp:ListItem>Company Website</asp:ListItem>
                                <asp:ListItem>Employee Referral</asp:ListItem>
                                <asp:ListItem>Other</asp:ListItem>
                            </asp:DropDownList></div>
                        </div>
                    </div>
                    <asp:Button ID="btnSubmitApplication" runat="server" Text="Submit Application" CssClass="btn btn-primary" OnClick="btnSubmitApplication_Click" />
                    <asp:Label ID="lblApplicationFormMessage" runat="server" CssClass="ms-3"></asp:Label>
                </div>
            </div>
        </div>
    </div>

    <!-- ===== JOB OPENING ADD/EDIT MODAL ===== -->
    <div class="modal fade" id="jobModal" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header"><h5 class="modal-title" id="jobModalLabel">Job Opening Details</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnCurrentJobID" runat="server" />
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label">Job Title</label><asp:TextBox ID="txtJobTitleModal" runat="server" CssClass="form-control"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Department</label><asp:DropDownList ID="ddlDepartmentModal" runat="server" CssClass="form-select"></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Location Type</label><asp:DropDownList ID="ddlLocationType" runat="server" CssClass="form-select"><asp:ListItem>On-site</asp:ListItem><asp:ListItem>Remote</asp:ListItem><asp:ListItem>Hybrid</asp:ListItem><asp:ListItem Value="Specific City">Specific City</asp:ListItem></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Specific Location (e.g., City, State)</label><asp:TextBox ID="txtSpecificLocation" runat="server" CssClass="form-control"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Employment Type</label><asp:DropDownList ID="ddlEmploymentType" runat="server" CssClass="form-select"><asp:ListItem>Full-time</asp:ListItem><asp:ListItem>Part-time</asp:ListItem><asp:ListItem>Contract</asp:ListItem><asp:ListItem>Internship</asp:ListItem></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Experience Level</label><asp:DropDownList ID="ddlExperienceLevel" runat="server" CssClass="form-select"><asp:ListItem>Entry Level</asp:ListItem><asp:ListItem>Mid Level</asp:ListItem><asp:ListItem>Senior Level</asp:ListItem><asp:ListItem>Lead/Principal</asp:ListItem></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Education Level</label><asp:DropDownList ID="ddlEducationLevel" runat="server" CssClass="form-select"><asp:ListItem>High School</asp:ListItem><asp:ListItem>Bachelor's Degree</asp:ListItem><asp:ListItem>Master's Degree</asp:ListItem><asp:ListItem>PhD</asp:ListItem><asp:ListItem>Vocational Training</asp:ListItem></asp:DropDownList></div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label">Min Salary</label><asp:TextBox ID="txtMinSalary" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Max Salary</label><asp:TextBox ID="txtMaxSalary" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Currency</label><asp:DropDownList ID="ddlCurrency" runat="server" CssClass="form-select"><asp:ListItem>INR</asp:ListItem><asp:ListItem>USD</asp:ListItem><asp:ListItem>EUR</asp:ListItem></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Benefits Description</label><asp:TextBox ID="txtBenefitsDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Key Responsibilities</label><asp:TextBox ID="txtKeyResponsibilities" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Required Skills (Comma separated)</label><asp:TextBox ID="txtRequiredSkills" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Full Job Description</label><asp:TextBox ID="txtJobDescriptionModal" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Application Deadline</label><asp:TextBox ID="txtApplicationDeadlineModal" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Hiring Manager</label><asp:DropDownList ID="ddlHiringManager" runat="server" CssClass="form-select"></asp:DropDownList></div>
                            <div class="mb-3"><label class="form-label">Number of Positions</label><asp:TextBox ID="txtNumberOfPositions" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
                            <div class="mb-3"><label class="form-label">Job Status</label><asp:DropDownList ID="ddlJobStatus" runat="server" CssClass="form-select"><asp:ListItem>Open</asp:ListItem><asp:ListItem>Closed</asp:ListItem><asp:ListItem>Draft</asp:ListItem><asp:ListItem>On Hold</asp:ListItem><asp:ListItem>Pending Approval</asp:ListItem></asp:DropDownList></div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Label ID="lblJobModalMessage" runat="server" CssClass="me-auto"></asp:Label>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveJobOpening" runat="server" Text="Save Job Opening" CssClass="btn btn-primary" OnClick="btnSaveJobOpening_Click" />
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
                    <div class="row">
                        <div class="col-md-6">
                            <p><strong>Name:</strong> <asp:Label ID="lblName" runat="server"></asp:Label></p>
                            <p><strong>Job Applied:</strong> <asp:Label ID="lblCandidateJobTitle" runat="server"></asp:Label></p>
                            <p><strong>Email:</strong> <asp:Label ID="lblEmail" runat="server"></asp:Label></p>
                            <p><strong>Phone:</strong> <asp:Label ID="lblPhone" runat="server"></asp:Label></p>
                            <p><strong>Address:</strong> <asp:Label ID="lblAddress" runat="server"></asp:Label></p>
                            <p><strong>Desired Salary:</strong> <asp:Label ID="lblDesiredSalary" runat="server"></asp:Label></p>
                            <p><strong>Earliest Start:</strong> <asp:Label ID="lblEarliestStart" runat="server"></asp:Label></p>
                            <p><strong>LinkedIn:</strong> <asp:HyperLink ID="hlLinkedIn" runat="server" Target="_blank"></asp:HyperLink></p>
                            <p><strong>Application Source:</strong> <asp:Label ID="lblApplicationSource" runat="server"></asp:Label></p>
                            <p><strong>Applied On:</strong> <asp:Label ID="lblAppliedOn" runat="server"></asp:Label></p>
                            <p><strong>Resume:</strong> <asp:HyperLink ID="hlResume" runat="server" Target="_blank">Download Resume</asp:HyperLink></p>
                            <p><strong>Cover Letter:</strong> <asp:HyperLink ID="hlCoverLetter" runat="server" Target="_blank">Download Cover Letter</asp:HyperLink></p>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3"><label class="form-label"><strong>Update Status:</strong></label>
                                <asp:DropDownList ID="ddlCandidateStatus" runat="server" CssClass="form-select"></asp:DropDownList>
                            </div>
                            <div class="mb-3"><label class="form-label"><strong>Interview Notes:</strong></label>
                                <asp:TextBox ID="txtInterviewNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="mb-3"><label class="form-label"><strong>Assessment Scores:</strong></label>
                                <asp:TextBox ID="txtAssessmentScores" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <h6>Onboarding Checklist:</h6>
                            <div class="form-check"><asp:CheckBox ID="chkIDProof" runat="server" Text="ID Proof Collected" CssClass="form-check-input" /></div>
                            <div class="form-check"><asp:CheckBox ID="chkAddressProof" runat="server" Text="Address Proof Collected" CssClass="form-check-input" /></div>
                            <div class="form-check"><asp:CheckBox ID="chkEduCerts" runat="server" Text="Educational Certificates Collected" CssClass="form-check-input" /></div>
                            <div class="form-check"><asp:CheckBox ID="chkPrevEmpLetter" runat="server" Text="Previous Employment Letters Collected" CssClass="form-check-input" /></div>
                            <div class="form-check"><asp:CheckBox ID="chkBackgroundCheck" runat="server" Text="Background Check Initiated/Cleared" CssClass="form-check-input" /></div>
                            <div class="form-check"><asp:CheckBox ID="chkReferenceCheck" runat="server" Text="Reference Check Completed" CssClass="form-check-input" /></div>
                            <div class="mb-3 mt-3"><label class="form-label"><strong>HR Remarks:</strong></label>
                                <asp:TextBox ID="txtHRRemarks" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Label ID="lblCandidateModalMessage" runat="server" CssClass="me-auto"></asp:Label>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnUpdateCandidate" runat="server" Text="Update Candidate" CssClass="btn btn-primary" OnClick="btnUpdateCandidate_Click" />
                </div>
            </div>
        </div>
    </div>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <%-- Optional: Initialize Rich Text Editor here if used --%>
    <%-- <script>
        document.addEventListener('DOMContentLoaded', function() {
            if (typeof CKEDITOR !== 'undefined') {
                CKEDITOR.replace('txtJobDescriptionModal');
                CKEDITOR.replace('txtKeyResponsibilities'); // If you want RTE for responsibilities too
                CKEDITOR.replace('txtBenefitsDescription'); // If you want RTE for benefits too
            }
        });
    </script> --%>
</asp:Content>