<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="AttendanceAnalytics.aspx.vb" Inherits="HRMS_WebApp.AttendanceAnalytics" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <%-- DevExtreme scripts pehle se master page me hain --%>
    <style>
        .chart-container {
            width: 100%;
            height: 400px;
            margin-bottom: 30px;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Attendance Analytics</h2>
        <p>Visual insights into company-wide attendance and punctuality.</p>
    </div>

    <div class="row">
        <!-- Daily Attendance Trend Chart -->
        <div class="col-lg-12">
            <h4>Daily Attendance Trend (Last 30 Days)</h4>
            <div id="dailyTrendChart" class="chart-container"></div>
        </div>
    </div>
    
    <div class="row">
        <!-- Punctuality/Status Distribution Chart -->
        <div class="col-lg-6">
            <h4>Punctuality & Status Distribution</h4>
            <div id="statusDistributionChart" class="chart-container"></div>
        </div>
        
        <!-- Top 5 Lists -->
        <div class="col-lg-6">
            <h4>Top 5 Late Comers (Last 30 Days)</h4>
            <ul class="list-group mb-4">
                <asp:Repeater ID="rptLateComers" runat="server">
                    <ItemTemplate>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <%# Eval("Name") %>
                            <span class="badge bg-warning text-dark rounded-pill"><%# Eval("LateCount") %> Times</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>

            <h4>Top 5 Overtime Employees (Last 30 Days)</h4>
            <ul class="list-group">
                 <asp:Repeater ID="rptOvertime" runat="server">
                    <ItemTemplate>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <%# Eval("Name") %>
                            <span class="badge bg-info rounded-pill"><%# Eval("TotalOT", "{0:F2}") %> hrs</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>
    </div>

    <!-- Hidden fields to pass data from server to client-side script -->
    <asp:HiddenField ID="hdnDailyTrendData" runat="server" />
    <asp:HiddenField ID="hdnStatusDistributionData" runat="server" />

    <script type="text/javascript">
        $(function () {
            // --- DAILY TREND CHART ---
            var dailyTrendJson = $('#<%= hdnDailyTrendData.ClientID %>').val();
            if (dailyTrendJson) {
                var dailyTrendData = JSON.parse(dailyTrendJson);
                $("#dailyTrendChart").dxChart({
                    dataSource: dailyTrendData,
                    commonSeriesSettings: {
                        argumentField: "Date",
                        type: "bar"
                    },
                    series: [
                        { valueField: "Present", name: "Present", color: "#198754" },
                        { valueField: "Absent", name: "Absent", color: "#dc3545" },
                        { valueField: "OnLeave", name: "OnLeave", name: "On Leave", color: "#ffc107" }
                    ],
                    legend: {
                        verticalAlignment: "bottom",
                        horizontalAlignment: "center"
                    },
                    argumentAxis: {
                        label: {
                            format: "dd-MMM"
                        }
                    },
                    title: "Present vs Absent vs Leave (Last 30 Days)"
                });
            }

            // --- STATUS DISTRIBUTION PIE CHART ---
            var statusDistributionJson = $('#<%= hdnStatusDistributionData.ClientID %>').val();
            if (statusDistributionJson) {
                var statusData = JSON.parse(statusDistributionJson);
                $("#statusDistributionChart").dxPieChart({
                    dataSource: statusData,
                    series: [{
                        argumentField: "Status",
                        valueField: "Count",
                        label: {
                            visible: true,
                            connector: { visible: true },
                            customizeText: function (arg) {
                                return arg.argumentText + " (" + arg.percentText + ")";
                            }
                        }
                    }],
                    title: "Overall Status Distribution (Last 30 Days)"
                });
            }
        });
    </script>
</asp:Content>