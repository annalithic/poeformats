using System;
using System.IO;
using PoeFormats.Util;
using System.Text;

namespace PoeFormats {

    //to note i think fmts always have a single mesh
    public class Fmt : PoeModel {
        struct Unk1 {
            public byte a;
            public byte b;
            public byte c;
            public byte d;
            public byte e;
            public byte f;
        }

        byte version;
        int shapeCount; //Should be equal to meshes[0].shapeCount

        Unk1[] unk1;
        System.Numerics.Vector3[] unk2;
        //unk3 isnt handled for now
        BBox bbox;

        public string[] shapeNames;
        public string[] shapeMaterials;

        public Fmt(string gamePath, string path) : this(Path.Combine(gamePath, path)) { }

        public Fmt(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                version = r.ReadByte();

                if (version < 9) {
                    int triCount = r.ReadInt32();
                    int vertCount = r.ReadInt32();
                    shapeCount = r.ReadUInt16();
                    meshes = new PoeMesh[1];
                    meshes[0] = new PoeMesh(triCount, vertCount, shapeCount);
                } else {
                    shapeCount = r.ReadUInt16();
                }
                int[] shapeNameIndex = new int[shapeCount];
                int[] shapeMatIndex = new int[shapeCount];
                shapeNames = new string[shapeCount];
                shapeMaterials = new string[shapeCount];


                unk1 = new Unk1[r.ReadByte()];
                unk2 = new System.Numerics.Vector3[r.ReadUInt16()];
                byte unk3Count = r.ReadByte(); //todo
                bbox = r.ReadBBox();
                if (version == 9) {
                    Read(r);
                    for (int i = 0; i < shapeCount; i++) {
                        shapeNameIndex[i] = r.ReadInt32();
                        shapeMatIndex[i] = r.ReadInt32();
                    }
                    for (int i = 0; i < unk1.Length; i++) {
                        unk1[i] = new Unk1 {
                            a = r.ReadByte(), b = r.ReadByte(), c = r.ReadByte(),
                            d = r.ReadByte(), e = r.ReadByte(), f = r.ReadByte()
                        };
                    }
                } else {
                    if (version == 8) r.Seek(4);
                    PoeMesh m = meshes[0];

                    for (int i = 0; i < shapeCount; i++) {
                        shapeNameIndex[i] = r.ReadInt32();
                        shapeMatIndex[i] = r.ReadInt32();
                        m.shapeOffsets[i] = r.ReadInt32();
                    }
                    m.SetShapeSizes();

                    for (int i = 0; i < unk1.Length; i++) {
                        unk1[i] = new Unk1 {
                            a = r.ReadByte(), b = r.ReadByte(), c = r.ReadByte(),
                            d = r.ReadByte(), e = r.ReadByte(), f = r.ReadByte()
                        };
                    }

                    for (int i = 0; i < m.idx.Length; i++) {
                        m.idx[i] = r.ReadUInt16();
                    }

                    for (int i = 0; i < m.vertCount; i++) {
                        m.verts[i * 3] = r.ReadSingle();
                        m.verts[i * 3 + 1] = r.ReadSingle();
                        m.verts[i * 3 + 2] = r.ReadSingle();
                        r.Seek(8);
                        m.uvs[i * 2] = r.ReadUInt16();
                        m.uvs[i * 2 + 1] = r.ReadUInt16();
                    }
                }
                for (int i = 0; i < unk2.Length; i++) {
                    unk2[i] = new System.Numerics.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                }

                //todo read unk3
                int nameTableLength = r.ReadInt32();

                long pos = r.BaseStream.Position;
                for (int i = 0; i < shapeCount; i++) {
                    r.BaseStream.Seek(pos + shapeNameIndex[i] * 2, SeekOrigin.Begin);
                    shapeNames[i] = r.ReadWStringNullTerminated();
                    r.BaseStream.Seek(pos + shapeMatIndex[i] * 2, SeekOrigin.Begin);
                    shapeMaterials[i] = r.ReadWStringNullTerminated();
                }
                Console.WriteLine("DONE");
            }
        }
    }

    public class FmtOld : PoeMeshOld {
        byte version;

        public ushort shapeCount;
        public byte unk;



        public FmtOld(string path) {
            using(BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                version = r.ReadByte();

                if(version == 9) {
                    r.Seek(37);
                    shapeCount = r.ReadUInt16();
                    if (shapeCount == 0) return;

                    r.Seek(4);
                    triCount = r.ReadUInt32();
                    vertCount = r.ReadUInt32();
                    r.Seek(8);
                    r.Seek(shapeCount * 8);

                } else {
                    triCount = r.ReadUInt32();
                    vertCount = r.ReadUInt32();
                    shapeCount = r.ReadUInt16();
                    unk = r.ReadByte();
                    r.Seek(3);
                    r.Seek(24);
                    if (unk != 0) r.Seek(unk * 6);
                    if (version == 8) r.Seek(4);
                    r.Seek(shapeCount * 12);
                }


                idx = new int[triCount * 3];
                if (vertCount < 65535) {
                    for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadUInt16();
                } else {
                    for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadInt32();
                }

                x = new float[vertCount];
                y = new float[vertCount];
                z = new float[vertCount];
                u = new ushort[vertCount];
                v = new ushort[vertCount];
                for (int vert = 0; vert < vertCount; vert++) {
                    x[vert] = r.ReadSingle();
                    y[vert] = r.ReadSingle();
                    z[vert] = r.ReadSingle();
                    r.Seek(8);
                    u[vert] = r.ReadUInt16();
                    v[vert] = r.ReadUInt16();
                }

            }
        }

        public string Print() {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < 10; i++) {
                if (i >= vertCount) break;
                s.Append($"{x[i]} {y[i]} {z[i]} - {u[i]} {v[i]}\n");
            }
            return s.ToString();
        }
    }
}
