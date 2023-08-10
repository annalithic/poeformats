using System;
using System.IO;

namespace PoeFormats {
    public struct MinimapImage {
        public string filename;
        public int nameOffset;
        public byte orientation;
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public int height;
        public int width;
        public float leftPadding;
        public float topPadding;
        public float originX;
        public float originY;

        public MinimapImage(BinaryReader r) {
            filename = "";
            nameOffset = r.ReadInt32();
            orientation = r.ReadByte();
            unk2 = r.ReadByte();
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

    public class Mtp {
        public string path;
        public MinimapImage[] images;
        public string names;

        public Mtp(string path) {
            this.path = path;
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
                reader.BaseStream.Seek(4, SeekOrigin.Begin);
                images = new MinimapImage[reader.ReadInt32()];
                for (int i = 0; i < images.Length; i++) images[i] = new MinimapImage(reader);
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
