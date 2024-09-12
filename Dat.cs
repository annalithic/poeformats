using PoeFormats.Util;
using System;
using System.IO;

namespace PoeFormats {
    public class Dat {

        public int rowCount;
        public int rowWidth;
        public byte[][] rows;
        public byte[] varying;

        public Dat(string path) {
            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                rowCount = r.ReadInt32();
                while(true) {
                    r.BaseStream.Seek(4 + rowWidth * rowCount, SeekOrigin.Begin);
                    if (r.ReadUInt64() == 0xbbbbbbbbbbbbbbbb) break;
                    rowWidth++;
                }

                r.BaseStream.Seek(4, SeekOrigin.Begin);
                rows = new byte[rowCount][];
                for (int i = 0; i < rowCount; i++) {
                    rows[i] = r.ReadBytes(rowWidth);
                }

                varying = r.ReadBytes((int)r.BaseStream.Length - rowWidth * rowCount - 4);
            }
        }
    }

    public class DatReader : IDisposable {
        public int rowCount;
        public int rowWidth;

        int varyingOffset;
        BinaryReader r;

        public DatReader(string path) {
            r = new BinaryReader(File.OpenRead(path));
            rowCount = r.ReadInt32();
            while (true) {
                r.BaseStream.Seek(4 + rowWidth * rowCount, SeekOrigin.Begin);
                if (r.ReadUInt64() == 0xbbbbbbbbbbbbbbbb) break;
                rowWidth++;
            }
            varyingOffset = 4 + rowWidth * rowCount;
        }

        public void SeekRow(int i) {
            r.BaseStream.Seek(4 + rowWidth *  i, SeekOrigin.Begin);
        }

        public string String() {
            long offset = r.ReadInt64();
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            string s = r.ReadWStringNullTerminated();
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return s;
        }
        public int Int() {
            return r.ReadInt32();
        }
        public bool Bool() {
            return r.ReadBoolean();
        }
        public int Ref() {
            int refnum = r.ReadInt32();
            r.Seek(12);
            return refnum;
        }

        public void Dispose() {
            r.Close();
        }
    }
}
