using PoeTerrain.Util;
using System.IO;

namespace PoeTerrain {
    
    public struct BBox {
        public float x1, x2, y1, y2, z1, z2;
    }

    public struct PoeVert {
        public float x, y, z;

        static Dictionary<int, int> tgmVertEndSizes = new Dictionary<int, int>() { { 48, 8 }, { 56, 12 }, { 58, 16 } };

        public PoeVert(BinaryReader r, int format) {
            x = r.ReadSingle(); 
            y = r.ReadSingle(); 
            z = r.ReadSingle(); 
            r.BaseStream.Seek(tgmVertEndSizes[format], SeekOrigin.Current);
        }
    }

    public struct TgmFSData {
        public short i;
        public int format58unk;
        public BBox bbox;

        public TgmFSData(BinaryReader r, int version, bool includeUnk) {
            i = r.ReadInt16();
            format58unk = 0;
            if (includeUnk) {
                if (version >= 13) format58unk = r.ReadInt32();
                else if (version >= 10) format58unk = r.ReadUInt16();
            }
            bbox = r.ReadBBox();
        }
    }

    public class PoeMesh {
        public PoeVert[] verts;
        public int[] idx;
        public int[] fsOffsets;
        public int[] fsSizes;


        public PoeMesh(int triCount, int vertCount, int fsCount) {
            idx = new int[triCount * 3];
            verts = new PoeVert[vertCount];
            fsOffsets = new int[fsCount];
            fsSizes = new int[fsCount];
        }

        public void Read(BinaryReader r, int format) {

            //face sets
            for(int i = 0; i < fsOffsets.Length; i++ ) {
                fsOffsets[i] = r.ReadInt32();
                fsSizes[i] = r.ReadInt32();
            }

            //idx
            if(verts.Length > 65535) {
                for (int i = 0; i < idx.Length; i++) {
                    idx[i] = r.ReadInt32();
                }
            } else {
                for (int i = 0; i < idx.Length; i++) {
                    idx[i] = r.ReadUInt16();
                }
            }

            //verts
            for(int i = 0; i <  verts.Length; i++ ) {
                verts[i] = new PoeVert(r, format);
            }
        }
    }


    public class PoeModel {
        public short unk1;
        public PoeMesh[] meshes;
        public int format;
        public TgmFSData[] faceSets;


        public PoeModel(BinaryReader r, int version, bool useFaceSetData = false, bool ground = false) {
            string magic = new string(r.ReadChars(4));
            if (magic != "DOLm") Console.WriteLine("MODEL MAGIC IS WRONG - " + magic);
            unk1 = r.ReadInt16();
            meshes = new PoeMesh[r.ReadByte()];
            faceSets = new TgmFSData[r.ReadUInt16()];
            format = r.ReadInt32();
            for(int i = 0; i < meshes.Length; i++) {
                meshes[i] = new PoeMesh(r.ReadInt32(), r.ReadInt32(), faceSets.Length);
            }
            for (int i = 0; i < meshes.Length; i++) {
                meshes[i].Read(r, format);
            }
            for (int i = 0; i < faceSets.Length; i++) {
                faceSets[i] = new TgmFSData(r, version, !ground);
            }
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
        public PoeModel groundModel;

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
                model = new PoeModel(r, version, true, false);
                groundModel = new PoeModel(r, version, true, true);
            }
        }

        public void ToObj() {
            int vertCount = 1;
            using (TextWriter w = new StreamWriter(File.Open(filename + ".obj", FileMode.Create))) {
                for(int i = 0; i < model.meshes.Length; i++) {
                    
                    w.WriteLine($"o {filename}_{i}");
                    for(int vert = 0; vert < model.meshes[i].verts.Length; vert++) {
                        w.WriteLine($"v {model.meshes[i].verts[vert].x / 100 + 2.5 * col} {model.meshes[i].verts[vert].z / -100} {model.meshes[i].verts[vert].y / 100 -2.5 * row}");
                    }
                    for(int idx = 0; idx < model.meshes[i].idx.Length; idx += 3) {
                        w.WriteLine($"f {model.meshes[i].idx[idx] + vertCount} {model.meshes[i].idx[idx + 1] + vertCount} {model.meshes[i].idx[idx + 2] + vertCount}");
                    }
                    vertCount += model.meshes[i].verts.Length;
                }
                for (int i = 0; i < groundModel.meshes.Length; i++) {
                    w.WriteLine($"o {filename}_ground_{i}");
                    for (int vert = 0; vert < groundModel.meshes[i].verts.Length; vert++) {
                        w.WriteLine($"v {groundModel.meshes[i].verts[vert].x / 100 + 2.5 * col} {groundModel.meshes[i].verts[vert].z / -100} {groundModel.meshes[i].verts[vert].y / 100 -2.5 * row}");
                    }
                    for (int idx = 0; idx < groundModel.meshes[i].idx.Length; idx += 3) {
                        w.WriteLine($"f {groundModel.meshes[i].idx[idx] + vertCount} {groundModel.meshes[i].idx[idx + 1] + vertCount} {groundModel.meshes[i].idx[idx + 2] + vertCount}");
                    }
                    vertCount += groundModel.meshes[i].verts.Length;
                }
            }
        }

        public void ToObj(string[] fsNames, int x, int y, string mtlName) {
            int vertCount = 1;
            using (TextWriter w = new StreamWriter(File.Open(filename + ".obj", FileMode.Create))) {
                w.WriteLine("mtllib " + mtlName);
                
                for (int i = 0; i < model.meshes.Length; i++) {

                    for (int vert = 0; vert < model.meshes[i].verts.Length; vert++) {
                        w.WriteLine($"v {model.meshes[i].verts[vert].x / 100 + 2.5 * x} {model.meshes[i].verts[vert].z / -100} {model.meshes[i].verts[vert].y / 100 - 2.5 * y}");
                    }

                    string name = fsNames[0];
                    w.WriteLine($"o {filename}_{name}");
                    w.WriteLine("usemtl " + name);
                    for (int fs = 0; fs < model.meshes[i].fsOffsets.Length; fs++) {
                        if(fsNames[fs] != name) {
                            name = fsNames[fs];
                            w.WriteLine($"o {filename}_{name}");
                            w.WriteLine("usemtl " + name);
                        }
                        int offset = model.meshes[i].fsOffsets[fs];
                        for (int idx = 0; idx < model.meshes[i].fsSizes[fs]; idx += 3) {
                            w.WriteLine($"f {model.meshes[i].idx[idx + offset] + vertCount} {model.meshes[i].idx[idx + offset + 1] + vertCount} {model.meshes[i].idx[idx + offset + 2] + vertCount}");
                        }
                    }
                    vertCount += model.meshes[i].verts.Length;
                    break; //we don't need lods
                }
                
                
                for (int i = 0; i < groundModel.meshes.Length; i++) {
                    w.WriteLine($"o {filename}_ground_{i}");
                    w.WriteLine("usemtl annalithicground");
                    for (int vert = 0; vert < groundModel.meshes[i].verts.Length; vert++) {
                        w.WriteLine($"v {groundModel.meshes[i].verts[vert].x / 100 + 2.5 * x} {groundModel.meshes[i].verts[vert].z / -100} {groundModel.meshes[i].verts[vert].y / 100 - 2.5 * y}");
                    }
                    for (int idx = 0; idx < groundModel.meshes[i].idx.Length; idx += 3) {
                        w.WriteLine($"f {groundModel.meshes[i].idx[idx] + vertCount} {groundModel.meshes[i].idx[idx + 1] + vertCount} {groundModel.meshes[i].idx[idx + 2] + vertCount}");
                    }
                    vertCount += groundModel.meshes[i].verts.Length;
                }
                
            }
        }

    }
}
