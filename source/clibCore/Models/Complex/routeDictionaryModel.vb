

Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers

Namespace Contensive.Core.Models.Complex
    Public Class routeDictionaryModel
        '
        Private Const cacheNameRouteDictionary As String = "routeDictionary"
        '
        '===================================================================================================
        Public Shared Function create(cpCore As coreClass) As Dictionary(Of String, BaseClasses.CPSiteBaseClass.routeClass)
            Dim result As New Dictionary(Of String, BaseClasses.CPSiteBaseClass.routeClass)
            Try
                result = getCache(cpCore)
                If (result Is Nothing) Then
                    result = New Dictionary(Of String, BaseClasses.CPSiteBaseClass.routeClass)
                    Dim physicalFile As String = "~/" & cpCore.siteProperties.getText("serverpagedefault", "default.aspx")
                    Dim routesAdded As New List(Of String)
                    Dim uniqueRouteList As New List(Of String)
                    '
                    ' -- admin route
                    Dim adminRoute As String = genericController.normalizeRoute(cpCore.serverConfig.appConfig.adminRoute)
                    If (Not String.IsNullOrEmpty(adminRoute)) Then
                        result.Add(adminRoute, New BaseClasses.CPSiteBaseClass.routeClass() With {
                                .physicalRoute = physicalFile,
                                .virtualRoute = adminRoute,
                                .routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.admin
                            })
                        uniqueRouteList.Add(adminRoute)
                    End If
                    '
                    ' -- remote methods
                    Dim remoteMethods As List(Of addonModel) = addonModel.createList_RemoteMethods(cpCore, New List(Of String))
                    For Each remoteMethod As addonModel In remoteMethods
                        Dim route As String = genericController.normalizeRoute(remoteMethod.name)
                        If (Not String.IsNullOrEmpty(route)) Then
                            If (uniqueRouteList.Contains(route)) Then
                                cpCore.handleException(New ApplicationException("Route [" & route & "] cannot be added because it is a matches the Admin Route or another Remote Method."))
                            Else
                                result.Add(route, New BaseClasses.CPSiteBaseClass.routeClass() With {
                                        .physicalRoute = physicalFile,' & "?remoteMethodAddon=" & genericController.EncodeURL(remoteMethod.name),
                                        .virtualRoute = route,
                                        .routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.remoteMethod,
                                        .remoteMethodAddonId = remoteMethod.id
                                    })
                            End If
                        End If
                    Next
                    '
                    ' -- link forwards
                    Dim linkForwards As List(Of linkForwardModel) = linkForwardModel.createList(cpCore, "name Is Not null")
                    For Each linkForward As linkForwardModel In linkForwards
                        Dim route As String = genericController.normalizeRoute(linkForward.name)
                        If (Not String.IsNullOrEmpty(route)) Then
                            If (uniqueRouteList.Contains(route)) Then
                                cpCore.handleException(New ApplicationException("Link Foward Route [" & route & "] cannot be added because it is a matches the Admin Route, a Remote Method or another Link Forward."))
                            Else
                                result.Add(route, New BaseClasses.CPSiteBaseClass.routeClass() With {
                                        .physicalRoute = physicalFile,' & "?linkForward=" & genericController.EncodeURL(linkForward.name),
                                        .virtualRoute = route,
                                        .routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.linkForward,
                                        .linkForwardId = linkForward.id
                                    })
                            End If
                        End If
                    Next
                    '
                    ' -- link aliases
                    Dim linkAliasList As List(Of linkAliasModel) = linkAliasModel.createList(cpCore, "name Is Not null")
                    For Each linkAlias As linkAliasModel In linkAliasList
                        Dim route As String = genericController.normalizeRoute(linkAlias.name)
                        If (Not String.IsNullOrEmpty(route)) Then
                            If (uniqueRouteList.Contains(route)) Then
                                cpCore.handleException(New ApplicationException("Link Alias route [" & route & "] cannot be added because it is a matches the Admin Route, a Remote Method, a Link Forward o another Link Alias."))
                            Else
                                result.Add(route, New BaseClasses.CPSiteBaseClass.routeClass() With {
                                        .physicalRoute = physicalFile,' & "?linkAlias=" & genericController.EncodeURL(linkAlias.name),
                                        .virtualRoute = route,
                                        .routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.linkAlias,
                                        .linkAliasId = linkAlias.id
                                    })
                            End If
                        End If
                    Next
                    Call setCache(cpCore, result)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        Public Shared Sub setCache(cpCore As coreClass, routeDictionary As Dictionary(Of String, BaseClasses.CPSiteBaseClass.routeClass))
            Call cpCore.cache.setContent(cacheNameRouteDictionary, routeDictionary)
        End Sub
        '
        '====================================================================================================
        Public Shared Function getCache(cpCore As coreClass) As Dictionary(Of String, BaseClasses.CPSiteBaseClass.routeClass)
            Return cpCore.cache.getObject(Of Dictionary(Of String, BaseClasses.CPSiteBaseClass.routeClass))(cacheNameRouteDictionary)
        End Function
        '
        '====================================================================================================
        Public Shared Sub invalidateCache(cpCore As coreClass)
            cpCore.cache.invalidateContent(cacheNameRouteDictionary)
        End Sub
    End Class
End Namespace

