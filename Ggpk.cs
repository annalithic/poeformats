
using System.IO;
using PoeFormats.Util;

namespace PoeFormats {

    public class Ggpk {
        int version;
        long offset1;
        long offset2;
        Chunk root;

        class Chunk {
        }

        class ChunkDirectory : Chunk {
            public string name;
            public Chunk[] children;
            long[] childOffsets;
            public ChunkDirectory(BinaryReader r, string path) {
                int nameLength = r.ReadInt32();
                int childCount = r.ReadInt32();
                r.Seek(32); //signature
                name = path + '/' + System.Text.Encoding.Unicode.GetString(r.ReadBytes(nameLength * 2 - 2)); r.Seek(2);
                childOffsets = new long[childCount];
                for(int i = 0; i < childOffsets.Length; i++) {
                    r.Seek(4); //hash
                    childOffsets[i] = r.ReadInt64();
                }
                children = new Chunk[childCount];
                for(int i = 0; i < childOffsets.Length; i++) {
                    r.BaseStream.Seek(childOffsets[i], SeekOrigin.Begin);
                    children[i] = ReadChunk(r, name);
                }
            }
        }

        class ChunkFile : Chunk {
            public string name;
            public long dataOffset;
            public int dataLength;
            public ChunkFile(BinaryReader r, int chunkSize, string path) {
                int nameLength = r.ReadInt32();
                r.Seek(32); //signature
                name = path + '/' + System.Text.Encoding.Unicode.GetString(r.ReadBytes(nameLength * 2 - 2)); r.Seek(2);
                dataOffset = r.BaseStream.Position;
                dataLength = chunkSize - 12 - 32 - nameLength * 2;
                string exportPath = @"E:\Extracted\PathOfExile2\0.3.0.Preload" + name;
                if(!Directory.Exists(Path.GetDirectoryName(exportPath))) Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
                File.WriteAllBytes(exportPath, r.ReadBytes(dataLength));
            }
        }

        static Chunk ReadChunk(BinaryReader r, string path) {
            int chunkSize = r.ReadInt32();
            int chunkType = r.ReadInt32();
            switch (chunkType) {
                case 1380533328: //PDIR
                    return new ChunkDirectory(r, path);
                case 1162627398: //FILE
                    return new ChunkFile(r, chunkSize, path);
                default:
                    //Console.WriteLine("UNKNOWN CHUNK TYPE " + chunkType.ToString());
                    return new Chunk();
            }
        }

        public Ggpk(string path) {

            using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
                {
                    int headerLength = r.ReadInt32();
                    if (r.ReadInt32() != 1263552327) return;
                    version = r.ReadInt32();
                    offset1 = r.ReadInt64();
                }


                r.BaseStream.Seek(offset1, SeekOrigin.Begin);
                root = ReadChunk(r, "");
            }
        }
    }
}
