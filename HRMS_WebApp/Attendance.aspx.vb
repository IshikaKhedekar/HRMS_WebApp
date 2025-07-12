Imports System
Imports System.Configuration
Imports System.Data.SqlClient

Public Class Attendance
    Inherits System.Web.UI.Page
    ' === POLICY DEFINITIONS ===
    ' (Aap in values ko baad me Web.config ya database se laa sakti hain)
    ReadOnly Property LATE_MARK_TIME As TimeSpan = New TimeSpan(9, 31, 0)      ' 09:31:00 AM
    ReadOnly Property EARLY_LEAVING_TIME As TimeSpan = New TimeSpan(18, 0, 0) ' 06:00:00 PM
    ReadOnly Property MINIMUM_HOURS_FOR_FULL_DAY As Double = 6.0
    ReadOnly Property MINIMUM_HOURS_FOR_HALF_DAY As Double = 3.0
    ReadOnly Property LATE_COUNT_FOR_LEAVE_DEDUCTION As Integer = 3
    ' ==========================

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then Response.Redirect("~/Login.aspx") : Return

        If Not IsPostBack Then
            LoadTodaysAttendance()
            LoadAttendanceHistory()
        End If
    End Sub

    ' Helper function to format TimeSpan for display in Repeater
    Public Function FormatTime(ByVal timeData As Object) As String
        If IsDBNull(timeData) OrElse timeData Is Nothing Then
            Return "-"
        End If
        Dim timeSpan As TimeSpan = CType(timeData, TimeSpan)
        Return (Date.Today + timeSpan).ToString("hh:mm tt")
    End Function

    ' Helper function to get CSS badge class based on status
    Public Function GetStatusBadge(ByVal status As String) As String
        Select Case status
            Case "Present"
                Return "bg-success"
            Case "Absent"
                Return "bg-danger"
            Case "Half Day"
                Return "bg-warning text-dark"
            Case Else
                Return "bg-secondary"
        End Select
    End Function

    Private Sub LoadTodaysAttendance()
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim today As Date = Date.Today
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT PunchInTime, PunchOutTime FROM AttendanceLog WHERE EmployeeID = @EmployeeID AND LogDate = @LogDate"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                cmd.Parameters.AddWithValue("@LogDate", today)
                conn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    ' Set default button states
                    btnPunchIn.Enabled = True
                    btnPunchOut.Enabled = False

                    If reader.Read() Then
                        ' Record for today exists
                        If Not IsDBNull(reader("PunchInTime")) Then
                            lblPunchInTime.Text = FormatTime(reader("PunchInTime"))
                            btnPunchIn.Enabled = False
                            btnPunchIn.Text = "Punched In"
                            btnPunchOut.Enabled = True ' Can now punch out
                        End If

                        If Not IsDBNull(reader("PunchOutTime")) Then
                            lblPunchOutTime.Text = FormatTime(reader("PunchOutTime"))
                            btnPunchOut.Enabled = False
                            btnPunchOut.Text = "Punched Out"
                        End If
                    End If
                End Using
            End Using
        End Using
    End Sub

    Private Sub LoadAttendanceHistory()
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT TOP 30 LogDate, PunchInTime, PunchOutTime, Status FROM AttendanceLog WHERE EmployeeID = @EmployeeID ORDER BY LogDate DESC"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                rptAttendanceHistory.DataSource = cmd.ExecuteReader()
                rptAttendanceHistory.DataBind()
            End Using
        End Using
    End Sub

    Protected Sub btnPunchIn_Click(sender As Object, e As EventArgs) Handles btnPunchIn.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim today As Date = Date.Today
        Dim currentTime As DateTime = DateTime.Now
        Dim isLate As Boolean = False
        Dim remarks As String = ""

        ' --- LATE MARKING LOGIC ---
        If currentTime.TimeOfDay > LATE_MARK_TIME Then
            isLate = True
            remarks = "Marked as Late."
        End If
        ' -------------------------

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        ' Insert a new record with IsLate flag
        Dim query As String = "INSERT INTO AttendanceLog (EmployeeID, LogDate, PunchInTime, Status, IsLate, Remarks) VALUES (@EID, @Date, @Time, 'Present', @IsLate, @Remarks)"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EID", employeeID)
                cmd.Parameters.AddWithValue("@Date", today)
                cmd.Parameters.AddWithValue("@Time", currentTime.TimeOfDay)
                cmd.Parameters.AddWithValue("@IsLate", isLate)
                cmd.Parameters.AddWithValue("@Remarks", If(String.IsNullOrEmpty(remarks), DBNull.Value, remarks))
                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        lblPunchMessage.Text = "Successfully Punched In at " & currentTime.ToString("hh:mm tt")
        If isLate Then
            lblPunchMessage.Text &= " (You are late today)."
        End If
        lblPunchMessage.ForeColor = Drawing.Color.Green
        LoadTodaysAttendance()
        LoadAttendanceHistory()
    End Sub

    ' REPLACE THE EXISTING btnPunchOut_Click with this NEW version

    ' REPLACE the existing btnPunchOut_Click with this new version
    Protected Sub btnPunchOut_Click(sender As Object, e As EventArgs) Handles btnPunchOut.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim today As Date = Date.Today
        Dim currentTime As DateTime = DateTime.Now

        Dim isEarly As Boolean = False
        Dim finalStatus As String = "Present"
        Dim remarks As String = ""
        Dim punchInTime As TimeSpan
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString

        Using conn As New SqlConnection(connStr)
            conn.Open()
            ' First, get PunchInTime to calculate total hours
            Dim getTimeQuery As String = "SELECT PunchInTime, Remarks FROM AttendanceLog WHERE EmployeeID = @EID AND LogDate = @Date"
            Using cmdGet As New SqlCommand(getTimeQuery, conn)
                cmdGet.Parameters.AddWithValue("@EID", employeeID)
                cmdGet.Parameters.AddWithValue("@Date", today)
                Using reader As SqlDataReader = cmdGet.ExecuteReader()
                    If reader.Read() Then
                        punchInTime = CType(reader("PunchInTime"), TimeSpan)
                        remarks = If(IsDBNull(reader("Remarks")), "", reader("Remarks").ToString()) ' Get existing remarks
                    End If
                End Using
            End Using

            ' --- HALF-DAY & EARLY-LEAVING LOGIC ---
            Dim totalWorkSpan As TimeSpan = currentTime.TimeOfDay - punchInTime
            If totalWorkSpan.TotalHours < MINIMUM_HOURS_FOR_FULL_DAY AndAlso totalWorkSpan.TotalHours >= MINIMUM_HOURS_FOR_HALF_DAY Then
                finalStatus = "Half Day"
                remarks &= " Marked as Half Day."
            ElseIf totalWorkSpan.TotalHours < MINIMUM_HOURS_FOR_HALF_DAY Then
                finalStatus = "Absent" ' Too short to be even a half day
                remarks &= " Marked as Absent (Short Hours)."
            End If

            If currentTime.TimeOfDay < EARLY_LEAVING_TIME Then
                isEarly = True
                remarks &= " Marked as Early Leaving."
            End If
            ' ------------------------------------

            ' Update the record
            Dim updateQuery As String = "UPDATE AttendanceLog SET PunchOutTime = @Time, Status = @Status, IsEarlyLeaving = @IsEarly, Remarks = @Remarks WHERE EmployeeID = @EID AND LogDate = @Date"
            Using cmdUpdate As New SqlCommand(updateQuery, conn)
                cmdUpdate.Parameters.AddWithValue("@EID", employeeID)
                cmdUpdate.Parameters.AddWithValue("@Date", today)
                cmdUpdate.Parameters.AddWithValue("@Time", currentTime.TimeOfDay)
                cmdUpdate.Parameters.AddWithValue("@Status", finalStatus)
                cmdUpdate.Parameters.AddWithValue("@IsEarly", isEarly)
                cmdUpdate.Parameters.AddWithValue("@Remarks", remarks.Trim())
                cmdUpdate.ExecuteNonQuery()
            End Using
        End Using

        ' --- LATE POLICY ENFORCEMENT ---
        CheckAndApplyLatePolicy(employeeID)
        ' -----------------------------

        lblPunchMessage.Text = "Successfully Punched Out at " & currentTime.ToString("hh:mm tt")
        lblPunchMessage.ForeColor = Drawing.Color.Blue
        LoadTodaysAttendance()
        LoadAttendanceHistory()
    End Sub

    ' --- Regularization Logic ---

    Protected Sub btnShowRegularizeModal_Click(sender As Object, e As EventArgs)
        txtDateToCorrect.Text = DateTime.Today.ToString("yyyy-MM-dd")
        txtNewPunchIn.Text = ""
        txtNewPunchOut.Text = ""
        txtReason.Text = ""
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "OpenRegularizeModal", "var myModal = new bootstrap.Modal(document.getElementById('regularizeModal')); myModal.show();", True)
    End Sub

    Protected Sub btnSendRequest_Click(sender As Object, e As EventArgs) Handles btnSendRequest.Click
        Dim employeeID As Integer = CInt(Session("UserID"))
        Dim dateToCorrect As Date
        Dim reason As String = txtReason.Text.Trim()

        If Not Date.TryParse(txtDateToCorrect.Text, dateToCorrect) OrElse String.IsNullOrEmpty(reason) Then
            ' You can add a label in the modal to show this error
            Return
        End If

        Dim managerID As Integer = GetManagerID(employeeID)
        If managerID = 0 Then
            ' Handle case where employee has no manager
            Return
        End If

        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "INSERT INTO RegularizationRequests (EmployeeID, ManagerID, DateToCorrect, NewPunchIn, NewPunchOut, Reason) VALUES (@EID, @MID, @Date, @In, @Out, @Reason)"

        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EID", employeeID)
                cmd.Parameters.AddWithValue("@MID", managerID)
                cmd.Parameters.AddWithValue("@Date", dateToCorrect)
                cmd.Parameters.AddWithValue("@Reason", reason)

                If String.IsNullOrWhiteSpace(txtNewPunchIn.Text) Then
                    cmd.Parameters.AddWithValue("@In", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@In", TimeSpan.Parse(txtNewPunchIn.Text))
                End If

                If String.IsNullOrWhiteSpace(txtNewPunchOut.Text) Then
                    cmd.Parameters.AddWithValue("@Out", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@Out", TimeSpan.Parse(txtNewPunchOut.Text))
                End If

                conn.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "CloseRegularizeModal", "var myModal = bootstrap.Modal.getInstance(document.getElementById('regularizeModal')); if(myModal) myModal.hide();", True)
        lblPunchMessage.Text = "Your regularization request has been sent to your manager."
        lblPunchMessage.ForeColor = Drawing.Color.Green
    End Sub

    Private Function GetManagerID(ByVal employeeID As Integer) As Integer
        Dim managerID As Integer = 0
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim query As String = "SELECT ManagerID FROM Users WHERE UserID = @EmployeeID"
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                conn.Open()
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    managerID = CInt(result)
                End If
            End Using
        End Using
        Return managerID
    End Function
    Private Sub CheckAndApplyLatePolicy(ByVal employeeID As Integer)
        Dim connStr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Dim currentMonthStart As New Date(Date.Today.Year, Date.Today.Month, 1)

        ' Get count of late marks in the current month
        Dim lateCountQuery As String = "SELECT COUNT(*) FROM AttendanceLog WHERE EmployeeID = @EID AND IsLate = 1 AND LogDate >= @MonthStart"
        Dim lateCount As Integer
        Using conn As New SqlConnection(connStr)
            Using cmd As New SqlCommand(lateCountQuery, conn)
                cmd.Parameters.AddWithValue("@EID", employeeID)
                cmd.Parameters.AddWithValue("@MonthStart", currentMonthStart)
                conn.Open()
                lateCount = CInt(cmd.ExecuteScalar())
            End Using
        End Using

        ' Check if policy needs to be applied
        ' We check for a multiple of the limit (e.g., at 3 lates, 6 lates, 9 lates etc.)
        If lateCount > 0 AndAlso lateCount Mod LATE_COUNT_FOR_LEAVE_DEDUCTION = 0 Then
            ' Check if leave was already deducted for this specific late count
            Dim remarkToFind As String = "Half-day leave deducted for " & lateCount & " late marks."
            Dim checkRemarkQuery As String = "SELECT COUNT(*) FROM Leaves WHERE EmployeeID = @EID AND Reason = @Reason"
            Dim alreadyDeducted As Boolean
            Using conn As New SqlConnection(connStr)
                Using cmd As New SqlCommand(checkRemarkQuery, conn)
                    cmd.Parameters.AddWithValue("@EID", employeeID)
                    cmd.Parameters.AddWithValue("@Reason", remarkToFind)
                    conn.Open()
                    alreadyDeducted = (CInt(cmd.ExecuteScalar()) > 0)
                End Using
            End Using

            If Not alreadyDeducted Then
                ' Deduct 0.5 Casual Leave
                Dim deductLeaveQuery As String = "INSERT INTO Leaves (EmployeeID, LeaveType, StartDate, EndDate, Reason, Status) VALUES (@EID, 'Casual Leave', @Date, @Date, @Reason, 'Approved')"
                ' Note: We are using a simplified leave entry. You might need to adjust this for your Leaves table structure.
                Using conn As New SqlConnection(connStr)
                    Using cmd As New SqlCommand(deductLeaveQuery, conn)
                        cmd.Parameters.AddWithValue("@EID", employeeID)
                        cmd.Parameters.AddWithValue("@Date", Date.Today)
                        cmd.Parameters.AddWithValue("@Reason", remarkToFind)
                        conn.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
                ' You can also add a notification for the employee here
            End If
        End If
    End Sub

End Class