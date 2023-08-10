using System;
using System.IO;
using PoeFormats.Util;
using System.Text;

namespace PoeFormats {
    public class Fmt : PoeMeshOld {
        byte version;

        public ushort shapeCount;
        public byte unk;



        public Fmt(string path) {
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
