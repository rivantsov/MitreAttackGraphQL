## Mitre-Att&ck GraphQL API
This project implements a GraphQL API over the [Mitre-Att&ck framework](https://attack.mitre.org/) dataset. The data resides in the SQL Server database. To run the API locally, you need a local server. You can either restore the database from the provided backup, or import the original Mitre data.

## Restoring the database. 
Download the zipped backup file from the repo: https://github.com/rivantsov/MitreAttackData . Restore the database as new MitreAttack database using the SQL Management Studio.   
 
 ## Importing MitreAttack data 
 1. Clone the repo, open and build the solution in Visual Studio.
 1. Get the Mitre data zip from the repo: https://github.com/rivantsov/MitreAttackData; unzip the 3 json files into some data folder. You need 3 files: enterprise-attack.json, mobile-attack.json, ics-attack.json.
 1. Locate the project Mag.Import and make it a startup project. 
 1. Open App.config file in this project, set the "StixFolder" value to the full path of the data folder with 3 json files.
 1. Open SQL Management studio (SMS), connect to local or remote server. Create a new empty database MitreAttack. In the App.Config file adjust the "ConnString" value if necessary to point to this database. 
 1. Run the app. It should show Console window and start reporting the progress. It takes a few seconds to initialize the database 
   and import the data. 
 1. Explore the database tables and data imported in the SMS
 
 ## Viewing the data with GraphQL API and GraphiQL UI
 1. Make the project Mag.GraphQL.Web a startup project. 
 1. Open its App.config file, adjust the ConnString value. 
 1. Run the project. It should open browser with the GraphiQL UI. Explore the data. 

