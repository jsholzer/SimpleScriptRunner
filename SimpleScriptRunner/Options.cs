using System;
using System.Collections.Generic;
using SimpleScriptRunner.Util;

namespace SimpleScriptRunner
{
    public class Options
    {
        public bool RequireRollback { get; set; }
        public bool UseTransactions { get; set; }
        public int? MaxPatch { get; set; }
        public bool SkipVersion { get; set; }
        public bool NoPrompt { get; set; }
        public bool SqlFile { get; set; }
        public double TransactionMinutes { get; set; }

        public List<String> Params { get; set; }

        public Options()
        {
            Params = new List<string>();
            TransactionMinutes = 10;            // waits up to 10 minutes by default
        }

        public void parseArgs(String[] argArray)
        {
            Params.Clear();                       // makes re-entrant

            for (int i = 0; i < argArray.Length; i++)
            {
                String param = argArray[i];
                if (!param.StartsWith("-"))
                {
                    Params.Add(param);
                    continue;
                }

                String toCheck = param.ToLower().splitAt("=").Item1;                        // ignores any values assigned via a switch
                switch (toCheck)
                {
                    case "-requirerollback": RequireRollback = true; break;
                    case "-usetransactions": UseTransactions = true; break;
                    case "-maxpatch": MaxPatch = int.Parse(argArray[++i]); break;
                    case "-skipconfirm": 
                    case "--noprompt":
                    case "-np": 
                        NoPrompt = true; 
                        break;
                    case "-sqlfile": SqlFile = true; break;
                    case "-trantimeout": TransactionMinutes = double.Parse(argArray[++i]); break;
                }
            }
        }

        public static Options build(String[] argArray)
        {
            Options options = new Options();
            options.parseArgs(argArray);
            return options;
        }
    }
}
