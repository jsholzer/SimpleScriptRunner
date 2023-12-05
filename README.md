Available on nuget at https://www.nuget.org/packages/SimpleScriptRunnerBto/1.0.0

Fork of SimpleScriptRunner with the following customizations:
1. Uses MySQL instead of MSSQL
2. Strictly enforces number of scripts to be sequential
3. Project is built as a DLL instead of an EXE

---

This tool can be used to run a directory of numbered sql scripts against an MSSQL database. The tool can easily be extended to run against other versioned objects.

This is very handy if you are maintaining multiple database environments (e.g. dev, test, production) or working in a team.

The usage for is
```
SimpleScriptRunner.exe <serverName> <databaseName> <path to folder containing sql scripts> [options]
```
or for SQL Authentication
```
SimpleScriptRunner.exe <serverName> <databaseName> <username> <password> <path to folder containing sql scripts> [options]
```

the sql scripts should start with the script version number and then a space, and be grouped into numbered Release folders.

e.g.
```
Release 1\0001 - Create Users table.sql
Release 1\0002 - Create Customers table.sql
Release 2\0001 - Create Items table.sql
```

```[options]``` can be any combination of the following switches

switch| what it does
------|-------------
-usetransactions|Each numbered migration will take place in a new transaction, and rollback if there are any issues|
-requirerollback|Each script x.sql must have a corresponding x.rollback.sql. When the script is run, the rollup is executed, then the rollback, then the rollup again, to confirm the rollback doesn't cause errors|

*What it does*

The tool will only execute scripts with higher numbers than have been previously executed - i.e. if the last script to be applied was script 0003, scripts 0004 and onwards will be applied - with one exception. If the last script to be applied has been modified since it was last executed, it will be executed again. This is handy if you are working on a script and need to make slight changes to it. The tool will also create the database on the server if it does not exist.

*Getting started*
Assuming your solution file is called MySolution and that you are running a local copy of MySQL. Use these steps to get running:
1. Add a CommandLine project called 'MySolution.Database' or something similar to Visual Studio. 
2. Import the SimpleScriptBto DLL. 
3. Within your `App.config` file, add a line for each target environment:
```
    <add key="local1" value="-usetransactions localhost DbName UserName Password"/>
    <add key="dev1" value="-usetransactions dev1.domain.com DbName UserName Password"/>
    <add key="qa1" value="-usetransactions qa1.domain.com DbName UserName Password"/>
    <add key="prod1" value="-usetransactions proddb.domain.com DbName UserName Password"/>
```
4. Add the following to `Program.cs`:
```C#
        public static int Main(string[] args)
        {
            return TopProgram.main(ConfigurationManager.AppSettings, args);
        }
```
6
5. Now add a script called 0001 - hello world.sql to the project root. 
6. Run the program and provide the target environment, like `local1` to test against your local environment.

