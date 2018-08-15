using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SimpleScriptRunner.MySql
{
    public class SqlDatabase : ITextScriptTarget, IDisposable
    {
        private String hostString;
        private MySqlConnection connection;

        private const String VERSION_TABLE_CREATE =
@"CREATE TABLE db_version (
Major INT NOT NULL,
Minor INT NOT NULL,
Patch INT NOT NULL,
Modified TIMESTAMP NOT NULL,
`MachineName` varchar(100) NOT NULL,  
`Description` varchar(200) NOT NULL,  
PRIMARY KEY (Major,Minor,Patch)
)";

        private const String READ_QUERY = @"
SELECT Major,Minor,Patch,Modified,MachineName,Description
FROM db_version
order by Major desc, Minor desc, Patch desc
";
        
        private const String INSERT_COMMAND = @"
INSERT INTO db_version
(Major,Minor,Patch,Modified,MachineName,Description)     
VALUES (@Major,@Minor,@Patch,@Modified,@MachineName,@Description)
";

        private const String TEST_CONNECTION_QUERY = @"select 519";
        private const long TEST_CONNECTION_RESULT = 519;

        private const String TEST_TABLE_QUERY = "select count(*) from db_version";

        public SqlDatabase(String hostString)
        {
            createConnection(theHostString: hostString);
            Console.WriteLine("CurrentVersion: " + CurrentVersion);
        }

        public SqlDatabase(String serverName, String databaseName, String username, String password)
        {
            createConnection(serverName: serverName, databaseName: databaseName, username: username, password: password);
            Console.WriteLine("CurrentVersion: " + CurrentVersion);
        }

        public SqlDatabase(String serverName, String databaseName)
        {
            createConnection(serverName: serverName, databaseName: databaseName);
            Console.WriteLine("CurrentVersion: " + CurrentVersion);
        }

        private void createConnection(String theHostString = null, String serverName = null, String databaseName = null, String username = null, String password = null, int? timeOut = null)
        {
            hostString = theHostString;

            if (hostString == null)
            {
                if (username != null && password != null)
                    hostString = String.Format("Server={0};Port=3306;Database={1};Uid={2};Pwd={3};pooling=false;", serverName, databaseName, username, password);
                else
                    hostString = String.Format("Server={0};Port=3306;Database={1};pooling=false;", serverName, databaseName);
            }

            timeOut = timeOut ?? 15 * 60;         // defaults to 15 minutes
            if (!hostString.Contains("defaultcommandtimeout="))
            {
                if (!hostString.EndsWith(";")) hostString += ";";
                hostString += "defaultcommandtimeout=" + timeOut + ";";                   
            }

            connection = new MySqlConnection(hostString);
            connection.Open();
        }

        public ScriptVersion CurrentVersion
        {
            get
            {
                verify();

                if (!isTablePresent())
                    createTable();

                ScriptVersion result = readScriptVersion();
                if (result != null)
                    return result;

                return new ScriptVersion(0, 0, 0, DateTime.MinValue, Environment.MachineName, "EMPTY DB");
            }

            private set
            {
                using (MySqlCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = INSERT_COMMAND;
                    cmd.Parameters.AddWithValue("@Major", value.Major);
                    cmd.Parameters.AddWithValue("@Minor", value.Minor);
                    cmd.Parameters.AddWithValue("@Patch", value.Patch);
                    cmd.Parameters.AddWithValue("@Modified", value.Date);
                    cmd.Parameters.AddWithValue("@MachineName", value.MachineName);
                    cmd.Parameters.AddWithValue("@Description", value.Description);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void verify()
        {
            using (MySqlDataAdapter sql = new MySqlDataAdapter(TEST_CONNECTION_QUERY, connection))
            {
                DataSet dataSet = new DataSet();
                sql.Fill(dataSet);
                
                long result = (long) dataSet.Tables[0].Rows[0].ItemArray[0];
                if (result != TEST_CONNECTION_RESULT)
                    throw new Exception("Could not verify connection to DB");
            }
        }

        private bool isTablePresent()
        {
            try
            {
                using (MySqlDataAdapter sql = new MySqlDataAdapter(TEST_TABLE_QUERY, connection))
                {
                    DataSet dataSet = new DataSet();
                    sql.Fill(dataSet);

                    long result = (long)dataSet.Tables[0].Rows[0].ItemArray[0];
                    return result >= 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void createTable()
        {
            using (MySqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = VERSION_TABLE_CREATE;
                cmd.ExecuteNonQuery();
            }
        }

        private ScriptVersion readScriptVersion()
        {
            using (MySqlDataAdapter sql = new MySqlDataAdapter(READ_QUERY, connection))
            {
                DataSet dataSet = new DataSet();
                sql.Fill(dataSet, 0, 1, "db_version");

                DataTable dbVersion = dataSet.Tables[0];
                if (dbVersion.Rows.Count == 0)
                    return null;

                DataRow itemArray = dbVersion.Rows[0];
                return new ScriptVersion((int)itemArray[0], (int)itemArray[1], (int)itemArray[2], (DateTime)itemArray[3], (String)itemArray[4], (String)itemArray[5]);
            }
        }

        public void apply(string content)
        {
            MySqlScript script = new MySqlScript(connection, content);
            script.Execute();
        }

        public void updateVersion(ScriptVersion version)
        {
            version.Date = DateTime.UtcNow;                                   // always use NOW as date of record
            CurrentVersion = version;
        }

        public void Dispose()
        {
            if (connection != null)
                try { connection.Dispose(); } catch (Exception) { /*ignore*/ }
            connection = null;
        }
    }
}
