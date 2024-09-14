using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace PoeFormats {
    public class Schema {

        public class Enumeration {
            public int indexing;
            public string[] values;
        }

        public class Column {
            public string name;
            public bool array;
            public enum Type {
                Bool,
                Int,
                Float,
                String,
                Reference,
                Row,
                Enum,
                Unknown
            }
            public Type type;
            public string references;
            public bool isEnum;

            public Column(JObject o, ref int unkCount, HashSet<string> test) {
                name = o["name"].Value<string>();
                if (name == null) {
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
                    case "enumrow":
                        type = Type.Enum;
                        if (o["references"] != null && o["references"].HasValues) {
                            references = o["references"]["table"].Value<string>();
                        }
                        break;
                    case "foreignrow":
                        type = Type.Reference;
                        if (o["references"] != null && o["references"].HasValues) {
                            references = o["references"]["table"].Value<string>();
                        }
                        break;
                    case "row":
                        type = Type.Row; break;
                    default:
                        type = Type.Unknown; break;
                }
            }
        }



        Dictionary<string, Column[]> schema;
        Dictionary<string, Enumeration> enums;

        public Schema(string schemaPath) {
            HashSet<string> columnTypes = new HashSet<string>();
            schema = new Dictionary<string, Column[]>();
            enums = new Dictionary<string, Enumeration>();
            using (StreamReader reader = File.OpenText(schemaPath)) {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                foreach (JObject table in o["tables"].Value<JArray>()) {
                    JArray c = (JArray)table["columns"];
                    Column[] columns = new Column[c.Count];
                    int unkCount = 0;
                    for (int i = 0; i < columns.Length; i++) {
                        columns[i] = new Column((JObject)c[i], ref unkCount, columnTypes);
                    }
                    schema[table["name"].Value<string>().ToLower()] = columns; 
                    //should we keep capitalization in dat names? I cant think of a reason
                }
                foreach (JObject enumeration in o["enumerations"].Value<JArray>()) {
                    Enumeration e = new Enumeration();
                    e.indexing = enumeration["indexing"].Value<int>();
                    var enumerators = enumeration["enumerators"].Value<JArray>();

                    e.values = new string[enumerators.Count];
                    for (int i = 0; i < e.values.Length; i++)
                        e.values[i] = enumerators[i].Value<string>();

                    enums[enumeration["name"].Value<string>().ToLower()] = e;
                    //should we keep capitalization in dat names? I cant think of a reason
                }
            }
            foreach (string type in columnTypes) Console.WriteLine(type);
        }

        void WriteVariable(string columnName, string type, TextWriter w, List<string> readLines, bool array = false) {
            if (array) {
                w.WriteLine($"\t\tpublic {type.ToLower()}[] {columnName};");
                readLines.Add($"\t\t\t{columnName} = r.{type}Array();");
            }
            else {
                w.WriteLine($"\t\tpublic {type.ToLower()} {columnName};");
                readLines.Add($"\t\t\t{columnName} = r.{type}();");
            }

        }

        
        HashSet<string> reserved = new HashSet<string>() { "Object", "Override", "Event", "Double" };

        public void GenerateCode(string datFolder) {
            var datClassNames = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\PathOfExile\datclassnames.txt")) {
                string[] words = line.Split('\t');
                datClassNames[words[0]] = words[1];
            }


            using (TextWriter w = new StreamWriter(File.Create(@"C:\temp\Rows.cs"))) {
                w.WriteLine("namespace PoeFormats.Rows {"); w.WriteLine();
                foreach(string table in schema.Keys) {
                    List<string> readLines = new List<string>();
                    //if (!table.StartsWith("g")) continue;
                    if(!datClassNames.ContainsKey(table)) {
                        Console.WriteLine("MISSING DAT CLASS NAME " + table);
                        continue;
                    }
                    string className = datClassNames[table];

                    w.WriteLine($"\tpublic class {className} : Row {{");

                    var columns = schema[table];
                    for (int i = 0; i < columns.Length; i++) {
                        var column = columns[i];
                        string columnName = column.name;
                        if(columnName.Length > 1 && char.IsUpper(columnName[0]) && !char.IsUpper(columnName[1]) && !reserved.Contains(columnName)) 
                            columnName = char.ToLower(columnName[0]) + columnName.Substring(1);
                        switch (column.type) {
                            case Column.Type.Bool:
                                WriteVariable(columnName, "Bool",   w, readLines); break;
                            case Column.Type.Int:
                                WriteVariable(columnName, "Int",    w, readLines, column.array); break;
                            case Column.Type.Float:
                                WriteVariable(columnName, "Float",  w, readLines, column.array); break;
                            case Column.Type.String:
                                WriteVariable(columnName, "String", w, readLines, column.array); break;
                            case Column.Type.Reference:
                                if(column.references == null) {
                                    if(column.array) {
                                        readLines.Add($"\t\t\tr.RefArray();");
                                    } else {
                                        readLines.Add($"\t\t\tr.Ref();");
                                    }
                                } else {
                                    string references = column.references.ToLower();
                                    string refClass = datClassNames[references];
                                    
                                    if (column.array) {
                                        w.WriteLine($"\t\tpublic {refClass}[] {columnName};");
                                        readLines.Add($"\t\t\t{columnName} = d.GetArray<{refClass}>(r.RefArray());");
                                    }
                                    else {
                                        w.WriteLine($"\t\tpublic {refClass} {columnName};");
                                        readLines.Add($"\t\t\t{columnName} = d.Get<{refClass}>(r.Ref());");
                                    }
                                    
                                }
                                break;
                            case Column.Type.Enum:
                                if (column.references == null) {
                                    if (column.array) {
                                        readLines.Add($"\t\t\tr.IntArray();");
                                    }
                                    else {
                                        readLines.Add($"\t\t\tr.Int();");
                                    }
                                }
                                else {
                                    string references = column.references.ToLower();
                                    string refClass = datClassNames[references];
                                    if (column.array) {
                                        w.WriteLine($"\t\tpublic {refClass}[] {columnName};");
                                        readLines.Add($"\t\t\t{columnName} = r.EnumArray<{refClass}>();");
                                    }
                                    else {
                                        w.WriteLine($"\t\tpublic {refClass} {columnName};");
                                        readLines.Add($"\t\t\t{columnName} = r.Enum<{refClass}>();");
                                    }


                                }
                                break;
                            case Column.Type.Row:
                                if (column.array) {
                                    w.WriteLine($"\t\tpublic {className}[] {columnName};");
                                    readLines.Add($"\t\t\t{columnName} = d.GetArray<{className}>(r.RowArray());");
                                }
                                else {
                                    w.WriteLine($"\t\tpublic {className} {columnName};");
                                    readLines.Add($"\t\t\t{columnName} = d.Get<{className}>(r.Row());");
                                }
                                break;
                            default:
                                if (column.array) {
                                    readLines.Add($"\t\t\tr.UnknownArray();");
                                }
                                else {
                                    Console.WriteLine($"{table} COLUMN {i} {column.name} TYPE {column.type.ToString()} NOT SUPPORTED");
                                }
                                break;

                        }
                    }

                    w.WriteLine();
                    w.WriteLine("\t\tpublic override void Read(Database d, DatReader r) {");
                    for (int i = 0; i < readLines.Count; i++) {
                        w.WriteLine(readLines[i]);
                    }
                    w.WriteLine("\t\t}");
                    w.WriteLine("\t}");
                    w.WriteLine();
                }
                foreach(string enumName in enums.Keys) {
                    if (!File.Exists(Path.Combine(datFolder, enumName) + ".dat64")) continue;

                    Enumeration e = enums[enumName];
                    int rowCount;
                    using (BinaryReader r = new BinaryReader(File.OpenRead(Path.Combine(datFolder, enumName) + ".dat64"))) {
                        rowCount = r.ReadInt32();
                    }

                    string className = datClassNames[enumName];

                    w.WriteLine($"\tpublic enum {className} {{");
                    for(int i = 0; i < e.indexing; i++) {
                        w.WriteLine($"\t\tINDEXING_{i},");
                    }
                    int unkCount = 1;
                    for (int i = 0; i < rowCount; i++) {
                        if (e.values.Length > i && e.values[i] != null) {
                            w.WriteLine($"\t\t{e.values[i]},");
                        } else {
                            w.WriteLine($"\t\tUNK_{unkCount},");
                            unkCount++;
                        }
                    }
                    w.WriteLine("\t}");
                    w.WriteLine();
                }
                w.WriteLine("}");
            }
        }
    }
}
