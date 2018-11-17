
using System;

namespace Contensive.BaseClasses {
    public abstract class CPGroupBaseClass {
        /// <summary>
        /// Add a new group
        /// </summary>
        /// <param name="GroupName"></param>
        public abstract void Add(string GroupName);
        /// <summary>
        /// Add a new group
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="GroupCaption"></param>
        public abstract void Add(string GroupName, string GroupCaption);
        /// <summary>
        /// Add the current user to a group. If argument is numeric, record is referenced by Id. If argument is guid, record is referenced by ccGuid. Otherwise argument is name.
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        public abstract void AddUser(string GroupNameIdOrGuid);
        /// <summary>
        /// Add the current user to a group.
        /// </summary>
        /// <param name="GroupId"></param>
        public abstract void AddUser(int GroupId);
        /// <summary>
        /// Add a user to a group. If argument is numeric, record is referenced by Id. If argument is guid, record is referenced by ccGuid. Otherwise argument is name.
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        /// <param name="UserId"></param>
        public abstract void AddUser(string GroupNameIdOrGuid, int UserId);
        /// <summary>
        /// Add a user to a group. If argument is numeric, record is referenced by Id. If argument is guid, record is referenced by ccGuid. Otherwise argument is name.
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        /// <param name="UserId"></param>
        /// <param name="DateExpires"></param>
        public abstract void AddUser(string GroupNameIdOrGuid, int UserId, DateTime DateExpires);
        /// <summary>
        /// Add a user to a group.
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="UserId"></param>
        public abstract void AddUser(int GroupId, int UserId);
        /// <summary>
        /// Add a user to a group.
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="UserId"></param>
        /// <param name="DateExpires"></param>
        public abstract void AddUser(int GroupId, int UserId, DateTime DateExpires);
        /// <summary>
        /// Delete a group. If argument is numeric, record is referenced by Id. If argument is guid, record is referenced by ccGuid. Otherwise argument is name.
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        public abstract void Delete(string GroupNameIdOrGuid);
        /// <summary>
        /// Delete a group.
        /// </summary>
        /// <param name="GroupId"></param>
        public abstract void Delete(int GroupId);
        /// <summary>
        /// Get a group Id. If argument is guid, record is referenced by ccGuid. Otherwise argument is name.
        /// </summary>
        /// <param name="GroupNameOrGuid"></param>
        /// <returns></returns>
        public abstract int GetId(string GroupNameOrGuid);
        /// <summary>
        /// Get a group name. If argument is numeric, record is referenced by Id. Otherwise record is referenced by ccGuid.
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        /// <returns></returns>
        public abstract string GetName(string GroupNameIdOrGuid);
        /// <summary>
        /// Get a group Name
        /// </summary>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public abstract string GetName(int GroupId);
        /// <summary>
        /// Remove the current user from a group
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        /// <param name="UserId"></param>
        public abstract void RemoveUser(string GroupNameIdOrGuid);
        /// <summary>
        /// Remove a user from a group
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        /// <param name="UserId"></param>
        public abstract void RemoveUser(string GroupNameIdOrGuid, int UserId);
        //
        //====================================================================================================
        // deprecated
        //
    }
}

