using System.IO;

namespace PoeTerrain {

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

        public Ast(string path) : this(new BinaryReader(File.OpenRead(path))) { }

        Ast(BinaryReader r) {
            version = r.ReadByte();
            bones = new AstBone[r.ReadByte()];
            unk1 = r.ReadByte();
            animations = new AstAnimation[r.ReadByte()];
            unk2 = r.ReadByte();
            unk3 = r.ReadByte();
            unk3 = r.ReadByte();
            lights = new AstLight[r.ReadByte()];

            for (int i = 0; i < bones.Length; i++) {
                bones[i] = new AstBone();
                bones[i].sibling = r.ReadByte();
                bones[i].child = r.ReadByte();
                bones[i].transform = new float[16];
                for (int j = 0; j < 16; j++) bones[i].transform[j] = r.ReadSingle();
                byte nameLength = r.ReadByte();
                bones[i].unk2 = r.ReadByte();
                bones[i].name = new string(r.ReadChars(nameLength));
            }

            for (int i = 0; i < lights.Length; i++) {
                lights[i] = new AstLight();
                byte nameLength = r.ReadByte();
                r.BaseStream.Seek(55, SeekOrigin.Current);
                if (version > 8) r.BaseStream.Seek(4, SeekOrigin.Current);
                lights[i].name = new string(r.ReadChars(nameLength));
            }

            for (int i = 0; i < animations.Length; i++) {
                animations[i] = new AstAnimation();
                animations[i].tracks = new AstTrack[r.ReadByte()];
                animations[i].unk1 = r.ReadByte();
                animations[i].framerate = r.ReadByte();
                animations[i].unk2 = r.ReadByte();
                if (version == 11) animations[i].version11a = r.ReadByte();
                byte nameLength = r.ReadByte();
                int parentNameLength = 0;
                if (version == 11) parentNameLength = r.ReadByte();
                animations[i].dataOffset = r.ReadInt32();
                animations[i].dataSize = r.ReadInt32();
                animations[i].name = new string(r.ReadChars(nameLength));
                animations[i].parent = new string(r.ReadChars(parentNameLength));
            }

            byte[] payload = Bundle.DecompressBundle(r);
            using (BinaryReader r2 = new BinaryReader(new MemoryStream(payload))) {
                for (int anim = 0; anim < animations.Length; ++anim) {
                    for (int track = 0; track < animations[anim].tracks.Length; track++) {
                        animations[anim].tracks[track] = new AstTrack();
                        r2.BaseStream.Seek(1, SeekOrigin.Current); //unk
                        animations[anim].tracks[track].bone = r2.ReadInt32();
                        animations[anim].tracks[track].scaleKeys = new float[r2.ReadInt32()][];
                        animations[anim].tracks[track].rotationKeys = new float[r2.ReadInt32()][];
                        animations[anim].tracks[track].positionKeys = new float[r2.ReadInt32()][];
                        animations[anim].tracks[track].scaleKeys2 = new float[r2.ReadInt32()][];
                        animations[anim].tracks[track].rotationKeys2 = new float[r2.ReadInt32()][];
                        animations[anim].tracks[track].positionKeys2 = new float[r2.ReadInt32()][];
                        if (version == 11) r2.BaseStream.Seek(4, SeekOrigin.Current);

                        for (int i = 0; i < animations[anim].tracks[track].scaleKeys.Length; i++) {
                            animations[anim].tracks[track].scaleKeys[i] = new float[4]; //time + vec3
                            for (int j = 0; j < animations[anim].tracks[track].scaleKeys[i].Length; j++) animations[anim].tracks[track].scaleKeys[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < animations[anim].tracks[track].rotationKeys.Length; i++) {
                            animations[anim].tracks[track].rotationKeys[i] = new float[5]; //time + quaternion
                            for (int j = 0; j < animations[anim].tracks[track].rotationKeys[i].Length; j++) animations[anim].tracks[track].rotationKeys[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < animations[anim].tracks[track].positionKeys.Length; i++) {
                            animations[anim].tracks[track].positionKeys[i] = new float[4]; //time + vec3
                            for (int j = 0; j < animations[anim].tracks[track].positionKeys[i].Length; j++) animations[anim].tracks[track].positionKeys[i][j] = r2.ReadSingle();
                        }

                        for (int i = 0; i < animations[anim].tracks[track].scaleKeys2.Length; i++) {
                            animations[anim].tracks[track].scaleKeys2[i] = new float[4]; //time + vec3
                            for (int j = 0; j < animations[anim].tracks[track].scaleKeys2[i].Length; j++) animations[anim].tracks[track].scaleKeys2[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < animations[anim].tracks[track].rotationKeys2.Length; i++) {
                            animations[anim].tracks[track].rotationKeys2[i] = new float[5]; //time + quaternion
                            for (int j = 0; j < animations[anim].tracks[track].rotationKeys2[i].Length; j++) animations[anim].tracks[track].rotationKeys2[i][j] = r2.ReadSingle();
                        }
                        for (int i = 0; i < animations[anim].tracks[track].positionKeys2.Length; i++) {
                            animations[anim].tracks[track].positionKeys2[i] = new float[4]; //time + vec3
                            for (int j = 0; j < animations[anim].tracks[track].positionKeys2[i].Length; j++) animations[anim].tracks[track].positionKeys2[i][j] = r2.ReadSingle();
                        }

                    }
                }
            }
        }
    }
}
