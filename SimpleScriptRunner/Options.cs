using System;
using System.Collections.Generic;
using System.Linq;
using SimpleScriptRunner.Util;

namespace SimpleScriptRunner
{
    public class Options
    {
        private const double DEFAULT_TRANSACTION_TIMEOUT = 10.0;

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
            TransactionMinutes = DEFAULT_TRANSACTION_TIMEOUT;            // waits up to 10 minutes by default
        }

        public void parseArgs(String[] argArray)
        {
            argArray = argArray.ToArray();        // Copies so that caller is unchanged
            Dictionary<String, String> switches = ArgsUtil.parseDictionary(ref argArray);

            Params.Clear();                       // makes re-entrant
            Params.AddRange(argArray);

            RequireRollback = switches.hasAny("-requirerollback", "--requirerollback", "-rr");
            UseTransactions = switches.hasAny("-usetransactions", "--usetransactions", "-ut");
            MaxPatch = switches.valueInt("-maxpatch", "--maxpatch", "-mp");
            NoPrompt = switches.hasAny("-skipconfirm", "--noprompt", "-np");
            SqlFile = switches.hasAny("-sqlfile", "--sqlfile", "-sf");
            TransactionMinutes = switches.valueDouble("-trantimeout", "--trantimeout", "-to") ?? DEFAULT_TRANSACTION_TIMEOUT;
        }

        public static Options build(String[] argArray)
        {
            Options options = new Options();
            options.parseArgs(argArray);
            return options;
        }
    }
}
