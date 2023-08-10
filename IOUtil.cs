using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PoeTerrain.Util {
    public static class BinaryReaderEx {
        public static BBox ReadBBox(this BinaryReader r) {
            return new BBox() { x1 = r.ReadSingle(), x2 = r.ReadSingle(), y1 = r.ReadSingle(), y2 = r.ReadSingle(), z1 = r.ReadSingle(), z2 = r.ReadSingle() };
        }
    }

    public static class TextReaderEx {
        public static int ReadLineInt(this TextReader r) {
            return int.Parse(r.ReadLine());
        }

        public static int ReadValueInt(this TextReader r) {
            string line = r.ReadLine();
            return int.Parse(line.Substring(line.LastIndexOf(' ') + 1));
        }

        public static int ReadValueInt(this TextReader r, string assertVal) {
            string[] words = r.ReadLine().Trim().Split();
            if (words[0] != assertVal) 
                Console.WriteLine($"ASSERTION FAILED [{words[0]}] != [{assertVal}]");
            return int.Parse(words[words.Length - 1]);
        }

        public static float[] ReadValueBbox(this TextReader r, string assertVal) {
            var words = r.ReadLine().Trim().Split();
            if (words[0] != assertVal) Console.WriteLine($"ASSERTION FAILED {words[0]} != {assertVal}");
            float[] bbox = new float[6];
            for (int i = 0; i < bbox.Length; i++) bbox[i] = float.Parse(words[i + 1]);
            return bbox;
        }

        public static string ReadLineString(this TextReader r) {
            return r.ReadLine().Trim('\"');
        }

        public static string ReadValueString(this TextReader r) {
            string line = r.ReadLine();
            return line.Substring(line.IndexOf(' ') + 1).Trim('\"');
        }

        public static string ReadValueString(this TextReader r, string assertVal) {
            var words = r.ReadLine().SplitQuotes();
            if (words[0] != assertVal) Console.WriteLine($"ASSERTION FAILED {words[0]} != {assertVal}");
            return words[words.Length - 1].Trim('"');
        }

        public static void ReadLineInt(this TextReader r, out int a, out int b) {
            string[] words = r.ReadLine().Split(' ');
            a = int.Parse(words[0]); b = int.Parse(words[1]);
        }

        public static void ReadValueInt(this TextReader r, out int a, out int b) {
            string[] words = r.ReadLine().Split(' ');
            a = int.Parse(words[words.Length - 2]); b = int.Parse(words[words.Length - 1]);
        }



        public static string[] SplitQuotes(this string s) {
            if(s.Length == 0) return new string[0];

            bool inQuotes = false;
            bool inWord = true;
            List<string> words = new List<string>();
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < s.Length; i++) {
                if (char.IsWhiteSpace(s[i]) && !inQuotes) {
                    if (inWord) {
                        inWord = false;
                        words.Add(builder.ToString());
                        builder.Clear();
                    } else continue;
                } else if (s[i] == '"') inQuotes = !inQuotes;
                else {
                    inWord = true;
                    builder.Append(s[i]);
                }
            }
            if (inWord) words.Add(builder.ToString());
            return words.ToArray();

                /*
            List<string> newWords = new List<string>();
            string[] words = s.Split(' ');
            for(int i = 0; i < words.Length; i++) {
                if (words[i][0] == '"' && words[i][words[i].Length - 1] != '"') {
                    string combined = words[i];

                    do {
                        i++;
                        combined += words[i];
                    } while (words[i][words[i].Length - 1] != '"');
                    newWords.Add(combined);
                } else newWords.Add(words[i]);
            }
            return newWords.ToArray();
                */
        }
    }
    public class WordReader {

        int pos;
        string[] words;

        public WordReader(string line, int start = 0) { words = line.Split(' '); pos = start; }

        public WordReader(string[] words, int start = 0) { this.words = words; pos = start; }

        public void Read(string line, int start = 0) { pos = start; words = line.Split(' '); }

        public void Skip(int count = 1) { pos += count; }

        public int ReadInt() {
            if (pos >= words.Length) return int.MinValue;
            pos++;
            return int.Parse(words[pos - 1]);
        }

        public float ReadFloat() {
            if (pos >= words.Length) return float.MinValue;
            pos++;
            return float.Parse(words[pos - 1]);
        }

        public string ReadString(bool inQuotes = true) {
            if (pos >= words.Length) return "";
            pos++;
            return words[pos - 1].Trim('\"');
        }

        public char ReadChar() {
            if (pos >= words.Length) return '0';
            pos++;
            return words[pos-1][0];
        }

        public int[] ReadIntArray() {
            int[] array = new int[ReadInt()];
            for (int i = 0; i < array.Length; i++) array[i] = ReadInt();
            return array;
        }

        public string[] ReadStringArray(bool inQuotes = true) {
            string[] array = new string[ReadInt()];
            for (int i = 0; i < array.Length; i++) array[i] = ReadString(inQuotes);
            return array;
        }

    }
}
