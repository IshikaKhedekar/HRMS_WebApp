Imports System.Web.Services
Imports System.Web.Script.Services
Imports System.Data.SqlClient

Partial Class AttendanceHandler
    Inherits System.Web.UI.Page

    <WebMethod()>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Shared Function GetEmployeeAttendance() As Object
        Dim userId As Integer = HttpContext.Current.Session("UserID")
        Dim list As New List(Of Dictionary(Of String, Object))()

        Dim constr As String = ConfigurationManager.ConnectionStrings("dbconnection").ConnectionString
        Using con As New SqlConnection(constr)
            Dim query As String = "SELECT LogID, LogDate, PunchInTime, PunchOutTime, Status, Notes 
                                   FROM AttendanceLog WHERE EmployeeID = @EmployeeID ORDER BY LogDate DESC"
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@EmployeeID", userId)
                con.Open()
                Dim rdr As SqlDataReader = cmd.ExecuteReader()
                While rdr.Read()
                    Dim item As New Dictionary(Of String, Object)()
                    item("LogID") = rdr("LogID")
                    item("LogDate") = Convert.ToDateTime(rdr("LogDate")).ToString("yyyy-MM-dd")
                    item("PunchInTime") = If(IsDBNull(rdr("PunchInTime")), "", rdr("PunchInTime").ToString())
                    item("PunchOutTime") = If(IsDBNull(rdr("PunchOutTime")), "", rdr("PunchOutTime").ToString())
                    item("Status") = rdr("Status").ToString()
                    item("Notes") = If(IsDBNull(rdr("Notes")), "", rdr("Notes").ToString())
                    list.Add(item)
                End While
            End Using
        End Using

        Return list
    End Function
End Class
