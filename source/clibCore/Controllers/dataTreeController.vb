
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers.genericController
Imports System.Xml
'
Namespace Contensive.Core.Controllers
    Public Class dataTreeController
        '========================================================================
        ' This page and its contents are copyright by Kidwell McGowan Associates.
        '========================================================================
        '
        ' ----- global scope variables
        '
        Private cpCore As coreClass
        Private ClassInitialized As Boolean       ' if true, the module has been
        Private MSxml As XmlDocument
        '
        ' ----- Tracking values, should be set before each exit
        '
        'Private NodePtr as integer
        Private Structure TierNode
            Dim Node As XmlNode
            '    Element As xmlNode
            Dim ChildPtr As Integer
            Dim ChildPtrOK As Boolean
            Dim AttrPtr As Integer
        End Structure
        '
        ' Track node position
        '   TierPtr = 0 is above the Root
        '   TierPtr = 1 is the Root Node
        '   Tier 2... are child tiers
        '
        Dim TierPtr As Integer
        Dim Tier() As TierNode
        '
        '
        '
        Private Private_IsEmpty As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
            ClassInitialized = True
        End Sub
        '
        '
        '
        Private Function Load(ByVal XMLSource As String, ByVal LoadType As Integer) As Boolean
            'On Error GoTo ErrorTrap
            '
            Dim Ptr As Integer
            Dim ChildPtr As Integer
            '
            ' Clear Globals
            '
            Private_IsEmpty = True
            Load = False
            ReDim Tier(0)
            TierPtr = 0
            Tier(0).Node = Nothing
            'Set Tier(0).Element = Nothing
            Tier(0).ChildPtr = 0
            Tier(0).ChildPtrOK = False
            Tier(0).AttrPtr = 0
            '
            ' Load new msxml
            '
            MSxml = New XmlDocument
            If XMLSource <> "" Then
                Dim loadOK As Boolean = True
                Try
                    Select Case LoadType
                        Case 1
                            Call MSxml.Load(XMLSource)
                        Case Else
                            Call MSxml.LoadXml(XMLSource)
                    End Select
                Catch ex As Exception
                    Call cpCore.handleException(ex) : Throw
                    loadOK = False
                End Try
                If loadOK Then
                    Load = True
                    Private_IsEmpty = False
                    If MSxml.ChildNodes.Count <> 0 Then
                        '
                        ' ----- Set Tier(1) to root node
                        '
                        For Ptr = 0 To MSxml.ChildNodes.Count - 1
                            If (MSxml.ChildNodes(Ptr).NodeType = System.Xml.XmlNodeType.Element) Then
                                '
                                Tier(0).ChildPtr = ChildPtr
                                Tier(0).ChildPtrOK = True
                                '
                                TierPtr = 1
                                ReDim Preserve Tier(1)
                                Tier(1).Node = MSxml.ChildNodes(Ptr)
                                Tier(1).ChildPtr = 0
                                Tier(1).ChildPtrOK = True
                                Tier(1).AttrPtr = 0
                                Exit For
                            End If
                        Next
                    End If
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("unexpected exception"))
        End Function
        '
        '
        '
        Public Sub LoadURL(ByVal XMLURL As String)
            Call Load(XMLURL, 1)
        End Sub
        '
        '
        '
        Public Sub LoadText(ByVal XMLData As String)
            Call Load(XMLData, 0)
        End Sub
        '
        '
        '
        Public Sub LoadFile(ByVal Filename As String)
            Call Load(cpCore.appRootFiles.readFile(Filename), 0)
        End Sub
        '
        ' Get Name
        '
        Public Property NodeName() As String
            Get
                NodeName = ""
                If TierPtr > 0 Then
                    If Not (Tier(TierPtr).Node Is Nothing) Then
                        NodeName = Tier(TierPtr).Node.Name
                    End If
                End If

            End Get
            Set(ByVal value As String)
                If Private_IsEmpty Then
                    TierPtr = 1
                    MSxml = New XmlDocument
                    ReDim Tier(TierPtr)
                    Tier(TierPtr).Node = MSxml.CreateElement(value)
                    Private_IsEmpty = False
                    'Call AddChild(vNewValue)
                Else
                    If Not (Tier(TierPtr).Node Is Nothing) Then
                        'Tier(TierPtr).Node.Name = vNewValue
                    End If
                End If

            End Set
        End Property
        '
        ' Next Node
        '
        Public Sub GoNext()
            Try
                Dim ParentPtr As Integer
                '
                If TierPtr > 0 Then
                    ParentPtr = TierPtr - 1
                    If Tier(ParentPtr).ChildPtrOK Then
                        Tier(ParentPtr).ChildPtr = Tier(ParentPtr).ChildPtr + 1
                        Tier(ParentPtr).ChildPtrOK = (Tier(ParentPtr).Node.ChildNodes.Count > Tier(ParentPtr).ChildPtr)
                        Tier(TierPtr).Node = Nothing
                        Tier(TierPtr).AttrPtr = 0
                        If Tier(ParentPtr).ChildPtrOK Then
                            If Tier(ParentPtr).Node.ChildNodes(Tier(ParentPtr).ChildPtr).NodeType = System.Xml.XmlNodeType.Element Then
                                Tier(TierPtr).Node = Tier(ParentPtr).Node.ChildNodes(Tier(ParentPtr).ChildPtr)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        ' IsNodeOK
        '
        Public Function IsNodeOK() As Boolean
            IsNodeOK = False
            If TierPtr > 0 Then
                IsNodeOK = Tier(TierPtr - 1).ChildPtrOK
            End If
        End Function
        '
        ' Previous Node
        '
        Public Sub GoPrevious()
            '
            Dim ParentPtr As Integer
            '
            If TierPtr > 0 Then
                ParentPtr = TierPtr - 1
                If Tier(ParentPtr).ChildPtrOK Then
                    Tier(ParentPtr).ChildPtr = Tier(ParentPtr).ChildPtr + 1
                    Tier(ParentPtr).ChildPtrOK = (Tier(ParentPtr).ChildPtr >= 0)
                    Tier(TierPtr).Node = Nothing
                    Tier(TierPtr).AttrPtr = 0
                    If Tier(ParentPtr).ChildPtrOK Then
                        If Tier(ParentPtr).Node.ChildNodes(Tier(TierPtr).ChildPtr).NodeType = System.Xml.XmlNodeType.Element Then
                            Tier(TierPtr).Node = Tier(ParentPtr).Node.ChildNodes(Tier(TierPtr).ChildPtr)
                        End If
                    End If
                End If
            End If
        End Sub
        '
        '
        '
        Public Function ChildCount() As Integer
            '
            ChildCount = 0
            If Not (Tier(TierPtr).Node Is Nothing) Then
                ChildCount = Tier(TierPtr).Node.ChildNodes.Count
            End If
        End Function
        '
        '
        '
        Public Sub GoFirstChild()
            Try
                Dim Ptr As Integer
                '
                If Not (Tier(TierPtr).Node Is Nothing) Then
                    Tier(TierPtr).ChildPtr = 0
                    Tier(TierPtr).ChildPtrOK = False
                    If Tier(TierPtr).Node.ChildNodes.Count > 0 Then
                        '
                        ' setup new tier
                        '
                        TierPtr = TierPtr + 1
                        ReDim Preserve Tier(TierPtr)
                        Tier(TierPtr).Node = Tier(TierPtr - 1).Node.ChildNodes(Ptr)
                        Tier(TierPtr).ChildPtr = 0
                        Tier(TierPtr).ChildPtrOK = (Tier(TierPtr - 1).Node.ChildNodes.Count > 0)
                        Tier(TierPtr).AttrPtr = 0
                        '
                        ' set parent tier ptrs
                        '
                        Tier(TierPtr - 1).ChildPtr = 0
                        Tier(TierPtr - 1).ChildPtrOK = True
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub

        '
        '
        '
        Public Sub GoParent()
            TierPtr = TierPtr - 1
        End Sub
        '
        '
        '
        Public Function GetAttrNames() As String
            '
            Dim NodeAttribute As XmlAttribute
            '
            GetAttrNames = ""
            If TierPtr > 0 Then
                For Each NodeAttribute In Tier(TierPtr).Node.Attributes
                    GetAttrNames = GetAttrNames & vbCrLf & NodeAttribute.Name
                    'End If
                Next
            End If
            If GetAttrNames <> "" Then
                GetAttrNames = Mid(GetAttrNames, 3)
            End If
        End Function
        '
        '
        '
        Public Sub AddChild(ByVal NodeName As String)
            '
            Dim ChildNode As XmlNode
            '
            If Private_IsEmpty Then
                NodeName = "RootNode"
            End If
            If TierPtr > 0 Then
                ChildNode = MSxml.CreateNode(System.Xml.XmlNodeType.Element, NodeName, "")
                Call Tier(TierPtr).Node.AppendChild(ChildNode)
                Private_IsEmpty = False
            End If
        End Sub
        '
        '
        '
        Public Sub AddAttr(ByVal AttrName As String, ByVal AttrValue As String)
            '
            '    MSxml.documentElement
            '
            '    Dim DocElement As xmlNode
            '    Dim ChildNode As xmlnode
            '    Dim Attr As xmlattribute
            '    '
            '    If TierPtr > 0 Then
            '        Attr = MSxml.createAttribute(AttrName)
            '        Tier(TierPtr).Node.seta
            '    End If
        End Sub
        '
        '
        '
        Public Function AttrCount() As Integer
            '
            AttrCount = 0
            If Not (Tier(TierPtr).Node Is Nothing) Then
                If Not Tier(TierPtr).Node.Attributes Is Nothing Then
                    AttrCount = Tier(TierPtr).Node.Attributes.Count
                End If
            End If
        End Function
        '
        '
        '
        Public Sub GoPreviousAttr()
            '
            If IsAttrOK() Then
                Tier(TierPtr).AttrPtr = Tier(TierPtr).AttrPtr - 1
            End If
        End Sub
        '
        '
        '
        Public Sub GoNextAttr()
            '
            If IsAttrOK() Then
                Tier(TierPtr).AttrPtr = Tier(TierPtr).AttrPtr + 1
            End If
        End Sub
        '
        ' IsAttrOK
        '
        Public Function IsAttrOK() As Boolean
            IsAttrOK = False
            If TierPtr > 0 Then
                If Not (Tier(TierPtr).Node Is Nothing) Then
                    IsAttrOK = (Tier(TierPtr).AttrPtr >= 0) And (Tier(TierPtr).AttrPtr < Tier(TierPtr).Node.Attributes.Count)
                End If
            End If
        End Function
        '
        '
        '
        Public Function GetAttr(ByVal AttrName As String) As String
            GetAttr = ""
            '
            Dim AttrPtr As Integer
            Dim AttrCnt As Integer
            Dim UCaseAttrName As String
            '
            AttrCnt = Tier(TierPtr).Node.Attributes.Count
            If AttrCnt > 0 And (AttrName <> "") Then
                UCaseAttrName = genericController.vbUCase(AttrName)
                For AttrPtr = 0 To AttrCnt - 1
                    If UCaseAttrName = genericController.vbUCase(Tier(TierPtr).Node.Attributes(AttrPtr).Name) Then
                        GetAttr = Tier(TierPtr).Node.Attributes(AttrPtr).Value
                        Exit For
                    End If
                Next
            End If
        End Function
        '
        '
        '
        Public Function GetAttrName() As String
            Dim AttrPtr As Integer
            GetAttrName = ""
            If IsAttrOK() Then
                AttrPtr = Tier(TierPtr).AttrPtr
                GetAttrName = Tier(TierPtr).Node.Attributes(AttrPtr).Name
            End If
        End Function
        '
        '
        '
        Public Function GetAttrValue() As String
            Dim AttrPtr As Integer
            GetAttrValue = ""
            If IsAttrOK() Then
                AttrPtr = Tier(TierPtr).AttrPtr
                GetAttrValue = Tier(TierPtr).Node.Attributes(AttrPtr).Value
            End If
        End Function
        '
        Public ReadOnly Property XML() As String
            Get
                XML = ""
                If Not (Tier(TierPtr).Node Is Nothing) Then
                    XML = Tier(TierPtr).Node.InnerXml
                End If
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property Text() As String
            Get
                Text = ""
                If Not (Tier(TierPtr).Node Is Nothing) Then
                    Text = Tier(TierPtr).Node.InnerText
                End If
            End Get
        End Property
        '
        '
        '
        Public Sub Clear()
            Private_IsEmpty = True
            If TierPtr <= 1 Then
                MSxml = Nothing
            Else
                If Not (Tier(TierPtr).Node Is Nothing) Then
                    Call Tier(TierPtr - 1).Node.RemoveChild(Tier(TierPtr).Node)
                End If
            End If
        End Sub
        '
        '
        '
        Public Sub SaveFile(ByVal Filename As String)
            Call cpCore.appRootFiles.saveFile(Filename, XML)
        End Sub
        '
        '
        '


        Public ReadOnly Property IsEmpty() As Boolean
            Get
                IsEmpty = Private_IsEmpty
            End Get
        End Property
    End Class
End Namespace

