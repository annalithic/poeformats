using PoeFormats.Util;
using System;
using System.IO;

namespace PoeFormats {

    public struct BBox {
        public float x1, x2, y1, y2, z1, z2;

        public float SizeX { get { return x2 - x1; } }
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
        public int[] shapeOffsets;
        public int[] shapeLengths;
        public BoneWeightSortable[][] boneWeights;

        public ushort[] uv2s;
        public uint[] vcols;
        public int[] unks;

        public PoeMesh() {
        }

        public PoeMesh(int triCount, int vertCount, int shapeCount) {
            this.vertCount = vertCount;
            verts = new float[vertCount * 3];
            uvs = new ushort[vertCount * 2];
            idx = new int[triCount * 3];
            shapeOffsets = new int[shapeCount];
            shapeLengths = new int[shapeCount];
        }

        public void Read(BinaryReader r, int vertexFormat) {
            for(int i = 0; i < shapeOffsets.Length; i++) {
                shapeOffsets[i] = r.ReadInt32();
                shapeLengths[i] = r.ReadInt32();
            }
            if (vertCount > 65535) for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadInt32();
            else for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadUInt16();


            //FORMAT BREAKDOWN:
            //X, Y, Z: position
            //N, T:    normals
            //U, V:    uvs
            //B, W:    bone weights

            //61: XXXX YYYY ZZZZ NNNN TTTT UUVV BBBB WWWW ????
            //60: XXXX YYYY ZZZZ NNNN TTTT UUVV BBBB WWWW
            //58: XXXX YYYY ZZZZ NNNN TTTT UUVV ????
            //57: XXXX YYYY ZZZZ NNNN TTTT UUVV UUVV
            //56: XXXX YYYY ZZZZ NNNN TTTT UUVV
            //48: XXXX YYYY ZZZZ NNNN TTTT

            if((vertexFormat & 4) > 0) {
                boneWeights = new BoneWeightSortable[vertCount][];
            }

            if ((vertexFormat & 1) > 0) {
                uv2s = new ushort[vertCount * 2];
            }

            if ((vertexFormat & 2) > 0) {
                vcols = new uint[vertCount];
            }

            if ((vertexFormat & 64) > 0) {
                unks = new int[vertCount];
            }

            for (int i = 0; i < vertCount; i++) {

                verts[i * 3] = r.ReadSingle();
                verts[i * 3 + 1] = r.ReadSingle();
                verts[i * 3 + 2] = r.ReadSingle();
                r.Seek(8); //N+T

                if((vertexFormat & 8) > 0) {
                    uvs[i * 2] = r.ReadUInt16();
                    uvs[i * 2 + 1] = r.ReadUInt16();
                } else {
                    uvs[i * 2] = 0;
                    uvs[i * 2 + 1] = 0;
                }

                if(boneWeights != null) {
                    boneWeights[i] = new BoneWeightSortable[4];
                    for (int weight = 0; weight < 4; weight++) {
                        boneWeights[i][weight] = new BoneWeightSortable(r.ReadByte());
                    }
                    for (int weight = 0; weight < 4; weight++) {
                        boneWeights[i][weight].weight = r.ReadByte();
                    }
                }
                if(uv2s != null) {
                    uv2s[i * 2] = r.ReadUInt16();
                    uv2s[i * 2 + 1] = r.ReadUInt16();
                }

                if(vcols != null) {
                    vcols[i] = r.ReadUInt32();
                }

                if(unks != null) {
                    unks[i] = r.ReadInt32();
                }

            }

            if(vertexFormat >= 120) {
                for(int i = 0; i < shapeOffsets.Length; i++) {
                    r.ReadBBox();
                    r.Seek(12);
                }
            }

        }

        public void SetShapeSizes() {
            //submesh sizes
            for (int i = 0; i < shapeOffsets.Length - 1; i++) {
                shapeLengths[i] = shapeOffsets[i + 1] - shapeOffsets[i];
            }
            shapeLengths[shapeOffsets.Length - 1] = idx.Length - shapeOffsets[shapeOffsets.Length - 1];
        }
    }
    public class PoeModel {
        public short modelVersion;
        public PoeMesh[] meshes;
        public int shapeCount;
        public int vertexFormat;

        public PoeModel() { }

        public PoeModel(BinaryReader r) {
            Read(r);
        }

        public void Read(BinaryReader r) {
            string magic = new string(r.ReadChars(4));
            if (magic != "DOLm") Console.WriteLine("MODEL MAGIC IS WRONG - " + magic);
            modelVersion = r.ReadInt16();
            meshes = new PoeMesh[r.ReadByte()];
            shapeCount = r.ReadUInt16();
            vertexFormat = r.ReadByte();
            r.Seek(3);
            for (int i = 0; i < meshes.Length; i++) {
                meshes[i] = new PoeMesh(r.ReadInt32(), r.ReadInt32(), shapeCount);
            }
            for (int i = 0; i < meshes.Length; i++) {
                meshes[i].Read(r, vertexFormat);
            }

            //what?
            if (modelVersion == 4) {
                for (int i = 0; i < meshes.Length; i++)
                    r.ReadInt32(); //UNK COUNT???
            }
        }
    }

}
