using Newtonsoft.Json;
using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.XEvent;

namespace UpgradeTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args?.Length < 2)
            {
                throw new ArgumentException("Missing config file and versions folder!");
            }

            Console.WriteLine("Migration is started");
            Server server;
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                var config = JsonConvert.DeserializeObject<DatabaseConfig>(File.ReadAllText(args[0]));
                server = new Server(new ServerConnection(config.Server, config.UserName, config.Password));
            }
            else
            {
                server = new Server();
            }
            const string versionConfigName = "Version";

            var dataSets = server.ConnectionContext.ExecuteWithResults($"SELECT * FROM MasterDB.dbo.Config WHERE ConfigName = '{versionConfigName}'");
            var dataTable = dataSets.Tables[0];
            decimal currentVersion = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                currentVersion = Convert.ToDecimal(row["Value"]);
            }

            var versions = Directory.GetDirectories(args[1])
                .Select(f =>
                {
                    var fname = Path.GetFileName(f);
                    return new
                    {
                        Version = Convert.ToDecimal(fname),
                        Path = f
                    };
                })
                .Where(v => v.Version > currentVersion)
                .OrderBy(v => v.Version)
            .ToList();

            Console.WriteLine($"Current version is {currentVersion}");

            if (versions.Count == 0)
            {
                Console.WriteLine($"Nothing update");
            }
            foreach (var version in versions)
            {
                var files = Directory.GetFiles(version.Path).OrderBy(f => f).ToList();
                var sql = new StringBuilder();
                foreach (var file in files)
                {
                    sql.Append(File.ReadAllText(file));
                    sql.AppendLine("\nGO\n");
                }
                sql.AppendLine($"UPDATE MasterDB.dbo.Config SET Value = {version.Version} WHERE ConfigName = '{versionConfigName}'");

                var sqlString = sql.ToString();

                server.ConnectionContext.BeginTransaction();
                
                server.ConnectionContext.ExecuteNonQuery(sqlString);

                server.ConnectionContext.CommitTransaction();

                Console.WriteLine($"Done upgrade to version {version.Version}");
            }

        }
    }

    public class DatabaseConfig
    {
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
