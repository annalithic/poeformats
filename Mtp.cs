using System;
using System.Reflection.Metadata.Ecma335;
using ImageMagick;

namespace PoeFormats {
    struct MinimapImage {
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
        string path;
        MinimapImage[] images;
        string names;

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

        public void PrintBoxImage() {
            MagickImage image = new MagickImage(MagickColors.White, 4096, 4096);
            foreach(MinimapImage i in images) {
                string writeVal = $"{i.filename}\n{i.orientation}\n{i.unk2} {i.unk3} {i.unk4}\n{i.height}\n{i.width}\n{i.leftPadding}";
                image.Draw(new Drawables()
                    .FillColor(MagickColors.Transparent)
                    .StrokeColor(MagickColors.GreenYellow)
                    .StrokeWidth(2)
                    .Rectangle(i.originX - i.leftPadding, i.originY - i.topPadding, i.originX - i.leftPadding + i.height, i.originY - i.topPadding + i.height));
            }
            image.Write(Path.GetFileNameWithoutExtension(path) + ".png");
        }

        public void PrintValueImage() {
            MagickImage image = new MagickImage(MagickColors.White, 4096, 4096);
            foreach (MinimapImage i in images) {
                string writeVal = $"{i.filename}\n{i.orientation}\n{i.unk2} {i.unk3} {i.unk4}\n{i.height}\n{i.width}\n{i.leftPadding}";
                image.Draw(new Drawables().FillColor(MagickColors.YellowGreen).FontPointSize(16).Gravity(Gravity.Southwest).TextAlignment(TextAlignment.Center).Text(i.originX * 2, i.originY * 2, writeVal));
            }
            image.Write(Path.GetFileNameWithoutExtension(path) + ".png");
        }

        public void WriteImages(string outputFolder) {
            MagickImage image = File.Exists(path.Replace(".mtp", ".png")) ? new MagickImage(path.Replace(".mtp", ".png")) : new MagickImage(path.Replace(".mtp", ".dds"));
            foreach(MinimapImage i in images) {
                MagickImage i2 = new MagickImage(image);
                i2.Crop(new MagickGeometry((int)(i.originX - i.leftPadding), (int)(i.originY - i.topPadding), i.width * 40, i.height));
                i2.RePage();
                string filename = $"{i.filename.Substring(0, i.filename.Length - 4)}_{i.orientation}.png";
                string filePath = Path.Combine(outputFolder, filename);
                if (!Directory.Exists(Path.GetDirectoryName(filePath))) Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                i2.Write(filePath);
            }
        }

    }
}
