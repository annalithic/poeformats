using System.IO;

namespace PoeFormats {

    public struct AstBone {
        public byte sibling;
        public byte child;
        public float[] transform;
        public byte unk2;
        public string name;
    }
    public struct AstLight {
        public string name;
    }

    public struct AstTrack {
        public byte unk;
        public int bone;
        public float[][] scaleKeys;
        public float[][] rotationKeys;
        public float[][] positionKeys;

        public float[][] scaleKeys2;
        public float[][] rotationKeys2;
        public float[][] positionKeys2;

    }

    public class AstAnimation {
        public byte unk1;
        public byte framerate;
        public byte unk2;

        public byte version11a;
        public int dataOffset;
        public int dataSize;
        public string name;
        public string parent;

        public AstTrack[] tracks;
    }

    public class Ast {
        public byte version;
        public byte unk1;
        public byte unk2;
        public byte unk3;
        public byte unk4;



        public AstBone[] bones;
        public AstLight[] lights;
        public AstAnimation[] animations;

        public Ast(string path, bool loadTracks = true) : this(new BinaryReader(File.OpenRead(path)), loadTracks) { }

        public Ast() { }

        public static void WriteUncompressed(string path) {
            using(var r = new BinaryReader(File.OpenRead(path))) {
                Ast a = new Ast();
                ReadHeader(a, r);
                int headerSize = (int)r.BaseStream.Position;
                byte[] payload = Bundle.DecompressBundle(r);
                r.BaseStream.Seek(0, SeekOrigin.Begin);
                byte[] header = new byte[headerSize];
                r.Read(header, 0, headerSize);
                using (BinaryWriter w = new BinaryWriter(File.Open(path.Replace(".ast", ".uncompressed.ast"), FileMode.Create))) {
                    w.Write(header);
                    w.Write(payload);
                }
            }
        }

        static void ReadHeader(Ast a, BinaryReader r) {
            a.version = r.ReadByte();

            if (a.version < 8) {
                a.animations = new AstAnimation[0];
                return;
            }

            a.bones = new AstBone[r.ReadByte()];
            a.unk1 = r.ReadByte();
            a.animations = new AstAnimation[r.ReadByte()];
            a.unk2 = r.ReadByte();
            a.unk3 = r.ReadByte();
            a.unk3 = r.ReadByte();
            a.lights = new AstLight[r.ReadByte()];

            for (int i = 0; i < a.bones.Length; i++) {
                a.bones[i] = new AstBone();
                a.bones[i].sibling = r.ReadByte();
                a.bones[i].child = r.ReadByte();
                a.bones[i].transform = new float[16];
                for (int j = 0; j < 16; j++) a.bones[i].transform[j] = r.ReadSingle();
                byte nameLength = r.ReadByte();
                a.bones[i].unk2 = r.ReadByte();
                a.bones[i].name = new string(r.ReadChars(nameLength));
            }

            for (int i = 0; i < a.lights.Length; i++) {
                a.lights[i] = new AstLight();
                byte nameLength = r.ReadByte();
                r.BaseStream.Seek(55, SeekOrigin.Current);
                if (a.version > 8) r.BaseStream.Seek(4, SeekOrigin.Current);
                a.lights[i].name = new string(r.ReadChars(nameLength));
            }

            for (int i = 0; i < a.animations.Length; i++) {
                a.animations[i] = new AstAnimation();
                a.animations[i].tracks = new AstTrack[r.ReadByte()];
                a.animations[i].unk1 = r.ReadByte();
                a.animations[i].framerate = r.ReadByte();
                a.animations[i].unk2 = r.ReadByte();
                if (a.version > 9) a.animations[i].version11a = r.ReadByte();
                byte nameLength = r.ReadByte();
                int parentNameLength = 0;
                if (a.version > 10) parentNameLength = r.ReadByte();
                a.animations[i].dataOffset = r.ReadInt32();
                a.animations[i].dataSize = r.ReadInt32();
                a.animations[i].name = new string(r.ReadChars(nameLength));
                a.animations[i].parent = new string(r.ReadChars(parentNameLength));
            }
        }



        Ast(BinaryReader r, bool loadTracks = true) {
            ReadHeader(this, r);

            if(!loadTracks) return;
            byte[] payload = Bundle.DecompressBundle(r);
            using (BinaryReader r2 = new BinaryReader(new MemoryStream(payload))) {
                for (int anim = 0; anim < animations.Length; ++anim) {
                    for (int track = 0; track < animations[anim].tracks.Length; track++) {
                        AstTrack t = new AstTrack();

                        t.unk = r2.ReadByte();
                        t.bone = r2.ReadInt32();
                        t.scaleKeys = new float[r2.ReadInt32()][];
                        t.rotationKeys = new float[r2.ReadInt32()][];
                        t.positionKeys = new float[r2.ReadInt32()][];
                        t.scaleKeys2 = new float[r2.ReadInt32()][];
                        t.rotationKeys2 = new float[r2.ReadInt32()][];
                        t.positionKeys2 = new float[r2.ReadInt32()][];
                        if (version > 10) r2.BaseStream.Seek(4, SeekOrigin.Current);

                        for (int i = 0; i < t.scaleKeys.Length; i++) {
                            t.scaleKeys[i] = new float[4]; //time + vec3
                            for (int j = 0; j < t.scaleKeys[i].Length; j++) t.scaleKeys[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < t.rotationKeys.Length; i++) {
                            t.rotationKeys[i] = new float[5]; //time + quaternion
                            for (int j = 0; j < t.rotationKeys[i].Length; j++) t.rotationKeys[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < t.positionKeys.Length; i++) {
                            t.positionKeys[i] = new float[4]; //time + vec3
                            for (int j = 0; j < t.positionKeys[i].Length; j++) t.positionKeys[i][j] = r2.ReadSingle();
                        }

                        for (int i = 0; i < t.scaleKeys2.Length; i++) {
                            t.scaleKeys2[i] = new float[4]; //time + vec3
                            for (int j = 0; j < t.scaleKeys2[i].Length; j++) t.scaleKeys2[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < t.rotationKeys2.Length; i++) {
                            t.rotationKeys2[i] = new float[5]; //time + quaternion
                            for (int j = 0; j < t.rotationKeys2[i].Length; j++) t.rotationKeys2[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < t.positionKeys2.Length; i++) {
                            t.positionKeys2[i] = new float[4]; //time + vec3
                            for (int j = 0; j < t.positionKeys2[i].Length; j++) t.positionKeys2[i][j] = r2.ReadSingle();
                        }
                        animations[anim].tracks[track] = t;

                    }
                }
            }
        }
    }
}
