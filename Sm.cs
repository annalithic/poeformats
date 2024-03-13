using PoeFormats.Util;
using System.IO;

namespace PoeFormats {
    public class Sm {
        public int version;
        public string smd;
        public string[] materials;
        public int[] materialCounts;
        public float[] bbox;

        public Sm(string gamePath, string path) : this(Path.Combine(gamePath, path)) { }

        public Sm(string path) {
            using(TextReader r = new StreamReader(File.OpenRead(path), System.Text.Encoding.Unicode)) {
                
                version = r.ReadValueInt("version");
                smd = r.ReadValueString("SkinnedMeshData");

                materials = new string[r.ReadValueInt("Materials")];
                materialCounts = new int[materials.Length];
                for(int i = 0; i < materials.Length; i++) {
                    var words = r.ReadLine().Trim().SplitQuotes();
                    materials[i] = words[0].Trim('"');
                    materialCounts[i] = int.Parse(words[1]);
                }
                bbox = r.ReadValueBbox("BoundingBox");
            }
        }
    }
}
