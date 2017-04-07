<textarea id="##ELEMENTNAME##" name="##ELEMENTNAME##" width="##WIDTH##" height="##HEIGHT##">##CONTENT##</textarea>
<script language=JavaScript>
var ##EDITORNAME## = new InnovaEditor("##EDITORNAME##");
##EDITORNAME##.mode="HTMLBody"; 
##EDITORNAME##.width="<Width>"; 
##EDITORNAME##.height="<Height>"; 
##EDITORNAME##.btnPrint=true;
##EDITORNAME##.btnPasteText=true;
##EDITORNAME##.btnLTR=true;
##EDITORNAME##.btnRTL=true;
##EDITORNAME##.btnStrikethrough=true;
##EDITORNAME##.btnSuperscript=true;
##EDITORNAME##.btnSubscript=true;
##EDITORNAME##.btnClearAll=true;
##EDITORNAME##.btnStyles=true;
##EDITORNAME##.arrStyle=##STYLERULES##;
##EDITORNAME##.useDIV=false;
##EDITORNAME##.useBR=false;
##EDITORNAME##.TagSelectorPosition="top";
##EDITORNAME##.publishingPath="##PUBLISHINGPATH##";
##EDITORNAME##.features=[
"FullScreen","Preview","Print","Search","|","Cut","Copy","Paste","PasteWord","PasteText","SpellCheck","|","Undo","Redo","|","Bookmark","Hyperlink","HTMLSource","XHTMLSource","BRK",
"Numbering","Bullets","|","Indent","Outdent","|","JustifyLeft","JustifyCenter","JustifyRight","JustifyFull","|","Table","Guidelines","Absolute","|","Characters","Line","Form","RemoveFormat","ClearAll","BRK",
"StyleAndFormatting","TextFormatting","ListFormatting","BoxFormatting","ParagraphFormatting","CssText","Styles","|","Paragraph","FontName","FontSize","|","Bold","Italic","Underline","Strikethrough","Superscript","Subscript","|","ForeColor","BackColor"
];
##EDITORNAME##.REPLACE("##ELEMENTNAME##");
</script>

