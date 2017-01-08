
Imports Contensive.Core
Imports Contensive.BaseClasses
Imports Xunit
Imports System

Namespace integrationTests

    Public Class cpBlockTests
        '
        Const layoutContent As String = "content"
        Const layoutA As String = "<div id=""aid"" class=""aclass"">" & layoutContent & "</div>"
        Const layoutB As String = "<div id=""bid"" class=""bclass"">" & layoutA & "</div>"
        Const layoutC As String = "<div id=""cid"" class=""cclass"">" & layoutB & "</div>"
        Const templateHeadTag As String = "<meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" >"
        Const templateA As String = "<html><head>" & templateHeadTag & "</head><body>" & layoutC & "</body></html>"
        '
        '====================================================================================================
        ' unit test - cp.blockNew
        '
        <Fact> Public Sub cpBlockInnerOuterTest()
            ' arrange
            Dim cpApp As New CPClass("testapp")
            Dim block As CPBlockClass = cpApp.BlockNew()
            Dim layoutInnerLength As Integer = layoutA.Length
            ' act
            block.Load(layoutC)
            ' assert
            Assert.Equal(block.GetHtml(), layoutC)
            '
            Assert.Equal(block.GetInner("#aid"), layoutContent)
            Assert.Equal(block.GetInner(".aclass"), layoutContent)
            '
            Assert.Equal(block.GetOuter("#aid"), layoutA)
            Assert.Equal(block.GetOuter(".aclass"), layoutA)
            '
            Assert.Equal(block.GetInner("#bid"), layoutA)
            Assert.Equal(block.GetInner(".bclass"), layoutA)
            '
            Assert.Equal(block.GetOuter("#bid"), layoutB)
            Assert.Equal(block.GetOuter(".bclass"), layoutB)
            '
            Assert.Equal(block.GetInner("#cid"), layoutB)
            Assert.Equal(block.GetInner(".cclass"), layoutB)
            '
            Assert.Equal(block.GetOuter("#cid"), layoutC)
            Assert.Equal(block.GetOuter(".cclass"), layoutC)
            '
            Assert.Equal(block.GetInner("#cid .bclass"), layoutA)
            Assert.Equal(block.GetInner(".cclass #bid"), layoutA)
            '
            Assert.Equal(block.GetOuter("#cid .bclass"), layoutB)
            Assert.Equal(block.GetOuter(".cclass #bid"), layoutB)
            '
            Assert.Equal(block.GetInner("#cid .aclass"), layoutContent)
            Assert.Equal(block.GetInner(".cclass #aid"), layoutContent)
            '
            Assert.Equal(block.GetOuter("#cid .aclass"), layoutA)
            Assert.Equal(block.GetOuter(".cclass #aid"), layoutA)
            '
            block.Clear()
            Assert.Equal(block.GetHtml(), "")
            '
            block.Clear()
            block.Load(layoutA)
            block.SetInner("#aid", "1234")
            Assert.Equal(block.GetHtml(), layoutA.Replace(layoutContent, "1234"))
            '
            block.Load(layoutB)
            block.SetOuter("#aid", "1234")
            Assert.Equal(block.GetHtml(), layoutB.Replace(layoutA, "1234"))
            '
            ' dispose
            cpApp.Dispose()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block load and clear
        ''' </summary>
        <Fact> Public Sub cpBlockClearTest()
            Dim cp As New CPClass("testapp")
            Dim block As CPBlockClass = cp.BlockNew()
            ' act
            block.Load(layoutC)
            Assert.Equal(layoutC, block.GetHtml())
            block.Clear()
            ' assert
            Assert.Equal(block.GetHtml(), "")
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block import
        ''' </summary>
        <Fact> Public Sub cpBlockImportFileTest()
            Dim cp As New CPClass("testapp")
            Dim filename As String = "cpBlockTest" & GetRandomInteger.ToString & ".html"
            Try
                Dim block As CPBlockClass = cp.BlockNew()
                ' act
                cp.core.appRootFiles.saveFile(filename, templateA)
                block.ImportFile(filename)
                ' assert
                Assert.Equal(layoutC, block.GetHtml())
            Catch ex As Exception
                '
            Finally
                cp.core.appRootFiles.deleteFile(filename)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block openCopy
        ''' </summary>
        <Fact> Public Sub cpBlockOpenCopyTest()
            Dim cp As New CPClass("testapp")
            Dim recordName As String = "cpBlockTest" & GetRandomInteger.ToString
            Dim recordId As Integer = 0
            Try
                ' arrange
                Dim block As CPBlockClass = cp.BlockNew()
                Dim cs As CPCSClass = cp.CSNew()
                ' act
                If (cs.Insert("copy content")) Then
                    recordId = cs.GetInteger("id")
                    cs.SetField("name", recordName)
                    cs.SetField("copy", layoutC)
                End If
                cs.Close()
                block.OpenCopy(recordName)
                ' assert
                Assert.Equal(layoutC, block.GetHtml())
            Finally
                cp.Content.DeleteRecords("copy content", "id=" & recordId)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block openFile
        ''' </summary>
        <Fact> Public Sub cpBlockOpenFileTest()
            Dim cp As New CPClass("testapp")
            Dim filename As String = "cpBlockTest" & GetRandomInteger.ToString & ".html"
            Try
                Dim block As CPBlockClass = cp.BlockNew()
                ' act
                cp.core.appRootFiles.saveFile(filename, layoutA)
                block.OpenFile(filename)
                ' assert
                Assert.Equal(layoutA, block.GetHtml())
            Finally
                cp.core.appRootFiles.deleteFile(filename)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block openCopy
        ''' </summary>
        <Fact> Public Sub cpBlockOpenLayoutTest()
            Dim cp As New CPClass("testapp")
            Dim recordName As String = "cpBlockTest" & GetRandomInteger.ToString
            Dim recordId As Integer = 0
            Try
                ' arrange
                Dim block As CPBlockClass = cp.BlockNew()
                Dim cs As CPCSClass = cp.CSNew()
                ' act
                If (cs.Insert("layouts")) Then
                    recordId = cs.GetInteger("id")
                    cs.SetField("name", recordName)
                    cs.SetField("layout", layoutC)
                End If
                cs.Close()
                block.OpenLayout(recordName)
                ' assert
                Assert.Equal(layoutC, block.GetHtml())
            Finally
                cp.Content.DeleteRecords("layouts", "id=" & recordId)
            End Try
        End Sub
        '
        '====================================================================================================
        ' unit test - cp.blockNew
        '
        <Fact> Public Sub cpBlockAppendPrependTest()
            ' arrange
            Dim cpApp As New CPClass("testapp")
            Dim block As CPBlockClass = cpApp.BlockNew()
            Dim layoutInnerLength As Integer = layoutA.Length
            ' act
            block.Clear()
            block.Append("1")
            block.Append("2")
            Assert.Equal(block.GetHtml(), "12")
            '
            block.Clear()
            block.Prepend("1")
            block.Prepend("2")
            Assert.Equal(block.GetHtml(), "21")
            '
            ' dispose
            '
            cpApp.Dispose()
        End Sub
        '
    End Class
    'Public Class cpBlockTests

    'End Class
End Namespace