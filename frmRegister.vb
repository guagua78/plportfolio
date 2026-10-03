Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports MySql.Data.MySqlClient

Public Class frmRegister
    Inherits Form

    ' --- WinAPI for Native Placeholder Banner (If needed) ---
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As Integer, <MarshalAs(UnmanagedType.LPWStr)> ByVal lParam As String) As Int32
    End Function

    Private Const EM_SETCUEBANNER As Integer = &H1501

    Private Sub SetPlaceholder(txt As TextBox, placeholder As String)
        If txt IsNot Nothing AndAlso Not txt.IsDisposed Then
            SendMessage(txt.Handle, EM_SETCUEBANNER, 0, placeholder)
        End If
    End Sub

    ' --- Form Load Event ---
    Private Sub frmRegister_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.WindowState = FormWindowState.Maximized

        ' Apply rounded corners to textboxes and button
        ApplyRoundedRegion(txtStudentNum, 8)
        ApplyRoundedRegion(txtFullName, 8)
        ApplyRoundedRegion(txtProgram, 8)
        ApplyRoundedRegion(txtEmail, 8)
        ApplyRoundedRegion(txtPassword, 8)
        ApplyRoundedRegion(txtConfirmPassword, 8)
        ApplyRoundedRegion(btnRegister, 8)

        ' Ensure password masking is enabled by default
        txtPassword.UseSystemPasswordChar = True
        txtConfirmPassword.UseSystemPasswordChar = True

        CenterLayout()
    End Sub

    ' --- Apply Rounded Corners Helper ---
    Private Sub ApplyRoundedRegion(ctrl As Control, radius As Integer)
        If ctrl IsNot Nothing Then
            Dim path As New GraphicsPath()
            Dim rect As New Rectangle(0, 0, ctrl.Width, ctrl.Height)
            Dim diameter As Integer = radius * 2

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90)
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90)
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90)
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90)
            path.CloseFigure()

            ctrl.Region = New Region(path)
        End If
    End Sub

    ' --- Toggle Visibility for Password ---
    Private Sub chkShowPassword_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowPassword.CheckedChanged
        If chkShowPassword.Checked Then
            txtPassword.UseSystemPasswordChar = False
            chkShowPassword.Text = "Hide"
        Else
            txtPassword.UseSystemPasswordChar = True
            chkShowPassword.Text = "Show"
        End If
    End Sub

    ' --- Toggle Visibility for Confirm Password ---
    Private Sub chkShowConfirmPassword_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowConfirmPassword.CheckedChanged
        If chkShowConfirmPassword.Checked Then
            txtConfirmPassword.UseSystemPasswordChar = False
            chkShowConfirmPassword.Text = "Hide"
        Else
            txtConfirmPassword.UseSystemPasswordChar = True
            chkShowConfirmPassword.Text = "Show"
        End If
    End Sub

    ' --- Register Button Click Logic ---
    Private Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        Dim email As String = txtEmail.Text.Trim().ToLower()
        Dim pass As String = txtPassword.Text
        Dim confirmPass As String = txtConfirmPassword.Text

        ' 1. Basic Empty Field Validation
        If String.IsNullOrWhiteSpace(txtStudentNum.Text) OrElse
           String.IsNullOrWhiteSpace(txtFullName.Text) OrElse
           String.IsNullOrWhiteSpace(txtProgram.Text) OrElse
           String.IsNullOrWhiteSpace(email) OrElse
           String.IsNullOrWhiteSpace(pass) OrElse
           String.IsNullOrWhiteSpace(confirmPass) Then

            MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 2. Email Domain Validation (@plp.edu.ph and @plpasig.edu.ph)
        If Not (email.EndsWith("@plp.edu.ph") OrElse email.EndsWith("@plpasig.edu.ph") OrElse email.EndsWith(".plp.edu.ph")) Then
            MessageBox.Show("Only institutional email addresses ending in @plp.edu.ph or @plpasig.edu.ph are allowed to register.",
                            "Invalid Email Domain", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtEmail.Focus()
            Return
        End If

        ' 3. Confirm Password Match Validation
        If pass <> confirmPass Then
            MessageBox.Show("Passwords do not match. Please re-enter your password.",
                            "Password Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtConfirmPassword.Clear()
            txtConfirmPassword.Focus()
            Return
        End If

        ' 4. Insert Record into MySQL Database
        Try
            OpenConnection()
            Dim query As String = "INSERT INTO tbl_students (student_number, full_name, course_section, email, password) " &
                                  "VALUES (@StudentNum, @FullName, @Course, @Email, @Password)"

            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@StudentNum", txtStudentNum.Text.Trim())
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim())
                cmd.Parameters.AddWithValue("@Course", txtProgram.Text.Trim())
                cmd.Parameters.AddWithValue("@Email", email)
                cmd.Parameters.AddWithValue("@Password", pass)

                cmd.ExecuteNonQuery()
                MessageBox.Show("Registration successful! Welcome to Portfolioboxd.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Clear fields after success
                txtStudentNum.Clear()
                txtFullName.Clear()
                txtProgram.Clear()
                txtEmail.Clear()
                txtPassword.Clear()
                txtConfirmPassword.Clear()
            End Using
        Catch ex As Exception
            MessageBox.Show("Registration failed: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            CloseConnection()
        End Try
    End Sub

    ' --- Draw Frosted Glass Effect on pnlCard ---
    Private Sub pnlCard_Paint(sender As Object, e As PaintEventArgs) Handles pnlCard.Paint
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect As New Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1)
        Dim radius As Integer = 16

        Using path As GraphicsPath = GetRoundedPath(rect, radius)
            ' Dark Glass Fill
            Using fillBrush As New SolidBrush(Color.FromArgb(180, 20, 24, 28))
                e.Graphics.FillPath(fillBrush, path)
            End Using

            ' White Subtle Outer Border
            Using borderPen As New Pen(Color.FromArgb(80, 255, 255, 255), 1)
                e.Graphics.DrawPath(borderPen, path)
            End Using
        End Using
    End Sub

    Private Function GetRoundedPath(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim diameter As Integer = radius * 2

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90)
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90)
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90)
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90)
        path.CloseFigure()

        Return path
    End Function

    ' --- Layout Centering Logic ---
    Private Sub CenterLayout()
        If pnlCard IsNot Nothing Then
            pnlCard.Left = (Me.ClientSize.Width - pnlCard.Width) \ 2
            pnlCard.Top = ((Me.ClientSize.Height - pnlCard.Height) \ 2) + 20
        End If

        ' Position headline if present on form
        Dim headlineControls = Me.Controls.Find("lblHeadline", True)
        If headlineControls.Length > 0 AndAlso pnlCard IsNot Nothing Then
            Dim lblHeadline As Control = headlineControls(0)
            lblHeadline.Left = (Me.ClientSize.Width - lblHeadline.Width) \ 2
            lblHeadline.Top = pnlCard.Top - lblHeadline.Height - 20
        End If
    End Sub

    Private Sub frmRegister_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        CenterLayout()
    End Sub
End Class