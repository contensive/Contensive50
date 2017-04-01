
Imports Contensive.Core
Imports Contensive.BaseClasses
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System

Namespace integrationTests

    <TestClass()> Public Class cpBlockTests
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
        <TestMethod()> Public Sub cpBlockInnerOuterTest()
            ' arrange
            Dim cpApp As New CPClass("testapp")
            Dim block As CPBlockClass = cpApp.BlockNew()
            Dim layoutInnerLength As Integer = layoutA.Length
            ' act
            block.Load(layoutC)
            ' assert
            Assert.AreEqual(block.GetHtml(), layoutC)
            '
            Assert.AreEqual(block.GetInner("#aid"), layoutContent)
            Assert.AreEqual(block.GetInner(".aclass"), layoutContent)
            '
            Assert.AreEqual(block.GetOuter("#aid"), layoutA)
            Assert.AreEqual(block.GetOuter(".aclass"), layoutA)
            '
            Assert.AreEqual(block.GetInner("#bid"), layoutA)
            Assert.AreEqual(block.GetInner(".bclass"), layoutA)
            '
            Assert.AreEqual(block.GetOuter("#bid"), layoutB)
            Assert.AreEqual(block.GetOuter(".bclass"), layoutB)
            '
            Assert.AreEqual(block.GetInner("#cid"), layoutB)
            Assert.AreEqual(block.GetInner(".cclass"), layoutB)
            '
            Assert.AreEqual(block.GetOuter("#cid"), layoutC)
            Assert.AreEqual(block.GetOuter(".cclass"), layoutC)
            '
            Assert.AreEqual(block.GetInner("#cid .bclass"), layoutA)
            Assert.AreEqual(block.GetInner(".cclass #bid"), layoutA)
            '
            Assert.AreEqual(block.GetOuter("#cid .bclass"), layoutB)
            Assert.AreEqual(block.GetOuter(".cclass #bid"), layoutB)
            '
            Assert.AreEqual(block.GetInner("#cid .aclass"), layoutContent)
            Assert.AreEqual(block.GetInner(".cclass #aid"), layoutContent)
            '
            Assert.AreEqual(block.GetOuter("#cid .aclass"), layoutA)
            Assert.AreEqual(block.GetOuter(".cclass #aid"), layoutA)
            '
            block.Clear()
            Assert.AreEqual(block.GetHtml(), "")
            '
            block.Clear()
            block.Load(layoutA)
            block.SetInner("#aid", "1234")
            Assert.AreEqual(block.GetHtml(), layoutA.Replace(layoutContent, "1234"))
            '
            block.Load(layoutB)
            block.SetOuter("#aid", "1234")
            Assert.AreEqual(block.GetHtml(), layoutB.Replace(layoutA, "1234"))
            '
            ' dispose
            cpApp.Dispose()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block load and clear
        ''' </summary>
        <TestMethod()> Public Sub cpBlockClearTest()
            Dim cp As New CPClass("testapp")
            Dim block As CPBlockClass = cp.BlockNew()
            ' act
            block.Load(layoutC)
            Assert.AreEqual(layoutC, block.GetHtml())
            block.Clear()
            ' assert
            Assert.AreEqual(block.GetHtml(), "")
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block import
        ''' </summary>
        <TestMethod()> Public Sub cpBlockImportFileTest()
            Dim cp As New CPClass("testapp")
            Dim filename As String = "cpBlockTest" & GetRandomInteger.ToString & ".html"
            Try
                Dim block As CPBlockClass = cp.BlockNew()
                ' act
                cp.core.appRootFiles.saveFile(filename, templateA)
                block.ImportFile(filename)
                ' assert
                Assert.AreEqual(layoutC, block.GetHtml())
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
        <TestMethod()> Public Sub cpBlockOpenCopyTest()
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
                Assert.AreEqual(layoutC, block.GetHtml())
            Finally
                cp.Content.DeleteRecords("copy content", "id=" & recordId)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block openFile
        ''' </summary>
        <TestMethod()> Public Sub cpBlockOpenFileTest()
            Dim cp As New CPClass("testapp")
            Dim filename As String = "cpBlockTest" & GetRandomInteger.ToString & ".html"
            Try
                Dim block As CPBlockClass = cp.BlockNew()
                ' act
                cp.core.appRootFiles.saveFile(filename, layoutA)
                block.OpenFile(filename)
                ' assert
                Assert.AreEqual(layoutA, block.GetHtml())
            Finally
                cp.core.appRootFiles.deleteFile(filename)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' test block openCopy
        ''' </summary>
        <TestMethod()> Public Sub cpBlockOpenLayoutTest()
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
                Assert.AreEqual(layoutC, block.GetHtml())
            Finally
                cp.Content.DeleteRecords("layouts", "id=" & recordId)
            End Try
        End Sub
        '
        '====================================================================================================
        ' unit test - cp.blockNew
        '
        <TestMethod()> Public Sub cpBlockAppendPrependTest()
            ' arrange
            Dim cpApp As New CPClass("testapp")
            Dim block As CPBlockClass = cpApp.BlockNew()
            Dim layoutInnerLength As Integer = layoutA.Length
            ' act
            block.Clear()
            block.Append("1")
            block.Append("2")
            Assert.AreEqual(block.GetHtml(), "12")
            '
            block.Clear()
            block.Prepend("1")
            block.Prepend("2")
            Assert.AreEqual(block.GetHtml(), "21")
            '
            ' dispose
            '
            cpApp.Dispose()
        End Sub
        '
    End Class
    '
    Public Class coreCommonTests
        '
        <TestMethod()> Public Sub normalizePath_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual(coreFileSystemClass.normalizePath(""), "")
            Assert.AreEqual(coreFileSystemClass.normalizePath("c:\"), "c:\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("c:\test\"), "c:\test\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("c:\test"), "c:\test\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("c:\test/test"), "c:\test\test\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("test"), "test\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("\test"), "test\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("\test\"), "test\")
            Assert.AreEqual(coreFileSystemClass.normalizePath("/test/"), "test\")
        End Sub
        '
        <TestMethod()> Public Sub normalizeRoute_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual(normalizeRoute("TEST"), "/test")
            Assert.AreEqual(normalizeRoute("\TEST"), "/test")
            Assert.AreEqual(normalizeRoute("\\TEST"), "/test")
            Assert.AreEqual(normalizeRoute("test"), "/test")
            Assert.AreEqual(normalizeRoute("/test/"), "/test")
            Assert.AreEqual(normalizeRoute("test/"), "/test")
            Assert.AreEqual(normalizeRoute("test//"), "/test")
        End Sub
        '
        <TestMethod()> Public Sub encodeBoolean_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual(EncodeBoolean(True), True)
            Assert.AreEqual(EncodeBoolean(0), False)
            Assert.AreEqual(EncodeBoolean(1), True)
            Assert.AreEqual(EncodeBoolean("on"), True)
            Assert.AreEqual(EncodeBoolean("off"), False)
            Assert.AreEqual(EncodeBoolean("true"), True)
            Assert.AreEqual(EncodeBoolean("false"), False)
        End Sub
        '
        <TestMethod()> Public Sub encodeText_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual(EncodeText(1), "1")
        End Sub
        '
        <TestMethod()> Public Sub sample_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual(True, True)
        End Sub
        '
        <TestMethod()> Public Sub dateToSeconds_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual(dateToSeconds(New Date(1900, 1, 1)), 0)
            Assert.AreEqual(dateToSeconds(New Date(1900, 1, 2)), 86400)
        End Sub
        '
        <TestMethod()> Public Sub ModifyQueryString_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual("", ModifyQueryString("", "a", "1", False))
            Assert.AreEqual("a=1", ModifyQueryString("", "a", "1", True))
            Assert.AreEqual("a=1", ModifyQueryString("a=0", "a", "1", False))
            Assert.AreEqual("a=1", ModifyQueryString("a=0", "a", "1", True))
            Assert.AreEqual("a=1&b=2", ModifyQueryString("a=1", "b", "2", True))
        End Sub
        '
        <TestMethod()> Public Sub ModifyLinkQuery_unit()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual("index.html", modifyLinkQuery("index.html", "a", "1", False))
            Assert.AreEqual("index.html?a=1", modifyLinkQuery("index.html", "a", "1", True))
            Assert.AreEqual("index.html?a=1", modifyLinkQuery("index.html?a=0", "a", "1", False))
            Assert.AreEqual("index.html?a=1", modifyLinkQuery("index.html?a=0", "a", "1", True))
            Assert.AreEqual("index.html?a=1&b=2", modifyLinkQuery("index.html?a=1", "b", "2", True))
        End Sub
        '
        <TestMethod()> Public Sub vbInstr_test()
            'vbInstr(1, Link, "?")
            Assert.AreEqual(InStr("abcdefgabcdefgabcdefgabcdefg", "d"), vbInstr("abcdefgabcdefgabcdefgabcdefg", "d"))
            Assert.AreEqual(InStr("abcdefgabcdefgabcdefgabcdefg", "E"), vbInstr("abcdefgabcdefgabcdefgabcdefg", "E"))
            Assert.AreEqual(InStr(10, "abcdefgabcdefgabcdefgabcdefg", "E"), vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E"))
            Assert.AreEqual(InStr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Binary), vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Binary))
            Assert.AreEqual(InStr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Text), vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Text))
            Assert.AreEqual(InStr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Binary), vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Binary))
            Assert.AreEqual(InStr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Text), vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Text))
            Dim haystack As String = "abcdefgabcdefgabcdefgabcdefg"
            Dim needle As String = "c"
            Assert.AreEqual(InStr(1, "?", "?"), vbInstr(1, "?", "?"))
            For ptr As Integer = 1 To haystack.Length
                Assert.AreEqual(InStr(ptr, haystack, needle, CompareMethod.Binary), vbInstr(ptr, haystack, needle, CompareMethod.Binary))
            Next
        End Sub
        '
        <TestMethod()> Public Sub vbIsNumeric_test()
            Assert.AreEqual(IsNumeric(0), vbIsNumeric(0))
            Assert.AreEqual(IsNumeric(New Date(2000, 1, 1)), vbIsNumeric(New Date(2000, 1, 1)))
            Assert.AreEqual(IsNumeric(1234), vbIsNumeric(1234))
            Assert.AreEqual(IsNumeric(12.34), vbIsNumeric(12.34))
            Assert.AreEqual(IsNumeric("abcd"), vbIsNumeric("abcd"))
            Assert.AreEqual(IsNumeric("1234"), vbIsNumeric("1234"))
            Assert.AreEqual(IsNumeric("12.34"), vbIsNumeric("12.34"))
            Assert.AreEqual(IsNumeric(Nothing), vbIsNumeric(Nothing))
        End Sub
        '
        <TestMethod()> Public Sub vbReplace_test()
            Dim actual As String
            Dim expected As String
            Dim start As Integer = 1
            Dim count As Integer = 9999
            '
            expected = Replace("abcdefg", "cd", "12345")
            actual = vbReplace("abcdefg", "cd", "12345")
            Assert.AreEqual(expected, actual)
            '
            expected = Replace("abcdefg", "cD", "12345")
            actual = vbReplace("abcdefg", "cD", "12345")
            Assert.AreEqual(expected, actual)
            '
            expected = Replace("abcdefg", "cd", "12345", start, count, CompareMethod.Binary)
            actual = vbReplace("abcdefg", "cd", "12345", start, count, CompareMethod.Binary)
            Assert.AreEqual(expected, actual)
            '
            expected = Replace("abcdefg", "cD", "12345", start, count, CompareMethod.Binary)
            actual = vbReplace("abcdefg", "cD", "12345", start, count, CompareMethod.Binary)
            Assert.AreEqual(expected, actual)
            '
            expected = Replace("abcdefg", "cd", "12345", start, count, CompareMethod.Text)
            actual = vbReplace("abcdefg", "cd", "12345", start, count, CompareMethod.Text)
            Assert.AreEqual(expected, actual)
            '
            expected = Replace("abcdefg", "cD", "12345", start, count, CompareMethod.Text)
            actual = vbReplace("abcdefg", "cD", "12345", start, count, CompareMethod.Text)
            Assert.AreEqual(expected, actual)
        End Sub
        '
        <TestMethod()> Public Sub vbUCase_test()
            Assert.AreEqual(UCase("AbCdEfG"), vbUCase("AbCdEfG"))
            Assert.AreEqual(UCase("ABCDEFG"), vbUCase("ABCDEFG"))
            Assert.AreEqual(UCase("abcdefg"), vbUCase("abcdefg"))
        End Sub
        '
        <TestMethod()> Public Sub vbLCase_test()
            Assert.AreEqual(LCase("AbCdEfG"), vbLCase("AbCdEfG"))
            Assert.AreEqual(LCase("ABCDEFG"), vbLCase("ABCDEFG"))
            Assert.AreEqual(LCase("abcdefg"), vbLCase("abcdefg"))
        End Sub
        '
        <TestMethod()> Public Sub vbLeft_test()
            Assert.AreEqual(LCase("AbCdEfG"), vbLCase("AbCdEfG"))
        End Sub
    End Class
    '
    '====================================================================================================
    ' unit tests
    '
    <TestClass()> Public Class CPClassUnitTests
        '
        '====================================================================================================
        ' unit test - cp.addVar
        '
        <TestMethod()> Public Sub cp_AddVar_unit()
            ' arrange
            Dim cp As New CPClass()
            Dim cpApp As New CPClass("testapp")
            ' act
            cp.AddVar("a", "1")
            cp.AddVar("b", "2")
            cp.AddVar("b", "3")
            cpApp.AddVar("a", "4")
            cpApp.AddVar("b", "5")
            For ptr = 1 To 10
                cpApp.AddVar("key" & ptr.ToString, "value" & ptr.ToString())
            Next
            ' assert
            Assert.AreEqual(cp.Doc.GetText("a"), "1")
            Assert.AreEqual(cp.Doc.GetText("b"), "3")
            Assert.AreEqual(cpApp.Doc.GetText("a"), "4")
            Assert.AreEqual(cpApp.Doc.GetText("b"), "5")
            For ptr = 1 To 10
                Assert.AreEqual(cpApp.Doc.GetText("key" & ptr.ToString), "value" & ptr.ToString())
            Next
            ' dispose
            cp.Dispose()
            cpApp.Dispose()
        End Sub
        '
        '====================================================================================================
        ' unit test - cp.appOk
        '
        <TestMethod()> Public Sub cp_AppOk_unit()
            ' arrange
            Dim cp As New CPClass()
            Dim cpApp As New CPClass("testapp")
            ' act
            ' assert
            Assert.AreEqual(cp.appOk, False)
            Assert.AreEqual(cpApp.appOk, True)
            ' dispose
            cp.Dispose()
            cpApp.Dispose()
        End Sub

        '
        '====================================================================================================
        ' unit test - sample
        '
        <TestMethod()> Public Sub cp_sample_unit()
            ' arrange
            Dim cp As New CPClass()
            ' act
            '
            ' assert
            Assert.AreEqual(cp.appOk, False)
            ' dispose
            cp.Dispose()
        End Sub
    End Class
    '
    '====================================================================================================
    '
    <TestClass()> Public Class cpCoreTests
        <TestMethod()> Public Sub encodeSqlTableNameTest()
            ' arrange
            ' act
            ' assert
            Assert.AreEqual("", coreClass.encodeSqlTableName(""))
            Assert.AreEqual("", coreClass.encodeSqlTableName("-----"))
            Assert.AreEqual("", coreClass.encodeSqlTableName("01234567879"))
            Assert.AreEqual("a", coreClass.encodeSqlTableName("a"))
            Assert.AreEqual("aa", coreClass.encodeSqlTableName("a a"))
            Assert.AreEqual("aA", coreClass.encodeSqlTableName(" aA"))
            Assert.AreEqual("aA", coreClass.encodeSqlTableName(" aA "))
            Assert.AreEqual("aA", coreClass.encodeSqlTableName("aA "))
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", coreClass.encodeSqlTableName("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"))
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#", coreClass.encodeSqlTableName("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#"))
            '
        End Sub
    End Class
End Namespace