
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Contensive.BaseClasses
Imports Contensive.Core

<TestClass()> Public Class cpPatternGetDoc

    <TestMethod()> Public Sub TestMethod1()
        Dim cpResult As String
        '
        Try

            Using cp As New CPClass()
                cpResult = cp.executeRoute()
                Assert.AreNotEqual(cpResult, "")
            End Using
        Catch ex As Exception
            Assert.Fail("exception [" & ex.ToString & "]")
        End Try
    End Sub

End Class