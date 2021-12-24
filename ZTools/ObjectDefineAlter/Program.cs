using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectDefineAlter
{

    class Program
    {
        static void Main(string[] args)
        {
            var file = $"/usr/verp/config/config.Development.json";
            if (!System.IO.File.Exists(file))
            {
                Console.WriteLine("File not found: " + file);
                return;
            }
            var setting = JObject.Parse(System.IO.File.ReadAllText(file));

            var cnn = setting.SelectToken("$.DatabaseConnections.MasterDatabase").ToString();

            var dbHelper = new DbHelper(cnn);

            var sqlStr = @"SELECT
                            
	                        o.name                                  [Name],
                            SCHEMA_NAME(o.SCHEMA_ID)                [Schema],
                            m.definition                            [Definition],
                            o.type                                  [Type],

	                        referenced_database_name                [RefDbName],
                            referenced_schema_name                  [RefSchema],
                            o1.name                                 [RefName]

                        FROM
                            _DatabaseName_.sys.sql_expression_dependencies sed
                            INNER JOIN _DatabaseName_.sys.objects o ON sed.referencing_id = o.[object_id]
                            LEFT OUTER JOIN _DatabaseName_.sys.objects o1 ON sed.referenced_id = o1.[object_id]
                            LEFT JOIN _DatabaseName_.sys.sql_modules m on o.[object_id] = m.[object_id]

                        WHERE o.type_desc <> 'USER_TABLE' AND ISNULL(o.name,'') <> ISNULL(o1.name, '')
";
            var dbs = new[] { "AccountancyDB", "ManufacturingDB", "MasterDB", "OrganizationDB", "PurchaseOrderDB", "StockDB" };

            var objectRefs = new List<KeyValuePair<string, string>>();

            var objectDefines = new Dictionary<string, DataDefine>();


            foreach (var db in dbs)
            {

                var sql = sqlStr.Replace("_DatabaseName_", db);
                var dataTable = dbHelper.GetDataTable(sql);

                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var name = dataTable.Rows[i]["Name"]?.ToString();
                    var schema = dataTable.Rows[i]["Schema"]?.ToString();
                    var definition = dataTable.Rows[i]["Definition"]?.ToString();
                    var type = dataTable.Rows[i]["Type"]?.ToString();

                    var refDbName = dataTable.Rows[i]["RefDbName"]?.ToString();
                    var refSchema = dataTable.Rows[i]["RefSchema"]?.ToString();
                    var refName = dataTable.Rows[i]["RefName"]?.ToString();

                    var objName = NormalizeObjectName(db, schema, name);
                    var refObjName = NormalizeObjectName(refDbName, refSchema, refName);
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

                    if (refs.Count == 0 || refs.All(r => objDefineSorts.Keys.Contains(r)))
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

            System.IO.File.WriteAllText("/usr/Verp.sql", alterStr.ToString());
        }

        private static void ReplaceAlter(DataDefine obj)
        {
            var str = obj.Definition;
            var fullType = obj.Type?.Trim();

            switch (fullType)
            {
                case "P":
                    fullType = "PROCEDURE";
                    break;
                case "TF":
                    fullType = "FUNCTION";
                    break;
                case "FN":
                    fullType = "FUNCTION";
                    break;
                case "V":
                    fullType = "VIEW";
                    break;
                case "TR":
                    fullType = "TRIGGER";
                    break;
                default:
                    throw new Exception("Not support " + fullType);
            }

            while (str.IndexOf("  " + fullType) >= 0)
            {
                str = str.Replace("  " + fullType, " " + fullType);
            }

            while (str.IndexOf(fullType + "  ") >= 0)
            {
                str = str.Replace(fullType + "  ", fullType + " ");
            }

            var objDef = $"CREATE OR ALTER {fullType} [{obj.Schema}].[{obj.Name}]";

            var schemas = new[] { $"{obj.Schema}.", $"[{obj.Schema}].", "" };
            var objs = new[] { obj.Name, $"[{obj.Name}]" };

            foreach (var s in schemas)
            {
                foreach (var d in objs)
                {
                    var originalDef = $"CREATE {fullType} {s}{d}";
                    str = str.Replace(originalDef, objDef);
                }
            }

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
