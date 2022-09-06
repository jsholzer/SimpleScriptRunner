using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleScriptRunnerBto.MySql;
using SimpleScriptRunnerBto.Util;

namespace SimpleScriptRunnerBto
{
    public class Program
    {
        public enum Category
        {
            error,
            warning
        }

        public static int originalMain(string[] argArray)
        {
            try
            {
                Options options = Options.build(argArray);
                executeRelease(options);
                return 0;
            }
            catch (Exception ex)
            {
                WriteMessage(Assembly.GetExecutingAssembly().FullName, string.Empty, Category.error, "1", ex.ToString());
                return 1;
            }
        }

        public static void executeRelease(Options options)
        {
            SqlDatabase scriptTarget = new SqlDatabase(options);

            String releasePath = Path.Combine(options.Path, "Release");
            
            ScriptVersion currentVersion = scriptTarget.CurrentVersion;                                                     // only reads version once and relies on scripts executing in proper order
            foreach (String releaseDirectoryPath in Directory.GetDirectories(options.Path, "*").OrderBy(x => x))
            {
                if (!releaseDirectoryPath.startsWithIgnoreCase(releasePath))        // case insensitive directory scan
                    continue;
                
                IScriptSource<ITextScriptTarget> scriptSource = new FolderContainingNumberedSqlScripts(releaseDirectoryPath, "*.sql");
                Updater updater = new Updater(scriptSource, scriptTarget, options);
                updater.applyScripts(currentVersion);
            }
        }

        public static int executeSqlFile(Options options)
        {
            try
            {
            
                SqlDatabase scriptTarget = new SqlDatabase(options);

                options.SkipVersion = true;         // turns off setting patch version since file is free form sql

                FileInfo file = new FileInfo(options.Path);
                IScript<ITextScriptTarget> script = new NumberedTextScript(file, 0, 0, 0, "");
                script.apply(scriptTarget, options);

                return 0;
            }
            catch (Exception ex)
            {
                WriteMessage(Assembly.GetExecutingAssembly().FullName, string.Empty, Category.error, "1", ex.ToString());
                return 1;
            }
        }

        private static void WriteMessage(string origin, string subcategory, Category category, string code, string text)
        {
            Console.WriteLine("{0} : {1} {2} {3} : {4}", origin, subcategory, category, code, text);
        }
    }
}