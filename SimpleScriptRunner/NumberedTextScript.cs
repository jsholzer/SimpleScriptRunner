using System;
using System.IO;
using System.Transactions;

namespace SimpleScriptRunner
{
    public class NumberedTextScript : IScript<ITextScriptTarget>, IComparable
    {
        private readonly string path;

        public NumberedTextScript(FileInfo fileInfo, int major, int minor, int scriptNumber, string description)
        {
            path = fileInfo.FullName;
            Version = new ScriptVersion(major, minor, scriptNumber, fileInfo.LastWriteTime, Environment.MachineName, description);
        }

        public ScriptVersion Version { get; private set; }

        public void apply(ITextScriptTarget scriptTarget, Options options)
        {
            var prevVersion = scriptTarget.CurrentVersion;
            var sql = File.ReadAllText(path);
            TransactionScope ts = null;
            if (options.UseTransactions)
            {
                ts = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(options.TransactionMinutes));            // maximum wait is 30
            }
            var success = false;
            try
            {
                scriptTarget.apply(sql); if (!options.SkipVersion) scriptTarget.updateVersion(Version);

                if (options.RequireRollback)
                {
                    var rollbackSql = File.ReadAllText(path.Replace(".sql", ".rollback.sql"));

                    scriptTarget.apply(sql); if (!options.SkipVersion) scriptTarget.updateVersion(Version);
                    scriptTarget.apply(rollbackSql); if (!options.SkipVersion) scriptTarget.updateVersion(prevVersion);
                    scriptTarget.apply(rollbackSql); if (!options.SkipVersion) scriptTarget.updateVersion(prevVersion);
                    scriptTarget.apply(sql); if (!options.SkipVersion) scriptTarget.updateVersion(Version);
                }
                success = true;
            }
            finally
            {
                if (ts != null)
                {
                    if (success)
                        ts.Complete();
                    ts.Dispose();
                }
            }
        }

        public override string ToString()
        {
            return path;
        }

        public int CompareTo(object obj)
        {
            NumberedTextScript other = (NumberedTextScript)obj;
            return Version.CompareTo(other.Version);
        }
    }
}