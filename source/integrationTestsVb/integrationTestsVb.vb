
Imports Contensive.Core
Imports Contensive.BaseClasses
Imports Xunit
Imports System

Namespace integrationTests

    Public Class testCpBlock
        '
        '====================================================================================================
        ' unit test - cp.blockNew
        '
        <Fact> Public Sub cpBlockNew()
            ' arrange
            Dim cpApp As New CPClass("testapp")
            Dim block As CPBlockClass = cpApp.BlockNew()
            Const layoutContent As String = "content"
            Const layoutA As String = "<div id=""aid"" class=""aclass"">" & layoutContent & "</div>"
            Const layoutB As String = "<div id=""bid"" class=""bclass"">" & layoutA & "</div>"
            Const layoutC As String = "<div id=""cid"" class=""cclass"">" & layoutB & "</div>"
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
            block.Append("1")
            block.Append("2")
            Assert.Equal(block.GetHtml(), "12")
            '
            block.Clear()
            block.Prepend("1")
            block.Prepend("2")
            Assert.Equal(block.GetHtml(), "21")
            '
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
    End Class
End Namespace