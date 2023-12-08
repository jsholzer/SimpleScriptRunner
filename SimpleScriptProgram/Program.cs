using System;
using System.Configuration;
using SimpleScriptRunnerBto;

try
{
    return TopProgram.main(ConfigurationManager.AppSettings, args);
}
catch (Exception error)
{
    Console.Error.WriteLine("Error: " + error.Message);
    Console.Error.WriteLine(error.StackTrace);
    return 1;
}