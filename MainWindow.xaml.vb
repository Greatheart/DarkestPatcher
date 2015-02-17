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

        RichTextBox_ModLines.AppendText(Environment.NewLine + "Starting File Patching of: " + oOModPath + Environment.NewLine)

        ' Loop through and display each path.
        For Each path In list
            Dim oOTempStr As String = Right(path, Len(path) - Len(oOModPath))
            If Strings.Right(oOTempStr, 18) <> "DarkestPatcher.txt" And Strings.Right(oOTempStr, 9) <> "Thumbs.db" Then
                RichTextBox_ModLines.AppendText(Environment.NewLine + "  Processing file: " + oOTempStr)
                Call PatchFiles(oOBasePath, oOModPath, oOTempStr)
            End If
        Next

        RichTextBox_ModLines.AppendText(Environment.NewLine + "Completed File Patching of: " + oOModPath + Environment.NewLine)

        MsgBox("Complete!")

    End Sub

    Private Sub PatchFiles(oOBasePath As String, oOModPath As String, oOFilePath As String)

        '########################################################
        '# Works with:                                          #
        '# \scripts\effects.darkest                             #
        '# \heroes\[hero name]\[hero name].info.darkest         #
        '# \heroes\[hero name]\[hero name].art.darkest          #
        '# \inventory\inventory.darkest                         #
        '# \monsters\[monster name]\[monster name].info.darkest #
        '# \monsters\[monster name]\[monster name].art.darkest  #
        '########################################################

        Dim oOSubFolder As String = Strings.Left(oOFilePath, InStrRev(oOFilePath, "\", -1, CompareMethod.Text))
        Dim oOSearchStr As String = ""
        Dim oOBaseLine As String = ""

        If My.Computer.FileSystem.FileExists(oOBasePath & oOFilePath) Then
            If Strings.Right(oOFilePath, 8) <> ".darkest" Then GoTo SkipFile

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
                            If oOBaseLine = oOModLine Then GoTo SkipLine
                            If oOBaseLine.StartsWith(oOSearchStr) Then
                                i = i + 1

                                RichTextBox_ModLines.AppendText(Environment.NewLine + "    Replaced 'effects.darkest' line: " + Environment.NewLine + "      Mod: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine + "      Ori: " & oOBaseLine.Replace(vbTab, " "))

                                System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                            Else

                            End If
                        Next

                        For Each oOBaseLine In Filter(oOBaseText, oOModLine)
                            If oOBaseLine.StartsWith(oOModLine) Then
                                'Skip the line it exists!
                            Else
                                If My.Settings.oOAppend = True Then
                                    RichTextBox_ModLines.AppendText(Environment.NewLine + "    Appended 'effects.darkest' line: " + Environment.NewLine + "      New: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                    System.IO.File.AppendAllText(oOBasePath & oOFilePath, Environment.NewLine + oOModLine)
                                Else
                                    RichTextBox_ModLines.AppendText(Environment.NewLine + "    Could not find: " + Environment.NewLine + "      " + oOModLine + Environment.NewLine)
                                End If
                            End If
                        Next
                    End If

                ElseIf Strings.Left(oOSubFolder, 7) = "heroes\" Then
                    '    MsgBox("Editing heroes")
                    y = 0
                    Do Until y = 3
                        y = y + 1
                        If y = 1 Then oOSearchStr = "resistances:"
                        If y = 2 Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 3)) 'combat_skill: .id "mace_bash" .level 0 .
                        If y = 3 Then oOSearchStr = "commonfx:"

                        If Len(oOSearchStr) > 0 Then
                            For Each oOBaseLine In Filter(oOBaseText, oOSearchStr)
                                If oOBaseLine = oOModLine Then GoTo SkipLine
                                If oOModLine.StartsWith(oOSearchStr) Then
                                    If oOBaseLine.StartsWith(oOSearchStr) Then

                                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    Replaced 'heroes' line: " + Environment.NewLine + "      Mod: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine + "      Ori: " & oOBaseLine.Replace(vbTab, " "))

                                        System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                                    End If
                                End If
                            Next

                            For Each oOBaseLine In Filter(oOBaseText, oOModLine)
                                If oOBaseLine.StartsWith(oOModLine) Then
                                    'Skip the line it exists!
                                Else
                                    If My.Settings.oOAppend = True Then
                                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    Appended 'heroes' line: " + Environment.NewLine + "      New: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                        System.IO.File.AppendAllText(oOBasePath & oOFilePath, Environment.NewLine + oOModLine)
                                    Else
                                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    Could not find: " + Environment.NewLine + "      " + oOModLine + Environment.NewLine)
                                    End If
                                End If
                            Next

                        End If
                    Loop


                ElseIf Strings.Left(oOSubFolder, 9) = "monsters\" Then
                    '    MsgBox("Editing monsters")
                    y = 0
                    Do Until y = 9
                        y = y + 1
                        If y = 1 Then oOSearchStr = "display:"
                        If y = 2 Then oOSearchStr = "enemy_type:"
                        If y = 3 Then oOSearchStr = "stats:"
                        If y = 4 Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 2)) 'skill: .id "slime" .
                        If y = 5 Then oOSearchStr = "personality:"
                        If y = 6 Then oOSearchStr = "loot:"
                        If y = 7 Then oOSearchStr = "initiative:"
                        If y = 8 Then oOSearchStr = "monster_brain:"
                        If y = 9 Then oOSearchStr = "commonfx:"

                        If Len(oOSearchStr) > 0 Then
                            For Each oOBaseLine In Filter(oOBaseText, oOSearchStr)
                                If oOBaseLine = oOModLine Then GoTo SkipLine
                                If oOModLine.StartsWith(oOSearchStr) Then
                                    If oOBaseLine.StartsWith(oOSearchStr) Then
                                        i = i + 1

                                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    Replaced 'monsters' line: " + Environment.NewLine + "      Mod: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine + "      Ori: " & oOBaseLine.Replace(vbTab, " "))

                                        System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                                    End If
                                End If
                            Next

                            For Each oOBaseLine In Filter(oOBaseText, oOModLine)
                                If oOBaseLine.StartsWith(oOModLine) Then
                                    'Skip the line it exists!
                                Else
                                    If My.Settings.oOAppend = True Then
                                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    Appended 'monsters' line: " + Environment.NewLine + "      New: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                        System.IO.File.AppendAllText(oOBasePath & oOFilePath, Environment.NewLine + oOModLine)
                                    Else
                                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    Could not find: " + Environment.NewLine + "      " + oOModLine + Environment.NewLine)
                                    End If
                                End If
                            Next
                        End If
                    Loop


                ElseIf Strings.Left(oOSubFolder, 10) = "inventory\" Then
                    '    MsgBox("Editing inventory")
                    If oOBaseLine.StartsWith("inventory_system_config:") Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 2)) 'inventory_system_config: .type "raid" .
                    If oOBaseLine.StartsWith("inventory_item:") Then oOSearchStr = Strings.Left(oOModLine, FindN(".", oOModLine, 3)) 'inventory_item: .type "gold" .id "" .

                    If Len(oOSearchStr) > 0 Then
                        For Each oOBaseLine In Filter(oOBaseText, oOSearchStr)
                            If oOBaseLine = oOModLine Then GoTo SkipLine
                            If oOBaseLine.StartsWith(oOSearchStr) Then
                                i = i + 1

                                RichTextBox_ModLines.AppendText(Environment.NewLine + "    Replaced 'inventory.darkest' line: " + Environment.NewLine + "      Mod: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine + "      Ori: " & oOBaseLine.Replace(vbTab, " "))

                                System.IO.File.WriteAllText(oOBasePath & oOFilePath, System.IO.File.ReadAllText(oOBasePath & oOFilePath).Replace(oOBaseLine, oOModLine))
                            End If
                        Next

                        For Each oOBaseLine In Filter(oOBaseText, oOModLine)
                            If oOBaseLine.StartsWith(oOModLine) Then
                                'Skip the line it exists!
                            Else
                                If My.Settings.oOAppend = True Then
                                    RichTextBox_ModLines.AppendText(Environment.NewLine + "    Appended 'inventory.darkest' line: " + Environment.NewLine + "      New: " + oOModLine.Replace(vbTab, " ") + Environment.NewLine)

                                    System.IO.File.AppendAllText(oOBasePath & oOFilePath, Environment.NewLine + oOModLine)
                                Else
                                    RichTextBox_ModLines.AppendText(Environment.NewLine + "    Could not find: " + Environment.NewLine + "      " + oOModLine + Environment.NewLine)
                                End If
                            End If
                        Next
                    End If

                Else
                    RichTextBox_ModLines.AppendText(Environment.NewLine + "    Folder: " + Environment.NewLine + "      " + oOSubFolder + " update is not supported! Folder skipped!" + Environment.NewLine)
                    'MsgBox("Not processing: " & oOSubFolder)
                    GoTo SkipFile
                End If
SkipLine:
            Next
        Else
            If My.Settings.oOAppend = True Then
                If My.Computer.FileSystem.DirectoryExists(oOBasePath & oOSubFolder) = False Then
                    RichTextBox_ModLines.AppendText(Environment.NewLine + "    Folder does not exist, copying folder: '" + Strings.Left(oOModPath, InStrRev(oOModPath, "\", -1, CompareMethod.Text)) + oOFilePath + "'" + Environment.NewLine)
                    My.Computer.FileSystem.CopyDirectory(oOModPath & oOSubFolder, oOBasePath & oOSubFolder, False)
                Else
                    If My.Computer.FileSystem.FileExists(oOBasePath & oOFilePath) = False Then
                        RichTextBox_ModLines.AppendText(Environment.NewLine + "    File does not exist, copying file: '" + oOModPath + oOFilePath + "'" + Environment.NewLine)
                        My.Computer.FileSystem.CopyFile(oOModPath & oOFilePath, oOBasePath & oOFilePath, False)
                    Else
                        MsgBox("Error Code: 'Inception'" & vbNewLine & "File: " & oOBasePath & oOFilePath & " exists and does not.... (0_0)")
                    End If
                End If

                GoTo SkipFile
            Else
                RichTextBox_ModLines.AppendText(Environment.NewLine + "    File: " & oOBasePath & oOFilePath & " does not exist!" + Environment.NewLine)
            End If
        End If
SkipFile:
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
            If My.Computer.FileSystem.DirectoryExists(Strings.Left(oOFolderStr, Len(oOFolderStr) - 1)) Then
                Button_Patch.Foreground = Brushes.White
                Button_Patch.IsEnabled = True

                RichTextBox_ModLines.Document.Blocks.Clear()

                RichTextBox_ModLines.AppendText(Environment.NewLine + "    'DarkestPatcher.txt' not found." + Environment.NewLine)
                RichTextBox_ModLines.AppendText(Environment.NewLine + "    Mod: '" + oOFolderStr + "' ready to patch!" + Environment.NewLine)

            Else
                MsgBox("'" & Strings.Left(oOFolderStr, Len(oOFolderStr) - 1) & "' not found!")
            End If
        End If
    End Sub

    Private Sub CheckBox_AppendData_Checked(sender As Object, e As RoutedEventArgs) Handles CheckBox_AppendData.Checked
        If CheckBox_AppendData.IsChecked = True Then
            My.Settings.oOAppend = True
            My.Settings.Save()
        Else
            My.Settings.oOAppend = False
            My.Settings.Save()
        End If
    End Sub

    Private Sub CheckBox_AppendData_UnChecked(sender As Object, e As RoutedEventArgs) Handles CheckBox_AppendData.Unchecked
        If CheckBox_AppendData.IsChecked = True Then
            My.Settings.oOAppend = True
            My.Settings.Save()
        Else
            My.Settings.oOAppend = False
            My.Settings.Save()
        End If
    End Sub

    Private Sub Window_Closed(ByValsender As Object, ByVal e As System.EventArgs) Handles Me.Closed
        My.Settings.oOAppend = False
        My.Settings.Save()
    End Sub

    Private Sub Window_Opened(ByValsender As Object, ByVal e As System.EventArgs) Handles Me.Loaded
        My.Settings.oOCurrModLoc = ""
        My.Settings.oOAppend = False
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
        End If
    End Sub
End Class
