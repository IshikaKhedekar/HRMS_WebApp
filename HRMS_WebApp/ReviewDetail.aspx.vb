Imports System.Configuration
Imports System.Data.SqlClient

Public Class ReviewDetail
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            ' URL se ReviewID nikalo
            If Request.QueryString("ReviewID") IsNot Nothing Then
                Dim reviewID As Integer = CInt(Request.QueryString("ReviewID"))
                hdnReviewID.Value = reviewID.ToString()
                LoadReviewDetails(reviewID)
            Else
                ' Agar ReviewID nahi hai to wapis bhej do
                Response.Redirect("Performance.aspx")
            End If
        End If
    End Sub

    Private Sub LoadReviewDetails(ByVal reviewID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT u.Name as EmployeeName, pr.EmployeeComments, pr.ManagerComments, pr.ManagerRating, pr.Status FROM PerformanceReviews pr JOIN Users u ON pr.EmployeeID = u.UserID WHERE pr.ReviewID = @ReviewID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        lblEmployeeName.Text = reader("EmployeeName").ToString()
                        ' Employee ke comments HTML me dikhao
                        litEmployeeComments.Text = Server.HtmlEncode(reader("EmployeeComments").ToString()).Replace(vbCrLf, "<br/>")

                        ' Manager comments aur rating load karna
                        If reader("ManagerComments") IsNot DBNull.Value Then
                            txtManagerComments.Text = reader("ManagerComments").ToString()
                        End If
                        If reader("ManagerRating") IsNot DBNull.Value Then
                            ddlRating.SelectedValue = reader("ManagerRating").ToString()
                        End If

                        ' === YAHAN SE MANAGER EDITING CONTROL LOGIC SHURU HOTA HAI ===
                        Dim reviewStatus As String = reader("Status").ToString()

                        ' Default state: Editable (Agar status "Pending Manager Review" hai)
                        txtManagerComments.ReadOnly = False
                        ddlRating.Enabled = True
                        btnFinalizeReview.Enabled = True
                        btnFinalizeReview.Text = "Finalize & Submit Review"
                        lblMessage.Text = "" ' Message clear karo

                        Select Case reviewStatus
                            Case "Completed"
                                ' Review complete ho gaya hai, to form lock kar do
                                txtManagerComments.ReadOnly = True
                                ddlRating.Enabled = False
                                btnFinalizeReview.Enabled = False
                                btnFinalizeReview.Text = "Review Already Completed"
                                lblMessage.Text = "This review has been finalized."
                                lblMessage.ForeColor = Drawing.Color.Green
                            Case "Pending Self-Appraisal"
                                ' Employee ne apna hissa hi nahi bhara, to manager edit nahi kar sakta
                                txtManagerComments.ReadOnly = True
                                ddlRating.Enabled = False
                                btnFinalizeReview.Enabled = False
                                btnFinalizeReview.Text = "Employee Self-Appraisal Pending"
                                lblMessage.Text = "Employee's self-appraisal is pending. You cannot review yet."
                                lblMessage.ForeColor = Drawing.Color.Red
                            Case "Not Started" ' Agar koi review record hai par shuru nahi hua
                                txtManagerComments.ReadOnly = True
                                ddlRating.Enabled = False
                                btnFinalizeReview.Enabled = False
                                btnFinalizeReview.Text = "Review Not Started"
                                lblMessage.Text = "Review process has not been initiated for this employee."
                                lblMessage.ForeColor = Drawing.Color.Gray
                        End Select
                        ' === MANAGER EDITING CONTROL LOGIC KHATAM HOTA HAI ===
                    End If
                End Using
            End Using
        End Using
    End Sub
    Protected Sub btnFinalizeReview_Click(sender As Object, e As EventArgs)
        Dim reviewID As Integer = CInt(hdnReviewID.Value)
        Dim managerComments As String = txtManagerComments.Text.Trim()
        Dim rating As Integer = CInt(ddlRating.SelectedValue)

        If String.IsNullOrEmpty(managerComments) OrElse rating = 0 Then
            lblMessage.Text = "Please provide both comments and a rating."
            lblMessage.ForeColor = Drawing.Color.Red
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "UPDATE PerformanceReviews SET ManagerComments = @ManagerComments, ManagerRating = @Rating, Status = 'Completed' WHERE ReviewID = @ReviewID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@ReviewID", reviewID)
                cmd.Parameters.AddWithValue("@ManagerComments", managerComments)
                cmd.Parameters.AddWithValue("@Rating", rating)
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        ' Success hone par waapis Performance page par bhej do
        Response.Redirect("Performance.aspx")
    End Sub

End Class