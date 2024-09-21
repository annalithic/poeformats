using PoeFormats.Util;
using System.Runtime.CompilerServices;
using System.Text;

namespace PoeFormats {
    public class Dat {

        public int rowCount;
        public int rowWidth;
        public byte[] data;
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
                data = r.ReadBytes(rowCount * rowWidth);

                varying = r.ReadBytes((int)r.BaseStream.Length - rowWidth * rowCount - 4);
                
            }
        }

        public string[] ColumnInt(int offset) {
            string[] values = new string[rowCount];
            if(offset + 4 > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "OOB";
                }
            } else {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = BitConverter.ToInt32(data, offset + rowWidth * i).ToString();
                }
            }

            return values;
        }

        public string[] ColumnFloat(int offset) {
            string[] values = new string[rowCount];
            if (offset + 4 > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "OOB";
                }
            } else {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = BitConverter.ToSingle(data, offset + rowWidth * i).ToString();
                }
            }
            return values;
        }



        public string[] ColumnString(int offset) {
            string[] values = new string[rowCount];
            if (offset + 8 > rowWidth) {
                for (int i = 0; i < rowCount; i++) {
                    values[i] = "OOB";
                }
            }
            for (int i = 0; i < rowCount; i++) {
                int strOffset = BitConverter.ToInt32(data, offset + rowWidth * i);
                values[i] = ReadWStringNullTerminated(varying, strOffset);
            }
            
            return values;
        }

        static string ReadWStringNullTerminated(byte[] d, int offset) {
            int length = 0;
            while (d[offset + length] != 0) {
                length += 2;
            }
            return Encoding.Unicode.GetString(new ReadOnlySpan<byte>(d, offset, length));
        }


    }

    public class DatReader : IDisposable {
        public int rowCount;
        public int rowWidth;

        int varyingOffset;
        public BinaryReader r;

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

        public string[] StringArray() {
            int count = (int)r.ReadInt64();
            long[] stringOffsets = new long[count];
            string[] strings = new string[count];
            long offset = r.ReadInt64();
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                stringOffsets[i] = r.ReadInt64();
            }
            for (int i = 0; i < count; i++) {
                r.BaseStream.Seek(varyingOffset + stringOffsets[i], SeekOrigin.Begin);
                strings[i] = r.ReadWStringNullTerminated();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return strings;
        }

        public int Int() {
            return r.ReadInt32();
        }
        public int[] IntArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] values = new int[count];
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadInt32();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return values;
        }

        public float Float() {
            return r.ReadSingle();
        }

        public float[] FloatArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            float[] values = new float[count];
            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadSingle();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return values;
        }

        public bool Bool() {
            return r.ReadBoolean();
        }
        public int Ref() {
            int refnum = r.ReadInt32();
            r.Seek(12);
            return refnum;
        }

        public int[] RefArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] values = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadInt32();
                r.Seek(12);
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return values;
        }

        public int Row() {
            int test = (int)r.ReadInt64();
            return test;
            return (int)r.ReadInt64();
        }

        public int[] RowArray() {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] offsets = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                offsets[i] = (int)r.ReadInt64();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return offsets;
        }

        public void Dispose() {
            r.Close();
        }

        public void UnknownArray() {
            r.Seek(16);
        }

        public T Enum<T>() where T : struct, IConvertible {
            int i = r.ReadInt32();
            return Unsafe.As<int, T>(ref i);
        }

        public T[] EnumArray<T>() where T : struct, IConvertible {
            int count = (int)r.ReadInt64();
            long offset = r.ReadInt64();
            int[] values = new int[count];

            var currentPos = r.BaseStream.Position;
            r.BaseStream.Seek(varyingOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++) {
                values[i] = r.ReadInt32();
            }
            r.BaseStream.Seek(currentPos, SeekOrigin.Begin);
            return Unsafe.As<int[], T[]>(ref values);
        }

    }
}
