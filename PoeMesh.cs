using System;
using System.IO;

namespace PoeFormats {

    public struct BBox {
        public float x1, x2, y1, y2, z1, z2;
    }

    public class PoeMeshOld {
        public uint triCount;
        public uint vertCount;

        public uint[] shapeStart;
        public uint[] shapeLength;
        public string[] shapeNames;

        public int[] idx;
        public float[] x;
        public float[] y;
        public float[] z;
        public ushort[] u;
        public ushort[] v;
    }

    public class PoeMesh {
        public int vertCount;
        public float[] verts;
        public ushort[] uvs;
        public int[] idx;
        public int[] submeshOffsets;
        public int[] submeshSizes;
        public BoneWeightSortable[][] boneWeights;

        public PoeMesh() {
        }

        public PoeMesh(int triCount, int vertCount, int submeshCount) {
            this.vertCount = vertCount;
            verts = new float[vertCount * 3];
            uvs = new ushort[vertCount * 2];
            idx = new int[triCount * 3];
            submeshOffsets = new int[submeshCount];
            submeshSizes = new int[submeshCount];
        }

        public void Read(BinaryReader r, int vertexFormat) {
            for(int i = 0; i < submeshOffsets.Length; i++) {
                submeshOffsets[i] = r.ReadInt32();
                submeshSizes[i] = r.ReadInt32();
            }
            if (vertCount > 65535) for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadInt32();
            else for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadUInt16();


            if (vertexFormat >= 60) {
                boneWeights = new BoneWeightSortable[vertCount][];

                //TODO vertex format, tgm is broken
                for (int i = 0; i < vertCount; i++) {
                    verts[i * 3] = r.ReadSingle();
                    verts[i * 3 + 1] = r.ReadSingle();
                    verts[i * 3 + 2] = r.ReadSingle();
                    r.BaseStream.Seek(8, SeekOrigin.Current);
                    uvs[i * 2] = r.ReadUInt16();
                    uvs[i * 2 + 1] = r.ReadUInt16();
                    boneWeights[i] = new BoneWeightSortable[4];
                    for (int weight = 0; weight < 4; weight++) {
                        boneWeights[i][weight] = new BoneWeightSortable(r.ReadByte());
                    }
                    for (int weight = 0; weight < 4; weight++) {
                        boneWeights[i][weight].weight = r.ReadByte();
                    }

                }
            } else if (vertexFormat >= 58) {
                Console.WriteLine("NOT IMPLEMENTED VERTEX FORMAT " + vertexFormat.ToString());
                //28 bytes
            } else if (vertexFormat >= 56) {
                for (int i = 0; i < vertCount; i++) {
                    verts[i * 3] = r.ReadSingle();
                    verts[i * 3 + 1] = r.ReadSingle();
                    verts[i * 3 + 2] = r.ReadSingle();
                    r.BaseStream.Seek(8, SeekOrigin.Current);
                    uvs[i * 2] = r.ReadUInt16();
                    uvs[i * 2 + 1] = r.ReadUInt16();
                }
            } else {
                Console.WriteLine("NOT IMPLEMENTED VERTEX FORMAT " + vertexFormat.ToString());
                //20 bytes
            }


        }
    }


    public class PoeModel {
        public short unk1;
        public PoeMesh[] meshes;
        public int meshCount;
        public int vertexFormat;

        public PoeModel() { }

        public PoeModel(BinaryReader r) {
            Read(r);
        }

        public void Read(BinaryReader r) {
            string magic = new string(r.ReadChars(4));
            if (magic != "DOLm") Console.WriteLine("MODEL MAGIC IS WRONG - " + magic);
            unk1 = r.ReadInt16();
            meshes = new PoeMesh[r.ReadByte()];
            meshCount = r.ReadUInt16();
            vertexFormat = r.ReadInt32();
            for (int i = 0; i < meshes.Length; i++) {
                meshes[i] = new PoeMesh(r.ReadInt32(), r.ReadInt32(), meshCount);
            }
            for (int i = 0; i < meshes.Length; i++) {
                meshes[i].Read(r, vertexFormat);
            }
        }
    }

}
