Imports MySql.Data.MySqlClient
Imports System.Data

Module dbConnection
    Public connString As String = "Server=localhost;Database=db_plportfolio;Uid=root;Pwd=;"
    Public conn As New MySqlConnection(connString)

    Public CurrentStudentID As Integer = 0
    Public CurrentStudentName As String = ""
    Public CurrentStudentNumber As String = ""

    Public Sub OpenConnection()
        If conn.State = ConnectionState.Closed Then
            conn.Open()
        End If
    End Sub

    Public Sub CloseConnection()
        If conn.State = ConnectionState.Open Then
            conn.Close()
        End If
    End Sub
End Module