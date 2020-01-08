using System;
using System.Configuration;
using SimpleScriptRunnerBto;

namespace SimpleScriptProgram
{
    public class Program
    {
        public static int Main(string[] args)
        {
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
        }
    }
}
