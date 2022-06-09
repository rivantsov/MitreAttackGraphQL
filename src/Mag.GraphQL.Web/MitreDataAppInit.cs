using Mag.Data;
using System;
using System.Configuration;
using System.IO;
using Vita.Data;
using Vita.Data.MsSql;

namespace Mag.GraphQL.Web {
  public static class MitreDataAppInit {
    public const string LogFileName = @"_operationLog.log";
    public static MitreDataEntityApp MitreDataApp;
    static DbSettings _dbSettings;

    public static void InitConnectDataApp() {
      MitreDataApp = new MitreDataEntityApp();
      // Log file
      MitreDataApp.LogPath = LogFileName; // in bin folder
      if (File.Exists(LogFileName))
        File.Delete(LogFileName); //delete old file
      
      // initialize entity app
      MitreDataApp.Init();
      
      // connect to db
      var connString = ConfigurationManager.AppSettings["ConnString"];
      connString = Environment.ExpandEnvironmentVariables(connString); // you can put real conn string in env variables
      var driver = new MsSqlDbDriver();
      _dbSettings = new DbSettings(driver, MsSqlDbDriver.DefaultMsSqlDbOptions, connString);
      MitreDataApp.ConnectTo(_dbSettings);
    }

  }
}
