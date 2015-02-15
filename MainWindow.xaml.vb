Imports System
Imports System.IO
Imports Microsoft.VisualBasic

Class MainWindow
    Public Class FileHelper
        Public Shared Function GetFilesRecursive(ByVal initial As String) As List(Of String)
            ' This list stores the results.
            Dim result As New List(Of String)


            ' This stack stores the directories to process.
            Dim stack As New Stack(Of String)

            ' Add the initial directory
            stack.Push(initial)

            ' Continue processing for each stacked directory
            Do While (stack.Count > 0)

                ' Get top directory string
                Dim dir As String = stack.Pop

                Try
                    ' Add all immediate file paths
                    result.AddRange(Directory.GetFiles(dir, "*.*"))

                    ' Loop through all subdirectories and add them to the stack.
                    Dim directoryName As String

                    For Each directoryName In Directory.GetDirectories(dir)
                        stack.Push(directoryName)
                    Next

                Catch ex As Exception
                End Try
            Loop

            Return result
        End Function
    End Class

    Private Sub Button_Patch_Click(sender As Object, e As RoutedEventArgs) Handles Button_Patch.Click

        If Len(My.Settings.oOCurrModLoc) < 1 Then
            MsgBox("You have not selected a mod. Please click the button 'Find Mod'.", vbOKOnly, "Mod Not Detected")
            Exit Sub
        End If

        Dim oOBasePath As String = My.Settings.oOGameDir
        Dim oOModPath As String = My.Settings.oOLastModLoc
        Dim dir As IO.DirectoryInfo
        Dim oOGameDirLoc As String

        If Len(My.Settings.oOGameDir) > 1 Then
            oOGameDirLoc = My.Settings.oOGameDir

            dir = New IO.DirectoryInfo(oOGameDirLoc)
            If dir.Exists Then
                TextBox_GameDir.Text = oOGameDirLoc
                My.Settings.oOGameDir = oOGameDirLoc
                My.Settings.Save()
            Else
                MsgBox("'" & oOGameDirLoc & "' does not exist!")
            End If
        End If

        dir = New IO.DirectoryInfo(oOBasePath)
        If dir.Exists = False Then
            MsgBox(oOBasePath)
            MsgBox("Game path is not valid. Please click the 'Find Game Dir' button.", vbOKOnly, "Game Not Found")
            Exit Sub
        End If

        dir = New IO.DirectoryInfo(oOModPath)
        If dir.Exists = False Then
            MsgBox("Mod path is not valid. Please click the 'Find Mod' button.", vbOKOnly, "Mod Not Found")
            Exit Sub
        End If



        ' Get recursive List of all files starting in this directory.
        Dim list As List(Of String) = FileHelper.GetFilesRecursive(Strings.Left(oOModPath, Len(oOModPath) - 1))

        RichTextBox_ModLines.Document.Blocks.Clear()

        ' Loop through and display each path.
        For Each path In list
            Dim oOTempStr As String = Right(path, Len(path) - Len(oOModPath))
            If oOTempStr <> "DarkestPatcher.txt" Then
                Call PatchFiles(oOBasePath, oOModPath, oOTempStr)
            End If
        Next

        MsgBox("Complete!")

    End Sub

    Private Sub PatchFiles(oOBasePath As String, oOModPath As String, oOFilePath As String)

        '################################################
        '# Works with:                                  #
        '# \scripts\effects.darkest                     #
        '# \heroes\[hero name]\[hero name].info.darkest #
        '# \heroes\[hero name]\[hero name].art.darkest  #
        '# \inventory\inventory.darkest                 #
        '################################################

        Dim oOSubFolder As String = Strings.Left(oOFilePath, InStrRev(oOFilePath, "\", -1, CompareMethod.Text))
        Dim oOSearchStr As String = ""
        Dim oOBaseLine As String = ""

        'MsgBox(oOBasePath & oOFilePath)
        'MsgBox(oOModPath & oOFilePath)

        If My.Computer.FileSystem.FileExists(oOBasePath & oOFilePath) Then
            Dim oOBaseText() As String = System.IO.File.ReadAllLines(oOBasePath & oOFilePath)
            Dim oOModText() As String = System.IO.File.ReadAllLines(oOModPath & oOFilePath)
            Dim i As Integer, y As Integer

            For Each oOModLine As String In oOModText
                If Strings.Left(oOModLine, 1) = "*" Or Len(oOModLine) < 1 Then GoTo SkipLine
                i = 0
                If oOSubFolder = "scripts\" Then
                    'MsgBox("Editing scripts")
                    oOSearchStr = Strings.Left(oOModLine, InStr(oOModLine, ".curio_result_type", CompareMethod.Text) + 17)

                    If Len(oOSearchStr) > 0 Then
                        For Each oOBaseLine In Filter(oOBaseText, oOSearchStr)
                            If oOBaseLine.StartsWith(oOSearchStr) Then
                                i = i + 1
                                'MsgBox("Lookfor: " & oOSearchStr & vbNewLine & vbNewLine & "Result: " & oOBaseLine)

                                RichTextBox_ModLines.AppendText(Environment.NewLine + "Replaced 'effects.darkest' line: " + Environment.NewLine + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                            End If
                        Next

                        If i < 1 Then MsgBox("Could not find: " & oOSearchStr)
                    End If

                ElseIf Strings.Left(oOSubFolder, 7) = "heroes\" Then
                    '    MsgBox("Editing heroes")
                    y = 0
                    Do Until y = 2
                        y = y + 1
                        If y = 1 Then oOSearchStr = "resistances:"
                        If y = 2 Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 2)) 'combat_skill: .id "mace_bash" .

                        If Len(oOSearchStr) > 0 Then
                            For Each oOBaseLine In Filter(oOBaseText, oOSearchStr)
                                If oOBaseLine.StartsWith(oOSearchStr) Then
                                    i = i + 1
                                    'MsgBox("Lookfor: " & oOSearchStr & vbNewLine & vbNewLine & "Result: " & oOBaseLine)

                                    RichTextBox_ModLines.AppendText(Environment.NewLine + "Replaced 'heroes' line: " + Environment.NewLine + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                    System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                                End If
                            Next

                            'If i < 1 Then MsgBox("Could not find: " & oOSearchStr)
                        End If
                    Loop


                ElseIf Strings.Left(oOSubFolder, 10) = "inventory\" Then
                    '    MsgBox("Editing inventory")
                    If oOBaseLine.StartsWith("inventory_system_config:") Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 2)) 'inventory_system_config: .type "raid" .
                    If oOBaseLine.StartsWith("inventory_item:") Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 3)) 'inventory_item: .type "gold" .id "" .

                    If Len(oOSearchStr) > 0 Then
                        For Each oOBaseLine In Filter(oOBaseText, oOSearchStr)
                            If oOBaseLine.StartsWith(oOSearchStr) Then
                                i = i + 1
                                'MsgBox("Lookfor: " & oOSearchStr & vbNewLine & vbNewLine & "Result: " & oOBaseLine)

                                RichTextBox_ModLines.AppendText(Environment.NewLine + "Replaced 'inventory.darkest' line: " + Environment.NewLine + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                            End If
                        Next

                        'If i < 1 Then MsgBox("Could not find: " & oOSearchStr)
                    End If

                Else
                    MsgBox("Not processing: " & oOSubFolder)
                    GoTo SkipFile
                End If
SkipLine:
            Next
SkipFile:
        Else
            MsgBox("File: " & oOBasePath & oOFilePath & " does not exist!")
        End If
    End Sub

    Function FindN(sFindWhat As String, sInputString As String, N As Integer) As Integer
        Dim J As Integer
        FindN = 0
        For J = 1 To N
            FindN = InStr(FindN + 1, sInputString, sFindWhat)
            If FindN = 0 Then Exit For
        Next
    End Function

    Private Sub Button_LoadVars_Click(sender As Object, e As RoutedEventArgs)

        My.Settings.oOCurrModLoc = ""
        My.Settings.Save()

        RichTextBox_ModLines.Document.Blocks.Clear()


        Button_Patch.Foreground = Brushes.Black
        Button_Patch.IsEnabled = False

        Dim oOGameDirLoc As String

        If Len(My.Settings.oOGameDir) > 1 Then
            oOGameDirLoc = My.Settings.oOGameDir

            Dim dir As New IO.DirectoryInfo(oOGameDirLoc)
            If dir.Exists Then
                TextBox_GameDir.Text = oOGameDirLoc
                My.Settings.oOGameDir = oOGameDirLoc
                My.Settings.Save()
            Else
                MsgBox("'" & oOGameDirLoc & "' does not exist!")
            End If
        Else
            MsgBox("No prior settings found. Please click the 'Find Game Dir' button.")
            'Dim oOEXELoc As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            'oOGameDirLoc = Right(oOEXELoc, Len(oOEXELoc) - 6)
        End If

    End Sub

    Private Sub Button_FindGameDir_Click(sender As Object, e As RoutedEventArgs)

        Dim fd As Microsoft.Win32.OpenFileDialog = New Microsoft.Win32.OpenFileDialog()
        Dim oOFolderStr As String

        fd.Title = "Find Game Directory"

        If Len(My.Settings.oOGameDir) > 0 Then
            fd.InitialDirectory = My.Settings.oOGameDir
        Else
            Dim oOEXELoc As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            fd.InitialDirectory = oOEXELoc
        End If

        fd.Filter = "All files (*.*)|*.*|All files (*.*)|*.*"
        fd.FilterIndex = 2
        fd.RestoreDirectory = True

        ' Set validate names and check file exists to false otherwise windows will
        ' not let you select "Folder Selection."
        fd.ValidateNames = False
        fd.CheckFileExists = False
        fd.CheckPathExists = True

        ' Always default to Folder Selection.
        fd.FileName = "Folder Selection."
        fd.ShowDialog()

        fd.FileName = Strings.Left(fd.FileName, Len(fd.FileName) - 17)
        oOFolderStr = fd.FileName

        If Len(oOFolderStr) > 0 Then
            TextBox_GameDir.Text = ""
            TextBox_GameDir.Text = oOFolderStr

            My.Settings.oOGameDir = oOFolderStr
            My.Settings.Save()
        Else
            MsgBox("Location invalid.", vbOKOnly, "No Location Selected")
        End If

    End Sub

    Private Sub Button_FindMod_Click(sender As Object, e As RoutedEventArgs)

        Dim fd As Microsoft.Win32.OpenFileDialog = New Microsoft.Win32.OpenFileDialog()
        Dim i As Integer
        Dim oOFolderStr As String

        fd.Title = "Find Mod Directory"

        If Len(My.Settings.oOLastModLoc) > 0 Then
            fd.InitialDirectory = My.Settings.oOLastModLoc
        Else
            Dim oOEXELoc As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)
            fd.InitialDirectory = oOEXELoc
        End If

        fd.Filter = "All files (*.*)|*.*|All files (*.*)|*.*"
        fd.FilterIndex = 2
        fd.RestoreDirectory = True

        ' Set validate names and check file exists to false otherwise windows will
        ' not let you select "Folder Selection."
        fd.ValidateNames = False
        fd.CheckFileExists = False
        fd.CheckPathExists = True

        ' Always default to Folder Selection.
        fd.FileName = "Folder Selection."
        fd.ShowDialog()

        fd.FileName = Strings.Left(fd.FileName, Len(fd.FileName) - 17)
        oOFolderStr = fd.FileName

        If Len(oOFolderStr) < 1 Then Exit Sub

        My.Settings.oOLastModLoc = oOFolderStr
        My.Settings.oOCurrModLoc = oOFolderStr
        My.Settings.Save()

        If My.Computer.FileSystem.FileExists(oOFolderStr & "DarkestPatcher.txt") Then
            Dim oOModText() As String = System.IO.File.ReadAllLines(oOFolderStr & "DarkestPatcher.txt")

            RichTextBox_ModLines.Document.Blocks.Clear()
            i = 0
            For Each oOModLine As String In oOModText
                i = i + 1
                If i > 1 Then
                    RichTextBox_ModLines.AppendText(Environment.NewLine + oOModLine)
                Else
                    RichTextBox_ModLines.AppendText(oOModLine)
                End If
            Next

            Button_Patch.Foreground = Brushes.White
            Button_Patch.IsEnabled = True
        Else
            If My.Computer.FileSystem.FileExists(oOFolderStr) Then

            Else
                MsgBox("'" & oOFolderStr & "' not found!")
            End If
        End If
    End Sub
End Class
