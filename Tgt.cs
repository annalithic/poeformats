﻿using PoeFormats.Util;
using System;
using System.IO;
using System.Collections.Generic;

namespace PoeFormats {
    public class Tgt {
        public string filename;
        string basePath;
        public int version;
        public int sizeX, sizeY;
        public string tileMeshRoot;
        public string groundMask;
        public string[] materials;
        public int[][] subtileMaterialIndices;



        public Tgt(string gamePath, string path) {
            filename = Path.GetFileNameWithoutExtension(path);
            basePath = gamePath;
            //basePath = path.Contains("\\Art\\") ? path.Substring(0, path.IndexOf("\\Art\\") + 1) : path.Substring(0, path.IndexOf("\\art\\") + 1);

            using (TextReader r = new StreamReader(File.OpenRead(Path.Combine(basePath, path)), System.Text.Encoding.Unicode)) {
                version = r.ReadValueInt();
                string line = r.ReadLine();
                if (line.StartsWith("SourceScene")) line = r.ReadLine();
                TextReaderEx.ReadValueInt(line, out sizeX, out sizeY);
                tileMeshRoot = r.ReadValueString();
                if(r.Peek() == 'G')
                    groundMask = r.ReadValueString();
                materials = new string[r.ReadValueInt()];
                for(int i = 0; i < materials.Length; i++) {
                    materials[i] = r.ReadLine().Trim('\t', '"');
                }
                if(materials.Length > 0) {
                    r.ReadLine();
                    subtileMaterialIndices = new int[sizeX * sizeY][];
                    for (int i = 0; i < sizeX * sizeY; i++) {
                        string[] words = r.ReadLine().Trim('\t').Split();
                        int count = int.Parse(words[0]);
                        int[] indices = new int[count * 2];
                        if(words.Length == count * 3 + 1) {
                            for(int idx = 0; idx < count; idx++) {
                                indices[idx * 2] = int.Parse(words[idx * 3 + 2]);
                                indices[idx * 2 + 1] = int.Parse(words[idx * 3 + 3]);
                            }
                        } else {
                            for (int idx = 0; idx < indices.Length; idx++) {
                                indices[idx] = int.Parse(words[idx + 1]);
                            }
                        }
                        subtileMaterialIndices[i] = indices;
                        //WordReader wr = new WordReader(r.ReadLine().Trim('\t'));

                        //subtileMaterialIndices[i] = new int[wr.ReadInt() * 2];
                        //for (int w = 0; w < subtileMaterialIndices[i].Length; w++) subtileMaterialIndices[i][w] = wr.ReadInt();
                    }
                }
            }
        }

        public Tgm GetTgm(int x, int y) {
            return new Tgm(GetTgmPath(x, y));
        }

        public string GetTgmPath(int x, int y) {
            if (sizeX == 1 && sizeY == 1) return Path.Combine(basePath, tileMeshRoot) + ".tgm";
            return Path.Combine(basePath, tileMeshRoot) + $"_c{x + 1}r{y + 1}.tgm";
        }

        public string[] GetSubtileMaterials(int x, int y) {
            List<string> names = new List<string>();
            int[] indices = subtileMaterialIndices[x + (sizeY - y - 1) * sizeX];
            //Console.WriteLine($"c{x + 1} r {y + 1}");
            for (int i = 0; i < indices.Length; i += 2) {
                for (int j = 0; j < indices[i + 1]; j++) {
                    names.Add(materials[indices[i]]);
                }
            }
            return names.ToArray();
        }

        public string[] GetSubtileMaterialsCombined(int x, int y) {
            List<string> names = new List<string>();
            int[] indices = subtileMaterialIndices[x + (sizeY - y - 1) * sizeX];
            string[] materialNames = new string[indices.Length / 2];

            for (int i = 0; i < materialNames.Length; i ++) {
                materialNames[i] = materials[indices[i * 2]];
            }
            return materialNames;
        }

        public int[] GetCombinedShapeLengths(int x, int y) {
            if(subtileMaterialIndices == null) return new int[] {0};
            int[] indices = subtileMaterialIndices[x + (sizeY - y - 1) * sizeX];
            int[] lengths = new int[indices.Length / 2];
            for(int i = 0; i < lengths.Length; i++) {
                lengths[i] = indices[i * 2 + 1];
            }
            return lengths;
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
                    writer.WriteLine($"Kd {random.NextDouble()} {random.NextDouble()} {random.NextDouble()}");
                }
            }


            for(int y = 0; y < sizeY; y++) {
                for (int x = 0; x < sizeX; x++) {
                    Tgm tgm = new Tgm(GetTgmPath(x, y));
                    var names = GetSubtileObjNames(x, y);
                    Console.WriteLine($"({x},{y}) {names.Length} - {tgm.model.shapeCount}");
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
