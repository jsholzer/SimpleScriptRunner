using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleScriptRunnerBto.Util;

namespace SimpleScriptRunnerBto
{
    internal class FolderContainingNumberedSqlScripts : IScriptSource<ITextScriptTarget>
    {
        private readonly string path;
        private readonly string searchPattern;

        public FolderContainingNumberedSqlScripts(string path, string searchPattern)
        {
            this.path = path;
            this.searchPattern = searchPattern;
        }

        #region IScriptSource<ITextScriptTarget> Members

        public IEnumerable<IScript<ITextScriptTarget>> Scripts
        {
            get
            {
                DirectoryInfo directory = new DirectoryInfo(path);

                // Parses release number
                int major, minor;
                string releaseNumberText = directory.Name.Substring(directory.Name.IndexOf(' ')).Trim();
                String[] releaseNumberSplit = releaseNumberText.Split('.');

                if (releaseNumberSplit.Length != 2 || !int.TryParse(releaseNumberSplit[0], out major) || !int.TryParse(releaseNumberSplit[1], out minor))
                    throw new ArgumentException(String.Format("Invalid release number format: {0} for directory: {1}", releaseNumberText, directory.FullName));

                int counter = 1;
                List<FileInfo> files = directory.GetFiles(searchPattern).Where(fi => !fi.FullName.EndsWith("rollback.sql")).OrderBy(fi => fi.Name).ToList();
                
                List<NumberedTextScript> result = new List<NumberedTextScript>();
                foreach (FileInfo file in files)
                    result.Add(buildScript(major, minor, file, counter++));
                result.Sort();

                return result;
            }
        }

        private NumberedTextScript buildScript(int major, int minor, FileInfo file, int counter)
        {
            Tuple<String, String> pair = file.Name.splitAt(" ");
            string scriptNumberText = pair.Item1.Trim();
            string description = pair.Item2.Trim();
            
            // Parses script number
            long scriptNumber;            
            if (!long.TryParse(scriptNumberText, out scriptNumber))
                throw new ArgumentException(String.Format("Invalid script number format: {0} for file: {1}", scriptNumberText, file.FullName));
            
            // cleans up description
            if (description.StartsWith("-")) description = description.Substring(1).Trim();
            if (description.EndsWith(".sql", StringComparison.CurrentCultureIgnoreCase)) description = description.Substring(0, description.Length - 4);
            
            NumberedTextScript result = new NumberedTextScript(file, major, minor, scriptNumber, description);
            
            // Checks sequential number, if a number is provided.  Anything greater than 1000 is assumed to be a timestamp and sequence isn't enforced
            if (result.Version.IsPatchNumeric && scriptNumber != counter)
                throw new ArgumentException(String.Format("Script number: {0} doesn't match expected value: {1} for file: {2}", scriptNumber, counter, file.FullName));

            return result;
        }

        #endregion
    }
}