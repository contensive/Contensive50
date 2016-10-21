using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  Contensive.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            appConfigClass config = new appConfigClass();
            fileSystemClass fs = new fileSystemClass();
            string jsonText;
            string dataSource;
            string sql;
            appServicesClass asv;
            string appName;
            
            System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
            //
            Console.Write("Application Name:");
            appName = Console.ReadLine();
            config.name = appName;
            //;
            Console.Write("DataSource type (1=odbcSqlServer, 2=nativeSqlServer):");
            dataSource = Console.ReadLine();
            switch (dataSource)
            {
                case "1":
                    config.dataSourceType = dataSourceTypeEnum.sqlServerOdbc;
                    //
                    Console.Write("odbc sqlserver connectionstring:");
                    config.dataSourceODBCConnectionString = Console.ReadLine();
                    break;
                case "2":
                    config.dataSourceType = dataSourceTypeEnum.sqlServerNative;
                    //
                    Console.Write("native sqlserver endpoint:");
                    config.dataSourceEndPoint = Console.ReadLine();
                    //
                    Console.Write("native sqlserver userId:");
                    config.dataSourceUserId = Console.ReadLine();
                    //
                    Console.Write("native sqlserver password:");
                    config.dataSourcePassword = Console.ReadLine();
                    break;
            }

            //;
            config.enabled = true;
            jsonText = json.Serialize(config);
            fs.SaveFile(ccCommonModule.getProgramDataPath() + "/config/" + config.name + ".json", jsonText);
            //
            // create appServices
            //
            asv = new appServicesClass(appName);
            //
            // create the database on the server
            //
            sql = "create database " + config.name;
            asv.executeSql(sql);
            //
            // connect to the db and build / upgrade
            //
            upgradeClass upgrade = new upgradeClass();
            upgrade.Upgrade2(asv);
        }
    }
}
