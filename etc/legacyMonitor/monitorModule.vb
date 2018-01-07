

Module monitorModule


    '
    ' Global Configuration Variables
    '   Read in from config file each timer hit and on start
    ''
    Public Structure AppLogType
        Dim Name As String
        Dim StatusCheckCount As Integer
        Dim ErrorCount As Integer
        Dim SiteLink As String
        Dim StatusLink As String
        Dim LastStatusResponse As String
        Dim LastCheckOK As Boolean                      ' true if the last check was OK
    End Structure
    '
    Structure ConnType
        Dim ConnectionID As Integer
        Dim BytesToSend As Integer
        Dim HTTPVersion As String
        Dim HTTPMethod As String
        Dim Path As String
        Dim Query As String
        Dim Headers As String
        Dim PostData As String
        Dim DataReadyToSend As Boolean
        Dim State As Integer
        Dim ContentLength As Integer
        Dim dateOpened As Date
        Dim sourceIp As String
    End Structure
    '
    ' Socket Request Block
    '
    Public Structure RobotPathType
        Dim Agent As String
        Dim Path As String
    End Structure
    '
    Public Structure DocType
        ' Initialize before socket call
        Dim URI As String                       ' <scheme>://<user>:<Password>@<host>:<Port>/<url-path>?<Query>
        Dim URIScheme As String                 ' http,ftp, etc. (semi supported)
        Dim URIUser As String                   ' (not supported yet)
        Dim URIPassword As String               ' (not supported yet)
        Dim URIPort As String                   ' (not supported yet)
        Dim URIHost As String                   '
        Dim URIPath As String                   '
        Dim URIPage As String                   '
        Dim URIQuery As String                  ' (not supported yet - stuck to path)
        Dim Id As Integer                          ' ID in database for this doc record
        Dim DontGet As Boolean                ' if true, do not request or analyze the document
        ' values set by socket
        Dim SocketResponse As String            ' Socket Response (if not "", socket error)
        Dim HTTPResponse As String              ' HTTP response (version,code,description)
        Dim HTTPResponseCode As String              ' HTTP response Code (200, etc)
        Dim ResponseTime As Double              ' time to fetch this page
        Dim RequestFilename As String           ' the client request (only saved if testing)
        Dim ResponseFilename As String          ' the server response
        Dim ResponseFileNumber As Integer       ' filenumber for the response file
        Dim EntityStart As Integer                 ' character count of the first byte of entity in the response file
        Dim ResponseFileLength As Integer                      ' length of content read in from HTTP
        Dim RetryCountAuth As Integer           ' retires for authorization
        Dim TextOnlyFilename As String          '
        Dim RetryCountTimeout As Integer        ' retires for timeouts
        ' Set by .
        Dim Found As Boolean                    ' if true, doc was found and read
        Dim OffSite As Boolean                  ' true if URI is not on Host being tested, set during .
        Dim HTML As Boolean                     ' content_type is html/text
        Dim Title As String                     '
        Dim MetaKeywords As String              '
        Dim MetaDescription As String           '
        Dim UnknownWordList As String           ' List not found by spell checker
        ' accumulated errors and warnings
        Dim ErrorCount As Integer                  ' count of site errors
    End Structure
    '
    ' Task value defaults
    '
    Public Const URLOnSiteDefault As Boolean = False
    Public Const DocLinkCountMaxDefault As Integer = 1000
    Public Const URIRootLimitDefault As Integer = 1
    Public Const HonorRobotsDefault As Integer = 1
    Public Const CookiesPageToPageDefault As Integer = 1
    Public Const CookiesVisitToVisitDefault As Integer = 1
    Public Const AuthUsernameDefault As String = ""
    Public Const AuthPasswordDefault As String = ""
    '
    Public AppLog() As AppLogType
    Public AppLogCnt As Integer
End Module
