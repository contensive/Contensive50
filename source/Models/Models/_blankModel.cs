﻿
//using System;

//namespace Contensive.Models.Db {
//    [Serializable] public class _BlankModel : DbBaseModel {
//        //
//        //====================================================================================================
//        //-- const (must be const not property)
//        /// <summary>
//        /// The content metadata name for this table
//        /// </summary>
//        public const string contentName = "tables";
//        /// <summary>
//        /// The sql server table name
//        /// </summary>
//        public const string contentTableNameLowerCase = "cctables";
//        /// <summary>
//        /// The Contensive datasource. Use "default" or blank for the default datasource stored in the server config file
//        /// </summary>
//        public const string contentDataSource = "default";
//        /// <summary>
//        /// set true if the name field's value for all records must be unique (no duplicates). Used for cache ptr generation
//        /// </summary>
//        public const bool nameFieldIsUnique = false;
//        //
//        //====================================================================================================
//        // -- instance properties (must be properties not fields)
//        //
//        public int ReplaceMeWithThisTablesFields { get; set; }
//    }
//}