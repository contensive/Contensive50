
Imports Contensive.Core

Namespace Contensive.Core
    Module housekeepModule

        Sub Main()
            Dim HouseKeep As houseKeepClass
            'Dim ServerCmd As String
            Dim Args As String
            'Dim MonitorPort as integer
            'Dim InstanceCnt as integer
            Dim DebugMode As Boolean

            If Not PrevInstance() Then
                Args = Command()
                Args = Trim(Args)
                If Args <> "" Then
                    DebugMode = EncodeBoolean(InStr(1, Args, "now") <> 0)
                End If


                HouseKeep = New houseKeepClass
                Call HouseKeep.HouseKeep(DebugMode)
                HouseKeep = Nothing
            End If
        End Sub

    End Module
End Namespace