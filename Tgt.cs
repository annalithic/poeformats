using PoeTerrain.Util;
using System.Diagnostics.Metrics;
using System.IO;

namespace PoeTerrain {
    public class Tgt {
        public string filename;
        string basePath;
        public int version;
        public int sizeX, sizeY;
        public string tileMeshRoot;
        public string groundMask;
        public string[] materials;
        public int[][] subtileMaterialIndices;

        public Tgt(string path) {
            filename = Path.GetFileNameWithoutExtension(path);
            basePath = path.Contains("\\Art\\") ? path.Substring(0, path.IndexOf("\\Art\\") + 1) : path.Substring(0, path.IndexOf("\\art\\") + 1);

            using (TextReader r = new StreamReader(File.OpenRead(path), System.Text.Encoding.Unicode)) {
                version = r.ReadValueInt();
                r.ReadValueInt(out sizeX, out sizeY);
                tileMeshRoot = r.ReadValueString();
                groundMask = r.ReadValueString();
                materials = new string[r.ReadValueInt()];
                for(int i = 0; i < materials.Length; i++) {
                    materials[i] = r.ReadLine().Trim('\t', '"');
                }
                r.ReadLine();
                subtileMaterialIndices = new int[sizeX * sizeY][];
                for(int i = 0; i < sizeX * sizeY; i++) {
                    WordReader wr = new WordReader(r.ReadLine().Trim('\t'));
                    subtileMaterialIndices[i] = new int[wr.ReadInt() * 2];
                    for (int w = 0; w < subtileMaterialIndices[i].Length; w++) subtileMaterialIndices[i][w] = wr.ReadInt();
                }
            }
        }

        string GetTgmPath(int x, int y) {
            return $"{basePath}{tileMeshRoot.ToLower()}_c{x + 1}r{y + 1}.tgm";
        }

        public string[] GetSubtileObjNames(int x, int y) {
            List<string> names = new List<string>();
            int[] indices = subtileMaterialIndices[x + (sizeY - y - 1) * sizeX];
            //Console.WriteLine($"c{x + 1} r {y + 1}");
            for (int i = 0; i < indices.Length; i += 2) {
                for (int j = 0; j < indices[i + 1]; j++) {
                    names.Add(Path.GetFileNameWithoutExtension(materials[indices[i]]));
                }
            }
            return names.ToArray();
        }

        public void ToObj() {
            Random random = new Random(filename.GetHashCode()); 
            using(TextWriter writer = new StreamWriter(filename + ".mtl")) {
                writer.WriteLine("newmtl annalithicground");
                writer.WriteLine($"Kd 0.8 0.8 0.8");

                for (int i = 0; i < materials.Length; i++) {
                    writer.WriteLine("newmtl " + Path.GetFileNameWithoutExtension(materials[i]));
                    writer.WriteLine($"Kd {random.NextSingle()} {random.NextSingle()} {random.NextSingle()}");
                }
            }


            for(int y = 0; y < sizeY; y++) {
                for (int x = 0; x < sizeX; x++) {
                    Tgm tgm = new Tgm(GetTgmPath(x, y));
                    var names = GetSubtileObjNames(x, y);
                    Console.WriteLine($"({x},{y}) {names.Length} - {tgm.model.submeshCount}");
                    tgm.ToObj(names, x, y, filename + ".mtl");
                }
            }
        }

        public void PrintSubtileMaterials(int x, int y) {
            int counter = 0;
            int[] indices = subtileMaterialIndices[x + y * sizeX];
            Console.WriteLine($"c{x+1}r{y+1}");
            for(int i = 0; i < indices.Length; i += 2) {
                for(int j = 0; j < indices[i + 1]; j++) {
                    Console.WriteLine($"{counter}  {materials[indices[i]]}");
                    counter++;
                }
            }
        }
    }
}
