<%@ Page Title="Dashboard" Language="vb" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeBehind="Dashboard.aspx.vb" Inherits="HRMS_WebApp.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<!-- ===== DASHBOARD HEADER ===== -->
 <div class="dashboard-header">
     <h1><i class="fas fa-chart-line"></i> Indas Analytics HR Dashboard</h1>
     <p>Welcome! Track real-time employee metrics, leave status, and upcoming events.</p>
 </div>

 <!-- ===== DASHBOARD CARDS (ADVANCED & CLICKABLE) ===== -->
 <div class="card-grid">
     <!-- Total Employees -->
     <a href="EmployeeDirectory.aspx" class="card-link">
         <div class="card card-blue">
             <div class="card-icon"><i class="fas fa-users"></i></div>
             <div class="card-content">
                 <h4>Total Employees</h4>
                 <asp:Literal ID="litTotalEmployees" runat="server">0</asp:Literal>
             </div>
         </div>
     </a>

     <!-- Present Today -->
     <a href="MonthlyAttendance.aspx" class="card-link" id="linkPresentToday" runat="server">
         <div class="card card-green">
             <div class="card-icon"><i class="fas fa-user-check"></i></div>
             <div class="card-content">
                 <h4>Present Today</h4>
                 <asp:Literal ID="litPresentToday" runat="server">0</asp:Literal>
             </div>
         </div>
     </a>

     <!-- On Leave -->
     <a href="LeaveManagement.aspx" class="card-link">
         <div class="card card-yellow">
             <div class="card-icon"><i class="fas fa-user-clock"></i></div>
             <div class="card-content">
                 <h4>On Leave Today</h4>
                 <asp:Literal ID="litOnLeave" runat="server">0</asp:Literal>
             </div>
         </div>
     </a>

     <!-- Upcoming Holidays -->
     <a href="#" class="card-link">
         <div class="card card-purple">
             <div class="card-icon"><i class="fas fa-calendar-alt"></i></div>
             <div class="card-content">
                 <h4>Upcoming Holiday</h4>
                 <asp:Literal ID="litUpcomingHolidays" runat="server">No upcoming holidays.</asp:Literal>
             </div>
         </div>
     </a>

     <!-- Upcoming Birthdays -->
     <a href="#" class="card-link">
         <div class="card card-pink">
             <div class="card-icon"><i class="fas fa-birthday-cake"></i></div>
             <div class="card-content">
                 <h4>Upcoming Birthday</h4>
                 <asp:Literal ID="litUpcomingBirthdays" runat="server">No birthdays soon.</asp:Literal>
             </div>
         </div>
     </a>
 </div>    <!-- ===== DASHBOARD STYLES ===== -->
    <style>
        .dashboard-header {
            background: #ffffff;
            border-radius: 12px;
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
        }

        .dashboard-header h1 {
            font-size: 28px;
            color: #2c3e50;
            margin-bottom: 10px;
        }

        .dashboard-header h1 i {
            color: #007bff;
            margin-right: 10px;
        }

        .dashboard-header p {
            font-size: 15px;
            color: #6c757d;
        }

        .card-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
        }

        .card {
            background: white;
            border-radius: 12px;
            padding: 20px;
            display: flex;
            align-items: center;
            box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
            transition: transform 0.3s;
        }

        .card:hover {
            transform: translateY(-5px);
        }

        .card-value {
    font-size: 30px;
    font-weight: 800;
    color: #ffffff;
    display: block;
    margin-top: 5px;
}


        .card-icon {
            font-size: 38px;
            margin-right: 20px;
            color: white;
            padding: 15px;
            border-radius: 50%;
        }


        .card-link {
    text-decoration: none;
    display: block;
    color: inherit;
}

        .card-content h4 {
            margin: 0;
            font-size: 16px;
            color: #555;
        }

        .card-content literal, .card-content span {
            font-size: 28px;
            font-weight: bold;
            color: #2c3e50;
        }

        .card-blue    { background: linear-gradient(45deg, #007bff, #3399ff); color: white; }
        .card-green   { background: linear-gradient(45deg, #28a745, #5cd68d); color: white; }
        .card-yellow  { background: linear-gradient(45deg, #ffc107, #ffd76e); color: white; }
        .card-purple  { background: linear-gradient(45deg, #6f42c1, #a58df5); color: white; }
        .card-pink    { background: linear-gradient(45deg, #e83e8c, #f87db2); color: white; }

        .card-blue .card-content h4,
        .card-green .card-content h4,
        .card-yellow .card-content h4,
        .card-purple .card-content h4,
        .card-pink .card-content h4 {
            color: #ffffff;
        }

        .card-blue .card-content literal,
        .card-green .card-content literal,
        .card-yellow .card-content literal,
        .card-purple .card-content literal,
        .card-pink .card-content literal {
            color: #ffffff;
        }
    </style>

</asp:Content>
