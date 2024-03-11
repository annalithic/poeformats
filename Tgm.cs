using PoeFormats.Util;
using System;
using System.IO;

namespace PoeFormats {
    



    public struct TgmModelExtraData {
        public short i;
        public int format58unk;
        public BBox bbox;

        public TgmModelExtraData(BinaryReader r, int version, bool includeUnk) {
            i = r.ReadInt16();
            format58unk = 0;
            if (includeUnk) {
                if (version >= 13) format58unk = r.ReadInt32();
                else if (version >= 10) format58unk = r.ReadUInt16();
            }
            bbox = r.ReadBBox();
        }
    }


    public class Tgm {
        string filename;


        public byte version;
        public BBox bBox;
        public short unk1;
        public short unk2;
        public short unk3;
        public PoeModel model;
        public TgmModelExtraData[] modelExtraData;
        public PoeModel groundModel;
        public TgmModelExtraData[] groundExtraData;

        int col = 0;
        int row = 0;

        public Tgm(string path) {
            filename = Path.GetFileNameWithoutExtension(path);
            
            /*
            string possibleColRow = filename.Substring(filename.LastIndexOf('_') + 1);
            if (possibleColRow.StartsWith('c')) {
                var colrow = possibleColRow.Split('r');
                if(colrow.Length == 2) {
                    col = int.Parse(colrow[0].Substring(1));
                    row = int.Parse(colrow[1]);
                    Console.WriteLine($" {col} {row}");
                }
            }
            */

            using(BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                version = r.ReadByte();

                if(version < 9) {
                    Console.WriteLine($"OLD TGM VERSION {version} NOT SUPPORTED LOL");
                    return;
                }

                bBox = r.ReadBBox();
                unk1 = r.ReadInt16();
                unk2 = r.ReadInt16();
                unk3 = r.ReadInt16();
                model = new PoeModel(r);

                modelExtraData = new TgmModelExtraData[model.meshCount];
                for (int i = 0; i < model.meshCount; i++) {
                    modelExtraData[i] = new TgmModelExtraData(r, version, true);
                }

                groundModel = new PoeModel(r);

                groundExtraData = new TgmModelExtraData[model.meshCount];
                for (int i = 0; i < model.meshCount; i++) {
                    groundExtraData[i] = new TgmModelExtraData(r, version, false);
                }
            }
        }

        public void ToObj() {
            int vertCount = 1;
            using (TextWriter w = new StreamWriter(File.Open(filename + ".obj", FileMode.Create))) {
                for(int i = 0; i < model.meshes.Length; i++) {
                    
                    w.WriteLine($"o {filename}_{i}");
                    for(int vert = 0; vert < model.meshes[i].verts.Length; vert += 3) {
                        w.WriteLine($"v {model.meshes[i].verts[vert] / 100 + 2.5 * col} {model.meshes[i].verts[vert + 2] / -100} {model.meshes[i].verts[vert + 1] / 100 -2.5 * row}");
                    }
                    for(int idx = 0; idx < model.meshes[i].idx.Length; idx += 3) {
                        w.WriteLine($"f {model.meshes[i].idx[idx] + vertCount} {model.meshes[i].idx[idx + 1] + vertCount} {model.meshes[i].idx[idx + 2] + vertCount}");
                    }
                    vertCount += model.meshes[i].vertCount;
                }
                for (int i = 0; i < groundModel.meshes.Length; i++) {
                    w.WriteLine($"o {filename}_ground_{i}");
                    for (int vert = 0; vert < groundModel.meshes[i].verts.Length; vert += 3) {
                        w.WriteLine($"v {groundModel.meshes[i].verts[vert] / 100 + 2.5 * col}   {groundModel.meshes[i].verts[vert + 2] / -100}   {groundModel.meshes[i].verts[vert + 1] / 100 - 2.5 * row}");
                    }
                    for (int idx = 0; idx < groundModel.meshes[i].idx.Length; idx += 3) {
                        w.WriteLine($"f {groundModel.meshes[i].idx[idx] + vertCount} {groundModel.meshes[i].idx[idx + 1] + vertCount} {groundModel.meshes[i].idx[idx + 2] + vertCount}");
                    }
                    vertCount += groundModel.meshes[i].vertCount;
                }
            }
        }

        public void ToObj(string[] submeshNames, int x, int y, string mtlName) {
            int vertCount = 1;
            using (TextWriter w = new StreamWriter(File.Open(filename + ".obj", FileMode.Create))) {
                w.WriteLine("mtllib " + mtlName);
                
                for (int i = 0; i < model.meshes.Length; i++) {

                    for (int vert = 0; vert < model.meshes[i].verts.Length; vert += 3) {
                        w.WriteLine($"v {model.meshes[i].verts[vert]  / 100 + 2.5 * x} {model.meshes[i].verts[vert + 2] / -100} {model.meshes[i].verts[vert + 1] / 100 - 2.5 * y}");
                    }

                    string name = submeshNames[0];
                    w.WriteLine($"o {filename}_{name}");
                    w.WriteLine("usemtl " + name);
                    for (int submesh = 0; submesh < model.meshes[i].shapeOffsets.Length; submesh++) {
                        if(submeshNames[submesh] != name) {
                            name = submeshNames[submesh];
                            w.WriteLine($"o {filename}_{name}");
                            w.WriteLine("usemtl " + name);
                        }
                        int offset = model.meshes[i].shapeOffsets[submesh];
                        for (int idx = 0; idx < model.meshes[i].shapeLengths[submesh]; idx += 3) {
                            w.WriteLine($"f {model.meshes[i].idx[idx + offset] + vertCount} {model.meshes[i].idx[idx + offset + 1] + vertCount} {model.meshes[i].idx[idx + offset + 2] + vertCount}");
                        }
                    }
                    vertCount += model.meshes[i].vertCount;
                    break; //we don't need lods
                }
                
                
                for (int i = 0; i < groundModel.meshes.Length; i++) {
                    w.WriteLine($"o {filename}_ground_{i}");
                    w.WriteLine("usemtl annalithicground");
                    for (int vert = 0; vert < groundModel.meshes[i].verts.Length; vert += 3) {
                        w.WriteLine($"v {groundModel.meshes[i].verts[vert] / 100 + 2.5 * x} {groundModel.meshes[i].verts[vert + 2] / -100} {groundModel.meshes[i].verts[vert + 1] / 100 - 2.5 * y}");
                    }
                    for (int idx = 0; idx < groundModel.meshes[i].idx.Length; idx += 3) {
                        w.WriteLine($"f {groundModel.meshes[i].idx[idx] + vertCount} {groundModel.meshes[i].idx[idx + 1] + vertCount} {groundModel.meshes[i].idx[idx + 2] + vertCount}");
                    }
                    vertCount += groundModel.meshes[i].vertCount;
                }
                
            }
        }

    }
}
