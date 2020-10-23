using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using VErp.Commons.Constants;

namespace SynTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = $"/usr/verp/config/config.{args[0]}.json";
            if (!System.IO.File.Exists(file))
            {
                Console.WriteLine("File not found: " + file);
                return;
            }
            var setting = JObject.Parse(System.IO.File.ReadAllText(file));

            var cnn = setting.SelectToken(args[1]).ToString();

            //var path = "..\\..\\..\\..\\..\\Infrastructure\\EntityFramework";


            var projectFolder = args[2];

            var files = Directory.GetFiles(projectFolder, "*.cs");
            foreach (var f in files)
            {
                Console.WriteLine("Del " + f);
                System.IO.File.Delete(f);
            }

            var partialFiles = System.IO.Directory.GetFiles(projectFolder + "\\Partial", "*.cs");
            foreach (var f in partialFiles)
            {
                File.Move(f, f + ".txt");
            }

            var context = $"{args[2]}Context";

            var contextPath = projectFolder + "\\" + context + ".cs";

            var tableOnly = "";

            var dbHelper = new DbHelper(cnn);
            var dataTable = dbHelper.GetDataTable("SELECT [TABLE_CATALOG],[TABLE_SCHEMA], [TABLE_NAME], [TABLE_TYPE]  FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_TYPE] = N'BASE TABLE' OR [TABLE_NAME] LIKE N'vMapping%' ");
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var tblName = dataTable.Rows[i]["TABLE_NAME"].ToString();
                if (!tblName.StartsWith("_") && !tblName.Equals(AccountantConstants.INPUTVALUEROW_TABLE, StringComparison.OrdinalIgnoreCase)
                    && !!tblName.Equals(VoucherConstants.VOUCHER_VALUE_ROW_TABLE, StringComparison.OrdinalIgnoreCase)
                    )
                    tableOnly += " -t " + dataTable.Rows[i]["TABLE_NAME"];
            }


            var cmd = $"dotnet ef dbcontext scaffold \"{cnn}\" {tableOnly} Microsoft.EntityFrameworkCore.SqlServer -p {projectFolder}\\{args[2]}.csproj -c {context} -f -s EF.Generator\\EF.Generator.csproj";
            Console.WriteLine("\n\n" + cmd + "\n\n");
            Console.WriteLine(Bash(cmd));

            foreach (var f in partialFiles)
            {
                File.Move(f + ".txt", f);
            }

            var text = System.IO.File.ReadAllText(contextPath);
            //text = text.Replace("protected override void OnModelCreating", "protected void OnModelCreated");
            //text.Replace("OnModelCreating", "OnModelCreated");

            var reg = new Regex("(?<fName>protected override void OnConfiguring[^\\}]*})", RegexOptions.Multiline);
            text = text.Replace(reg.Match(text).Groups["fName"].Value, "protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {");

            System.IO.File.WriteAllText(contextPath, text);

        }
        static string Bash(string cmd)
        {
            var cmdRoot = cmd.Split(' ')[0];
            cmd = cmd.Substring(cmdRoot.Length);

            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmdRoot,
                    Arguments = $" {cmd}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

    }
}
