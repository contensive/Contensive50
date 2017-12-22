

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Controllers
    ''' <summary>
    ''' Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    ''' </summary>
    Public Class htmlController
        '
        '========================================================================
        '   Decodes ActiveContent and EditIcons into <AC tags
        '       Detect IMG tags
        '           If IMG ID attribute is "AC,IMAGE,recordid", convert to AC Image tag
        '           If IMG ID attribute is "AC,DOWNLOAD,recordid", convert to AC Download tag
        '           If IMG ID attribute is "AC,ACType,ACFieldName,ACInstanceName,QueryStringArguments,AddonGuid", convert it to generic AC tag
        '   ACInstanceID - used to identify an AC tag on a page. Each instance of an AC tag must havea unique ACinstanceID
        '========================================================================
        '
        Public Function convertEditorResponseToActiveContent(ByVal SourceCopy As String) As String
            Dim result As String = ""
            Try
                Dim imageNewLink As String
                Dim ACQueryString As String = ""
                Dim ACGuid As String
                Dim ACIdentifier As String
                Dim RecordFilename As String
                Dim RecordFilenameNoExt As String
                Dim RecordFilenameExt As String
                Dim Ptr As Integer
                Dim ACInstanceID As String
                Dim QSHTMLEncoded As String
                Dim Pos As Integer
                Dim ImageSrcOriginal As String
                Dim VirtualFilePathBad As String
                Dim Paths() As String
                Dim ImageVirtualFilename As String
                Dim ImageFilename As String
                Dim ImageFilenameExt As String
                Dim ImageFilenameNoExt As String
                Dim SizeTest() As String
                Dim Styles() As String
                Dim StyleName As String
                Dim StyleValue As String
                Dim StyleValueInt As Integer
                Dim Style() As String
                Dim ImageVirtualFilePath As String
                Dim RecordVirtualFilename As String
                Dim RecordWidth As Integer
                Dim RecordHeight As Integer
                Dim RecordAltSizeList As String
                Dim ImageAltSize As String
                Dim NewImageFilename As String
                Dim DHTML As New htmlParserController(cpCore)
                Dim ElementPointer As Integer
                Dim ElementCount As Integer
                Dim AttributeCount As Integer
                Dim ACType As String
                Dim ACFieldName As String
                Dim ACInstanceName As String
                Dim RecordID As Integer
                Dim ImageWidthText As String
                Dim ImageHeightText As String
                Dim ImageWidth As Integer
                Dim ImageHeight As Integer
                Dim ElementText As String
                Dim ImageID As String
                Dim ImageSrc As String
                Dim ImageAlt As String
                Dim ImageVSpace As Integer
                Dim ImageHSpace As Integer
                Dim ImageAlign As String
                Dim ImageBorder As String
                Dim ImageLoop As String
                Dim ImageStyle As String
                Dim IMageStyleArray As String()
                Dim ImageStyleArrayCount As Integer
                Dim ImageStyleArrayPointer As Integer
                Dim ImageStylePair As String
                Dim PositionColon As Integer
                Dim ImageStylePairName As String
                Dim ImageStylePairValue As String
                Dim Stream As New stringBuilderLegacyController
                Dim ImageIDArray As String() = {}
                Dim ImageIDArrayCount As Integer
                Dim QueryString As String
                Dim QSSplit() As String
                Dim QSPtr As Integer
                Dim serverFilePath As String
                Dim ImageAllowSFResize As Boolean
                Dim sf As imageEditController
                '
                result = SourceCopy
                If result <> "" Then
                    '
                    ' leave this in to make sure old <acform tags are converted back
                    ' new editor deals with <form, so no more converting
                    '
                    result = genericController.vbReplace(result, "<ACFORM>", "<FORM>")
                    result = genericController.vbReplace(result, "<ACFORM ", "<FORM ")
                    result = genericController.vbReplace(result, "</ACFORM>", "</form>")
                    result = genericController.vbReplace(result, "</ACFORM ", "</FORM ")
                    If DHTML.Load(result) Then
                        result = ""
                        ElementCount = DHTML.ElementCount
                        If ElementCount > 0 Then
                            '
                            ' ----- Locate and replace IMG Edit icons with AC tags
                            '
                            Stream = New stringBuilderLegacyController
                            For ElementPointer = 0 To ElementCount - 1
                                ElementText = DHTML.Text(ElementPointer)
                                If DHTML.IsTag(ElementPointer) Then
                                    Select Case genericController.vbUCase(DHTML.TagName(ElementPointer))
                                        Case "FORM"
                                        '
                                        ' User created form - add the attribute "Contensive=1"
                                        '
                                        ' 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                        'ElementText = genericController.vbReplace(ElementText, "<FORM", "<FORM ContensiveUserForm=1 ", vbTextCompare)
                                        Case "IMG"
                                            AttributeCount = DHTML.ElementAttributeCount(ElementPointer)

                                            If AttributeCount > 0 Then
                                                ImageID = DHTML.ElementAttribute(ElementPointer, "id")
                                                ImageSrcOriginal = DHTML.ElementAttribute(ElementPointer, "src")
                                                VirtualFilePathBad = cpCore.serverConfig.appConfig.name & "/files/"
                                                serverFilePath = "/" & VirtualFilePathBad
                                                If Left(LCase(ImageSrcOriginal), Len(VirtualFilePathBad)) = genericController.vbLCase(VirtualFilePathBad) Then
                                                    '
                                                    ' if the image is from the virtual file path, but the editor did not include the root path, add it
                                                    '
                                                    ElementText = genericController.vbReplace(ElementText, VirtualFilePathBad, "/" & VirtualFilePathBad, 1, 99, vbTextCompare)
                                                    ImageSrcOriginal = genericController.vbReplace(ImageSrcOriginal, VirtualFilePathBad, "/" & VirtualFilePathBad, 1, 99, vbTextCompare)
                                                End If
                                                ImageSrc = genericController.decodeHtml(ImageSrcOriginal)
                                                ImageSrc = DecodeURL(ImageSrc)
                                                '
                                                ' problem with this case is if the addon icon image is from another site.
                                                ' not sure how it happened, but I do not think the src of an addon edit icon
                                                ' should be able to prevent the addon from executing.
                                                '
                                                ACIdentifier = ""
                                                ACType = ""
                                                ACFieldName = ""
                                                ACInstanceName = ""
                                                ACGuid = ""
                                                ImageIDArrayCount = 0
                                                If 0 <> genericController.vbInstr(1, ImageID, ",") Then
                                                    ImageIDArray = Split(ImageID, ",")
                                                    ImageIDArrayCount = UBound(ImageIDArray) + 1
                                                    If ImageIDArrayCount > 5 Then
                                                        For Ptr = 5 To ImageIDArrayCount - 1
                                                            ACGuid = ImageIDArray(Ptr)
                                                            If (Left(ACGuid, 1) = "{") And (Right(ACGuid, 1) = "}") Then
                                                                '
                                                                ' this element is the guid, go with it
                                                                '
                                                                Exit For
                                                            ElseIf (ACGuid = "") And (Ptr = (ImageIDArrayCount - 1)) Then
                                                                '
                                                                ' this is the last element, leave it as the guid
                                                                '
                                                                Exit For
                                                            Else
                                                                '
                                                                ' not a valid guid, add it to element 4 and try the next
                                                                '
                                                                ImageIDArray(4) = ImageIDArray(4) & "," & ACGuid
                                                                ACGuid = ""
                                                            End If
                                                        Next
                                                    End If
                                                    If (ImageIDArrayCount > 1) Then
                                                        ACIdentifier = genericController.vbUCase(ImageIDArray(0))
                                                        ACType = ImageIDArray(1)
                                                        If ImageIDArrayCount > 2 Then
                                                            ACFieldName = ImageIDArray(2)
                                                            If ImageIDArrayCount > 3 Then
                                                                ACInstanceName = ImageIDArray(3)
                                                                If ImageIDArrayCount > 4 Then
                                                                    ACQueryString = ImageIDArray(4)
                                                                    'If ImageIDArrayCount > 5 Then
                                                                    '    ACGuid = ImageIDArray(5)
                                                                    'End If
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                                If ACIdentifier = "AC" Then
                                                    If True Then
                                                        If True Then
                                                            '
                                                            ' ----- Process AC Tag
                                                            '
                                                            ACInstanceID = DHTML.ElementAttribute(ElementPointer, "ACINSTANCEID")
                                                            If ACInstanceID = "" Then
                                                                'GUIDGenerator = New guidClass
                                                                ACInstanceID = Guid.NewGuid().ToString
                                                                'ACInstanceID = Guid.NewGuid.ToString()
                                                            End If
                                                            ElementText = ""
                                                            '----------------------------- change to ACType
                                                            Select Case genericController.vbUCase(ACType)
                                                                Case "IMAGE"
                                                                    '
                                                                    ' ----- AC Image, Decode Active Images to Resource Library references
                                                                    '
                                                                    If ImageIDArrayCount >= 4 Then
                                                                        RecordID = genericController.EncodeInteger(ACInstanceName)
                                                                        ImageWidthText = DHTML.ElementAttribute(ElementPointer, "WIDTH")
                                                                        ImageHeightText = DHTML.ElementAttribute(ElementPointer, "HEIGHT")
                                                                        ImageAlt = encodeHTML(DHTML.ElementAttribute(ElementPointer, "Alt"))
                                                                        ImageVSpace = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "vspace"))
                                                                        ImageHSpace = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "hspace"))
                                                                        ImageAlign = DHTML.ElementAttribute(ElementPointer, "Align")
                                                                        ImageBorder = DHTML.ElementAttribute(ElementPointer, "BORDER")
                                                                        ImageLoop = DHTML.ElementAttribute(ElementPointer, "LOOP")
                                                                        ImageStyle = DHTML.ElementAttribute(ElementPointer, "STYLE")

                                                                        If ImageStyle <> "" Then
                                                                            '
                                                                            ' ----- Process styles, which override attributes
                                                                            '
                                                                            IMageStyleArray = Split(ImageStyle, ";")
                                                                            ImageStyleArrayCount = UBound(IMageStyleArray) + 1
                                                                            For ImageStyleArrayPointer = 0 To ImageStyleArrayCount - 1
                                                                                ImageStylePair = Trim(IMageStyleArray(ImageStyleArrayPointer))
                                                                                PositionColon = genericController.vbInstr(1, ImageStylePair, ":")
                                                                                If PositionColon > 1 Then
                                                                                    ImageStylePairName = Trim(Mid(ImageStylePair, 1, PositionColon - 1))
                                                                                    ImageStylePairValue = Trim(Mid(ImageStylePair, PositionColon + 1))
                                                                                    Select Case genericController.vbUCase(ImageStylePairName)
                                                                                        Case "WIDTH"
                                                                                            ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "")
                                                                                            ImageWidthText = ImageStylePairValue
                                                                                        Case "HEIGHT"
                                                                                            ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "")
                                                                                            ImageHeightText = ImageStylePairValue
                                                                                    End Select
                                                                                    'If genericController.vbInstr(1, ImageStylePair, "WIDTH", vbTextCompare) = 1 Then
                                                                                    '    End If
                                                                                End If
                                                                            Next
                                                                        End If
                                                                        ElementText = "<AC type=""IMAGE"" ACInstanceID=""" & ACInstanceID & """ RecordID=""" & RecordID & """ Style=""" & ImageStyle & """ Width=""" & ImageWidthText & """ Height=""" & ImageHeightText & """ VSpace=""" & ImageVSpace & """ HSpace=""" & ImageHSpace & """ Alt=""" & ImageAlt & """ Align=""" & ImageAlign & """ Border=""" & ImageBorder & """ Loop=""" & ImageLoop & """>"
                                                                    End If
                                                                Case ACTypeDownload
                                                                    '
                                                                    ' AC Download
                                                                    '
                                                                    If ImageIDArrayCount >= 4 Then
                                                                        RecordID = genericController.EncodeInteger(ACInstanceName)
                                                                        ElementText = "<AC type=""DOWNLOAD"" ACInstanceID=""" & ACInstanceID & """ RecordID=""" & RecordID & """>"
                                                                    End If
                                                                Case ACTypeDate
                                                                    '
                                                                    ' Date
                                                                    '
                                                                    ElementText = "<AC type=""" & ACTypeDate & """>"
                                                                Case ACTypeVisit, ACTypeVisitor, ACTypeMember, ACTypeOrganization, ACTypePersonalization
                                                                    '
                                                                    ' Visit, etc
                                                                    '
                                                                    ElementText = "<AC type=""" & ACType & """ ACInstanceID=""" & ACInstanceID & """ field=""" & ACFieldName & """>"
                                                                Case ACTypeChildList, ACTypeLanguage
                                                                    '
                                                                    ' ChildList, Language
                                                                    '
                                                                    If ACInstanceName = "0" Then
                                                                        ACInstanceName = genericController.GetRandomInteger().ToString()
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """>"
                                                                Case ACTypeAggregateFunction
                                                                    '
                                                                    ' Function
                                                                    '
                                                                    QueryString = ""
                                                                    If ACQueryString <> "" Then
                                                                        ' I added this because single stepping through it I found it split on the & in &amp;
                                                                        ' I had added an Add-on and was saving
                                                                        ' I find it VERY odd that this could be the case
                                                                        '
                                                                        QSHTMLEncoded = genericController.encodeText(ACQueryString)
                                                                        QueryString = genericController.decodeHtml(QSHTMLEncoded)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            Pos = genericController.vbInstr(1, QSSplit(QSPtr), "[")
                                                                            If Pos > 0 Then
                                                                                QSSplit(QSPtr) = Mid(QSSplit(QSPtr), 1, Pos - 1)
                                                                            End If
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """ guid=""" & ACGuid & """>"
                                                                Case ACTypeContact, ACTypeFeedback
                                                                    '
                                                                    ' Contact and Feedback
                                                                    '
                                                                    ElementText = "<AC type=""" & ACType & """ ACInstanceID=""" & ACInstanceID & """>"
                                                                Case ACTypeTemplateContent, ACTypeTemplateText
                                                                    '
                                                                    '
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")

                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """>"
                                                                Case ACTypeWatchList
                                                                    '
                                                                    ' Watch List
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QueryString = genericController.decodeHtml(QueryString)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """>"
                                                                Case ACTypeRSSLink
                                                                    '
                                                                    ' RSS Link
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QueryString = genericController.decodeHtml(QueryString)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """>"
                                                                Case Else
                                                                    '
                                                                    ' All others -- added querystring from element(4) to all others to cover the group access AC object
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QueryString = genericController.decodeHtml(QueryString)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ field=""" & ACFieldName & """ querystring=""" & QueryString & """>"
                                                            End Select
                                                        End If
                                                    End If
                                                ElseIf genericController.vbInstr(1, ImageSrc, "cclibraryfiles", vbTextCompare) <> 0 Then
                                                    ImageAllowSFResize = cpCore.siteProperties.getBoolean("ImageAllowSFResize", True)
                                                    If ImageAllowSFResize And True Then
                                                        '
                                                        ' if it is a real image, check for resize
                                                        '
                                                        Pos = genericController.vbInstr(1, ImageSrc, "cclibraryfiles", vbTextCompare)
                                                        If Pos <> 0 Then
                                                            ImageVirtualFilename = Mid(ImageSrc, Pos)
                                                            Paths = Split(ImageVirtualFilename, "/")
                                                            If UBound(Paths) > 2 Then
                                                                If genericController.vbLCase(Paths(1)) = "filename" Then
                                                                    RecordID = genericController.EncodeInteger(Paths(2))
                                                                    If RecordID <> 0 Then
                                                                        ImageFilename = Paths(3)
                                                                        ImageVirtualFilePath = genericController.vbReplace(ImageVirtualFilename, ImageFilename, "")
                                                                        Pos = InStrRev(ImageFilename, ".")
                                                                        If Pos > 0 Then
                                                                            Dim ImageFilenameAltSize As String = ""
                                                                            ImageFilenameExt = Mid(ImageFilename, Pos + 1)
                                                                            ImageFilenameNoExt = Mid(ImageFilename, 1, Pos - 1)
                                                                            Pos = InStrRev(ImageFilenameNoExt, "-")
                                                                            If Pos > 0 Then
                                                                                '
                                                                                ' ImageAltSize should be set from the width and height of the img tag,
                                                                                ' NOT from the actual width and height of the image file
                                                                                ' NOT from the suffix of the image filename
                                                                                ' ImageFilenameAltSize is used when the image has been resized, then 'reset' was hit
                                                                                '  on the properties dialog before the save. The width and height come from this suffix
                                                                                '
                                                                                ImageFilenameAltSize = Mid(ImageFilenameNoExt, Pos + 1)
                                                                                SizeTest = Split(ImageFilenameAltSize, "x")
                                                                                If UBound(SizeTest) <> 1 Then
                                                                                    ImageFilenameAltSize = ""
                                                                                Else
                                                                                    If genericController.vbIsNumeric(SizeTest(0)) And genericController.vbIsNumeric(SizeTest(1)) Then
                                                                                        ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                                        'RecordVirtualFilenameNoExt = Mid(RecordVirtualFilename, 1, Pos - 1)
                                                                                    Else
                                                                                        ImageFilenameAltSize = ""
                                                                                    End If
                                                                                End If
                                                                                'ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                            End If
                                                                            If genericController.vbInstr(1, sfImageExtList, ImageFilenameExt, vbTextCompare) <> 0 Then
                                                                                '
                                                                                ' Determine ImageWidth and ImageHeight
                                                                                '
                                                                                ImageStyle = DHTML.ElementAttribute(ElementPointer, "style")
                                                                                ImageWidth = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "width"))
                                                                                ImageHeight = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "height"))
                                                                                If ImageStyle <> "" Then
                                                                                    Styles = Split(ImageStyle, ";")
                                                                                    For Ptr = 0 To UBound(Styles)
                                                                                        Style = Split(Styles(Ptr), ":")
                                                                                        If UBound(Style) > 0 Then
                                                                                            StyleName = genericController.vbLCase(Trim(Style(0)))
                                                                                            If StyleName = "width" Then
                                                                                                StyleValue = genericController.vbLCase(Trim(Style(1)))
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "")
                                                                                                StyleValueInt = genericController.EncodeInteger(StyleValue)
                                                                                                If StyleValueInt > 0 Then
                                                                                                    ImageWidth = StyleValueInt
                                                                                                End If
                                                                                            ElseIf StyleName = "height" Then
                                                                                                StyleValue = genericController.vbLCase(Trim(Style(1)))
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "")
                                                                                                StyleValueInt = genericController.EncodeInteger(StyleValue)
                                                                                                If StyleValueInt > 0 Then
                                                                                                    ImageHeight = StyleValueInt
                                                                                                End If
                                                                                            End If
                                                                                        End If
                                                                                    Next
                                                                                End If
                                                                                '
                                                                                ' Get the record values
                                                                                '
                                                                                Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, RecordID)
                                                                                If (file IsNot Nothing) Then
                                                                                    RecordVirtualFilename = file.Filename
                                                                                    RecordWidth = file.pxWidth
                                                                                    RecordHeight = file.pxHeight
                                                                                    RecordAltSizeList = file.AltSizeList
                                                                                    RecordFilename = RecordVirtualFilename
                                                                                    Pos = InStrRev(RecordVirtualFilename, "/")
                                                                                    If Pos > 0 Then
                                                                                        RecordFilename = Mid(RecordVirtualFilename, Pos + 1)
                                                                                    End If
                                                                                    RecordFilenameExt = ""
                                                                                    RecordFilenameNoExt = RecordFilename
                                                                                    Pos = InStrRev(RecordFilenameNoExt, ".")
                                                                                    If Pos > 0 Then
                                                                                        RecordFilenameExt = Mid(RecordFilenameNoExt, Pos + 1)
                                                                                        RecordFilenameNoExt = Mid(RecordFilenameNoExt, 1, Pos - 1)
                                                                                    End If
                                                                                    '
                                                                                    ' if recordwidth or height are missing, get them from the file
                                                                                    '
                                                                                    If RecordWidth = 0 Or RecordHeight = 0 Then
                                                                                        sf = New imageEditController
                                                                                        If sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename)) Then
                                                                                            file.pxWidth = sf.width
                                                                                            file.pxHeight = sf.height
                                                                                            file.save(cpCore)
                                                                                        End If
                                                                                        Call sf.Dispose()
                                                                                        sf = Nothing
                                                                                    End If
                                                                                    '
                                                                                    ' continue only if we have record width and height
                                                                                    '
                                                                                    If RecordWidth <> 0 And RecordHeight <> 0 Then
                                                                                        '
                                                                                        ' set ImageWidth and ImageHeight if one of them is missing
                                                                                        '
                                                                                        If (ImageWidth = RecordWidth) And (ImageHeight = 0) Then
                                                                                            '
                                                                                            ' Image only included width, set default height
                                                                                            '
                                                                                            ImageHeight = RecordHeight
                                                                                        ElseIf (ImageHeight = RecordHeight) And (ImageWidth = 0) Then
                                                                                            '
                                                                                            ' Image only included height, set default width
                                                                                            '
                                                                                            ImageWidth = RecordWidth
                                                                                        ElseIf (ImageHeight = 0) And (ImageWidth = 0) Then
                                                                                            '
                                                                                            ' Image has no width or height, default both
                                                                                            ' This happens when you hit 'reset' on the image properties dialog
                                                                                            '
                                                                                            sf = New imageEditController
                                                                                            If sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename)) Then
                                                                                                ImageWidth = sf.width
                                                                                                ImageHeight = sf.height
                                                                                            End If
                                                                                            Call sf.Dispose()
                                                                                            sf = Nothing
                                                                                            If (ImageHeight = 0) And (ImageWidth = 0) And (Not String.IsNullOrEmpty(ImageFilenameAltSize)) Then
                                                                                                Pos = genericController.vbInstr(1, ImageFilenameAltSize, "x")
                                                                                                If Pos <> 0 Then
                                                                                                    ImageWidth = genericController.EncodeInteger(Mid(ImageFilenameAltSize, 1, Pos - 1))
                                                                                                    ImageHeight = genericController.EncodeInteger(Mid(ImageFilenameAltSize, Pos + 1))
                                                                                                End If
                                                                                            End If
                                                                                            If ImageHeight = 0 And ImageWidth = 0 Then
                                                                                                ImageHeight = RecordHeight
                                                                                                ImageWidth = RecordWidth
                                                                                            End If
                                                                                        End If
                                                                                        '
                                                                                        ' Set the ImageAltSize to what was requested from the img tag
                                                                                        ' if the actual image is a few rounding-error pixels off does not matter
                                                                                        ' if either is 0, let altsize be 0, set real value for image height/width
                                                                                        '
                                                                                        ImageAltSize = CStr(ImageWidth) & "x" & CStr(ImageHeight)
                                                                                        '
                                                                                        ' determine if we are OK, or need to rebuild
                                                                                        '
                                                                                        If (RecordVirtualFilename = (ImageVirtualFilePath & ImageFilename)) And ((RecordWidth = ImageWidth) Or (RecordHeight = ImageHeight)) Then
                                                                                            '
                                                                                            ' OK
                                                                                            ' this is the raw image
                                                                                            ' image matches record, and the sizes are the same
                                                                                            '
                                                                                            RecordVirtualFilename = RecordVirtualFilename
                                                                                        ElseIf (RecordVirtualFilename = ImageVirtualFilePath & ImageFilenameNoExt & "." & ImageFilenameExt) And (InStr(1, RecordAltSizeList, ImageAltSize, vbTextCompare) <> 0) Then
                                                                                            '
                                                                                            ' OK
                                                                                            ' resized image, and altsize is in the list - go with resized image name
                                                                                            '
                                                                                            NewImageFilename = ImageFilenameNoExt & "-" & ImageAltSize & "." & ImageFilenameExt
                                                                                            ' images included in email have spaces that must be converted to "%20" or they 404
                                                                                            imageNewLink = genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) & NewImageFilename)
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(imageNewLink))
                                                                                        ElseIf (RecordWidth < ImageWidth) Or (RecordHeight < ImageHeight) Then
                                                                                            '
                                                                                            ' OK
                                                                                            ' reize image larger then original - go with it as is
                                                                                            '
                                                                                            ' images included in email have spaces that must be converted to "%20" or they 404
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, RecordVirtualFilename))))
                                                                                        Else
                                                                                            '
                                                                                            ' resized image - create NewImageFilename (and add new alt size to the record)
                                                                                            '
                                                                                            If RecordWidth = ImageWidth And RecordHeight = ImageHeight Then
                                                                                                '
                                                                                                ' set back to Raw image untouched, use the record image filename
                                                                                                '
                                                                                                ElementText = ElementText
                                                                                                'ElementText = genericController.vbReplace(ElementText, ImageVirtualFilename, RecordVirtualFilename)
                                                                                            Else
                                                                                                '
                                                                                                ' Raw image filename in content, but it is resized, switch to an alternate size
                                                                                                '
                                                                                                NewImageFilename = RecordFilename
                                                                                                If (ImageWidth = 0) Or (ImageHeight = 0) Or (InStr(1, vbCrLf & RecordAltSizeList & vbCrLf, vbCrLf & ImageAltSize & vbCrLf) = 0) Then
                                                                                                    '
                                                                                                    ' Alt image has not been built
                                                                                                    '
                                                                                                    sf = New imageEditController
                                                                                                    If Not sf.load(genericController.convertCdnUrlToCdnPathFilename(RecordVirtualFilename)) Then
                                                                                                        '
                                                                                                        ' image load failed, use raw filename
                                                                                                        '
                                                                                                        Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error while loading image to resize, [" & RecordVirtualFilename & "]", "dll", "cpCoreClass", "DecodeAciveContent", Err.Number, Err.Source, Err.Description, False, True, "")
                                                                                                        Err.Clear()
                                                                                                        NewImageFilename = ImageFilename
                                                                                                    Else
                                                                                                        '
                                                                                                        '
                                                                                                        '
                                                                                                        RecordWidth = sf.width
                                                                                                        RecordHeight = sf.height
                                                                                                        If ImageWidth = 0 Then
                                                                                                            '
                                                                                                            '
                                                                                                            '
                                                                                                            sf.height = ImageHeight
                                                                                                        ElseIf ImageHeight = 0 Then
                                                                                                            '
                                                                                                            '
                                                                                                            '
                                                                                                            sf.width = ImageWidth
                                                                                                        ElseIf RecordHeight = ImageHeight Then
                                                                                                            '
                                                                                                            ' change the width
                                                                                                            '
                                                                                                            sf.width = ImageWidth
                                                                                                        Else
                                                                                                            '
                                                                                                            ' change the height
                                                                                                            '
                                                                                                            sf.height = ImageHeight
                                                                                                        End If
                                                                                                        '
                                                                                                        ' if resized only width or height, set the other
                                                                                                        '
                                                                                                        If ImageWidth = 0 Then
                                                                                                            ImageWidth = sf.width
                                                                                                            ImageAltSize = CStr(ImageWidth) & "x" & CStr(ImageHeight)
                                                                                                        End If
                                                                                                        If ImageHeight = 0 Then
                                                                                                            ImageHeight = sf.height
                                                                                                            ImageAltSize = CStr(ImageWidth) & "x" & CStr(ImageHeight)
                                                                                                        End If
                                                                                                        '
                                                                                                        ' set HTML attributes so image properties will display
                                                                                                        '
                                                                                                        If genericController.vbInstr(1, ElementText, "height=", vbTextCompare) = 0 Then
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " height=""" & ImageHeight & """>")
                                                                                                        End If
                                                                                                        If genericController.vbInstr(1, ElementText, "width=", vbTextCompare) = 0 Then
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " width=""" & ImageWidth & """>")
                                                                                                        End If
                                                                                                        '
                                                                                                        ' Save new file
                                                                                                        '
                                                                                                        NewImageFilename = RecordFilenameNoExt & "-" & ImageAltSize & "." & RecordFilenameExt
                                                                                                        Call sf.save(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilePath & NewImageFilename))
                                                                                                        '
                                                                                                        ' Update image record
                                                                                                        '
                                                                                                        RecordAltSizeList = RecordAltSizeList & vbCrLf & ImageAltSize
                                                                                                    End If
                                                                                                End If
                                                                                                '
                                                                                                ' Change the image src to the AltSize
                                                                                                ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) & NewImageFilename)))
                                                                                            End If
                                                                                        End If
                                                                                    End If
                                                                                End If
                                                                                file.save(cpCore)
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                    End Select
                                End If
                                Stream.Add(ElementText)
                            Next
                        End If
                        result = Stream.Text
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
            '
        End Function
        '
        '========================================================================
        ' Modify a string to be printed through the HTML stream
        '   convert carriage returns ( 0x10 ) to <br>
        '   remove linefeeds ( 0x13 )
        '========================================================================
        '
        Public Function convertCRLFToHtmlBreak(ByVal Source As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("EncodeCRLF")
            '
            Dim iSource As String
            '
            iSource = genericController.encodeText(Source)
            convertCRLFToHtmlBreak = ""
            If (iSource <> "") Then
                convertCRLFToHtmlBreak = iSource
                convertCRLFToHtmlBreak = genericController.vbReplace(convertCRLFToHtmlBreak, vbCr, "")
                convertCRLFToHtmlBreak = genericController.vbReplace(convertCRLFToHtmlBreak, vbLf, "<br >")
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_EncodeCRLF")
        End Function
        '
        '========================================================================
        '   Encodes characters to be compatibile with HTML
        '   i.e. it converts the equation 5 > 6 to th sequence "5 &gt; 6"
        '
        '   convert carriage returns ( 0x10 ) to <br >
        '   remove linefeeds ( 0x13 )
        '========================================================================
        '
        Public Function main_encodeHTML(ByVal Source As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("encodeHTML")
            '
            main_encodeHTML = encodeHTML(genericController.encodeText(Source))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("EncodeHTML")
        End Function
        '
        '========================================================================
        '   Convert an HTML source to a text equivelent
        '
        '       converts CRLF to <br>
        '       encodes reserved HTML characters to their equivalent
        '========================================================================
        '
        Public Function convertTextToHTML(ByVal Source As String) As String
            Return convertCRLFToHtmlBreak(encodeHTML(Source))
        End Function
        '
        '========================================================================
        ' ----- Encode Active Content AI
        '========================================================================
        '
        Public Function convertHTMLToText(ByVal Source As String) As String
            Try
                Dim Decoder As New htmlToTextControllers(cpCore)
                Return Decoder.convert(Source)
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception")
            End Try
        End Function
        '
        '===============================================================================================================================
        '   Get Addon Selector
        '
        '   The addon selector is the string sent out with the content in edit-mode. In the editor, it is converted by javascript
        '   to the popup window that selects instance options. It is in this format:
        '
        '   Select (creates a list of names in a select box, returns the selected name)
        '       name=currentvalue[optionname0:optionvalue0|optionname1:optionvalue1|...]
        '   CheckBox (creates a list of names in checkboxes, and returns the selected names)
        '===============================================================================================================================
        '
        Public Function getAddonSelector(ByVal SrcOptionName As String, ByVal InstanceOptionValue_AddonEncoded As String, ByVal SrcOptionValueSelector As String) As String
            Dim result As String = ""
            Try
                '
                Const ACFunctionList = "List"
                Const ACFunctionList1 = "selectname"
                Const ACFunctionList2 = "listname"
                Const ACFunctionList3 = "selectcontentname"
                Const ACFunctionListID = "ListID"
                Const ACFunctionListFields = "ListFields"
                '
                Dim CID As Integer
                Dim IsContentList As Boolean
                Dim IsListField As Boolean
                Dim Choice As String
                Dim Choices() As String
                Dim ChoiceCnt As Integer
                Dim Ptr As Integer
                Dim IncludeID As Boolean
                Dim FnLen As Integer
                Dim RecordID As Integer
                Dim CS As Integer
                Dim ContentName As String
                Dim Pos As Integer
                Dim list As String
                Dim FnArgList As String
                Dim FnArgs() As String
                Dim FnArgCnt As Integer
                Dim ContentCriteria As String
                Dim RecordName As String
                Dim SrcSelectorInner As String
                Dim SrcSelectorSuffix As String = String.Empty
                Dim Cell(,) As Object
                Dim RowCnt As Integer
                Dim RowPtr As Integer
                Dim SrcSelector As String = Trim(SrcOptionValueSelector)
                '
                SrcSelectorInner = SrcSelector
                Dim PosLeft As Integer = genericController.vbInstr(1, SrcSelector, "[")
                If PosLeft <> 0 Then
                    Dim PosRight As Integer = genericController.vbInstr(1, SrcSelector, "]")
                    If PosRight <> 0 Then
                        If (PosRight < Len(SrcSelector)) Then
                            SrcSelectorSuffix = Mid(SrcSelector, PosRight + 1)
                        End If
                        SrcSelector = Trim(Mid(SrcSelector, PosLeft, PosRight - PosLeft + 1))
                        SrcSelectorInner = Trim(Mid(SrcSelector, 2, Len(SrcSelector) - 2))
                    End If
                End If
                list = ""
                '
                ' Break SrcSelectorInner up into individual choices to detect functions
                '
                If SrcSelectorInner <> "" Then
                    Choices = Split(SrcSelectorInner, "|")
                    ChoiceCnt = UBound(Choices) + 1
                    For Ptr = 0 To ChoiceCnt - 1
                        Choice = Choices(Ptr)
                        IsContentList = False
                        IsListField = False
                        '
                        ' List Function (and all the indecision that went along with it)
                        '
                        Pos = 0
                        If Pos = 0 Then
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList1 & "(", vbTextCompare)
                            If Pos > 0 Then
                                IsContentList = True
                                IncludeID = False
                                FnLen = Len(ACFunctionList1)
                            End If
                        End If
                        If Pos = 0 Then
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList2 & "(", vbTextCompare)
                            If Pos > 0 Then
                                IsContentList = True
                                IncludeID = False
                                FnLen = Len(ACFunctionList2)
                            End If
                        End If
                        If Pos = 0 Then
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList3 & "(", vbTextCompare)
                            If Pos > 0 Then
                                IsContentList = True
                                IncludeID = False
                                FnLen = Len(ACFunctionList3)
                            End If
                        End If
                        If Pos = 0 Then
                            Pos = genericController.vbInstr(1, Choice, ACFunctionListID & "(", vbTextCompare)
                            If Pos > 0 Then
                                IsContentList = True
                                IncludeID = True
                                FnLen = Len(ACFunctionListID)
                            End If
                        End If
                        If Pos = 0 Then
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList & "(", vbTextCompare)
                            If Pos > 0 Then
                                IsContentList = True
                                IncludeID = False
                                FnLen = Len(ACFunctionList)
                            End If
                        End If
                        If Pos = 0 Then
                            Pos = genericController.vbInstr(1, Choice, ACFunctionListFields & "(", vbTextCompare)
                            If Pos > 0 Then
                                IsListField = True
                                IncludeID = False
                                FnLen = Len(ACFunctionListFields)
                            End If
                        End If
                        '
                        If Pos > 0 Then
                            '
                            FnArgList = Trim(Mid(Choice, Pos + FnLen))
                            ContentName = ""
                            ContentCriteria = ""
                            If (Left(FnArgList, 1) = "(") And (Right(FnArgList, 1) = ")") Then
                                '
                                ' set ContentName and ContentCriteria from argument list
                                '
                                FnArgList = Mid(FnArgList, 2, Len(FnArgList) - 2)
                                FnArgs = genericController.SplitDelimited(FnArgList, ",")
                                FnArgCnt = UBound(FnArgs) + 1
                                If FnArgCnt > 0 Then
                                    ContentName = Trim(FnArgs(0))
                                    If (Left(ContentName, 1) = """") And (Right(ContentName, 1) = """") Then
                                        ContentName = Trim(Mid(ContentName, 2, Len(ContentName) - 2))
                                    ElseIf (Left(ContentName, 1) = "'") And (Right(ContentName, 1) = "'") Then
                                        ContentName = Trim(Mid(ContentName, 2, Len(ContentName) - 2))
                                    End If
                                End If
                                If FnArgCnt > 1 Then
                                    ContentCriteria = Trim(FnArgs(1))
                                    If (Left(ContentCriteria, 1) = """") And (Right(ContentCriteria, 1) = """") Then
                                        ContentCriteria = Trim(Mid(ContentCriteria, 2, Len(ContentCriteria) - 2))
                                    ElseIf (Left(ContentCriteria, 1) = "'") And (Right(ContentCriteria, 1) = "'") Then
                                        ContentCriteria = Trim(Mid(ContentCriteria, 2, Len(ContentCriteria) - 2))
                                    End If
                                End If
                            End If
                            CS = -1
                            If IsContentList Then
                                '
                                ' ContentList - Open the Content and build the options from the names
                                '
                                If ContentCriteria <> "" Then
                                    CS = cpCore.db.csOpen(ContentName, ContentCriteria, "name", , , , , "ID,Name")
                                Else
                                    CS = cpCore.db.csOpen(ContentName, , "name", , , , , "ID,Name")
                                End If
                            ElseIf IsListField Then
                                '
                                ' ListField
                                '
                                CID = Models.Complex.cdefModel.getContentId(cpCore, ContentName)
                                If CID > 0 Then
                                    CS = cpCore.db.csOpen("Content Fields", "Contentid=" & CID, "name", , , , , "ID,Name")
                                End If
                            End If

                            If cpCore.db.csOk(CS) Then
                                Cell = cpCore.db.cs_getRows(CS)
                                RowCnt = UBound(Cell, 2) + 1
                                For RowPtr = 0 To RowCnt - 1
                                    '
                                    RecordName = genericController.encodeText(Cell(1, RowPtr))
                                    RecordName = genericController.vbReplace(RecordName, vbCrLf, " ")
                                    RecordID = genericController.EncodeInteger(Cell(0, RowPtr))
                                    If RecordName = "" Then
                                        RecordName = "record " & RecordID
                                    ElseIf Len(RecordName) > 50 Then
                                        RecordName = Left(RecordName, 50) & "..."
                                    End If
                                    RecordName = genericController.encodeNvaArgument(RecordName)
                                    list = list & "|" & RecordName
                                    If IncludeID Then
                                        list = list & ":" & RecordID
                                    End If
                                Next
                            End If
                            Call cpCore.db.csClose(CS)
                        Else
                            '
                            ' choice is not a function, just add the choice back to the list
                            '
                            list = list & "|" & Choices(Ptr)
                        End If
                    Next
                    If list <> "" Then
                        list = Mid(list, 2)
                    End If
                End If
                '
                ' Build output string
                '
                'csv_result = encodeNvaArgument(SrcOptionName)
                result = encodeHTML(genericController.encodeNvaArgument(SrcOptionName)) & "="
                If InstanceOptionValue_AddonEncoded <> "" Then
                    result = result & encodeHTML(InstanceOptionValue_AddonEncoded)
                End If
                If SrcSelectorSuffix = "" And list = "" Then
                    '
                    ' empty list with no suffix, return with name=value
                    '
                ElseIf genericController.vbLCase(SrcSelectorSuffix) = "resourcelink" Then
                    '
                    ' resource link, exit with empty list
                    '
                    result = result & "[]ResourceLink"
                Else
                    '
                    '
                    '
                    result = result & "[" & list & "]" & SrcSelectorSuffix
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' main_Get an HTML Form text input (or text area)
        '
        Public Function getFormInputHTML(ByVal htmlName As String, Optional ByVal DefaultValue As String = "", Optional ByVal styleHeight As String = "", Optional ByVal styleWidth As String = "", Optional ByVal readOnlyfield As Boolean = False, Optional ByVal allowActiveContent As Boolean = False, Optional ByVal addonListJSON As String = "", Optional ByVal styleList As String = "", Optional ByVal styleOptionList As String = "", Optional ByVal allowResourceLibrary As Boolean = False) As String
            Dim returnHtml As String = ""
            Try
                Dim FieldTypeDefaultEditorAddonIdList As String = editorController.getFieldTypeDefaultEditorAddonIdList(cpCore)
                Dim FieldTypeDefaultEditorAddonIds() As String = Split(FieldTypeDefaultEditorAddonIdList, ",")
                Dim FieldTypeDefaultEditorAddonId As Integer = genericController.EncodeInteger(FieldTypeDefaultEditorAddonIds(FieldTypeIdHTML))
                If FieldTypeDefaultEditorAddonId = 0 Then
                    '
                    '    use default wysiwyg
                    returnHtml = html_GetFormInputTextExpandable2(htmlName, DefaultValue)
                Else
                    '
                    ' use addon editor
                    Dim arguments As New Dictionary(Of String, String)
                    arguments.Add("editorName", htmlName)
                    arguments.Add("editorValue", DefaultValue)
                    arguments.Add("editorFieldType", FieldTypeIdHTML.ToString())
                    arguments.Add("editorReadOnly", readOnlyfield.ToString())
                    arguments.Add("editorWidth", styleWidth)
                    arguments.Add("editorHeight", styleHeight)
                    arguments.Add("editorAllowResourceLibrary", allowResourceLibrary.ToString())
                    arguments.Add("editorAllowActiveContent", allowActiveContent.ToString())
                    arguments.Add("editorAddonList", addonListJSON)
                    arguments.Add("editorStyles", styleList)
                    arguments.Add("editorStyleOptions", styleOptionList)
                    returnHtml = cpCore.addon.execute(addonModel.create(cpCore, FieldTypeDefaultEditorAddonId), New CPUtilsBaseClass.addonExecuteContext() With {
                        .addonType = CPUtilsBaseClass.addonContext.ContextEditor,
                        .instanceArguments = arguments
                    })
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        ' ----- Process the reply from the Tools Panel form
        '========================================================================
        '
        Public Sub processFormToolsPanel(Optional legacyFormSn As String = "")
            Try
                Dim Button As String
                Dim username As String
                '
                ' ----- Read in and save the Member profile values from the tools panel
                '
                If (cpCore.doc.authContext.user.id > 0) Then
                    If Not (cpCore.doc.debug_iUserError <> "") Then
                        Button = cpCore.docProperties.getText(legacyFormSn & "mb")
                        Select Case Button
                            Case ButtonLogout
                                '
                                ' Logout - This can only come from the Horizonal Tool Bar
                                '
                                Call cpCore.doc.authContext.logout(cpCore)
                            Case ButtonLogin
                                '
                                ' Login - This can only come from the Horizonal Tool Bar
                                '
                                Call Controllers.loginController.processFormLoginDefault(cpCore)
                            Case ButtonApply
                                '
                                ' Apply
                                '
                                username = cpCore.docProperties.getText(legacyFormSn & "username")
                                If username <> "" Then
                                    Call Controllers.loginController.processFormLoginDefault(cpCore)
                                End If
                                '
                                ' ----- AllowAdminLinks
                                '
                                Call cpCore.visitProperty.setProperty("AllowEditing", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowEditing")))
                                '
                                ' ----- Quick Editor
                                '
                                Call cpCore.visitProperty.setProperty("AllowQuickEditor", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowQuickEditor")))
                                '
                                ' ----- Advanced Editor
                                '
                                Call cpCore.visitProperty.setProperty("AllowAdvancedEditor", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowAdvancedEditor")))
                                '
                                ' ----- Allow Workflow authoring Render Mode - Visit Property
                                '
                                Call cpCore.visitProperty.setProperty("AllowWorkflowRendering", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowWorkflowRendering")))
                                '
                                ' ----- developer Only parts
                                '
                                Call cpCore.visitProperty.setProperty("AllowDebugging", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowDebugging")))
                        End Select
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' -----
        '========================================================================
        '
        Public Sub processAddonSettingsEditor()
            '
            Dim constructor As String
            Dim ParseOK As Boolean
            Dim PosNameStart As Integer
            Dim PosNameEnd As Integer
            Dim AddonName As String
            'Dim CSAddon As Integer
            Dim OptionPtr As Integer
            Dim ArgValueAddonEncoded As String
            Dim OptionCnt As Integer
            Dim needToClearCache As Boolean
            Dim ConstructorSplit() As String
            Dim Ptr As Integer
            Dim Arg() As String
            Dim ArgName As String
            Dim ArgValue As String
            Dim AddonOptionConstructor As String = String.Empty
            Dim addonOption_String As String = String.Empty
            Dim fieldType As Integer
            Dim Copy As String = String.Empty
            Dim MethodName As String
            Dim RecordID As Integer
            Dim FieldName As String
            Dim ACInstanceID As String
            Dim ContentName As String
            Dim CS As Integer
            Dim PosACInstanceID As Integer
            Dim PosStart As Integer
            Dim PosIDStart As Integer
            Dim PosIDEnd As Integer
            '
            MethodName = "main_ProcessAddonSettingsEditor()"
            '
            ContentName = cpCore.docProperties.getText("ContentName")
            RecordID = cpCore.docProperties.getInteger("RecordID")
            FieldName = cpCore.docProperties.getText("FieldName")
            ACInstanceID = cpCore.docProperties.getText("ACInstanceID")
            Dim FoundAddon As Boolean = False
            If (ACInstanceID = PageChildListInstanceID) Then
                '
                ' ----- Page Content Child List Add-on
                '
                If (RecordID <> 0) Then
                    Dim addon As addonModel = cpCore.addonCache.getAddonById(cpCore.siteProperties.childListAddonID)
                    If (addon IsNot Nothing) Then
                        FoundAddon = True
                        AddonOptionConstructor = addon.ArgumentList
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                        If True Then
                            If AddonOptionConstructor <> "" Then
                                AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                            End If
                            If addon.IsInline Then
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                            Else
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                            End If
                        End If

                        ConstructorSplit = Split(AddonOptionConstructor, vbCrLf)
                        AddonOptionConstructor = ""
                        '
                        ' main_Get all responses from current Argument List and build new addonOption_String
                        '
                        For Ptr = 0 To UBound(ConstructorSplit)
                            Arg = Split(ConstructorSplit(Ptr), "=")
                            ArgName = Arg(0)
                            OptionCnt = cpCore.docProperties.getInteger(ArgName & "CheckBoxCnt")
                            If OptionCnt > 0 Then
                                ArgValueAddonEncoded = ""
                                For OptionPtr = 0 To OptionCnt - 1
                                    ArgValue = cpCore.docProperties.getText(ArgName & OptionPtr)
                                    If ArgValue <> "" Then
                                        ArgValueAddonEncoded = ArgValueAddonEncoded & "," & genericController.encodeNvaArgument(ArgValue)
                                    End If
                                Next
                                If ArgValueAddonEncoded <> "" Then
                                    ArgValueAddonEncoded = Mid(ArgValueAddonEncoded, 2)
                                End If
                            Else
                                ArgValue = cpCore.docProperties.getText(ArgName)
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue)
                            End If
                            addonOption_String = addonOption_String & "&" & genericController.encodeNvaArgument(ArgName) & "=" & ArgValueAddonEncoded
                        Next
                        If addonOption_String <> "" Then
                            addonOption_String = Mid(addonOption_String, 2)
                        End If

                    End If
                    Call cpCore.db.executeQuery("update ccpagecontent set ChildListInstanceOptions=" & cpCore.db.encodeSQLText(addonOption_String) & " where id=" & RecordID)
                    needToClearCache = True
                    'CS = main_OpenCSContentRecord("page content", RecordID)
                    'If app.csv_IsCSOK(CS) Then
                    '    Call app.SetCS(CS, "ChildListInstanceOptions", addonOption_String)
                    '    needToClearCache = True
                    'End If
                    'Call app.closeCS(CS)
                End If
            ElseIf (ACInstanceID = "-2") And (FieldName <> "") Then
                '
                ' ----- Admin Addon, ACInstanceID=-2, FieldName=AddonName
                '
                AddonName = FieldName
                FoundAddon = False
                Dim addon As addonModel = cpCore.addonCache.getAddonByName(AddonName)
                If (addon IsNot Nothing) Then
                    FoundAddon = True
                    AddonOptionConstructor = addon.ArgumentList
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                    If AddonOptionConstructor <> "" Then
                        AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                    End If
                    If genericController.EncodeBoolean(addon.IsInline) Then
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                    Else
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                    End If
                End If
                If Not FoundAddon Then
                    '
                    ' Hardcoded Addons
                    '
                    Select Case genericController.vbLCase(AddonName)
                        Case "block text"
                            FoundAddon = True
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockText
                        Case ""
                    End Select
                End If
                If FoundAddon Then
                    ConstructorSplit = Split(AddonOptionConstructor, vbCrLf)
                    addonOption_String = ""
                    '
                    ' main_Get all responses from current Argument List
                    '
                    For Ptr = 0 To UBound(ConstructorSplit)
                        Dim nvp As String
                        nvp = Trim(ConstructorSplit(Ptr))
                        If nvp <> "" Then
                            Arg = Split(ConstructorSplit(Ptr), "=")
                            ArgName = Arg(0)
                            OptionCnt = cpCore.docProperties.getInteger(ArgName & "CheckBoxCnt")
                            If OptionCnt > 0 Then
                                ArgValueAddonEncoded = ""
                                For OptionPtr = 0 To OptionCnt - 1
                                    ArgValue = cpCore.docProperties.getText(ArgName & OptionPtr)
                                    If ArgValue <> "" Then
                                        ArgValueAddonEncoded = ArgValueAddonEncoded & "," & genericController.encodeNvaArgument(ArgValue)
                                    End If
                                Next
                                If ArgValueAddonEncoded <> "" Then
                                    ArgValueAddonEncoded = Mid(ArgValueAddonEncoded, 2)
                                End If
                            Else
                                ArgValue = cpCore.docProperties.getText(ArgName)
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue)
                            End If
                            addonOption_String = addonOption_String & "&" & genericController.encodeNvaArgument(ArgName) & "=" & ArgValueAddonEncoded
                        End If
                    Next
                    If addonOption_String <> "" Then
                        addonOption_String = Mid(addonOption_String, 2)
                    End If
                    Call cpCore.userProperty.setProperty("Addon [" & AddonName & "] Options", addonOption_String)
                    needToClearCache = True
                End If
            ElseIf ContentName = "" Or RecordID = 0 Then
                '
                ' ----- Public Site call, must have contentname and recordid
                '
                cpCore.handleException(New Exception("invalid content [" & ContentName & "], RecordID [" & RecordID & "]"))
            Else
                '
                ' ----- Normal Content Edit - find instance in the content
                '
                CS = cpCore.db.csOpenRecord(ContentName, RecordID)
                If Not cpCore.db.csOk(CS) Then
                    cpCore.handleException(New Exception("No record found with content [" & ContentName & "] and RecordID [" & RecordID & "]"))
                Else
                    If FieldName <> "" Then
                        '
                        ' Field is given, find the position
                        '
                        Copy = cpCore.db.csGet(CS, FieldName)
                        PosACInstanceID = genericController.vbInstr(1, Copy, "=""" & ACInstanceID & """ ", vbTextCompare)
                    Else
                        '
                        ' Find the field, then find the position
                        '
                        FieldName = cpCore.db.cs_getFirstFieldName(CS)
                        Do While FieldName <> ""
                            fieldType = cpCore.db.cs_getFieldTypeId(CS, FieldName)
                            Select Case fieldType
                                Case FieldTypeIdLongText, FieldTypeIdText, FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdHTML, FieldTypeIdFileHTML
                                    Copy = cpCore.db.csGet(CS, FieldName)
                                    PosACInstanceID = genericController.vbInstr(1, Copy, "ACInstanceID=""" & ACInstanceID & """", vbTextCompare)
                                    If PosACInstanceID <> 0 Then
                                        '
                                        ' found the instance
                                        '
                                        PosACInstanceID = PosACInstanceID + 13
                                        Exit Do
                                    End If
                            End Select
                            FieldName = cpCore.db.cs_getNextFieldName(CS)
                        Loop
                    End If
                    '
                    ' Parse out the Addon Name
                    '
                    If PosACInstanceID = 0 Then
                        cpCore.handleException(New Exception("AC Instance [" & ACInstanceID & "] not found in record with content [" & ContentName & "] and RecordID [" & RecordID & "]"))
                    Else
                        Copy = upgradeActiveContent(Copy)
                        ParseOK = False
                        PosStart = InStrRev(Copy, "<ac ", PosACInstanceID, vbTextCompare)
                        If PosStart <> 0 Then
                            '
                            ' main_Get Addon Name to lookup Addon and main_Get most recent Argument List
                            '
                            PosNameStart = genericController.vbInstr(PosStart, Copy, " name=", vbTextCompare)
                            If PosNameStart <> 0 Then
                                PosNameStart = PosNameStart + 7
                                PosNameEnd = genericController.vbInstr(PosNameStart, Copy, """")
                                If PosNameEnd <> 0 Then
                                    AddonName = Mid(Copy, PosNameStart, PosNameEnd - PosNameStart)
                                    '????? test this
                                    FoundAddon = False
                                    Dim embeddedAddon As addonModel = cpCore.addonCache.getAddonByName(AddonName)
                                    If (embeddedAddon IsNot Nothing) Then
                                        FoundAddon = True
                                        AddonOptionConstructor = genericController.encodeText(embeddedAddon.ArgumentList)
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                                        If AddonOptionConstructor <> "" Then
                                            AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                                        End If
                                        If genericController.EncodeBoolean(embeddedAddon.IsInline) Then
                                            AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                                        Else
                                            AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                                        End If
                                    Else
                                        '
                                        ' -- Hardcoded Addons
                                        Select Case genericController.vbLCase(AddonName)
                                            Case "block text"
                                                FoundAddon = True
                                                AddonOptionConstructor = AddonOptionConstructor_ForBlockText
                                            Case ""
                                        End Select
                                    End If
                                    If FoundAddon Then
                                        ConstructorSplit = Split(AddonOptionConstructor, vbCrLf)
                                        addonOption_String = ""
                                        '
                                        ' main_Get all responses from current Argument List
                                        '
                                        For Ptr = 0 To UBound(ConstructorSplit)
                                            constructor = ConstructorSplit(Ptr)
                                            If constructor <> "" Then
                                                Arg = Split(constructor, "=")
                                                ArgName = Arg(0)
                                                OptionCnt = cpCore.docProperties.getInteger(ArgName & "CheckBoxCnt")
                                                If OptionCnt > 0 Then
                                                    ArgValueAddonEncoded = ""
                                                    For OptionPtr = 0 To OptionCnt - 1
                                                        ArgValue = cpCore.docProperties.getText(ArgName & OptionPtr)
                                                        If ArgValue <> "" Then
                                                            ArgValueAddonEncoded = ArgValueAddonEncoded & "," & genericController.encodeNvaArgument(ArgValue)
                                                        End If
                                                    Next
                                                    If ArgValueAddonEncoded <> "" Then
                                                        ArgValueAddonEncoded = Mid(ArgValueAddonEncoded, 2)
                                                    End If
                                                Else
                                                    ArgValue = cpCore.docProperties.getText(ArgName)
                                                    ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue)
                                                End If

                                                addonOption_String = addonOption_String & "&" & genericController.encodeNvaArgument(ArgName) & "=" & ArgValueAddonEncoded
                                            End If
                                        Next
                                        If addonOption_String <> "" Then
                                            addonOption_String = Mid(addonOption_String, 2)
                                        End If
                                    End If
                                End If
                            End If
                            '
                            ' Replace the new querystring into the AC tag in the content
                            '
                            PosIDStart = genericController.vbInstr(PosStart, Copy, " querystring=", vbTextCompare)
                            If PosIDStart <> 0 Then
                                PosIDStart = PosIDStart + 14
                                If PosIDStart <> 0 Then
                                    PosIDEnd = genericController.vbInstr(PosIDStart, Copy, """")
                                    If PosIDEnd <> 0 Then
                                        ParseOK = True
                                        Copy = Mid(Copy, 1, PosIDStart - 1) & encodeHTML(addonOption_String) & Mid(Copy, PosIDEnd)
                                        Call cpCore.db.csSet(CS, FieldName, Copy)
                                        needToClearCache = True
                                    End If
                                End If
                            End If
                        End If
                        If Not ParseOK Then
                            cpCore.handleException(New Exception("There was a problem parsing AC Instance [" & ACInstanceID & "] record with content [" & ContentName & "] and RecordID [" & RecordID & "]"))
                        End If
                    End If
                End If
                Call cpCore.db.csClose(CS)
            End If
            If needToClearCache Then
                '
                ' Clear Caches
                '
                If ContentName <> "" Then
                    Call cpCore.cache.invalidateAllObjectsInContent(ContentName)
                End If
            End If
        End Sub
        '
        '========================================================================
        ' ----- Process the little edit form in the help bubble
        '========================================================================
        '
        Public Sub processHelpBubbleEditor()
            '
            Dim SQL As String
            Dim MethodName As String
            Dim HelpBubbleID As String
            Dim IDSplit() As String
            Dim RecordID As Integer
            Dim HelpCaption As String
            Dim HelpMessage As String
            '
            MethodName = "main_ProcessHelpBubbleEditor()"
            '
            HelpBubbleID = cpCore.docProperties.getText("HelpBubbleID")
            IDSplit = Split(HelpBubbleID, "-")
            Select Case genericController.vbLCase(IDSplit(0))
                Case "userfield"
                    '
                    ' main_Get the id of the field, and save the input as the caption and help
                    '
                    If UBound(IDSplit) > 0 Then
                        RecordID = genericController.EncodeInteger(IDSplit(1))
                        If RecordID > 0 Then
                            HelpCaption = cpCore.docProperties.getText("helpcaption")
                            HelpMessage = cpCore.docProperties.getText("helptext")
                            SQL = "update ccfields set caption=" & cpCore.db.encodeSQLText(HelpCaption) & ",HelpMessage=" & cpCore.db.encodeSQLText(HelpMessage) & " where id=" & RecordID
                            Call cpCore.db.executeQuery(SQL)
                            cpCore.cache.invalidateAll()
                            cpCore.doc.clearMetaData()
                        End If
                    End If
            End Select
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Convert an active content field (html data stored with <ac></ac> html tags) to a wysiwyg editor request (html with edit icon <img> for <ac></ac>)
        ''' </summary>
        ''' <param name="editorValue"></param>
        ''' <returns></returns>
        Public Function convertActiveContentToHtmlForWysiwygEditor(editorValue As String) As String
            Return cpCore.html.convertActiveContent_internal(editorValue, 0, "", 0, 0, False, False, False, True, True, False, "", "", False, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, False, Nothing, False)
        End Function
        '
        '====================================================================================================
        '
        Public Function convertActiveContentToJsonForRemoteMethod(Source As String, ContextContentName As String, ContextRecordID As Integer, ContextContactPeopleID As Integer, ProtocolHostString As String, DefaultWrapperID As Integer, ignore_TemplateCaseOnly_Content As String, addonContext As CPUtilsBaseClass.addonContext) As String
            Return convertActiveContent_internal(Source, cpCore.doc.authContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, False, False, True, True, False, True, "", ProtocolHostString, False, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, cpCore.doc.authContext.isAuthenticated, Nothing, cpCore.doc.authContext.isEditingAnything())
            'False, False, True, True, False, True, ""
        End Function
        '
        '====================================================================================================
        '
        Public Function convertActiveContentToHtmlForWebRender(Source As String, ContextContentName As String, ContextRecordID As Integer, ContextContactPeopleID As Integer, ProtocolHostString As String, DefaultWrapperID As Integer, addonContext As CPUtilsBaseClass.addonContext) As String
            Return convertActiveContent_internal(Source, cpCore.doc.authContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, False, False, True, True, False, True, "", ProtocolHostString, False, DefaultWrapperID, "", addonContext, cpCore.doc.authContext.isAuthenticated, Nothing, cpCore.doc.authContext.isEditingAnything())
            'False, False, True, True, False, True, ""
        End Function
        '
        '====================================================================================================
        '
        Public Function convertActiveContentToHtmlForEmailSend(Source As String, personalizationPeopleID As Integer, queryStringForLinkAppend As String) As String
            Return convertActiveContent_internal(Source, personalizationPeopleID, "", 0, 0, False, True, True, True, False, True, queryStringForLinkAppend, "", True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
            'False, False, True, True, False, True, ""
        End Function

        '
        '========================================================================
        ' Print the Member Edit form
        '
        '   For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
        '       PrimaryContentName = "People"
        '       PrimaryRecordID = MemberID
        '       SecondaryContentName = "Groups"
        '       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        '       RulesContentName = "Member Rules"
        '       RulesPrimaryFieldName = "MemberID"
        '       RulesSecondaryFieldName = "GroupID"
        '========================================================================
        '
        Public Function getCheckList2(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyfield As Boolean = False) As String
            getCheckList2 = getCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, genericController.encodeText(CaptionFieldName), readOnlyfield, False, "")
        End Function
        '
        '========================================================================
        '   main_Get a list of checkbox options based on a standard set of rules
        '
        '   IncludeContentFolderDivs
        '       When true, the list of options (checkboxes) are grouped by ContentFolder and wrapped in a Div with ID="ContentFolder99"
        '
        '   For instance, list out a options of all public groups, with the ones checked that this member belongs to
        '       PrimaryContentName = "People"
        '       PrimaryRecordID = MemberID
        '       SecondaryContentName = "Groups"
        '       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        '       RulesContentName = "Member Rules"
        '       RulesPrimaryFieldName = "MemberID"
        '       RulesSecondaryFieldName = "GroupID"
        '========================================================================
        '
        Public Function getCheckList(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyfield As Boolean = False, Optional ByVal IncludeContentFolderDivs As Boolean = False, Optional ByVal DefaultSecondaryIDList As String = "") As String
            Dim returnHtml As String = ""
            Try
                Dim main_MemberShipText() As String
                Dim Ptr As Integer
                Dim main_MemberShipID As Integer
                Dim javaScriptRequired As String = ""
                Dim DivName As String
                Dim DivCnt As Integer
                Dim OldFolderVar As String
                Dim EndDiv As String
                Dim OpenFolderID As Integer
                Dim RuleCopyCaption As String
                Dim RuleCopy As String
                Dim SQL As String
                Dim CS As Integer
                Dim main_MemberShipCount As Integer
                Dim main_MemberShipSize As Integer
                Dim main_MemberShipPointer As Integer
                Dim SectionName As String
                Dim CheckBoxCnt As Integer
                Dim DivCheckBoxCnt As Integer
                Dim main_MemberShip As Integer() = {}
                Dim main_MemberShipRuleCopy As String() = {}
                Dim PrimaryContentID As Integer
                Dim SecondaryTablename As String
                Dim SecondaryContentID As Integer
                Dim rulesTablename As String
                Dim OptionName As String
                Dim OptionCaption As String
                Dim optionCaptionHtmlEncoded As String
                Dim CanSeeHiddenFields As Boolean
                Dim SecondaryCDef As Models.Complex.cdefModel
                Dim ContentIDList As New List(Of Integer)
                Dim Found As Boolean
                Dim RecordID As Integer
                Dim SingularPrefixHtmlEncoded As String
                Dim IsRuleCopySupported As Boolean
                Dim AllowRuleCopy As Boolean
                '
                ' IsRuleCopySupported - if true, the rule records include an allow button, and copy
                '   This is for a checkbox like [ ] Other [enter other copy here]
                '
                IsRuleCopySupported = Models.Complex.cdefModel.isContentFieldSupported(cpCore, RulesContentName, "RuleCopy")
                If IsRuleCopySupported Then
                    IsRuleCopySupported = IsRuleCopySupported And Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy")
                    If IsRuleCopySupported Then
                        IsRuleCopySupported = IsRuleCopySupported And Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption")
                    End If
                End If
                If CaptionFieldName = "" Then
                    CaptionFieldName = "name"
                End If
                CaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name")
                If PrimaryContentName = "" Or SecondaryContentName = "" Or RulesContentName = "" Or RulesPrimaryFieldname = "" Or RulesSecondaryFieldName = "" Then
                    returnHtml = "[Checklist not configured]"
                    cpCore.handleException(New Exception("Creating checklist, all required fields were not supplied, Caption=[" & CaptionFieldName & "], PrimaryContentName=[" & PrimaryContentName & "], SecondaryContentName=[" & SecondaryContentName & "], RulesContentName=[" & RulesContentName & "], RulesPrimaryFieldName=[" & RulesPrimaryFieldname & "], RulesSecondaryFieldName=[" & RulesSecondaryFieldName & "]"))
                Else
                    '
                    ' ----- Gather all the SecondaryContent that associates to the PrimaryContent
                    '
                    PrimaryContentID = Models.Complex.cdefModel.getContentId(cpCore, PrimaryContentName)
                    SecondaryCDef = Models.Complex.cdefModel.getCdef(cpCore, SecondaryContentName)
                    SecondaryTablename = SecondaryCDef.ContentTableName
                    SecondaryContentID = SecondaryCDef.Id
                    ContentIDList.Add(SecondaryContentID)
                    ContentIDList.AddRange(SecondaryCDef.childIdList(cpCore))
                    '
                    '
                    '
                    rulesTablename = Models.Complex.cdefModel.getContentTablename(cpCore, RulesContentName)
                    SingularPrefixHtmlEncoded = encodeHTML(genericController.GetSingular(SecondaryContentName)) & "&nbsp;"
                    '
                    main_MemberShipCount = 0
                    main_MemberShipSize = 0
                    returnHtml = ""
                    If (SecondaryTablename <> "") And (rulesTablename <> "") Then
                        OldFolderVar = "OldFolder" & cpCore.doc.checkListCnt
                        javaScriptRequired &= "var " & OldFolderVar & ";"
                        If PrimaryRecordID = 0 Then
                            '
                            ' New record, use the DefaultSecondaryIDList
                            '
                            If DefaultSecondaryIDList <> "" Then

                                main_MemberShipText = Split(DefaultSecondaryIDList, ",")
                                For Ptr = 0 To UBound(main_MemberShipText)
                                    main_MemberShipID = genericController.EncodeInteger(main_MemberShipText(Ptr))
                                    If main_MemberShipID <> 0 Then
                                        ReDim Preserve main_MemberShip(Ptr)
                                        main_MemberShip(Ptr) = main_MemberShipID
                                        main_MemberShipCount = Ptr + 1
                                    End If
                                Next
                                If main_MemberShipCount > 0 Then
                                    ReDim main_MemberShipRuleCopy(main_MemberShipCount - 1)
                                End If
                                'main_MemberShipCount = UBound(main_MemberShip) + 1
                                main_MemberShipSize = main_MemberShipCount
                            End If
                        Else
                            '
                            ' ----- Determine main_MemberShip (which secondary records are associated by a rule)
                            ' ----- (exclude new record issue ID=0)
                            '
                            If IsRuleCopySupported Then
                                SQL = "SELECT " & SecondaryTablename & ".ID AS ID," & rulesTablename & ".RuleCopy"
                            Else
                                SQL = "SELECT " & SecondaryTablename & ".ID AS ID,'' as RuleCopy"
                            End If
                            SQL &= "" _
                            & " FROM " & SecondaryTablename & " LEFT JOIN" _
                            & " " & rulesTablename & " ON " & SecondaryTablename & ".ID = " & rulesTablename & "." & RulesSecondaryFieldName _
                            & " WHERE " _
                            & " (" & rulesTablename & "." & RulesPrimaryFieldname & "=" & PrimaryRecordID & ")" _
                            & " AND (" & rulesTablename & ".Active<>0)" _
                            & " AND (" & SecondaryTablename & ".Active<>0)" _
                            & " And (" & SecondaryTablename & ".ContentControlID IN (" & String.Join(",", ContentIDList) & "))"
                            If SecondaryContentSelectCriteria <> "" Then
                                SQL &= "AND(" & SecondaryContentSelectCriteria & ")"
                            End If
                            CS = cpCore.db.csOpenSql(SQL)
                            If cpCore.db.csOk(CS) Then
                                If True Then
                                    main_MemberShipSize = 10
                                    ReDim main_MemberShip(main_MemberShipSize)
                                    ReDim main_MemberShipRuleCopy(main_MemberShipSize)
                                    Do While cpCore.db.csOk(CS)
                                        If main_MemberShipCount >= main_MemberShipSize Then
                                            main_MemberShipSize = main_MemberShipSize + 10
                                            ReDim Preserve main_MemberShip(main_MemberShipSize)
                                            ReDim Preserve main_MemberShipRuleCopy(main_MemberShipSize)
                                        End If
                                        main_MemberShip(main_MemberShipCount) = cpCore.db.csGetInteger(CS, "ID")
                                        main_MemberShipRuleCopy(main_MemberShipCount) = cpCore.db.csGetText(CS, "RuleCopy")
                                        main_MemberShipCount = main_MemberShipCount + 1
                                        cpCore.db.csGoNext(CS)
                                    Loop
                                End If
                            End If
                            cpCore.db.csClose(CS)
                        End If
                        '
                        ' ----- Gather all the Secondary Records, sorted by ContentName
                        '
                        SQL = "SELECT " & SecondaryTablename & ".ID AS ID, " & SecondaryTablename & "." & CaptionFieldName & " AS OptionCaption, " & SecondaryTablename & ".name AS OptionName, " & SecondaryTablename & ".SortOrder"
                        If IsRuleCopySupported Then
                            SQL &= "," & SecondaryTablename & ".AllowRuleCopy," & SecondaryTablename & ".RuleCopyCaption"
                        Else
                            SQL &= ",0 as AllowRuleCopy,'' as RuleCopyCaption"
                        End If
                        SQL &= " from " & SecondaryTablename & " where (1=1)"
                        If SecondaryContentSelectCriteria <> "" Then
                            SQL &= "AND(" & SecondaryContentSelectCriteria & ")"
                        End If
                        SQL &= " GROUP BY " & SecondaryTablename & ".ID, " & SecondaryTablename & "." & CaptionFieldName & ", " & SecondaryTablename & ".name, " & SecondaryTablename & ".SortOrder"
                        If IsRuleCopySupported Then
                            SQL &= ", " & SecondaryTablename & ".AllowRuleCopy," & SecondaryTablename & ".RuleCopyCaption"
                        End If
                        SQL &= " ORDER BY "
                        SQL &= SecondaryTablename & "." & CaptionFieldName
                        CS = cpCore.db.csOpenSql(SQL)
                        If Not cpCore.db.csOk(CS) Then
                            returnHtml = "(No choices are available.)"
                        Else
                            If True Then
                                OpenFolderID = -1
                                EndDiv = ""
                                SectionName = ""
                                CheckBoxCnt = 0
                                DivCheckBoxCnt = 0
                                DivCnt = 0
                                CanSeeHiddenFields = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)
                                DivName = TagName & ".All"
                                Do While cpCore.db.csOk(CS)
                                    OptionName = cpCore.db.csGetText(CS, "OptionName")
                                    If (Mid(OptionName, 1, 1) <> "_") Or CanSeeHiddenFields Then
                                        '
                                        ' Current checkbox is visible
                                        '
                                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                                        AllowRuleCopy = cpCore.db.csGetBoolean(CS, "AllowRuleCopy")
                                        RuleCopyCaption = cpCore.db.csGetText(CS, "RuleCopyCaption")
                                        OptionCaption = cpCore.db.csGetText(CS, "OptionCaption")
                                        If OptionCaption = "" Then
                                            OptionCaption = OptionName
                                        End If
                                        If OptionCaption = "" Then
                                            optionCaptionHtmlEncoded = SingularPrefixHtmlEncoded & RecordID
                                        Else
                                            optionCaptionHtmlEncoded = encodeHTML(OptionCaption)
                                        End If
                                        If DivCheckBoxCnt <> 0 Then
                                            ' leave this between checkboxes - it is searched in the admin page
                                            returnHtml &= "<br >" & vbCrLf
                                        End If
                                        RuleCopy = ""
                                        If False Then
                                            Found = False
                                            's = s & "<input type=""checkbox"" name=""" & TagName & "." & CheckBoxCnt & """ "
                                            If main_MemberShipCount <> 0 Then
                                                For main_MemberShipPointer = 0 To main_MemberShipCount - 1
                                                    If main_MemberShip(main_MemberShipPointer) = (RecordID) Then
                                                        RuleCopy = main_MemberShipRuleCopy(main_MemberShipPointer)
                                                        returnHtml &= html_GetFormInputHidden(TagName & "." & CheckBoxCnt, True)
                                                        Found = True
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            returnHtml &= genericController.main_GetYesNo(Found) & "&nbsp;-&nbsp;"
                                        Else
                                            Found = False
                                            If main_MemberShipCount <> 0 Then
                                                For main_MemberShipPointer = 0 To main_MemberShipCount - 1
                                                    If main_MemberShip(main_MemberShipPointer) = (RecordID) Then
                                                        's = s & main_GetFormInputHidden(TagName & "." & CheckBoxCnt, True)
                                                        RuleCopy = main_MemberShipRuleCopy(main_MemberShipPointer)
                                                        Found = True
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            ' must leave the first hidden with the value in this form - it is searched in the admin pge
                                            returnHtml &= vbCrLf
                                            returnHtml &= "<table><tr><td style=""vertical-align:top;margin-top:0;width:20px;"">"
                                            returnHtml &= "<input type=hidden name=""" & TagName & "." & CheckBoxCnt & ".ID"" value=" & RecordID & ">"
                                            If readOnlyfield And Not Found Then
                                                returnHtml &= "<input type=checkbox disabled>"
                                            ElseIf readOnlyfield Then
                                                returnHtml &= "<input type=checkbox disabled checked>"
                                                returnHtml &= "<input type=""hidden"" name=""" & TagName & "." & CheckBoxCnt & ".ID"" value=" & RecordID & ">"
                                            ElseIf Found Then
                                                returnHtml &= "<input type=checkbox name=""" & TagName & "." & CheckBoxCnt & """ checked>"
                                            Else
                                                returnHtml &= "<input type=checkbox name=""" & TagName & "." & CheckBoxCnt & """>"
                                            End If
                                            returnHtml &= "</td><td style=""vertical-align:top;padding-top:4px;"">"
                                            returnHtml &= SpanClassAdminNormal & optionCaptionHtmlEncoded
                                            If AllowRuleCopy Then
                                                returnHtml &= ", " & RuleCopyCaption & "&nbsp;" & html_GetFormInputText2(TagName & "." & CheckBoxCnt & ".RuleCopy", RuleCopy, 1, 20)
                                            End If
                                            returnHtml &= "</td></tr></table>"
                                        End If
                                        CheckBoxCnt = CheckBoxCnt + 1
                                        DivCheckBoxCnt = DivCheckBoxCnt + 1
                                    End If
                                    cpCore.db.csGoNext(CS)
                                Loop
                                returnHtml &= EndDiv
                                returnHtml &= "<input type=""hidden"" name=""" & TagName & ".RowCount"" value=""" & CheckBoxCnt & """>" & vbCrLf
                            End If
                        End If
                        cpCore.db.csClose(CS)
                        Call addScriptCode_head(javaScriptRequired, "CheckList Categories")
                    End If
                    'End If
                    cpCore.doc.checkListCnt = cpCore.doc.checkListCnt + 1
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function

    End Class
End Namespace
