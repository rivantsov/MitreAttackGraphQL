using Mag.Data;
using System;
using System.Configuration;
using System.IO;
using Vita.Data;
using Vita.Data.MsSql;

/*
Sample entity app setup and data access code. Do not include this class into entity model library, where you define
entities and entity module and entity app. The entity model is intended to be driver/database kind agnostic. 
Put this initialization code into the hosting app. Add reference to one of the VITA driver packages 
(ex: Vita.Data.MsSql).
The database MyDatabase must exist before you run this code, create it manually

*/

namespace Mag.Import
{
    public static class Setup
    {
        public const string LogFileName = "_operationLog.log"; 
        public static MitreDataEntityApp App;
        static DbSettings _dbSettings;

        public static void Init()
        {
            App = new MitreDataEntityApp();
            // Log file
            App.LogPath = LogFileName; // in bin folder
            if (File.Exists(LogFileName))
                File.Delete(LogFileName); //delete old file
            // initialize and connect to db
            App.Init();
            var connString = ConfigurationManager.AppSettings["ConnString"];
            connString = Environment.ExpandEnvironmentVariables(connString); 
            var driver = new MsSqlDbDriver();
            
            _dbSettings = new DbSettings(driver, MsSqlDbDriver.DefaultMsSqlDbOptions, connString);
            App.ConnectTo(_dbSettings);
        }

    }
}
