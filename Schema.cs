using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Text;

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
                @bool,
                i32,
                f32,
                @string,
                rid,
                Row,
                Enum,
                Unknown
            }
            public Type type;
            public string references;
            public bool isEnum;

            public override string ToString() {
                if(references != null) {
                    return array ? $"{name}: [{references}]" : $"{name}: {references}";
                } else {
                    return array ? $"{name}: [{type}]" : $"{name}: {type}";
                }
            }

            public Column(JObject o, ref int unkCount) {
                name = o["name"].Value<string>();
                if (name == null) {
                    unkCount++;
                    name = $"unk{unkCount}";
                }
                array = o["array"].Value<bool>();
                switch (o["type"].Value<string>()) {
                    case "bool":
                        type = Type.@bool; break;
                    case "i32":
                        type = Type.i32; break;
                    case "f32":
                        type = Type.f32; break;
                    case "string":
                        type = Type.@string; break;
                    case "enumrow":
                        type = Type.Enum;
                        if (o["references"] != null && o["references"].HasValues) {
                            references = o["references"]["table"].Value<string>();
                        }
                        break;
                    case "foreignrow":
                        type = Type.rid;
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

            public Column(string name, string columnType) {
                this.name = name;
                if (columnType.StartsWith('[')) {
                    array = true;
                    columnType = columnType.Substring(1, columnType.Length - 2);
                }
                switch (columnType) {
                    case "bool":
                        type = Type.@bool; break;
                    case "i32":
                        type = Type.i32; break;
                    case "f32":
                        type = Type.f32; break;
                    case "string":
                        type = Type.@string; break;
                    case "rid":
                        type = Type.rid; break;
                    default:
                        type = Type.rid;
                        references = columnType; break;

                }
            }
        }


        Dictionary<string, Column[]> schema;
        Dictionary<string, Enumeration> enums;

        public Schema(string schemaPath) {
            schema = new Dictionary<string, Column[]>();
            enums = new Dictionary<string, Enumeration>();
            if (Directory.Exists(schemaPath)) 
                foreach(string path in Directory.EnumerateFiles(schemaPath, "*.gql"))
                    ParseGql(path);
            else if (File.Exists(schemaPath)) {
                string ext = Path.GetExtension(schemaPath);
                if (ext == ".json") {
                    ParseJson(schemaPath);
                }
                else if (ext == ".gql") {
                    ParseGql(schemaPath);
                }

            }

        }

        void ParseJson(string path) {
            using (StreamReader reader = File.OpenText(path)) {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                foreach (JObject table in o["tables"].Value<JArray>()) {
                    JArray c = (JArray)table["columns"];
                    Column[] columns = new Column[c.Count];
                    int unkCount = 0;
                    for (int i = 0; i < columns.Length; i++) {
                        columns[i] = new Column((JObject)c[i], ref unkCount);
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
        }

        void ParseGql(string path) {
            using (TextReader r = new StreamReader(File.OpenRead(path))) {
                ParseGql(r);
            }
        }

        void ParseGql(TextReader r) {
            string token = GetNextToken(r);
            while(token != null) {
                if (token == "type") {
                    string table = GetNextToken(r);
                    List<Column> columns = new List<Column>();
                    List<string> attributes = new List<string>();
                    token = GetNextToken(r);
                    while (token != "{") {
                        attributes.Add(token);
                        token = GetNextToken(r);
                    }
                    while (token != "}") {
                        token = GetNextToken(r);
                        if (token[token.Length - 1] == ':') {
                            string column = token.Substring(0, token.Length - 1);
                            string columnType = GetNextToken(r);
                            //TODO column attributes go here
                            columns.Add(new Column(column, columnType));
                        }
                    }
                    schema[table] = columns.ToArray();
                } else if (token == "enum") {
                    string enumName = GetNextToken(r);
                    int indexing = GetNextToken(r) == "@indexing(first: 1)" ? 1 : 0;
                    while (token != "{") {
                        token = GetNextToken(r);
                    }
                    token = GetNextToken(r);
                    List<string> enumValues = new List<string>();
                    while (token != "}") {
                        enumValues.Add(token);
                        token = GetNextToken(r);
                    }
                    enums[enumName] = new Enumeration() { indexing = indexing, values = enumValues.ToArray() };

                }
                token = GetNextToken(r);
            }
        }

        //TODO some room for optimization
        string GetNextToken(TextReader r) {
            StringBuilder s = new StringBuilder();
            bool paren = false;
            int c = r.Read();
            while(char.IsWhiteSpace((char)c)) {
                c = r.Read();
            }
            if (c == -1) return null;
            do {
                s.Append((char)c);
                c = r.Read();
                if (paren && c == ')') paren = false;
                else if (c == '(') paren = true;
            } while (c != -1 && (!char.IsWhiteSpace((char)c) || paren));
            return s.ToString();
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

        public void GenerateGetAllMethod(string datFolder) {
            var datClassNames = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\PathOfExile\datclassnames.txt")) {
                string[] words = line.Split('\t');
                datClassNames[words[0]] = words[1];
            }
            foreach(string path in Directory.EnumerateFiles(datFolder, "*.dat64")) {
                string datName = Path.GetFileNameWithoutExtension(path);
                if (!schema.ContainsKey(datName)) continue;
                Console.WriteLine($"GetAll<Rows.{datClassNames[datName]}>();");
            }
        }

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
                            case Column.Type.@bool:
                                WriteVariable(columnName, "Bool",   w, readLines); break;
                            case Column.Type.i32:
                                WriteVariable(columnName, "Int",    w, readLines, column.array); break;
                            case Column.Type.f32:
                                WriteVariable(columnName, "Float",  w, readLines, column.array); break;
                            case Column.Type.@string:
                                WriteVariable(columnName, "String", w, readLines, column.array); break;
                            case Column.Type.rid:
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
