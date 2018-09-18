

using System.Text.RegularExpressions;
// 

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' static class controller
    // '' </summary>
    public class groupController : IDisposable {
        
        // 
        //  ----- constants
        // 
        // Private Const invalidationDaysDefault As Double = 365
        // 
        //  ----- objects constructed that must be disposed
        // 
        // Private cacheClient As Enyim.Caching.MemcachedClient
        // 
        //  ----- private instance storage
        // 
        // Private remoteCacheDisabled As Boolean
        // 
        // ====================================================================================================
        // '' <summary>
        // '' Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        // '' </summary>
        // '' <param name="groupName"></param>
        // '' <returns></returns>
        public static int group_add(coreClass cpCore, string groupName) {
            int returnGroupId = 0;
            try {
                DataTable dt;
                string sql;
                int createkey;
                int cid;
                string sqlGroupName = cpCore.db.encodeSQLText(groupName);
                // 
                dt = cpCore.db.executeQuery(("SELECT ID FROM CCGROUPS WHERE NAME=" 
                                + (sqlGroupName + "")));
                if ((dt.Rows.Count > 0)) {
                    returnGroupId = genericController.EncodeInteger(dt.Rows[0].Item["ID"]);
                }
                else {
                    cid = models.complex.cdefmodel.getcontentid(cpcore, "groups");
                    createkey = genericController.GetRandomInteger();
                    sql = ("insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" 
                                + (cid + (",1," 
                                + (createkey + ("," 
                                + (sqlGroupName + ("," 
                                + (sqlGroupName + ")"))))))));
                    cpCore.db.executeQuery(sql);
                    // 
                    sql = ("select top 1 id from ccgroups where createkey=" 
                                + (createkey + " order by id desc"));
                    dt = cpCore.db.executeQuery(sql);
                    if ((dt.Rows.Count > 0)) {
                        returnGroupId = genericController.EncodeInteger(dt.Rows[0].Item[0]);
                    }
                    
                }
                
                dt.Dispose();
            }
            catch (Exception ex) {
                throw ex;
            }
            
            return returnGroupId;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        // '' </summary>
        // '' <param name="GroupNameOrGuid"></param>
        // '' <param name="groupCaption"></param>
        // '' <returns></returns>
        public static int group_add2(coreClass cpCore, string GroupNameOrGuid, string groupCaption, void =, void ) {
            int returnGroupId = 0;
            // Warning!!! Optional parameters not supported
            try {
                // 
                csController cs = new csController(cpCore);
                bool IsAlreadyThere = false;
                string sqlCriteria = cpCore.db.getNameIdOrGuidSqlCriteria(GroupNameOrGuid);
                string groupName;
                string groupGuid;
                // 
                if ((GroupNameOrGuid == "")) {
                    throw new ApplicationException("A group cannot be added with a blank name");
                }
                else {
                    cs.open("Groups", sqlCriteria, ,, false, "id");
                    IsAlreadyThere = cs.ok;
                    cs.Close();
                    if (!IsAlreadyThere) {
                        cs.Insert("Groups");
                        if (!cs.ok) {
                            throw new ApplicationException("There was an error inserting a new group record");
                        }
                        else {
                            returnGroupId = cs.getInteger("id");
                            if (genericController.isGuid(GroupNameOrGuid)) {
                                groupName = ("Group " + cs.getInteger("id"));
                                groupGuid = GroupNameOrGuid;
                            }
                            else {
                                groupName = GroupNameOrGuid;
                                groupGuid = Guid.NewGuid().ToString();
                            }
                            
                            if ((groupCaption == "")) {
                                groupCaption = groupName;
                            }
                            
                            cs.setField("name", groupName);
                            cs.setField("caption", groupCaption);
                            cs.setField("ccGuid", groupGuid);
                            cs.setField("active", "1");
                        }
                        
                        cs.Close();
                    }
                    
                }
                
            }
            catch (Exception ex) {
                throw new ApplicationException("Unexpected error in cp.group.add()", ex);
            }
            
            return returnGroupId;
        }
        
        // 
        // ====================================================================================================
        // 
        //  Add User
        // 
        public static void group_addUser(coreClass cpCore, int groupId, int userid, void =, void 0, DateTime dateExpires, void =, void #, void 12, void :, void 00, void :, void 00, void AM, void #) {
            try {
                // 
                // Warning!!! Optional parameters not supported
                // Warning!!! Optional parameters not supported
                string groupName;
                // 
                if (true) {
                    if ((groupId < 1)) {
                        throw new ApplicationException(("Could not find or create the group with id [" 
                                        + (groupId + "]")));
                    }
                    else {
                        if ((userid == 0)) {
                            userid = cpCore.doc.authContext.user.id;
                        }
                        
                        Using;
                        ((void)(cs));
                        new csController(cpCore);
                        cs.open("Member Rules", ("(MemberID=" 
                                        + (userid.ToString + (")and(GroupID=" 
                                        + (groupId.ToString + ")")))), ,, false);
                        if (!cs.ok) {
                            cs.Close();
                            cs.Insert("Member Rules");
                        }
                        
                        if (!cs.ok) {
                            groupName = cpCore.db.getRecordName("groups", groupId);
                            throw new ApplicationException(("Could not find or create the Member Rule to add this member [" 
                                            + (userid + ("] to the Group [" 
                                            + (groupId + (", " 
                                            + (groupName + "]")))))));
                        }
                        else {
                            cs.setField("active", "1");
                            cs.setField("memberid", userid.ToString);
                            cs.setField("groupid", groupId.ToString);
                            12;
                            0;
                            0;
                            AM;
                            // TODO: # ... Warning!!! not translated
                            cs.setField("DateExpires", dateExpires.ToString);
                        }
                        
                    }
                    
                }
                else {
                    cs.setField("DateExpires", "");
                }
                
            }
            
            cs.Close();
        }
    }
}
CatchException ex;
throw ex;
Endtry {
}

EndIfIfthrow ex;
Endtry {
}

cpcore.db.csClose(CS);
EndIfIfSub// 
// ====================================================================================================
Endclass  {
}

    
    // 
    // ====================================================================================================
    // 
    public static void group_AddUser(coreClass cpCore, string groupNameOrGuid, int userid, void =, void 0, DateTime dateExpires, void =, void #, void 12, void :, void 00, void :, void 00, void AM, void #) {
        try {
            // 
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            int GroupID;
            // 
            if ((groupNameOrGuid != "")) {
                GroupID = cpCore.db.getRecordID("groups", groupNameOrGuid);
                if ((GroupID < 1)) {
                    void.group_add2(cpCore, groupNameOrGuid);
                    GroupID = cpCore.db.getRecordID("groups", groupNameOrGuid);
                }
                
                if ((GroupID < 1)) {
                    throw new ApplicationException(("Could not find or create the group [" 
                                    + (groupNameOrGuid + "]")));
                }
                else {
                    if ((userid == 0)) {
                        userid = cpCore.doc.authContext.user.id;
                    }
                    
                    Using;
                    ((void)(cs));
                    new csController(cpCore);
                    cs.open("Member Rules", ("(MemberID=" 
                                    + (userid.ToString + (")and(GroupID=" 
                                    + (GroupID.ToString + ")")))), ,, false);
                    if (!cs.ok) {
                        cs.Close();
                        cs.Insert("Member Rules");
                    }
                    
                    if (!cs.ok) {
                        throw new ApplicationException(("Could not find or create the Member Rule to add this member [" 
                                        + (userid + ("] to the Group [" 
                                        + (GroupID + (", " 
                                        + (groupNameOrGuid + "]")))))));
                    }
                    else {
                        cs.setField("active", "1");
                        cs.setField("memberid", userid.ToString);
                        cs.setField("groupid", GroupID.ToString);
                        12;
                        0;
                        0;
                        AM;
                        // TODO: # ... Warning!!! not translated
                        cs.setField("DateExpires", dateExpires.ToString);
                    }
                    
                }
                
            }
            else {
                cs.setField("DateExpires", "");
            }
            
        }
        
        cs.Close();
    }
    
    private Exception ex;
    
    // 
    // =============================================================================
    //  main_Get the GroupID from iGroupName
    // =============================================================================
    // 
    public static int group_GetGroupID(coreClass cpcore, string GroupName) {
        DataTable dt;
        string MethodName;
        string iGroupName;
        // 
        iGroupName = genericController.encodeText(GroupName);
        // 
        MethodName = "main_GetGroupID";
        group_GetGroupID = 0;
        if ((iGroupName != "")) {
            // 
            //  ----- main_Get the Group ID
            // 
            dt = cpcore.db.executeQuery(("select top 1 id from ccGroups where name=" + cpcore.db.encodeSQLText(iGroupName)));
            if ((dt.Rows.Count > 0)) {
                group_GetGroupID = genericController.EncodeInteger(dt.Rows[0].Item[0]);
            }
            
        }
        
    }
    
    // 
    // =============================================================================
    //  main_Get the GroupName from iGroupID
    // =============================================================================
    // 
    public static string group_GetGroupName(coreClass cpcore, int GroupID) {
        // 
        int CS;
        string MethodName;
        int iGroupID;
        // 
        iGroupID = genericController.EncodeInteger(GroupID);
        // 
        MethodName = "main_GetGroupByID";
        group_GetGroupName = "";
        if ((iGroupID > 0)) {
            // 
            //  ----- main_Get the Group name
            // 
            CS = cpcore.db.cs_open2("Groups", iGroupID);
            if (cpcore.db.csOk(CS)) {
                group_GetGroupName = genericController.encodeText(cpcore.db.cs_getValue(CS, "Name"));
            }
            
            cpcore.db.csClose(CS);
        }
        
    }
    
    // 
    // =============================================================================
    //  Add a new group, return its GroupID
    // =============================================================================
    // 
    public static int group_Add(coreClass cpcore, string GroupName, string GroupCaption, void =, void ) {
        int CS;
        // Warning!!! Optional parameters not supported
        string MethodName;
        string iGroupName;
        string iGroupCaption;
        // 
        MethodName = "main_AddGroup";
        iGroupName = genericController.encodeText(GroupName);
        iGroupCaption = genericController.encodeEmptyText(GroupCaption, iGroupName);
        // 
        group_Add = -1;
        DataTable dt;
        dt = cpcore.db.executeQuery(("SELECT ID FROM ccgroups WHERE NAME=" + cpcore.db.encodeSQLText(iGroupName)));
        if ((dt.Rows.Count > 0)) {
            group_Add = genericController.EncodeInteger(dt.Rows[0].Item[0]);
        }
        else {
            CS = cpcore.db.csInsertRecord("Groups", SystemMemberID);
            if (cpcore.db.csOk(CS)) {
                group_Add = genericController.EncodeInteger(cpcore.db.cs_getValue(CS, "ID"));
                cpcore.db.csSet(CS, "name", iGroupName);
                cpcore.db.csSet(CS, "caption", iGroupCaption);
                cpcore.db.csSet(CS, "active", true);
            }
            
            cpcore.db.csClose(CS);
        }
        
    }
    
    // 
    // =============================================================================
    //  Add a new group, return its GroupID
    // =============================================================================
    // 
    public static void group_DeleteGroup(coreClass cpcore, string GroupName) {
        cpcore.db.deleteContentRecords("Groups", ("name=" + cpcore.db.encodeSQLText(GroupName)));
    }
    
    // 
    // =============================================================================
    //  Add a member to a group
    // =============================================================================
    // 
    public static void group_AddGroupMember(coreClass cpcore, string GroupName, int NewMemberID, void =, void SystemMemberID, DateTime DateExpires, void =, void Nothing) {
        // 
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        int CS;
        int GroupID;
        string MethodName;
        string iGroupName;
        DateTime iDateExpires;
        // 
        MethodName = "main_AddGroupMember";
        iGroupName = genericController.encodeText(GroupName);
        iDateExpires = DateExpires;
        // encodeMissingDate(DateExpires, Date.MinValue)
        // 
        if ((iGroupName != "")) {
            GroupID = group_GetGroupID(cpcore, iGroupName);
            if ((GroupID < 1)) {
                GroupID = group_Add(cpcore, GroupName, GroupName);
            }
            
            if ((GroupID < 1)) {
                throw new ApplicationException(("main_AddGroupMember could not find or add Group [" 
                                + (GroupName + "]")));
            }
            else {
                CS = cpcore.db.csOpen("Member Rules", ("(MemberID=" 
                                + (cpcore.db.encodeSQLNumber(NewMemberID) + (")and(GroupID=" 
                                + (cpcore.db.encodeSQLNumber(GroupID) + ")")))), ,, false);
                if (!cpcore.db.csOk(CS)) {
                    cpcore.db.csClose(CS);
                    CS = cpcore.db.csInsertRecord("Member Rules");
                }
                
                if (!cpcore.db.csOk(CS)) {
                    throw new ApplicationException(("main_AddGroupMember could not add this member to the Group [" 
                                    + (GroupName + "]")));
                }
                else {
                    cpcore.db.csSet(CS, "active", true);
                    cpcore.db.csSet(CS, "memberid", NewMemberID);
                    cpcore.db.csSet(CS, "groupid", GroupID);
                    MinValue;
                    cpcore.db.csSet(CS, "DateExpires", iDateExpires);
                }
                
            }
            
        }
        else {
            cpcore.db.csSet(CS, "DateExpires", "");
        }
        
    }
    
    // 
    // =============================================================================
    //  Delete a member from a group
    // =============================================================================
    // 
    public static void group_DeleteGroupMember(coreClass cpcore, string GroupName, int NewMemberID, void =, void SystemMemberID) {
        // 
        // Warning!!! Optional parameters not supported
        int GroupID;
        string MethodName;
        string iGroupName;
        // 
        iGroupName = genericController.encodeText(GroupName);
        // 
        MethodName = "main_DeleteGroupMember";
        if ((iGroupName != "")) {
            GroupID = group_GetGroupID(cpcore, iGroupName);
            if ((GroupID < 1)) {
                
            }
            else if ((NewMemberID < 1)) {
                throw new ApplicationException("Member ID is invalid");
            }
            else {
                cpcore.db.deleteContentRecords("Member Rules", ("(MemberID=" 
                                + (cpcore.db.encodeSQLNumber(NewMemberID) + (")AND(groupid=" 
                                + (cpcore.db.encodeSQLNumber(GroupID) + ")")))));
            }
            
        }
        
    }
    
    // 
    //  this class must implement System.IDisposable
    //  never throw an exception in dispose
    //  Do not change or add Overridable to these methods.
    //  Put cleanup code in Dispose(ByVal disposing As Boolean).
    // ====================================================================================================
    // 
    protected bool disposed = false;
    
    public void Dispose() {
        //  do not add code here. Use the Dispose(disposing) overload
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    // 
    protected override void Finalize() {
        //  do not add code here. Use the Dispose(disposing) overload
        Dispose(false);
        base.Finalize();
    }
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' dispose.
    // '' </summary>
    // '' <param name="disposing"></param>
    protected virtual void Dispose(bool disposing) {
        if (!this.disposed) {
            this.disposed = true;
            if (disposing) {
                // If (cacheClient IsNot Nothing) Then
                //     cacheClient.Dispose()
                // End If
            }
            
            // 
            //  cleanup non-managed objects
            // 
        }
        
    }