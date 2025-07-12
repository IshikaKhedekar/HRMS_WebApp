Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Web.Script.Serialization ' For JSON serialization

Public Class PerformanceAnalytics
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' This page should be accessible only to HR/Admin
        Dim userRoleID As Integer = CInt(Session("RoleID"))
        If userRoleID > 2 Then
            Response.Redirect("~/Dashboard.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadAnalyticsData()
        End If
    End Sub

    Private Sub LoadAnalyticsData()
        Dim activeCycleID As Integer = GetActiveCycleID()
        If activeCycleID = 0 Then
            ' Handle case where no cycle is active
            litActiveCycleName.Text = "No Active Cycle"
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim allReviews As New List(Of ReviewData)

        Using conn As New SqlConnection(connStr)
            conn.Open()
            ' Fetch all completed review data for the active cycle
            Dim query As String = "SELECT u.Name, pr.ManagerRating FROM PerformanceReviews pr JOIN Users u ON pr.EmployeeID = u.UserID WHERE pr.CycleID = @CycleID AND pr.Status IN ('Completed', 'Published')"
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@CycleID", activeCycleID)
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        allReviews.Add(New ReviewData With {
                            .Name = reader("Name").ToString(),
                            .ManagerRating = If(IsDBNull(reader("ManagerRating")), 0, CInt(reader("ManagerRating")))
                        })
                    End While
                End Using
            End Using

            ' Get Active Cycle Name
            Using cmd As New SqlCommand("SELECT CycleName FROM PerformanceCycles WHERE CycleID = @CycleID", conn)
                cmd.Parameters.AddWithValue("@CycleID", activeCycleID)
                litActiveCycleName.Text = cmd.ExecuteScalar().ToString()
            End Using
        End Using

        ' === CALCULATE & BIND KPIS ===
        ' 1. Average Rating
        Dim validReviews = allReviews.Where(Function(r) r.ManagerRating > 0)
        If validReviews.Any() Then
            litAverageRating.Text = validReviews.Average(Function(r) r.ManagerRating).ToString("F2")
        Else
            litAverageRating.Text = "0.0"
        End If

        ' 2. Completion Rate
        Dim totalReviewsQuery As String = "SELECT COUNT(*) FROM PerformanceReviews WHERE CycleID = " & activeCycleID
        Dim completedReviewsQuery As String = "SELECT COUNT(*) FROM PerformanceReviews WHERE CycleID = " & activeCycleID & " AND Status IN ('Completed', 'Published')"
        Dim totalCount As Integer
        Dim completedCount As Integer
        Using conn As New SqlConnection(connStr)
            conn.Open()
            totalCount = CInt(New SqlCommand(totalReviewsQuery, conn).ExecuteScalar())
            completedCount = CInt(New SqlCommand(completedReviewsQuery, conn).ExecuteScalar())
        End Using
        If totalCount > 0 Then
            litCompletionRate.Text = (Convert.ToDouble(completedCount) / totalCount).ToString("P0")
        Else
            litCompletionRate.Text = "0%"
        End If

        ' === BIND LISTS ===
        ' 3. Top Performers (Top 5, Rating >= 4)
        Dim topPerformers = allReviews.Where(Function(r) r.ManagerRating >= 4).OrderByDescending(Function(r) r.ManagerRating).Take(5).ToList()
        rptTopPerformers.DataSource = topPerformers
        rptTopPerformers.DataBind()

        ' 4. Potential Attrition Risks (Bottom 5, Rating <= 2)
        Dim attritionRisks = allReviews.Where(Function(r) r.ManagerRating > 0 AndAlso r.ManagerRating <= 2).OrderBy(Function(r) r.ManagerRating).Take(5).ToList()
        rptAttritionRisks.DataSource = attritionRisks
        rptAttritionRisks.DataBind()

        ' === PREPARE DATA FOR CHART ===
        ' 5. Rating Distribution
        Dim ratingGroups = validReviews.GroupBy(Function(r) r.ManagerRating).Select(Function(g) New With {
            .Rating = g.Key.ToString() & " Star",
            .Count = g.Count()
        }).ToList()

        Dim serializer As New JavaScriptSerializer()
        hdnChartData.Value = serializer.Serialize(ratingGroups)
    End Sub

    Private Function GetActiveCycleID() As Integer
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT TOP 1 CycleID FROM PerformanceCycles WHERE Status = 'Active'"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                conn.Open()
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    Return CInt(result)
                End If
            End Using
        End Using
        Return 0
    End Function

    ' Helper class to hold review data temporarily
    Public Class ReviewData
        Public Property Name As String
        Public Property ManagerRating As Integer
    End Class

End Class