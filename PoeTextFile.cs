using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PoeFormats {
    public class PoeTextFile {


        public string path;
        int version;
        bool isAbstract;

        TextReader reader;

        public List<string> parents;

        public static HashSet<string> keywords = new HashSet<string> {
            "version",
            "extends",

            //act
            "animated_object",
            "movement_speed",
            "action_set"
        };


        //this doesnt work because some stuff needs to be in order - melee then, animation timings
        public class Block {
            public List<string> keys;
            public List<string> values;

            public Block() {
                keys = new List<string>();
                values = new List<string>();
            }

            public void Add(string key, string value) {
                keys.Add(key); values.Add(value);
            }
        }
        Dictionary<string, Block> blocks;

        public List<(string, string)> AocGetSockets() {
            List<(string, string)> sockets = new List<(string, string)>();
            if (blocks.TryGetValue("ClientAnimationController", out Block block)) {
                string socket = null;
                for(int i = 0; i < block.keys.Count; i++) {
                    string key = block.keys[i];
                    if (key == "socket") socket = block.values[i];
                    else if (key == "parent") sockets.Add( (socket, block.values[i]) );
                }
            }
            return sockets;
        }
        
        public List<string> GetList(string block, string key) {
            List<string> list = new List<string>();
            if(blocks.ContainsKey(block)) {
                var blockObj = blocks[block];
                for (int i = 0; i < blockObj.keys.Count; i++)
                    if (blockObj.keys[i] == key)
                        list.Add(blockObj.values[i]);
            }
            return list;
        }

        public bool TryGet(string block, string key, out string value) {
            if (blocks.ContainsKey(block)) {
                int i = blocks[block].keys.LastIndexOf(key);
                if(i != -1) {
                    value = blocks[block].values[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        public Block GetBlock(string block)  {
            if (blocks.ContainsKey(block)) return blocks[block];
            return new Block();
        }

        //lastindexof so children override parents
        public string Get(string block, string value) {
            if (!blocks.ContainsKey(block)) return null;
            int i = blocks[block].keys.LastIndexOf(value);
            if (i == -1) return null;
            return blocks[block].values[i];
        }


        public string GetFirst(string block, string value) {
            if (!blocks.ContainsKey(block)) return null;
            int i = blocks[block].keys.IndexOf(value);
            if (i == -1) return null;
            return blocks[block].values[i];
        }

        public string Get(string value) {
            return Get("NULL", value);
        }

        public static void DumpTokens(string path) {
            TextReader reader = new StreamReader(path);
            string token = GetNextToken(reader);
            while (token is not null) {
                Console.WriteLine(token);
                token = GetNextToken(reader);
            }
        }

        public PoeTextFile(string baseFolder, string path) {

            this.path = path;
            blocks = new Dictionary<string, Block>();
            blocks["NULL"] = new Block();

            ReadFile(baseFolder, path);
        }


        void ReadFile(string baseFolder, string path) {
            if (!File.Exists(Path.Combine(baseFolder, path))) {
                //Console.WriteLine(path + " DOES NOT EXIST");
                return;
            }

            string currentBlock = "NULL";

            TextReader reader = new StreamReader(File.OpenRead(Path.Combine(baseFolder, path)));
            //Console.WriteLine(path);

            string token = GetNextToken(reader);
            string prevToken = token;
            while (token != null) {

                //Console.WriteLine(prevToken + ", " + token);


                if (keywords.Contains(token)) {
                    string value = GetNextToken(reader);
                    blocks[currentBlock].Add(token, value);
                    //read parents immediately, so values are overwritten?
                    if (token == "extends" && value != "nothing") {
                        ReadFile(baseFolder, value + Path.GetExtension(path));
                    }
                } else if (token == "=") {
                    string value = GetNextToken(reader);
                    blocks[currentBlock].Add(prevToken, value);
                } else if (token == "{") {
                    currentBlock = prevToken;
                    if (!blocks.ContainsKey(prevToken)) blocks[prevToken] = new Block();
                } else if (token == "}") {
                    currentBlock = "NULL";
                }



                prevToken = token;
                token = GetNextToken(reader);
            }
            reader.Close();


        }

        static string GetNextToken(TextReader reader) {
            StringBuilder s = new StringBuilder();

            while (char.IsWhiteSpace((char)reader.Peek())) reader.Read(); //start whitespace
            if (reader.Peek() == -1) return null;
            int c = reader.Read();

            //single line comment
            if (c == '/' && ((char)reader.Peek()) == '/') {
                reader.ReadLine();
                //Console.WriteLine("COMMENT " + reader.ReadLine());
                return GetNextToken(reader);
            }

            //string
            if (c == '"') {
                s.Append((char)c);
                do {
                    c = reader.Read();
                    s.Append((char)c);
                } while (c != '"' && c != '\r'); //no multiline strings
                return s.ToString().Trim('"');
            }


            //regular token
            do {
                s.Append((char)c);
                c = reader.Read();
            } while (!char.IsWhiteSpace((char)c) && c != -1);
            return s.ToString();
        }
    }
}