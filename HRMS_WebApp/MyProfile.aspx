<%@ Page Title="My Profile" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="MyProfile.aspx.vb" Inherits="HRMS_WebApp.MyProfile" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .profile-header { display: flex; align-items: center; gap: 25px; margin-bottom: 25px; }
        .profile-photo {
            width: 120px; height: 120px; object-fit: cover; border-radius: 50%; 
            border: 4px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.1);
        }
        .profile-name { font-size: 2.25rem; font-weight: 700; color: #212529; margin: 0; }
        .profile-designation { font-size: 1.1rem; color: #6c757d; }
        .section-card { background-color: #fff; border: 1px solid #dee2e6; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.05); margin-bottom: 25px; }
        .section-title { font-size: 1.25rem; font-weight: 600; color: #007bff; padding: 15px 20px; border-bottom: 1px solid #dee2e6; }
        .info-grid { padding: 20px; display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
        .info-item .info-label { font-size: 0.9rem; color: #6c757d; display: block; margin-bottom: 2px; }
        .info-item .info-value { display: block; font-size: 1rem; font-weight: 500; color: #212529; }
        /* Edit Mode: Input fields default hidden, Labels default shown */
        .info-item .form-control, .info-item .form-select { display: none; }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h3">My Profile</h1>
        <div>
            <asp:Button ID="btnEdit" runat="server" Text="Edit Profile" CssClass="btn btn-primary" OnClientClick="enterEditMode(); return false;" />
            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-success d-none" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary ms-2 d-none" OnClientClick="cancelEditMode(); return false;" />
        </div>
    </div>

    <div class="profile-header">
        <%-- Photo Display: src ko code-behind se set karenge --%>
        <asp:Image ID="imgProfile" runat="server" CssClass="profile-photo" AlternateText="Employee Photo" />
        
        <%-- Photo Upload Controls --%>
        <div class="ms-3">
            <asp:FileUpload ID="fileUploadPhoto" runat="server" CssClass="form-control d-none" />
            <asp:Button ID="btnUploadPhoto" runat="server" Text="Upload Photo" CssClass="btn btn-sm btn-outline-secondary d-none mt-2" OnClick="btnUploadPhoto_Click" />
            <asp:Label ID="lblPhotoMessage" runat="server" CssClass="d-block mt-1"></asp:Label>
        </div>

        <div>
            <h2 class="profile-name" runat="server" id="lblEmpName"></h2>
            <p class="profile-designation" runat="server" id="lblDesignationDept"></p>
        </div>
    </div>

    <!-- Personal & Contact Details -->
    <div class="section-card">
        <div class="section-title">Personal & Contact Information</div>
        <div class="info-grid">
            <div class="info-item"><span class="info-label">Date of Birth</span><span class="info-value" runat="server" id="lblDob"></span></div>
            <div class="info-item"><span class="info-label">Gender</span><span class="info-value" runat="server" id="lblGender"></span></div>
            <div class="info-item"><span class="info-label">Marital Status</span><asp:DropDownList ID="ddlMaritalStatus" runat="server" CssClass="form-select"><asp:ListItem>Single</asp:ListItem><asp:ListItem>Married</asp:ListItem></asp:DropDownList><span class="info-value" runat="server" id="lblMaritalStatus"></span></div>
            <div class="info-item"><span class="info-label">Nationality</span><asp:TextBox ID="txtNationality" runat="server" CssClass="form-control"></asp:TextBox><span class="info-value" runat="server" id="lblNationality"></span></div>
            <div class="info-item"><span class="info-label">Blood Group</span><asp:TextBox ID="txtBloodGroup" runat="server" CssClass="form-control"></asp:TextBox><span class="info-value" runat="server" id="lblBloodGroup"></span></div>
            <div class="info-item"><span class="info-label">Mobile</span><asp:TextBox ID="txtMobile" runat="server" CssClass="form-control"></asp:TextBox><span class="info-value" runat="server" id="lblMobile"></span></div>
            <div class="info-item"><span class="info-label">Personal Email</span><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox><span class="info-value" runat="server" id="lblEmail"></span></div>
            <div class="info-item" style="grid-column: span 2;"><span class="info-label">Address</span><asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"></asp:TextBox><span class="info-value" runat="server" id="lblAddress"></span></div>
        </div>
    </div>

    <!-- Emergency Contact -->
    <div class="section-card">
        <div class="section-title">Emergency Contact</div>
        <div class="info-grid">
            <div class="info-item"><span class="info-label">Contact Name</span><asp:TextBox ID="txtEmergencyName" runat="server" CssClass="form-control"></asp:TextBox><span class="info-value" runat="server" id="lblEmergencyName"></span></div>
            <div class="info-item"><span class="info-label">Contact Phone</span><asp:TextBox ID="txtEmergencyPhone" runat="server" CssClass="form-control"></asp:TextBox><span class="info-value" runat="server" id="lblEmergencyPhone"></span></div>
        </div>
    </div>

    <!-- Job & Bank Details -->
    <div class="row">
        <div class="col-lg-6">
            <div class="section-card">
                <div class="section-title">Job Information</div>
                <div class="info-grid">
                    <div class="info-item"><span class="info-label">Employee ID</span><span class="info-value" runat="server" id="lblEmpID"></span></div>
                    <div class="info-item"><span class="info-label">Date of Joining</span><span class="info-value" runat="server" id="lblDoj"></span></div>
                    <div class="info-item"><span class="info-label">Reporting Manager</span><span class="info-value" runat="server" id="lblManager"></span></div>
                    <div class="info-item"><span class="info-label">Official Email</span><span class="info-value" runat="server" id="lblOfficialEmail"></span></div>
                </div>
            </div>
        </div>
        <div class="col-lg-6">
            <div class="section-card">
                <div class="section-title">Bank Details</div>
                 <div class="info-grid">
                    <div class="info-item"><span class="info-label">Bank Name</span><span class="info-value" runat="server" id="lblBankName"></span></div>
                    <div class="info-item"><span class="info-label">Account Number</span><span class="info-value" runat="server" id="lblAccountNumber"></span></div>
                    <div class="info-item"><span class="info-label">IFSC Code</span><span class="info-value" runat="server" id="lblIFSC"></span></div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // Helper function to show/hide elements in edit mode
        function setEditMode(isEdit) {
            // Toggle visibility of labels vs. textboxes
            const editableControls = [
                '<%= ddlMaritalStatus.ClientID %>', '<%= lblMaritalStatus.ClientID %>',
                '<%= txtNationality.ClientID %>', '<%= lblNationality.ClientID %>',
                '<%= txtBloodGroup.ClientID %>', '<%= lblBloodGroup.ClientID %>',
                '<%= txtMobile.ClientID %>', '<%= lblMobile.ClientID %>',
                '<%= txtEmail.ClientID %>', '<%= lblEmail.ClientID %>',
                '<%= txtAddress.ClientID %>', '<%= lblAddress.ClientID %>',
                '<%= txtEmergencyName.ClientID %>', '<%= lblEmergencyName.ClientID %>',
                '<%= txtEmergencyPhone.ClientID %>', '<%= lblEmergencyPhone.ClientID %>'
            ];

            for (let i = 0; i < editableControls.length; i += 2) {
                const input = document.getElementById(editableControls[i]);
                const label = document.getElementById(editableControls[i + 1]);
                if (input && label) {
                    input.style.display = isEdit ? 'block' : 'none';
                    label.style.display = isEdit ? 'none' : 'block';
                }
            }

            // Toggle visibility of buttons
            document.getElementById('<%= btnEdit.ClientID %>').style.display = isEdit ? 'none' : 'inline-block';
            document.getElementById('<%= btnSave.ClientID %>').classList.toggle('d-none', !isEdit);
            document.getElementById('<%= btnCancel.ClientID %>').classList.toggle('d-none', !isEdit);

            // Toggle visibility of photo upload controls
            document.getElementById('<%= fileUploadPhoto.ClientID %>').classList.toggle('d-none', !isEdit);
            document.getElementById('<%= btnUploadPhoto.ClientID %>').classList.toggle('d-none', !isEdit);
        }

        function enterEditMode() {
            setEditMode(true);
        }

        function cancelEditMode() {
            setEditMode(false);
            // Optional: Reload the page to revert unsaved changes
            window.location.reload();
        }
    </script>
</asp:Content>