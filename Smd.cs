using System;
using System.IO;
using PoeFormats.Util;

namespace PoeFormats {


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
        byte version;
        public byte unk1;
        public ushort shapeCount;
        public int unk2;
        public BBox bbox;
        public PoeModel model;

        public Smd(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                version = r.ReadByte();
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
                    PoeMesh mesh = new PoeMesh();
                    model.meshes[0] = mesh;
                    mesh.idx = new int[r.ReadInt32() * 3];

                    mesh.vertCount = r.ReadInt32();
                    mesh.verts = new float[mesh.vertCount * 3];
                    mesh.uvs = new ushort[mesh.vertCount * 2];

                    unk1 = r.ReadByte();

                    model.meshCount = r.ReadUInt16();
                    mesh.submeshOffsets = new int[model.meshCount];
                    mesh.submeshSizes = new int[model.meshCount];

                    int submeshNamesLength = r.ReadInt32();
                    bbox = r.ReadBBox();
                    if (version == 2) r.ReadInt32();
                    mesh.submeshOffsets = new int[model.meshCount];
                    mesh.submeshSizes = new int[model.meshCount];
                    for(int i = 0; i < model.meshCount; i++) {
                        r.ReadInt32(); //submesh name length?
                        mesh.submeshOffsets[i] = r.ReadInt32() * 3;
                        //mesh.submeshSizes[i] = r.ReadInt32();
                    }
                    //submesh sizes
                    for(int i = 0; i < model.meshCount - 1; i++) {
                        mesh.submeshSizes[i] = mesh.submeshOffsets[i + 1] - mesh.submeshOffsets[i];
                    }
                    mesh.submeshSizes[model.meshCount - 1] = mesh.idx.Length - mesh.submeshOffsets[model.meshCount - 1];


                    //if(version == 2) unk2 = r.ReadInt32();
                    r.Seek(submeshNamesLength); //submesh names, stored in .sm for version 3 i think

                    //copypasted from poemesh, todo fix
                    if (mesh.vertCount > 65535) for (int i = 0; i < mesh.idx.Length; i++) mesh.idx[i] = r.ReadInt32();
                    else for (int i = 0; i < mesh.idx.Length; i++) mesh.idx[i] = r.ReadUInt16();


                    mesh.boneWeights = new BoneWeightSortable[mesh.vertCount][];

                    for (int i = 0; i < mesh.vertCount; i++) {
                        mesh.verts[i * 3] = r.ReadSingle();
                        mesh.verts[i * 3 + 1] = r.ReadSingle();
                        mesh.verts[i * 3 + 2] = r.ReadSingle();
                        r.BaseStream.Seek(8, SeekOrigin.Current);
                        mesh.uvs[i * 2] = r.ReadUInt16();
                        mesh.uvs[i * 2 + 1] = r.ReadUInt16();
                        mesh.boneWeights[i] = new BoneWeightSortable[4];
                        for (int weight = 0; weight < 4; weight++) {
                            mesh.boneWeights[i][weight] = new BoneWeightSortable(r.ReadByte());
                        }
                        for (int weight = 0; weight < 4; weight++) {
                            mesh.boneWeights[i][weight].weight = r.ReadByte();
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
