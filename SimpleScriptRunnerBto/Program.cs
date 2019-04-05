using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleScriptRunnerBto.MySql;

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
                executeRelease(options.Params[0], options.Params[1], options.Params[4], options.Params[2], options.Params[3], options);
                return 0;
            }
            catch (Exception ex)
            {
                WriteMessage(Assembly.GetExecutingAssembly().FullName, string.Empty, Category.error, "1", ex.ToString());
                return 1;
            }
        }

        public static void executeRelease(string serverName, 
            string databaseName, 
            string path = ".", 
            string username = null, 
            string password = null,
            Options options = null
            )
        {
            options = options ?? new Options();
            
            SqlDatabase scriptTarget = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) ?
                new SqlDatabase(serverName, databaseName, username, password) :
                new SqlDatabase(serverName, databaseName);

            ScriptVersion currentVersion = scriptTarget.CurrentVersion;                                                     // only reads version once and relies on scripts executing in proper order
            foreach (String releaseDirectoryPath in Directory.GetDirectories(path, "Release *"))
            {
                IScriptSource<ITextScriptTarget> scriptSource = new FolderContainingNumberedSqlScripts(releaseDirectoryPath, "*.sql");
                Updater updater = new Updater(scriptSource, scriptTarget, options);
                updater.applyScripts(currentVersion);
            }
        }

        public static int executeSqlFile(string path, string serverName, string databaseName, string username = null, string password = null, Options options = null)
        {
            try
            {
                options = options ?? new Options();
            
                SqlDatabase scriptTarget = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) ?
                    new SqlDatabase(serverName, databaseName, username, password) :
                    new SqlDatabase(serverName, databaseName);

                options.SkipVersion = true;         // turns off setting patch version since file is free form sql

                FileInfo file = new FileInfo(path);
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