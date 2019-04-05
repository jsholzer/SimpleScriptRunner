using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                List<FileInfo> files = directory.GetFiles(searchPattern).Where(file => !file.FullName.EndsWith("rollback.sql")).ToList();
                List<NumberedTextScript> result = new List<NumberedTextScript>();
                foreach (FileInfo file in files)
                    result.Add(buildScipt(major, minor, file, counter++));
                result.Sort();

                return result;
            }
        }

        private NumberedTextScript buildScipt(int major, int minor, FileInfo file, int counter)
        {
            int splitLocation = file.Name.IndexOf(' ');

            // Parses script number
            int scriptNumber;
            string scriptNumberText = file.Name.Substring(0, splitLocation).Trim();
            string description = file.Name.Substring(splitLocation + 1).Trim();
            if (!int.TryParse(scriptNumberText, out scriptNumber))
                throw new ArgumentException(String.Format("Invalid script number format: {0} for file: {1}", scriptNumberText, file.FullName));

            if (scriptNumber != counter)
                throw new ArgumentException(String.Format("Script number: {0} doesn't match expected value: {1} for file: {2}", scriptNumber, counter, file.FullName));

            // cleans up description
            if (description.StartsWith("-")) description = description.Substring(1).Trim();
            if (description.EndsWith(".sql", StringComparison.CurrentCultureIgnoreCase)) description = description.Substring(0, description.Length - 4);

            return new NumberedTextScript(file, major, minor, scriptNumber, description);
        }

        #endregion
    }
}