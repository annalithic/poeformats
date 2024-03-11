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
        public Dictionary<string, string> values;
        public Dictionary<string, Dictionary<string, string>> blocks; 

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
            values = new Dictionary<string, string>();
            blocks = new Dictionary<string, Dictionary<string, string>>();

            ReadFile(baseFolder, path);



            /*

            while(reader.Peek() != -1) {
                string token = GetNextToken();

                if (token.StartsWith("//")) tokens.Clear(); // single line comment

                if(state == State.Toplevel) {
                    if(token == "version") {
                        version = int.Parse(GetNextToken());
                        if (version != 2) Console.WriteLine($"VERSION {version}");
                    } else if (token == "extends") {
                        parents.Add(GetNextToken());
                    }

                }

            }
            */





        }


        void ReadFile(string baseFolder, string path) {
            if (!File.Exists(Path.Combine(baseFolder, path))) {
                //Console.WriteLine(path + " DOES NOT EXIST");
                return;
            }

            string currentBlock = null;

            TextReader reader = new StreamReader(File.OpenRead(Path.Combine(baseFolder, path)));
            //Console.WriteLine(path);

            string token = GetNextToken(reader);
            string prevToken = token;
            while (token != null) {

                //Console.WriteLine(prevToken + ", " + token);


                if (keywords.Contains(token)) {
                    string value = GetNextToken(reader);
                    if (currentBlock == null)
                        values[token] = value;
                    else
                        blocks[currentBlock][token] = value;
                    //read parents immediately, so values are overwritten?
                    if (token == "extends" && value != "nothing") {
                        ReadFile(baseFolder, value + Path.GetExtension(path));
                    }
                } else if (token == "=") {
                    string value = GetNextToken(reader);
                    if (currentBlock == null) {
                        //Console.WriteLine($"VALUE {prevToken} = {value}");
                        values[prevToken] = value;

                    } else {
                        //Console.WriteLine($"VALUE {currentBlock}.{prevToken} = {value}");
                        blocks[currentBlock][prevToken] = value;
                    }
                } else if (token == "{") {
                    currentBlock = prevToken;
                    if (!blocks.ContainsKey(prevToken)) blocks[prevToken] = new Dictionary<string, string>();
                } else if (token == "}") {
                    currentBlock = null;
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