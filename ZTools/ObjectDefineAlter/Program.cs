using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectDefineAlter
{

    class Program
    {


        static Dictionary<string, string> databaseConfigKeys = new Dictionary<string, string>()
        {
            {"AccountancyDB","AccountancyPrivateDatabase" },
            {"AccountancyPublicDB","AccountancyPublicDatabase" },
            {"ManufacturingDB","ManufacturingDatabase" },
            {"MasterDB","MasterDatabase" },
            {"OrganizationDB","OrganizationDatabase" },
            {"PurchaseOrderDB","PurchaseOrderDatabase" },
            {"StockDB","StockDatabase" },
        };

        static string sv1 = $"/usr/verp/config/config.Development.json";
        static string sv2 = $"";
        static string scripts = "";


        static void Main(string[] args)
        {
            if (args?.Count() > 0)
            {
                foreach (var a in args)
                {
                    var vas = a.Split(':', 2);
                    var aName = vas[0]?.ToLower();
                    var aValue = vas.Length > 1 ? vas[1] : null;
                    if (!string.IsNullOrWhiteSpace(aName) && !string.IsNullOrWhiteSpace(aValue))
                    {
                        switch (aName)
                        {
                            case "/sv1":
                                sv1 = aValue?.Trim('"');
                                break;
                            case "/sv2":
                                sv2 = aValue?.Trim('"');
                                break;
                            case "/scripts":
                                scripts = aValue?.Trim('"');
                                break;
                        }
                    }
                }
            }
            if (!System.IO.File.Exists(sv1))
            {
                Console.WriteLine("File not found: " + sv1);
                return;
            }
            var setting = JObject.Parse(System.IO.File.ReadAllText(sv1));

            var cnn = setting.SelectToken("$.DatabaseConnections.MasterDatabase").ToString();

            var dbHelper = new DbHelper(cnn);

            var typeStr = string.Join(',', ObjectTypes.Keys.Select(t => $"'{t}'").ToArray());
            var refSqlStr = @$"SELECT
                            
	                        o.name                                          [Name],
                            SCHEMA_NAME(o.SCHEMA_ID)                        [Schema],
                            m.definition                                    [Definition],
                            o.type                                          [Type],

	                        sed.referenced_database_name                    [RefDbName],
                            sed.referenced_schema_name                      [RefSchema],
                            ISNULL(o1.name, sed.referenced_entity_name)     [RefName]

                        FROM                            
                            _DatabaseName_.sys.sql_expression_dependencies sed
                            LEFT JOIN _DatabaseName_.sys.objects o ON sed.referencing_id = o.[object_id]
                            LEFT JOIN _DatabaseName_.sys.objects o1 ON sed.referenced_id = o1.[object_id]
                            LEFT JOIN _DatabaseName_.sys.sql_modules m on o.[object_id] = m.[object_id]

                        WHERE o.type IN ({typeStr})
";

            var noRefSqlStr = @$"
                        SELECT
                            
	                        o.name                                  [Name],
                            SCHEMA_NAME(o.SCHEMA_ID)                [Schema],
                            m.definition                            [Definition],
                            o.type                                  [Type]	                       
                        FROM
                            _DatabaseName_.sys.objects o
                            LEFT JOIN _DatabaseName_.sys.sql_expression_dependencies sed ON sed.referencing_id = o.[object_id] AND (sed.referenced_id IS NOT NULL OR sed.referenced_schema_name IS NOT NULL)
                            LEFT JOIN _DatabaseName_.sys.sql_modules m on o.[object_id] = m.[object_id]

                        WHERE o.type IN ({typeStr}) AND sed.referenced_id IS NULL AND sed.referenced_schema_name IS NULL

";

            var dbs = databaseConfigKeys.Keys.ToList();

            var objectRefs = new List<KeyValuePair<string, string>>();

            var objectDefines = new Dictionary<string, DataDefine>();


            //No ref
            foreach (var db in dbs)
            {

                var sql = noRefSqlStr.Replace("_DatabaseName_", db);
                var dataTable = dbHelper.GetDataTable(sql);
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var objName = AddToDefines(objectDefines, db, dataTable.Rows[i]);
                }
            }

            //Refs
            foreach (var db in dbs)
            {

                var sql = refSqlStr.Replace("_DatabaseName_", db);
                var dataTable = dbHelper.GetDataTable(sql);

                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var objName = AddToDefines(objectDefines, db, dataTable.Rows[i]);

                    var refDbName = dataTable.Rows[i]["RefDbName"]?.ToString();
                    if (string.IsNullOrWhiteSpace(refDbName))
                    {
                        refDbName = db;
                    }

                    var refSchema = dataTable.Rows[i]["RefSchema"]?.ToString();

                    var refName = dataTable.Rows[i]["RefName"]?.ToString();

                    var refObjName = NormalizeObjectName(refDbName, refSchema, refName);
                    if (objName?.Equals(refObjName, StringComparison.OrdinalIgnoreCase) != true)
                        objectRefs.Add(new KeyValuePair<string, string>(objName, refObjName));
                }

            }

            foreach (var d in objectDefines)
            {

                ReplaceAlter(objectDefines[d.Key]);
            }

            var noRefs = objectRefs.Where(o => string.IsNullOrWhiteSpace(o.Value) || o.Key.Equals(o.Value, StringComparison.OrdinalIgnoreCase)).Select(o => o.Key).Distinct().ToList();

            var objDefineSorts = new Dictionary<string, DataDefine>();

            foreach (var o in noRefs)
            {
                objDefineSorts.Add(o, objectDefines[o]);
            }

            while (objDefineSorts.Count < objectDefines.Count)
            {
                foreach (var o in objectDefines.Where(d => !objDefineSorts.Keys.Contains(d.Key)))
                {
                    var refs = objectRefs.Where(r => r.Key == o.Key && objectDefines.Keys.Contains(r.Value)).Select(r => r.Value).Distinct().ToList();

                    if (refs.Count == 0 || refs.All(r =>
                    objDefineSorts.Keys.Contains(r)
                    || !objectDefines.Keys.Contains(r)
                    ))
                    {
                        objDefineSorts.Add(o.Key, o.Value);
                    }

                }
            }

            var alterStr = new StringBuilder();
            foreach (var d in objDefineSorts)
            {
                alterStr.AppendLine("USE " + d.Value.Database);
                alterStr.AppendLine("GO");
                alterStr.AppendLine(d.Value.Definition);
                alterStr.AppendLine("GO");
                alterStr.AppendLine("");
            }


            if (!Directory.Exists("/usr"))
            {
                Directory.CreateDirectory("/usr");
            }

            System.IO.File.WriteAllText($"/usr/Verp_{DateTime.Now.ToString("yyyy_MM_dd")}.sql", alterStr.ToString());


            if (!string.IsNullOrWhiteSpace(sv2))
                Deploy(sv2, objDefineSorts);

            if (!string.IsNullOrWhiteSpace(scripts))
                RunScripts(sv2, scripts);
        }

        private static void Deploy(string sv, Dictionary<string, DataDefine> objDefineSorts)
        {
            if (!System.IO.File.Exists(sv))
            {
                Console.WriteLine("File not found: " + sv);
                return;
            }
            var setting = JObject.Parse(System.IO.File.ReadAllText(sv));
            foreach (var obj in objDefineSorts)
            {
                var cnn = setting.SelectToken($"$.DatabaseConnections.{databaseConfigKeys[obj.Value.Database]}").ToString();

                var dbHelper = new DbHelper(cnn);
                dbHelper.ExecuteNonQuery(obj.Value.Definition);
            }
        }

        private static void RunScripts(string sv, string dic)
        {
            if (!System.IO.File.Exists(sv))
            {
                Console.WriteLine("File not found: " + sv);
                return;
            }

            var setting = JObject.Parse(System.IO.File.ReadAllText(sv));
            foreach (var db in databaseConfigKeys.Keys)
            {
                var cnn = setting.SelectToken($"$.DatabaseConnections.{databaseConfigKeys[db]}").ToString();

                var folder = dic + "/" + db;
                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder).OrderBy(f => f).ToList();
                    var dbHelper = new DbHelper(cnn);
                    foreach (var file in files)
                    {
                        var sql = System.IO.File.ReadAllText(file);

                        dbHelper.ExecuteNonQuery(sql);
                    }

                }
            }
        }

        private static string AddToDefines(Dictionary<string, DataDefine> objectDefines, string db, DataRow row)
        {
            var name = row["Name"]?.ToString();
            var schema = row["Schema"]?.ToString();
            var definition = row["Definition"]?.ToString();
            var type = row["Type"]?.ToString();

            var objName = NormalizeObjectName(db, schema, name);

            if (!objectDefines.ContainsKey(objName) && !name.ToLower().EndsWith("_bak") && !name.ToLower().EndsWith("_old") && !name.ToLower().StartsWith("test") && !name.ToLower().Contains("diagram"))
            {
                objectDefines.Add(objName, new DataDefine()
                {
                    Database = db,
                    Name = name,
                    Schema = string.IsNullOrWhiteSpace(schema) ? "dbo" : schema,
                    Definition = definition,
                    Type = type
                });
            }
            return objName;
        }

        static private Dictionary<string, string> ObjectTypes = new Dictionary<string, string>()
        {
            {"P","PROCEDURE" },
            {"TF","FUNCTION" },
            {"FN","FUNCTION" },
            {"V","VIEW" },
            {"TR","TRIGGER" },
        };


        private static void ReplaceAlter(DataDefine obj)
        {
            var str = obj.Definition;
            var fullType = obj.Type?.Trim();

            if (ObjectTypes.ContainsKey(fullType))
            {
                fullType = ObjectTypes[fullType];
            }
            else
            {
                throw new Exception("Not support " + fullType);
            }


            /*
            while (str.IndexOf("  " + fullType) >= 0)
            {
                str = str.Replace("  " + fullType, " " + fullType);
            }

            while (str.IndexOf(fullType + "  ") >= 0)
            {
                str = str.Replace(fullType + "  ", fullType + " ");
            }*/

            var objDef = $"CREATE OR ALTER {fullType} [{obj.Schema}].[{obj.Name}]";

            var defineReg = $"CREATE\\s*{fullType}\\s*\\[?({obj.Schema})?\\]?\\.?\\[?{obj.Name}\\]?";
            str = Regex.Replace(str, defineReg, objDef, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            /*
            var schemas = new[] { $"{obj.Schema}.", $"[{obj.Schema}].", "" };
            var objs = new[] { obj.Name, $"[{obj.Name}]" };

            foreach (var s in schemas)
            {
                foreach (var d in objs)
                {
                    var originalDef = $"CREATE {fullType} {s}{d}";
                    str = str.Replace(originalDef, objDef, StringComparison.OrdinalIgnoreCase);
                }
            }*/

            if (str.IndexOf(objDef) < 0)
            {
                throw new Exception($"[{obj.Database}].[{obj.Schema}].[{obj.Name}] ERROR");
            }
            obj.Definition = str;
        }


        private static string NormalizeObjectName(string dbName, string schema, string objectName)
        {
            if (string.IsNullOrWhiteSpace(schema)) schema = "dbo";
            return string.Format("[{0}].[{1}].[{2}]", dbName, schema, objectName);
        }

    }


    public class DataDefine
    {
        public string Database { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public string Definition { get; set; }
        public string Type { get; set; }
    }
}
