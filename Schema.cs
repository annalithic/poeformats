using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;
using PoeFormats.Util;
using PoeFormats.Rows;

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
            public int offset;

            public int TypeSize() {
                switch (type) {
                    case Type.@bool:
                        return 1;

                    case Type.i32:
                    case Type.f32:
                    case Type.Enum:
                        return 4;

                    case Type.@string:
                    case Type.Row:
                        return 8;

                    case Type.rid:
                        return 16;

                    default: return 0;
                }
            }

            public int Size() {
                if(array) {
                    return 16;
                }
                return TypeSize();
            }

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

            public Column(string name, string columnType, string tableName, int offset = 0) {
                this.name = name;
                this.offset = offset;
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
                        type = columnType == tableName ? Type.Row : Type.rid;
                        references = columnType; break;
                }
            }
        }


        public Dictionary<string, Column[]> schema;
        public Dictionary<string, Enumeration> enums;

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
            //convert enum rids
            foreach(var table in schema.Values) {
                int offetAdjust = 0;
                for (int i = 0; i < table.Length; i++) {
                    Column col = table[i];
                    col.offset += offetAdjust;
                    if (col.type == Column.Type.rid && col.references != null && enums.ContainsKey(col.references)) {
                        col.type = Column.Type.Enum;
                        offetAdjust -= 12;
                    }
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
            GqlReader r = new GqlReader(File.ReadAllText(path));
            ParseGql(r);
        }

        void ParseGql(GqlReader r) {
            string token = r.GetNextToken();
            while(token != null) {
                if (token == "type") {
                    string table = r.GetNextToken();
                    List<Column> columns = new List<Column>();
                    List<string> attributes = new List<string>();
                    int offset = 0;

                    token = r.GetNextToken();
                    while (token != "{") {
                        attributes.Add(token);
                        token = r.GetNextToken();
                    }
                    while (token != "}") {
                        token = r.GetNextToken();
                        if (token[token.Length - 1] == ':') {
                            string column = token.Substring(0, token.Length - 1);
                            string columnType = r.GetNextToken();
                            //TODO column attributes go here
                            Column c = new Column(column, columnType, table, offset);
                            columns.Add(c);
                            offset += c.Size();
                        }
                    }
                    schema[table] = columns.ToArray();
                } else if (token == "enum") {
                    string enumName = r.GetNextToken();
                    int indexing = r.GetNextToken() == "@indexing(first: 1)" ? 1 : 0;
                    while (token != "{") {
                        token = r.GetNextToken();
                    }
                    token = r.GetNextToken();
                    List<string> enumValues = new List<string>();
                    while (token != "}") {
                        enumValues.Add(token);
                        token = r.GetNextToken();
                    }
                    enums[enumName] = new Enumeration() { indexing = indexing, values = enumValues.ToArray() };

                }
                token = r.GetNextToken();
            }
        }

        
        public static Dictionary<string, string> SplitGqlTypes(string path, Dictionary<string, string> types = null) {
            if(types == null) types = new Dictionary<string, string>();

            if (!path.EndsWith(".gql")) {
                foreach(string gqlPath in Directory.EnumerateFiles(path, "*.gql")) {
                    SplitGqlTypes(gqlPath, types);
                    
                }
                return types;
            }
            string s = File.ReadAllText(path);
            GqlReader r = new GqlReader(s);
            string token = r.GetNextToken();
            while(token != null) {
                if (token == "type" || token == "enum") {
                    int tableStart = r.wordStart;
                    string table = r.GetNextToken();
                    while (token != "}") {
                        token = r.GetNextToken();
                    }
                    int tableEnd = r.wordEnd;
                    types[table] = s.Substring(tableStart, tableEnd - tableStart);
                }
                token = r.GetNextToken();
            }
            return types;
        }
        

        public class GqlReader {
            public int wordStart;
            public int wordEnd;
            string s;
            int i;

            public GqlReader(string s) {
                this.s = s;
                i = 0;
            }


            //TODO some room for optimization
            public string GetNextToken() {
                i++;
                while(i < s.Length) {
                    if (char.IsWhiteSpace(s[i])) i++;
                    else break;
                }
                if (i == s.Length) return null;
                wordStart = i;

                bool paren = false;
                bool quote = false;
                while (i < s.Length) {
                    char c = s[i];

                    if (c == '"') quote = !quote;
                    if (paren && c == ')') 
                        paren = false;
                    else if (c == '(') 
                        paren = true;
                    if (char.IsWhiteSpace(c) && !paren && !quote) break;
                    i++;
                }
                wordEnd = i;
                return s.Substring(wordStart, wordEnd - wordStart);
            }
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
