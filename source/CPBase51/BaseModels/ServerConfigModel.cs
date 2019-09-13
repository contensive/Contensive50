
using System;
using System.Collections.Generic;
using Contensive.Models.Db;

namespace Contensive.BaseModels {
    //
    //====================================================================================================
    /// <summary>
    /// configuration of the server (on or more apps in the serer)
    /// -- new() - to allow deserialization (so all methods must pass in cp)
    /// -- shared getObject( cp, id ) - returns loaded model
    /// -- saveObject( cp ) - saves instance properties, returns the record id
    /// </summary>
    [Serializable]
    public abstract class ServerConfigBaseModel {
        //
        // -- public properties
        //
        // -- set during installation
        /// <summary>
        /// full dos path to the contensive program file installation. 
        /// </summary>
        public abstract string programFilesPath { get; set; }
        //
        // -- control the task runner and task scheduler for the server group
        public abstract bool allowTaskRunnerService { get; set; }
        public abstract bool allowTaskSchedulerService { get; set; }
        public abstract int maxConcurrentTasksPerServer { get; set; }
        //
        // -- name for this server group
        public abstract string name { get; set; }
        //
        // -- If true, use local dotnet memory cache backed by filesystem
        public abstract bool enableLocalMemoryCache { get; set; }
        //
        // -- if true, used local files to cache, backing up local cache, then remote cache
        public abstract bool enableLocalFileCache { get; set; }
        //
        // -- AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object. this is the srver:port to the config file it uses.
        public abstract bool enableRemoteCache { get; set; }
        public abstract string awsElastiCacheConfigurationEndpoint { get; set; }
        //
        // -- includes NLog logging into Enyim. Leave off as it causes performance issues
        public abstract bool enableEnyimNLog { get; set; }
        //
        // -- datasource for the cluster (only sql support for now)
        public abstract DataSourceModel.DataSourceTypeEnum defaultDataSourceType { get; set; }
        public abstract string defaultDataSourceAddress { get; set; }
        public abstract string defaultDataSourceUsername { get; set; }
        public abstract string defaultDataSourcePassword { get; set; }
        //
        // -- aws programmatic user for all services
        public abstract string awsAccessKey { get; set; }
        public abstract string awsSecretAccessKey { get; set; }
        //
        // -- aws region for this server (default us-east-1)
        public abstract string awsRegionName { get; set; }
        //
        // -- endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        public abstract bool isLocalFileSystem { get; set; }
        public abstract string localDataDriveLetter { get; set; }
        public abstract string awsBucketName { get; set; }
        //
        // -- if provided, NLog data will be sent to this CloudWatch LogGroup 
        public abstract string awsCloudWatchLogGroup { get; set; }
        //
        // -- used by applications to enable/disable features, like 
        //      - ecommerce batch should only run in production
        // todo figure out how to expose this, add it to configuration setup
        public abstract bool productionEnvironment { get; set; }
        ////
        //public abstract CaseInsensitiveDictionary<string, AppConfigBaseModel> apps { get; set; }
        ////
        ///// <summary>
        ///// case insensative dictionary. Use for application lookup
        ///// </summary>
        ///// <typeparam name="V"></typeparam>
        //[Serializable]
        //public class CaseInsensitiveDictionary<S, V> : Dictionary<string, V> {
        //    public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase) {
        //    }
        //}

    }
}

