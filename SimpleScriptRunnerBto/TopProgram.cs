﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleScriptRunnerBto.Util;

namespace SimpleScriptRunnerBto
{
    public class TopProgram
    {
        public static int main(NameValueCollection appSettings, string[] argArray_)
        {
            Options options = Options.build(argArray_);
            if (options.SqlFile)
                return sqlFileMain(appSettings, options.Params.ToArray());

            if (options.Params.Length == 0)
            {
                Console.WriteLine("Please enter database to update [{0}]:", String.Join(", ", appSettings.AllKeys));
                String userInput = Console.ReadLine();

                // Merges options with user input
                options.parseArgs(userInput.Split(' '));

                if (options.Params.Length != 1 || !appSettings.AllKeys.Contains(options.Params[0].ToLower()))
                {
                    Console.WriteLine("Invalidate selection: {0}", userInput);
                    return 519;
                }
            }

            if (options.Params.Length == 1)
            {
                String value = appSettings[options.Params[0].ToLower()];
                Console.WriteLine("Argument {0} with value: {1}", options.Params[0], value);

                // Merges options with web config settings
                options.parseArgs(value.Split(' '));
            }

            Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

            int result = simpleScriptRunnerProgramMain(options);

            Console.WriteLine(result == 0 ? "COMPLETE" : "FAIL");
            return result;
        }

        public static int sqlFileMain(NameValueCollection appSettings, string[] argArray)
        {
            Dictionary<String, String> switches = ArgsUtil.parseDictionary(ref argArray);
            if (argArray.Length == 0)
            {
                Console.WriteLine("Please enter sqlFile to execute: ");
                String userPath = Console.ReadLine();

                Console.WriteLine("Please enter database to update [{0}]:", String.Join(", ", appSettings.AllKeys));
                String userTarget = Console.ReadLine();
                if (!appSettings.AllKeys.Contains(userTarget.ToLower()))
                {
                    Console.WriteLine("Invalidate selection: {0}", userTarget);
                    return 519;
                }

                MapHelper.setValue(switches, "-sqlFile", userPath);
                argArray = new[] { userTarget };
            }

            if (argArray.Length != 1 || switches.value("-sqlFile") == null)
            {
                Console.WriteLine("Missing required field.");
                Console.WriteLine("Usage: -sqlFile=<path> <db_target>");
                return 402;
            }

            Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

            String path = switches.value("-sqlFile");
            String target = argArray[0];

            String targetValue = appSettings[target.ToLower()];
            Console.WriteLine("Target {0} with value: {1}", target, targetValue);
            Options options = Options.build(targetValue.Split(' '));
            options.Path = path;
            
            int result = Program.executeSqlFile(options);

            Console.WriteLine(result == 0 ? "COMPLETE" : "FAIL");
            return result;
        }

        public static int simpleScriptRunnerProgramMain(Options options)
        {
            try
            {
                Program.executeRelease(options);
                return 0;
            }
            catch (Exception ex)
            {
                writeMessage(Assembly.GetExecutingAssembly().FullName, string.Empty, Program.Category.error, "1", ex.ToString());
                return 1;
            }
        }

        private static void writeMessage(string origin, string subcategory, Program.Category category, string code, string text)
        {
            Console.WriteLine("{0} : {1} {2} {3} : {4}", origin, subcategory, category, code, text);
        }
    }
}
