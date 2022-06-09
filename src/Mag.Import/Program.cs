using Mag.Data;
using System;
using System.Configuration;
using System.IO;

namespace Mag.Import {

  class Program {
    static void Main(string[] args) {
      try {
        Console.WriteLine("Mitre-Att&ck GraphQL API. Data Import Utility.");
        Console.WriteLine("Starting up, initializing the database...");
        Setup.Init();
        Console.WriteLine("Database initialized successfully.");
        LoadStixData();


      } catch (Exception ex) {
        Console.WriteLine("===================== Error ==============================");
        Console.WriteLine(ex.ToString());
      }
      Console.WriteLine("press any key to exit...");
      Console.ReadKey();
    }

    private static void LoadStixData() {
      Console.WriteLine("Starting data import. Deleting all data in the database...");
      var loader = new MitreStixFileImporter(Setup.App);
      loader.DeleteAll();
      Console.WriteLine("Done. Loading STIX files...");
      var folder = ConfigurationManager.AppSettings["StixFolder"];
      //var stixFile = Path.Combine(folder, "ics-attack.json");
      //var stixFile = Path.Combine(folder, "enterprise-attack.json");
      loader.ImportStixFile(Path.Combine(folder, "enterprise-attack.json"));
      loader.ImportStixFile(Path.Combine(folder, "ics-attack.json"));
      loader.ImportStixFile(Path.Combine(folder, "mobile-attack.json"));
    }

  }
}
