using PoeTerrain.Util;
using System;

namespace PoeTerrain {
    public class Aoc {
        public int version;
        public string skeleton;
        public string skin;

        public Aoc(string path) {
            using(TextReader reader = new StreamReader(path)) {
                version = reader.ReadValueInt("version");
                string line = reader.ReadLine();
                while (line != "ClientAnimationController") {
                    line = reader.ReadLine();
                    if (line == null) return;
                }
                if (reader.ReadLine() != "{") Console.WriteLine("?????????????????? no ClientAnimationController block start");
                while(line != "}") {
                    line = reader.ReadLine();
                    if(line.Trim().StartsWith("skeleton = ")) {
                        var words = line.SplitQuotes();
                        skeleton = words[words.Length - 1].Trim('"');
                    }
                }
                while (line != "SkinMesh") {
                    line = reader.ReadLine();
                    if (line == null) return;
                }
                if (reader.ReadLine() != "{") Console.WriteLine("?????????????????? no SkinMesh block start");
                while (line != "}") {
                    line = reader.ReadLine();
                    if (line.Trim().StartsWith("skin = ")) {
                        var words = line.SplitQuotes();
                        skin = words[words.Length - 1].Trim('"');
                    }
                }
            }
        }
    }
}
