using ImageMagick;
using System;
using System.Diagnostics.SymbolStore;

namespace PoeTerrain {

    public struct AstJoint {
        public short unk1;
        public float[] transform;
        public byte unk2;
        public string name;
    }
    public struct AstLight {
        public string name;
    }

    public struct AstAnimation {
        public int unk1;
        public byte version11a;
        public int unk2;
        public int unk3;
        public byte version11b;
        public string name;
        public string parent;
    }

    public class Ast {
        public byte version;
        public byte unk1;
        public byte unk2;
        public byte unk3;
        public byte unk4;



        public AstJoint[] joints;
        public AstLight[] lights;
        public AstAnimation[] animations;

        public Ast(string path) : this(new BinaryReader(File.OpenRead(path))) { }

        Ast(BinaryReader r) {
            version = r.ReadByte();
            joints = new AstJoint[r.ReadByte()];
            unk1 = r.ReadByte();
            animations = new AstAnimation[r.ReadByte()];
            unk2 = r.ReadByte();
            unk3 = r.ReadByte();
            unk3 = r.ReadByte();
            lights = new AstLight[r.ReadByte()];

            for (int i = 0; i < joints.Length; i++) {
                joints[i] = new AstJoint();
                joints[i].unk1 = r.ReadInt16();
                joints[i].transform = new float[16];
                for (int j = 0; j < 16; j++) joints[i].transform[j] = r.ReadSingle();
                byte nameLength = r.ReadByte();
                joints[i].unk2 = r.ReadByte();
                joints[i].name = new string(r.ReadChars(nameLength));
            }

            for(int i = 0; i < lights.Length; i++) {
                lights[i] = new AstLight();
                byte nameLength = r.ReadByte();
                r.BaseStream.Seek(55, SeekOrigin.Current);
                if(version > 8) r.BaseStream.Seek(4, SeekOrigin.Current);
                lights[i].name = new string(r.ReadChars(nameLength));
            }

            for (int i = 0; i < animations.Length; i++) {
                animations[i] = new AstAnimation();
                animations[i].unk1 = r.ReadInt32();
                if(version == 11) animations[i].version11a = r.ReadByte();
                byte nameLength = r.ReadByte();
                int parentNameLength = 0;
                if (version == 11) parentNameLength = r.ReadByte();

                animations[i].unk2 = r.ReadInt32();
                animations[i].unk3 = r.ReadInt32();
                animations[i].name = new string(r.ReadChars(nameLength));
                animations[i].parent = new string(r.ReadChars(parentNameLength));
            }
        }
    }
}
