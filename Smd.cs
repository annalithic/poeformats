using System;
using System.IO;
using PoeTerrain.Util;

namespace PoeTerrain {


    public class BoneWeightSortable : IComparable<BoneWeightSortable> {
        public byte id;
        public byte weight;

        public BoneWeightSortable(byte b) {
            id = b;
        }
        public int CompareTo(BoneWeightSortable other) {
            if (other.weight > weight) return 1;
            if (other.weight < weight) return -1;
            return 0;
        }
    }

    public class Smd {
        public byte unk1;
        public ushort shapeCount;
        public int unk2;
        public BBox bbox;
        public PoeModel model;

        public Smd(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                byte version = r.ReadByte();
                if(version == 3) {
                    unk1 = r.ReadByte();
                    shapeCount = r.ReadUInt16();
                    unk2 = r.ReadInt32();
                    bbox = r.ReadBBox();
                    model = new PoeModel(r);

                } else {
                    //kind of jamming the old model format into the version 3 format in a funny way
                    model = new PoeModel();
                    model.meshes = new PoeMesh[1];
                    model.meshes[0] = new PoeMesh();
                    model.meshes[0].idx = new int[r.ReadInt32() * 3];

                    model.meshes[0].vertCount = r.ReadInt32();
                    model.meshes[0].verts = new float[model.meshes[0].vertCount * 3];
                    model.meshes[0].uvs = new ushort[model.meshes[0].vertCount * 2];

                    unk1 = r.ReadByte();

                    model.submeshCount = r.ReadUInt16();
                    model.meshes[0].submeshOffsets = new int[model.submeshCount];
                    model.meshes[0].submeshSizes = new int[model.submeshCount];

                    int submeshNamesLength = r.ReadInt32();
                    bbox = r.ReadBBox();
                    model.meshes[0].submeshOffsets = new int[model.submeshCount];
                    model.meshes[0].submeshSizes = new int[model.submeshCount];
                    for(int i = 0; i < model.submeshCount; i++) {
                        model.meshes[0].submeshOffsets[i] = r.ReadInt32();
                        model.meshes[0].submeshSizes[i] = r.ReadInt32();
                    }
                    if(version == 2) unk2 = r.ReadInt32();
                    r.Seek(submeshNamesLength); //submesh names, stored in .sm for version 3 i think

                    //copypasted from poemesh, todo fix
                    if (model.meshes[0].vertCount > 65535) for (int i = 0; i < model.meshes[0].idx.Length; i++) model.meshes[0].idx[i] = r.ReadInt32();
                    else for (int i = 0; i < model.meshes[0].idx.Length; i++) model.meshes[0].idx[i] = r.ReadUInt16();


                    model.meshes[0].boneWeights = new BoneWeightSortable[model.meshes[0].vertCount][];

                    for (int i = 0; i < model.meshes[0].vertCount; i++) {
                        model.meshes[0].verts[i * 3] = r.ReadSingle();
                        model.meshes[0].verts[i * 3 + 1] = r.ReadSingle();
                        model.meshes[0].verts[i * 3 + 2] = r.ReadSingle();
                        r.BaseStream.Seek(8, SeekOrigin.Current);
                        model.meshes[0].uvs[i * 2] = r.ReadUInt16();
                        model.meshes[0].uvs[i * 2 + 1] = r.ReadUInt16();
                        model.meshes[0].boneWeights[i] = new BoneWeightSortable[4];
                        for (int weight = 0; weight < 4; weight++) {
                            model.meshes[0].boneWeights[i][weight] = new BoneWeightSortable(r.ReadByte());
                        }
                        for (int weight = 0; weight < 4; weight++) {
                            model.meshes[0].boneWeights[i][weight].weight = r.ReadByte();
                        }

                    }

                }



                /*
                if (version == 3) {
                    r.ReadByte();
                    shapeCount = r.ReadUInt16();
                    r.BaseStream.Seek(41, SeekOrigin.Current);
                    triCount = r.ReadUInt32();
                    vertCount = r.ReadUInt32();
                    shapeStart = new uint[shapeCount]; shapeLength = new uint[shapeCount];
                    for (int i = 0; i < shapeCount; i++) { shapeStart[i] = r.ReadUInt32(); shapeLength[i] = r.ReadUInt32(); }
                } else {
                    triCount = r.ReadUInt32();
                    vertCount = r.ReadUInt32();
                    r.ReadByte();
                    shapeCount = r.ReadUInt16();
                    int shapeNameLength = r.ReadInt32();
                    r.BaseStream.Seek(24, SeekOrigin.Current); //bbox
                    shapeStart = new uint[shapeCount]; shapeLength = new uint[shapeCount];
                    for (int i = 0; i < shapeCount; i++) { shapeStart[i] = r.ReadUInt32(); shapeLength[i] = r.ReadUInt32(); }
                    if (version == 2) r.BaseStream.Seek(4, SeekOrigin.Current);
                    r.BaseStream.Seek(shapeNameLength, SeekOrigin.Current);
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

                boneWeights = new BoneWeightSortable[vertCount][];


                for (int vert = 0; vert < vertCount; vert++) {
                    x[vert] = r.ReadSingle();
                    y[vert] = r.ReadSingle();
                    z[vert] = r.ReadSingle();
                    r.BaseStream.Seek(8, SeekOrigin.Current);
                    u[vert] = r.ReadUInt16();
                    v[vert] = r.ReadUInt16();
                    boneWeights[vert] = new BoneWeightSortable[4];
                    for(int weight = 0; weight < 4; weight++) {
                        boneWeights[vert][weight] = new BoneWeightSortable(r.ReadByte());
                    }
                    for (int weight = 0; weight < 4; weight++) {
                        boneWeights[vert][weight].weight = r.ReadByte();
                    }
                }


                int[] shapeNameLengths = new int[shapeCount];
                for (int i = 0; i < shapeCount; i++) shapeNameLengths[i] = r.ReadInt32();
                shapeNames = new string[shapeCount];
                for (int i = 0; i < shapeCount; i++) {
                    shapeNames[i] = Encoding.Unicode.GetString(r.ReadBytes(shapeNameLengths[i]));
                }
                */
            }
        }
        /*
        public string Print() {
            StringBuilder s = new StringBuilder();
            for(int i = 0; i < 20; i++) {
                if (i >= vertCount) break;
                s.Append($"{x[i]} {y[i]} {z[i]} - {u[i]} {v[i]}\n");
            }
            return s.ToString();
        }
        */
    }
}
