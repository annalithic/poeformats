using System.IO;
using System;

namespace PoeFormats {
    internal class Bundle {
        public static byte[] DecompressBundle(BinaryReader r) {
            int uncompressedSize = r.ReadInt32();
            int totalPayloadSize = r.ReadInt32();
            int headPayloadSize = r.ReadInt32();
            int compression = r.ReadInt32();
            r.BaseStream.Seek(4, SeekOrigin.Current);
            long uncompressedSize2 = r.ReadInt64();
            long totalPayloadSize2 = r.ReadInt64();
            int[] blockSizes = new int[r.ReadInt32()];
            int uncompressedBlockGranularity = r.ReadInt32();
            r.BaseStream.Seek(4 * 4, SeekOrigin.Current);
            for(int i = 0; i < blockSizes.Length; i++) blockSizes[i] = r.ReadInt32();

            byte[] uncompressedData = new byte[uncompressedSize];
            byte[] oozBuffer =  new byte[uncompressedBlockGranularity + 64]; //extra padding needed for decompress method

            int offset = 0;
            for(int i = 0; i < blockSizes.Length; i++) {
                byte[] block = r.ReadBytes(blockSizes[i]);
                int decompressedSize = i == blockSizes.Length - 1 ? uncompressedSize - uncompressedBlockGranularity * (blockSizes.Length - 1) : uncompressedBlockGranularity;

                ooz.Ooz_Decompress(block, block.Length, oozBuffer, decompressedSize);
                Array.Copy(oozBuffer, 0, uncompressedData, offset, decompressedSize);
                offset += decompressedSize;
            }
            return uncompressedData;
        }
    }
}
