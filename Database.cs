using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoeFormats {
    public class Database {

        class Column {
            string name;
            bool array;
            enum Type {
                Bool,
                Int,
                Float,
                String,
                Reference,
                Unknown
            }
            Type type;
            string references;

            public Column(JObject o, ref int unkCount, HashSet<string> test) {
                name = o["name"].Value<string>();
                if(name == null) {
                    unkCount++;
                    name = $"unk{unkCount}";
                }
                array = o["array"].Value<bool>();
                switch (o["type"].Value<string>()) {
                    case "bool":
                        type = Type.Bool; break;
                    case "i32":
                        type = Type.Int; break;
                    case "f32":
                        type = Type.Float; break;
                    case "string":
                        type = Type.String; break;
                    case "foreignrow":
                    case "enumrow":
                        type = Type.Reference;
                        if (o["references"] != null && o["references"].HasValues) {
                            references = o["references"]["table"].Value<string>();
                        }
                        break;
                    default:
                        type = Type.Unknown; break;
                }
            }
        }


        Dictionary<string, Column[]> schema;

        public Database(string datDirectory, string schemaPath) {
            HashSet<string> columnTypes = new HashSet<string>();
            schema = new Dictionary<string, Column[]>();
            using (StreamReader reader = File.OpenText(schemaPath)) {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                foreach(JObject table in o["tables"].Value<JArray>()) {
                    JArray c = (JArray)table["columns"];
                    Column[] columns = new Column[c.Count];
                    int unkCount = 0;
                    for (int i = 0; i < columns.Length; i++) {
                        columns[i] = new Column((JObject)c[i], ref unkCount, columnTypes);
                    }
                    schema[table["name"].Value<string>()] = columns;
                }
            }
            foreach(string type in columnTypes) Console.WriteLine(type);
        }
    }
}
