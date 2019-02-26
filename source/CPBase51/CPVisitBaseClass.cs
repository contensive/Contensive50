﻿
using System;

namespace Contensive.BaseClasses {
    public abstract class CPVisitBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// return true if the visit supports cookies
        /// </summary>
        public abstract bool CookieSupport { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Get the visit property that matches the key. If not found set and return the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract string GetText(string key, string defaultValue);
        /// <summary>
        /// Get the visit property that matches the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract string GetText(string key);
        //
        //====================================================================================================
        /// <summary>
        /// Get the visit property that matches the key. If not found set and return the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract bool GetBoolean(string key, bool defaultValue);
        /// <summary>
        /// Get the visit property that matches the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool GetBoolean(string key);
        //
        //====================================================================================================
        /// <summary>
        /// Get the visit property that matches the key. If not found set and return the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract DateTime GetDate(string key, DateTime defaultValue);
        /// <summary>
        /// Get the visit property that matches the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract DateTime GetDate(string key);
        //
        //====================================================================================================
        /// <summary>
        /// Get the visit property that matches the key. If not found set and return the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract int GetInteger(string key, int defaultValue);
        /// <summary>
        /// Get the visit property that matches the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract int GetInteger(string key);
        //
        //====================================================================================================
        /// <summary>
        /// Get the visit property that matches the key. If not found set and return the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract double GetNumber(string key, double defaultValue);
        /// <summary>
        /// Get the visit property that matches the key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract double GetNumber(string key);
        //
        //====================================================================================================
        /// <summary>
        /// Return the visit id
        /// </summary>
        public abstract int Id { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The time of the last hit
        /// </summary>
        public abstract DateTime LastTime { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Return the number of login attempts
        /// </summary>
        public abstract int LoginAttempts { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Return the name of the visit
        /// </summary>
        public abstract string Name { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Return the number of hits to the application
        /// </summary>
        public abstract int Pages { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Return the referer for the visit
        /// </summary>
        public abstract string Referer { get; }
        //
        //====================================================================================================
        /// <summary>
        /// Set a key value for the visit
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="targetVisitId"></param>
        public abstract void SetProperty(string key, string value);
        public abstract void SetProperty(string key, int value);
        public abstract void SetProperty(string key, double value);
        public abstract void SetProperty(string key, DateTime value);
        public abstract void SetProperty(string key, bool value);
        //
        //====================================================================================================
        /// <summary>
        /// The date when the visit started
        /// </summary>
        public abstract int StartDateValue { get; }
        //
        //====================================================================================================
        /// <summary>
        /// The time when the visit started
        /// </summary>
        public abstract DateTime StartTime { get; }
        //
        //====================================================================================================
        // deprecated
        //
        //
        [Obsolete("Use Get of the correct type", true)]
        public abstract string GetProperty(string key, string defaultValue, int targetVisitId);
        //
        [Obsolete("Use Get of the correct type", true)]
        public abstract string GetProperty(string key, string defaultValue);
        //
        [Obsolete("Use Get of the correct type", true)]
        public abstract string GetProperty(string key);
        //
        [Obsolete("Deprecated. Set and get only the current visit", true)]
        public abstract void SetProperty(string key, string value, int targetVisitId);
        //
        [Obsolete("Deprecated. Use the get with the correct default argumnet type", true)]
        public abstract bool GetBoolean(string key, string defaultValue);
        //
        [Obsolete("Deprecated. Use the get with the correct default argumnet type", true)]
        public abstract DateTime GetDate(string key, string defaultValue);
        //
        [Obsolete("Deprecated. Use the get with the correct default argumnet type", true)]
        public abstract int GetInteger(string key, string defaultValue);
        //
        [Obsolete("Deprecated. Use the get with the correct default argumnet type", true)]
        public abstract double GetNumber(string key, string defaultValue);
    }
}
