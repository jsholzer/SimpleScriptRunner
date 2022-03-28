using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace SimpleScriptRunnerBto.MySql
{
    public class SqlDatabase : ITextScriptTarget, IDisposable
    {
        public enum VersionEnum
        {
            None,
            V1, 
            V2
        }
        
        private String connectionString;
        private MySqlConnection connection;

        private const String VERSION_TABLE_CREATE =
@"CREATE TABLE db_version (
Major INT NOT NULL,
Minor INT NOT NULL,
Patch BIGINT NOT NULL,
Modified TIMESTAMP NOT NULL,
`MachineName` varchar(100) NOT NULL,  
`Description` varchar(200) NOT NULL,  
PRIMARY KEY (Major,Minor,Patch)
)";

        private const String VERSION_TABLE_UPGRADE_V2 =
@"ALTER TABLE db_version
MODIFY COLUMN `Patch` BIGINT NOT NULL
";
        
        private const String INSERT_COMMAND = @"
INSERT INTO db_version
(Major,Minor,Patch,Modified,MachineName,Description)     
VALUES (@Major,@Minor,@Patch,@Modified,@MachineName,@Description)
";

        private const String TEST_CONNECTION_QUERY = @"select 519";
        private const long TEST_CONNECTION_RESULT = 519;

        private const String TEST_TABLE_QUERY = "select `Patch` from db_version limit 1";

        public SqlDatabase(String theConnectionString)
        {
            createConnection(theConnectionString: theConnectionString);
            Console.WriteLine("Connection String: " + connectionString);
            Console.WriteLine("Current Version: " + CurrentVersion);
        }

        public SqlDatabase(Options options)
        {
            createConnection(options: options);
            Console.WriteLine("Connection String: " + connectionString);
            Console.WriteLine("Current Version: " + CurrentVersion);
        }

        private void createConnection(String theConnectionString = null, Options options = null)
        {
            connectionString = theConnectionString;

            if (connectionString == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Server=").Append(options.ServerName).Append(";");
                builder.Append("Port=").Append("3306").Append(";");
                builder.Append("Database=").Append(options.DatabaseName).Append(";");
                if (options.UserName != null && options.Password != null)
                {
                    builder.Append("Uid=").Append(options.UserName).Append(";");
                    builder.Append("Pwd=").Append(options.Password).Append(";");
                }
                builder.Append("pooling=").Append("false").Append(";");
                if (options.SslMode != null)
                    builder.Append("SslMode=").Append(options.SslMode).Append(";");

                int timeOut = 15 * 60; 
                builder.Append("defaultcommandtimeout=").Append(timeOut).Append(";");

                connectionString = builder.ToString();
            }

            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        public ScriptVersion CurrentVersion
        {
            get
            {
                verify();

                switch (getTableVersion())
                {
                    case VersionEnum.None:   createTable();      break;
                    case VersionEnum.V1:     upgradeVersion2();  break;
                    case VersionEnum.V2: break;
                }

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

        private VersionEnum getTableVersion()
        {
            try
            {
                using (MySqlDataAdapter sql = new MySqlDataAdapter(TEST_TABLE_QUERY, connection))
                {
                    DataSet dataSet = new DataSet();
                    sql.Fill(dataSet);

                    if (dataSet.Tables[0].Rows.Count == 0)
                        return VersionEnum.V2;
                    
                    object patchInstance = dataSet.Tables[0].Rows[0].ItemArray[0];
                    if (patchInstance is long)
                        return VersionEnum.V2;
                    else
                        return VersionEnum.V1;
                }
            }
            catch (Exception)
            {
                return VersionEnum.None;
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

        private void upgradeVersion2()
        {
            using (MySqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = VERSION_TABLE_UPGRADE_V2;
                cmd.ExecuteNonQuery();
            }
        }

        private ScriptVersion readScriptVersion()
        {
             const String READ_QUERY = @"
SELECT Major,Minor,Patch,Modified,MachineName,Description
FROM db_version
order by Major desc, Minor desc, Patch desc
";
            
            using (MySqlDataAdapter sql = new MySqlDataAdapter(READ_QUERY, connection))
            {
                DataSet dataSet = new DataSet();
                sql.Fill(dataSet, 0, 1, "db_version");

                DataTable dbVersion = dataSet.Tables[0];
                if (dbVersion.Rows.Count == 0)
                    return null;

                DataRow itemArray = dbVersion.Rows[0];
                return new ScriptVersion((int)itemArray[0], (int)itemArray[1], (long)itemArray[2], (DateTime)itemArray[3], (String)itemArray[4], (String)itemArray[5]);
            }
        }

        public List<ScriptVersion> getPatches(int major, int minor)
        {
            const String READ_QUERY = @"
SELECT Major,Minor,Patch,Modified,MachineName,Description
FROM db_version
where Major = @Major and Minor = @Minor
order by Patch
";
            List<ScriptVersion> results = new List<ScriptVersion>();
            using (MySqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = READ_QUERY;
                cmd.Parameters.AddWithValue("@Major", major);
                cmd.Parameters.AddWithValue("@Minor", minor);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ScriptVersion((int)reader[0], (int)reader[1], (long)reader[2], (DateTime)reader[3], (String)reader[4], (String)reader[5]));
                    }
                }
            }

            return results;
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
