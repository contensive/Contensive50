using System;


namespace Contensive.Processor.Addons.AdminNavigator {
    static class common {
        //
        internal const string IconAdvanced = "<img src=\"/cclib/images/NavAdv.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconAdvancedClosed = "<img src=\"/cclib/images/NavAdvClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconAdvancedOpened = "<img src=\"/cclib/images/NavAdvOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconContent = "<img src=\"/cclib/images/NavContent.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconContentOpened = "<img src=\"/cclib/images/NavContentOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconContentClosed = "<img src=\"/cclib/images/NavContentClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconFolder = "<img src=\"/cclib/images/NavFolderClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconFolderClosed = "<img src=\"/cclib/images/NavFolderClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconFolderOpened = "<img src=\"/cclib/images/NavFolderOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconFolderNoSubNodes = "<img src=\"/cclib/images/NavFolder.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconEmail = "<img src=\"/cclib/images/NavEmail.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconEmailClosed = "<img src=\"/cclib/images/NavEmailClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconEmailOpened = "<img src=\"/cclib/images/NavEmailOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconSystemEmail = "<img src=\"/cclib/images/NavEmail.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconConditionalEmail = "<img src=\"/cclib/images/NavEmail.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconGroupEmail = "<img src=\"/cclib/images/NavEmail.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconUsers = "<img src=\"/cclib/images/NavUsers.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconUsersOpened = "<img src=\"/cclib/images/NavUsersOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconUsersClosed = "<img src=\"/cclib/images/NavUsersClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconReports = "<img src=\"/cclib/images/NavReports.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconReportsOpened = "<img src=\"/cclib/images/NavReportsOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconReportsClosed = "<img src=\"/cclib/images/NavReportsClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconSettings = "<img src=\"/cclib/images/NavSettings.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconSettingsOpened = "<img src=\"/cclib/images/NavSettingsOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconSettingsClosed = "<img src=\"/cclib/images/NavSettingsClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconTools = "<img src=\"/cclib/images/NavTools.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconToolsOpened = "<img src=\"/cclib/images/NavToolsOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconToolsClosed = "<img src=\"/cclib/images/NavToolsClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconRecord = "<img src=\"/cclib/images/NavRecord.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconRecordOpened = "<img src=\"/cclib/images/NavRecord.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconRecordClosed = "<img src=\"/cclib/images/NavRecord.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconHelp = "<img src=\"/cclib/images/NavHelp.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconAddon = "<img src=\"/cclib/images/NavAddons.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconAddons = "<img src=\"/cclib/images/NavAddons.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconAddonsOpened = "<img src=\"/cclib/images/NavAddonsOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconAddonsClosed = "<img src=\"/cclib/images/NavAddonsClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string IconPublicHome = "<img src=\"/cclib/images/NavPublicHome.gif\" class=\"ccImgA\" title=\"Public Home\">";
        internal const string IconAdminHome = "<img src=\"/cclib/images/NavHome.gif\" class=\"ccImgA\" title=\"Admin Home\">";
        //
        internal const string IconBox = "<img src=\"/cclib/mktree/box.gif\" width=19 height=19 border=0 style=\"vertical-align:middle;\" title=\"{title}\">";
        //
        internal const string IconManageContent = "<img src=\"/cclib/images/NavContent.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconManageContentOpened = "<img src=\"/cclib/images/NavContentOpened.gif\" class=\"ccImgA\" title=\"{title}\">";
        internal const string IconManageContentClosed = "<img src=\"/cclib/images/NavContentClosed.gif\" class=\"ccImgA\" title=\"{title}\">";
        //
        internal const string NavigatorContentName = "Navigator Entries";
        internal const string LegacyMenuContentName = "Menu Entries";
        internal const string NodeIDAllContentList = "NodeIDAllContentList";
        internal const string NodeIDAddonsNoCollection = "NodeIDAddonsNoCollection";
        internal const string NodeIDLegacyMenu = "legacymenunode";
        internal const string NodeIDManageAddons = "manageaddons";
        internal const string NodeIDManageAddonsAdvanced = "manageaddonsadvanced";
        internal const string NodeIDManageAddonsCollectionPrefix = "collection";
        internal const string NodeIDSettings = "settings";
        internal const string NodeIDTools = "tools";
        internal const string NodeIDReports = "reports";
        //
        public struct SortNodeType {
            public string Name;
            public int addonid;
            public int contentControlId;
            public int collectionId;
            public string navigatorId;
            public bool newWindow;
            public int contentId;
            public string link;
            public int navIconType;
            public string navIconTitle;
            public int helpAddonId;
            public int helpCollectionId;
            public string nodeIdString;
        }
        //
        public enum NodeTypeEnum {
            NodeTypeEntry = 0,
            NodeTypeCollection = 1,
            NodeTypeAddon = 2,
            NodeTypeContent = 3
        }

        internal const string AddonManagerGuid = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}";
        internal static string cr = Environment.NewLine + "\t";
        internal static string cr2 = cr + "\t";
        internal static string cr3 = cr2 + "\t";
        internal static string cr4 = cr3 + "\t";
        internal static string cr5 = cr4 + "\t";
        internal static string cr6 = cr5 + "\t";
        internal const string RequestNameRemoteMethodAddon = "remotemethodaddon";
        internal const string NavIconTypeList = "Custom,Advanced,Content,Folder,Email,User,Report,Setting,Tool,Record,Addon,help";
        public const int NavIconTypeCustom = 1;
        public const int NavIconTypeAdvanced = 2;
        public const int NavIconTypeContent = 3;
        public const int NavIconTypeFolder = 4;
        public const int NavIconTypeEmail = 5;
        public const int NavIconTypeUser = 6;
        public const int NavIconTypeReport = 7;
        public const int NavIconTypeSetting = 8;
        public const int NavIconTypeTool = 9;
        public const int NavIconTypeRecord = 10;
        public const int NavIconTypeAddon = 11;
        public const int NavIconTypeHelp = 12;
    }
}