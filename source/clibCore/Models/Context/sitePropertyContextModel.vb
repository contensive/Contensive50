
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Core
    Public Class siteContextModel
        '
        Private cp As CPBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Symbol for currency
        ''' </summary>
        ''' <returns></returns>
        Public Property AdminUrl As String
            Get
                If (_AdminUrl Is Nothing) Then
                    _AdminUrl = cp.Site.GetText("AdminUrl")
                End If
                Return _AdminUrl
            End Get
            Set(value As String)
                '
                ' does not set site property, just mocks the site property for testing
                '
                _AdminUrl = value
            End Set
        End Property
        Private _AdminUrl As String = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' create the model for use in the application, where exposed properties lookup site properties
        ''' </summary>
        ''' <param name="cp"></param>
        Public Sub New(cp As CPBaseClass)
            Me.cp = cp
        End Sub
    End Class
End Namespace
