using System;
using System.Collections.Generic;
using System.Linq;
using SimpleScriptRunnerBto.Util;
using System.Transactions;

namespace SimpleScriptRunnerBto
{
    public class Options
    {
        public bool RequireRollback { get; set; }
        public bool UseTransactions { get; set; }
        public int? MaxPatch { get; set; }
        public bool SkipVersion { get; set; }
        public bool NoPrompt { get; set; }
        public bool SqlFile { get; set; }
        public TimeSpan TransactionTimeout { get; set; }

        public List<String> Params { get; set; }

        public Options()
        {
            Params = new List<string>();
            TransactionTimeout = TransactionManager.DefaultTimeout;
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

            double? timeoutValue = switches.valueDouble("-trantimeout", "--trantimeout", "-to");            // specified in minutes, fractional is fine 
            if (timeoutValue.HasValue)
            {
                // Validates that timeout against maximum value
                TimeSpan timeout = TimeSpan.FromMinutes(timeoutValue.Value);            
                if (timeout > TransactionManager.MaximumTimeout)
                    throw new ArgumentException(String.Format("TransactionManager restricts timeouts to a maximum of {0}.\n" +
                        "Specified timeout value of {1} is too large.\n" +
                        "Issue can be resolved by changing `maching.config` value for <system.transactions><machineSettings maxTimeout=\"\"/></system.transactions>",
                        TransactionManager.MaximumTimeout, 
                        timeout
                        ));

                if (timeout < TimeSpan.Zero)
                    throw new ArgumentException(String.Format("Invalid timeout ", timeout));

                TransactionTimeout = timeout;
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
