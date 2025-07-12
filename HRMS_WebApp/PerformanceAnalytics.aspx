<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="PerformanceAnalytics.aspx.vb" Inherits="HRMS_WebApp.PerformanceAnalytics" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <%-- DevExtreme scripts pehle se master page me hain --%>
    <style>
        .chart-container {
            width: 100%;
            height: 400px;
            margin-bottom: 30px;
        }
        .kpi-card {
            background-color: #fff;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            box-shadow: 0 4px 8px rgba(0,0,0,0.05);
        }
        .kpi-card h3 {
            font-size: 2.5rem;
            font-weight: 700;
            color: #0d6efd;
        }
        .kpi-card p {
            font-size: 1rem;
            color: #6c757d;
            margin: 0;
        }
        .list-group-item strong {
            color: #198754; /* Green for top performers */
        }
        .list-group-item.risk strong {
            color: #dc3545; /* Red for attrition risks */
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header" style="background-color: #0d6efd; color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px;">
        <h2>Performance Analytics Dashboard</h2>
        <p>Insights into company-wide employee performance.</p>
    </div>

    <!-- ===== KEY METRICS ===== -->
    <div class="row mb-4">
        <div class="col-md-4">
            <div class="kpi-card">
                <asp:Literal ID="litAverageRating" runat="server">0.0</asp:Literal>
                <p>Average Performance Rating</p>
            </div>
        </div>
        <div class="col-md-4">
            <div class="kpi-card">
                <asp:Literal ID="litCompletionRate" runat="server">0%</asp:Literal>
                <p>Review Completion Rate</p>
            </div>
        </div>
        <div class="col-md-4">
            <div class="kpi-card">
                <asp:Literal ID="litActiveCycleName" runat="server">N/A</asp:Literal>
                <p>Active Review Cycle</p>
            </div>
        </div>
    </div>
    
    <div class="row">
        <!-- ===== RATING DISTRIBUTION CHART ===== -->
        <div class="col-lg-8">
            <h4>Rating Distribution</h4>
            <div id="ratingDistributionChart" class="chart-container"></div>
        </div>
        
        <!-- ===== TOP PERFORMERS & ATTRITION RISKS ===== -->
        <div class="col-lg-4">
            <h4>Top Performers</h4>
            <ul class="list-group mb-4">
                <asp:Repeater ID="rptTopPerformers" runat="server">
                    <ItemTemplate>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <%# Eval("Name") %>
                            <span class="badge bg-primary rounded-pill"><%# Eval("ManagerRating", "{0:F1}") %> ★</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>

            <h4>Potential Attrition Risks</h4>
            <ul class="list-group">
                 <asp:Repeater ID="rptAttritionRisks" runat="server">
                    <ItemTemplate>
                        <li class="list-group-item d-flex justify-content-between align-items-center risk">
                            <strong><%# Eval("Name") %></strong>
                            <span class="badge bg-danger rounded-pill"><%# Eval("ManagerRating", "{0:F1}") %> ★</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>
    </div>
    
    <!-- Hidden fields to pass data from server to client-side script -->
    <asp:HiddenField ID="hdnChartData" runat="server" />

    <script type="text/javascript">
        $(function () {
            // Read data from the hidden field
            var chartDataJson = $('#<%= hdnChartData.ClientID %>').val();
            if (chartDataJson) {
                var chartData = JSON.parse(chartDataJson);

                // Initialize DevExtreme Pie Chart
                $("#ratingDistributionChart").dxPieChart({
                    dataSource: chartData,
                    title: "Overall Rating Distribution (Active Cycle)",
                    legend: {
                        verticalAlignment: "bottom",
                        horizontalAlignment: "center",
                        itemTextPosition: "right"
                    },
                    series: [{
                        argumentField: "Rating",
                        valueField: "Count",
                        label: {
                            visible: true,
                            connector: {
                                visible: true,
                                width: 1
                            },
                            format: "fixedPoint",
                            percentPrecision: 0,
                            customizeText: function (arg) {
                                return arg.argumentText + "★ (" + arg.percentText + ")";
                            }
                        }
                    }]
                });
            }
        });
    </script>
</asp:Content>