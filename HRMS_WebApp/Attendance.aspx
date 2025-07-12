<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Attendance.aspx.vb" Inherits="HRMS_WebApp.Attendance" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .punch-card-wrapper {
    background: linear-gradient(45deg, #0d6efd, #0dcaf0);
    color: white;
    border-radius: 12px;
    padding: 2rem;
    text-align: center;
    box-shadow: 0 5px 15px rgba(0,0,0,0.1);
}

.punch-card-wrapper h4 {
    font-weight: 300; /* Lighter font weight for heading */
}
/* REPLACE THE OLD .punch-btn with this NEW one */
.punch-btn {
    font-size: 1.1rem;      /* <<< Font size kam kiya */
    font-weight: 600;
    padding: 0.6rem 1.5rem; /* <<< Padding kam ki */
    border-radius: 50px;
    transition: all 0.3s;
    border: 2px solid white;
}

.punch-btn.punch-in {
    background-color: transparent;
    color: white;
}

.punch-btn.punch-in:hover {
    background-color: white;
    color: #198754;
}

.punch-btn.punch-out {
    background-color: transparent;
    color: white;
}

.punch-btn.punch-out:hover {
    background-color: white;
    color: #dc3545;
}

/* Disabled button style */
.punch-btn:disabled {
    background-color: rgba(255,255,255,0.2);
    border-color: rgba(255,255,255,0.3);
    color: rgba(255,255,255,0.5);
    cursor: not-allowed;
}

.punch-info {
    margin-top: 1.5rem;
    font-size: 1.1rem;
    background: rgba(0,0,0,0.1);
    border-radius: 8px;
    padding: 10px;
}

.punch-info .info-label {
    font-weight: 300;
}
.punch-info .info-value {
    font-weight: 600;
}

#<%= lblPunchMessage.ClientID %>
{
    font-weight: bold;
    background-color: rgba(255, 255, 255, 0.9);
    padding: 8px;
    border-radius: 5px;
    margin-top: 15px;
}
      
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>My Attendance</h2>
        <p>Mark your daily attendance and view your history.</p>
    </div>

    <div class="row">
        <!-- Punch In/Out Card -->
        <div class="col-md-5">
    <div class="punch-card-wrapper">
        <h4>Today's Attendance (<%= DateTime.Now.ToString("dd-MMM-yyyy") %>)</h4>
        <div class="mt-4 mb-4">
            <asp:Button ID="btnPunchIn" runat="server" Text="Punch In" CssClass="btn punch-btn punch-in" OnClick="btnPunchIn_Click" />
            <asp:Button ID="btnPunchOut" runat="server" Text="Punch Out" CssClass="btn punch-btn punch-out ms-2" OnClick="btnPunchOut_Click" />
        </div>
        <div class="punch-info">
            <div class="row">
                <div class="col-6 text-center">
                    <span class="info-label d-block">Punch In</span>
                    <asp:Label ID="lblPunchInTime" runat="server" Text="--:--" CssClass="info-value"></asp:Label>
                </div>
                <div class="col-6 text-center">
                    <span class="info-label d-block">Punch Out</span>
                    <asp:Label ID="lblPunchOutTime" runat="server" Text="--:--" CssClass="info-value"></asp:Label>
                </div>
            </div>
        </div>
        <asp:Label ID="lblPunchMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>
    </div>
             <!-- Regularization Request -->
            <div class="punch-card mt-4">
                <h5>Forgot to Punch?</h5>
                <p class="text-muted small">If you missed punching in or out, you can send a regularization request to your manager.</p>
                 <asp:Button ID="btnShowRegularizeModal" runat="server" Text="Request Regularization" CssClass="btn btn-warning" OnClick="btnShowRegularizeModal_Click" />
            </div>
        </div>

        <!-- Attendance History -->
        <div class="col-md-7">
            <div class="card">
                <div class="card-header">
                    <h4>My Attendance History</h4>
                </div>
                <div class="card-body">
                    <asp:Repeater ID="rptAttendanceHistory" runat="server">
                        <HeaderTemplate><table class="table table-striped history-table"><thead><tr><th>Date</th><th>Punch In</th><th>Punch Out</th><th>Status</th></tr></thead><tbody></HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("LogDate", "{0:dd-MMM-yyyy}") %></td>
                                <td><%# FormatTime(Eval("PunchInTime")) %></td>
                                <td><%# FormatTime(Eval("PunchOutTime")) %></td>
                                <td><span class="badge <%# GetStatusBadge(Eval("Status").ToString()) %>"><%# Eval("Status") %></span></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></tbody></table></FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>

    <!-- Regularization Modal -->
    <div class="modal fade" id="regularizeModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header"><h5 class="modal-title">Attendance Regularization Request</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Date to Correct:</label>
                        <asp:TextBox ID="txtDateToCorrect" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">New Punch In Time:</label>
                        <asp:TextBox ID="txtNewPunchIn" runat="server" CssClass="form-control" TextMode="Time"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">New Punch Out Time:</label>
                        <asp:TextBox ID="txtNewPunchOut" runat="server" CssClass="form-control" TextMode="Time"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Reason:</label>
                        <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSendRequest" runat="server" Text="Send Request" CssClass="btn btn-primary" OnClick="btnSendRequest_Click" />
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</asp:Content>