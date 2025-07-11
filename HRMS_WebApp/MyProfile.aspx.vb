Imports System.Configuration
Imports System.Data.SqlClient
Imports System.IO ' File operations ke liye zaroori

Public Class MyProfile
    Inherits System.Web.UI.Page

    Private targetUserID As Integer
    ' photoPathFromDB variable ki ab zaroorat nahi hai. imgProfile.ImageUrl directly set hoga.

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        ' Dual-Mode Logic (Self-View vs. HR/Admin Edit View)
        If Request.QueryString("UserID") IsNot Nothing Then
            Dim loggedInUserRoleID As Integer = CInt(Session("RoleID"))
            If loggedInUserRoleID = 1 OrElse loggedInUserRoleID = 2 Then ' SuperAdmin or HR
                targetUserID = CInt(Request.QueryString("UserID"))
                btnEdit.Visible = True
                Me.Title = "Edit Employee Profile"
            Else
                Response.Redirect("EmployeeDirectory.aspx")
            End If
        Else
            targetUserID = CInt(Session("UserID"))
            btnEdit.Visible = True
        End If

        If Not IsPostBack Then
            LoadUserProfile()
            ' imgProfile.ImageUrl ab LoadUserProfile() ke andar set hoga
        End If
    End Sub

    Private Sub LoadUserProfile()
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT u.*, m.Name as ManagerName FROM Users u LEFT JOIN Users m ON u.ManagerID = m.UserID WHERE u.UserID = @UserID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@UserID", targetUserID)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' --- PHOTO PATH KO YAHAN SET KAREIN ---
                        Dim photoPath As String = GetString(reader, "PhotoPath", defaultVal:="Images/default-user.png")
                        imgProfile.ImageUrl = photoPath
                        ' ------------------------------------

                        lblEmpName.InnerText = GetString(reader, "Name")
                        lblDesignationDept.InnerText = $"{GetString(reader, "Designation")}, {GetString(reader, "Department")}"
                        lblDob.InnerText = GetDateString(reader, "DateOfBirth")
                        lblGender.InnerText = GetString(reader, "Gender")
                        lblMaritalStatus.InnerText = GetString(reader, "MaritalStatus")
                        lblNationality.InnerText = GetString(reader, "Nationality")
                        lblBloodGroup.InnerText = GetString(reader, "BloodGroup")
                        lblMobile.InnerText = GetString(reader, "Mobile")
                        lblEmail.InnerText = GetString(reader, "Email") ' Assuming Email is personal email for display
                        lblAddress.InnerText = GetString(reader, "CurrentAddress")
                        lblEmergencyName.InnerText = GetString(reader, "EmergencyContactName")
                        lblEmergencyPhone.InnerText = GetString(reader, "EmergencyContactPhone")
                        lblEmpID.InnerText = "EMP" & GetString(reader, "UserID")
                        lblOfficialEmail.InnerText = GetString(reader, "Email") ' Assuming Official Email is also same as Email for display
                        lblDoj.InnerText = GetDateString(reader, "DateOfJoining")
                        lblManager.InnerText = GetString(reader, "ManagerName")
                        lblBankName.InnerText = GetString(reader, "BankName")
                        lblAccountNumber.InnerText = GetString(reader, "AccountNumber")
                        lblIFSC.InnerText = GetString(reader, "IFSCCode")

                        ddlMaritalStatus.SelectedValue = GetString(reader, "MaritalStatus", "Single")
                        txtNationality.Text = GetString(reader, "Nationality", returnEmpty:=True)
                        txtBloodGroup.Text = GetString(reader, "BloodGroup", returnEmpty:=True)
                        txtMobile.Text = GetString(reader, "Mobile", returnEmpty:=True)
                        txtEmail.Text = GetString(reader, "Email", returnEmpty:=True)
                        txtAddress.Text = GetString(reader, "CurrentAddress", returnEmpty:=True)
                        txtEmergencyName.Text = GetString(reader, "EmergencyContactName", returnEmpty:=True)
                        txtEmergencyPhone.Text = GetString(reader, "EmergencyContactPhone", returnEmpty:=True)
                    End If
                End Using
            End Using
        End Using
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim userID As Integer = targetUserID
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "UPDATE Users SET MaritalStatus=@MaritalStatus, Nationality=@Nationality, BloodGroup=@BloodGroup, Mobile=@Mobile, CurrentAddress=@Address, EmergencyContactName=@EName, EmergencyContactPhone=@EPhone WHERE UserID=@UserID"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@UserID", userID)
                cmd.Parameters.AddWithValue("@MaritalStatus", ddlMaritalStatus.SelectedValue)
                cmd.Parameters.AddWithValue("@Nationality", txtNationality.Text.Trim())
                cmd.Parameters.AddWithValue("@BloodGroup", txtBloodGroup.Text.Trim())
                cmd.Parameters.AddWithValue("@Mobile", txtMobile.Text.Trim())
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim())
                cmd.Parameters.AddWithValue("@EName", txtEmergencyName.Text.Trim())
                cmd.Parameters.AddWithValue("@EPhone", txtEmergencyPhone.Text.Trim())
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using
        LoadUserProfile()
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "SwitchToViewMode", "setEditMode(false);", True)
    End Sub

    Protected Sub btnUploadPhoto_Click(sender As Object, e As EventArgs) Handles btnUploadPhoto.Click
        If fileUploadPhoto.HasFile Then
            Try
                Dim fileExtension As String = Path.GetExtension(fileUploadPhoto.FileName).ToLower()
                If fileExtension = ".jpg" OrElse fileExtension = ".jpeg" OrElse fileExtension = ".png" Then
                    Dim fileName As String = targetUserID.ToString() & "_" & DateTime.Now.ToString("yyyyMMddHHmmss") & fileExtension
                    Dim relativePathDb As String = "EmployeePhotos/" & fileName ' Database ke liye path
                    Dim serverPath As String = Server.MapPath("~/EmployeePhotos/") & fileName ' Server par save karne ke liye path

                    fileUploadPhoto.SaveAs(serverPath)

                    Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
                    Dim query As String = "UPDATE Users SET PhotoPath = @PhotoPath WHERE UserID = @UserID"
                    Using conn As New SqlConnection(connStr)
                        Using cmd As New SqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@PhotoPath", relativePathDb)
                            cmd.Parameters.AddWithValue("@UserID", targetUserID)
                            conn.Open()
                            cmd.ExecuteNonQuery()
                        End Using
                    End Using

                    lblPhotoMessage.Text = "Photo uploaded successfully!"
                    lblPhotoMessage.ForeColor = Drawing.Color.Green
                    ' Page ko refresh karo taaki nayi photo dikhe
                    Response.Redirect(Request.RawUrl)
                Else
                    lblPhotoMessage.Text = "Only JPG or PNG images are allowed."
                    lblPhotoMessage.ForeColor = Drawing.Color.Red
                End If
            Catch ex As Exception
                lblPhotoMessage.Text = "Error uploading photo: " & ex.Message
                lblPhotoMessage.ForeColor = Drawing.Color.Red
            End Try
        Else
            lblPhotoMessage.Text = "Please select a file to upload."
            lblPhotoMessage.ForeColor = Drawing.Color.Red
        End If
    End Sub

    ' Helper functions to handle NULL from database
    Private Function GetString(ByVal reader As SqlDataReader, ByVal colName As String, Optional defaultVal As String = "N/A", Optional returnEmpty As Boolean = False) As String
        If Not reader.IsDBNull(reader.GetOrdinal(colName)) Then
            Return reader(colName).ToString()
        Else
            If returnEmpty Then Return String.Empty Else Return defaultVal
        End If
    End Function

    Private Function GetDateString(ByVal reader As SqlDataReader, ByVal colName As String) As String
        If Not reader.IsDBNull(reader.GetOrdinal(colName)) Then
            Return Convert.ToDateTime(reader(colName)).ToString("dd-MMM-yyyy")
        End If
        Return "N/A"
    End Function

End Class