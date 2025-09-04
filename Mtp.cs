using System;
using System.IO;

namespace PoeFormats {
    public struct MinimapImage {
        public string filename;
        public int nameOffset;
        public byte orientation;
        public byte textureIndex;
        public byte unk3;
        public byte unk4;
        public int height;
        public int width;
        public float leftPadding;
        public float topPadding;
        public float originX;
        public float originY;

        public override string ToString() {
            return filename;
        }

        public MinimapImage(BinaryReader r, bool newVersion) {
            filename = "";
            nameOffset = r.ReadInt32();
            orientation = r.ReadByte();
            textureIndex = r.ReadByte();
            if(newVersion) {
                unk3 = 0;
                unk4 = 0;
                height = r.ReadInt16();
                width = r.ReadInt16();
                originY = r.ReadInt16();
                originX = r.ReadInt16();
                topPadding = 0;
                leftPadding = 0;
                r.BaseStream.Seek(6, SeekOrigin.Current);
            } else {
                unk3 = r.ReadByte();
                unk4 = r.ReadByte();
                height = r.ReadInt32();
                width = r.ReadInt32();
                leftPadding = r.ReadSingle();
                topPadding = r.ReadSingle();
                originX = r.ReadSingle();
                originY = r.ReadSingle();
            }
        }
    }

    public class Mtp {
        public string path;
        public MinimapImage[] images;
        public string names;

        public Mtp(string path) {
            this.path = path;
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
                int begin = reader.ReadInt32();
                if (begin == 1) {
                    reader.BaseStream.Seek(reader.ReadInt32() * 4, SeekOrigin.Current);
                }
                images = new MinimapImage[reader.ReadInt32()];
                for (int i = 0; i < images.Length; i++) images[i] = new MinimapImage(reader, begin == 1);
                names = System.Text.Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32() * 2));
                for(int i = 0; i < images.Length; i++) {
                    string name = names.Substring(images[i].nameOffset);
                    name = name.Substring(0, name.IndexOf((char)0));
                    //if (name.StartsWith("Metadata/Terrain/")) name = name.Substring("Metadata/Terrain/".Length);
                    //Console.WriteLine($"{images[i].originX} {images[i].originY} {images[i].size} {images[i].topPadding} {name}");

                    //name = name.Replace('/', '\n');
                    images[i].filename = name;
                }
                //Console.WriteLine();
                //foreach (string name in names.Split((char)0)) Console.WriteLine(name);
            }

        }
    }
}
