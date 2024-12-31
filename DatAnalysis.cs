using PoeFormats;
using System;
using System.Collections.Generic;

namespace PoeFormats {
    public class DatAnalysis {
        public static int arrayMaxCount = 627;

        [Flags]
        public enum Error : ushort {
            NONE = 0,

            OOB = 1,

            VALUE_TOO_SMALL = 2,
            VALUE_TOO_BIG = 4,

            COUNT_TOO_SMALL = 8,
            COUNT_TOO_BIG = 16,

            OFFSET_TOO_SMALL = 32,
            OFFSET_TOO_BIG = 64,

            UPPER_BYTES_NOT_ZERO = 128,
            OFFSET_NOT_EVEN = 256,

            VALUES_NOT_UNIQUE = 512,
        }

        public Error isBool;
        public Error isInt;
        public Error isFloat;
        public Error isString;
        public Error isRef;
        public Error isHash16;

        public Error isArray;
        public Error isIntArray;
        public Error isFloatArray;
        public Error isStringArray;
        public Error isRefArray;

        public int maxRef;
        public int maxRefArray;

        public DatAnalysis(Dat dat, int columnOffset, int maxRows) {
            HashSet<ushort> hashValues = new HashSet<ushort>(dat.rowCount);
            for (int i = 0; i < dat.rowCount; i++) {
                Analyse(dat, columnOffset, i, maxRows, hashValues);
            }
        }

        public Error GetError(Schema.Column column) {
            if (column.array) {
                switch (column.type) {
                    case (Schema.Column.Type._): return isArray;
                    case (Schema.Column.Type.i32): return isIntArray;
                    case (Schema.Column.Type.f32): return isFloatArray;
                    case (Schema.Column.Type.@string): return isStringArray;
                    case (Schema.Column.Type.rid): return isRefArray;
                }
            } else {
                switch (column.type) {
                    case (Schema.Column.Type.@bool): return isBool;
                    case (Schema.Column.Type.i32): return isInt;
                    case (Schema.Column.Type.f32): return isFloat;
                    case (Schema.Column.Type.@string): return isString;
                    case (Schema.Column.Type.rid): return isRef;
                    case (Schema.Column.Type.i16): return isHash16;
                }
            }
            return Error.NONE;
        }

        static Error AnalyseFloat(float f) {
            if (f < 0.00001 && f > -0.00001 && f != 0)
                return Error.VALUE_TOO_SMALL;
            if (f > 1000000000 || f < -1000000000)
                return Error.VALUE_TOO_BIG;
            return Error.NONE;
        }

        static Error AnalyseString(long offset, byte[] varying) {
            if (offset < 8)
                return Error.OFFSET_TOO_SMALL;
            if (offset + 1 >= varying.Length)
                return Error.OFFSET_TOO_BIG;
            if (offset % 2 == 1)
                return Error.OFFSET_NOT_EVEN;
            return Error.NONE;
        }

        static Error AnalyseRef(byte[] data, int offset, int maxRows) {
            long lower = BitConverter.ToInt64(data, offset);
            long upper = BitConverter.ToInt64(data, offset + 8);
            return AnalyseRef(lower, upper, maxRows);
        }

        static Error AnalyseRef(long lower, long upper, int maxRows) {
            if (upper == -72340172838076674 && lower == -72340172838076674)
                return Error.NONE;
            if (upper != 0)
                return Error.UPPER_BYTES_NOT_ZERO;
            if (lower < 0)
                return Error.VALUE_TOO_SMALL;
            if (lower > maxRows)
                return Error.VALUE_TOO_BIG;
            return Error.NONE;
        }


        //todo how to combine with other analyse method
        public static Error AnalyseColumn(Dat dat, Schema.Column column, int maxRows) {
            Error e = Error.NONE;
            int distToEnd = dat.rowWidth - column.offset;
            if (distToEnd < column.Size()) return Error.OOB;
            if(column.array) {
                for(int i = 0; i < dat.rowCount; i++) {
                    long count = BitConverter.ToInt64(dat.data, dat.rowWidth * i + column.offset);
                    long offset = BitConverter.ToInt64(dat.data, dat.rowWidth * i + column.offset + 8);
                    if (count < 0) e = e | Error.COUNT_TOO_SMALL;
                    else if (count > arrayMaxCount) e = e | Error.COUNT_TOO_BIG;
                    else if (offset < 0) e = e | Error.OFFSET_TOO_SMALL;
                    else if (offset + column.TypeSize() * count > dat.varying.Length) e = e | Error.OFFSET_TOO_BIG;
                    else {
                        int intOffset = (int)offset;

                        switch (column.type) {
                            case Schema.Column.Type.f32:
                                for(int a = 0; a < count; a++) {
                                    e = e | AnalyseFloat(BitConverter.ToSingle(dat.varying, intOffset + a * 4));
                                }
                                break;
                            case Schema.Column.Type.@string:
                                for (int a = 0; a < count; a++) {
                                    e = e | AnalyseString(BitConverter.ToInt64(dat.varying, intOffset + a * 8), dat.varying);
                                }
                                break;
                            case Schema.Column.Type.rid:
                                for (int a = 0; a < count; a++) {
                                    e = e | AnalyseRef(dat.varying, intOffset + a * 16, maxRows);
                                }
                                break;
                        }
                    }
                }
            } else {
                switch(column.type) {
                    case Schema.Column.Type.@bool:
                        for(int i = 0; i < dat.rowCount; i++) {
                            if (dat.Row(i)[column.offset] > 1) return Error.VALUE_TOO_BIG;
                        }
                        break;
                    case Schema.Column.Type.f32:
                        for (int i = 0; i < dat.rowCount; i++) {
                            e = e | AnalyseFloat(BitConverter.ToSingle(dat.data, dat.rowWidth * i + column.offset));
                        }
                        break;
                    case Schema.Column.Type.@string:
                        for (int i = 0; i < dat.rowCount; i++) {
                            e = e | AnalyseString(BitConverter.ToInt64(dat.data, dat.rowWidth * i + column.offset), dat.varying);
                        }
                        break;
                    case Schema.Column.Type.rid:
                        for (int i = 0; i < dat.rowCount; i++) {
                            e = e | AnalyseRef(dat.data, dat.rowWidth * i + column.offset, maxRows);
                        }
                        break;
                    case Schema.Column.Type.i16:
                        HashSet<ushort> hashes = new HashSet<ushort>(dat.rowCount);
                        for (int i = 0; i < dat.rowCount; i++) {
                            ushort hash = BitConverter.ToUInt16(dat.data, dat.rowWidth * i + column.offset);
                            if (hashes.Contains(hash)) return Error.VALUES_NOT_UNIQUE;
                            hashes.Add(hash);
                        }
                        break;
                }
            }
            return e;
        }

        void Analyse(Dat dat, int columnOffset, int row, int maxRows, HashSet<ushort> hashValues = null) {
            byte[] data = dat.data;
            byte[] varying = dat.varying;
            int rowWidth = dat.rowWidth;
            int distToEnd = rowWidth - columnOffset;
            int offset = rowWidth * row + columnOffset;

            if (distToEnd <= 0) {
                isBool = Error.OOB;
            } else if (data[offset] > 1) {
                isBool = isBool | Error.VALUE_TOO_BIG;
            }

            if(distToEnd < 2) {
                isHash16 = Error.OOB;
            } else if (hashValues != null) {
                ushort shortValue = BitConverter.ToUInt16(data, offset);
                if (hashValues.Contains(shortValue)) isHash16 = Error.VALUES_NOT_UNIQUE;
                hashValues.Add(shortValue);
            }

            if (distToEnd < 4) {
                isInt = Error.OOB;
                isFloat = Error.OOB;
            } else {
                isFloat = isFloat | AnalyseFloat(BitConverter.ToSingle(data, offset));
            }

            if (distToEnd < 8) {
                isString = Error.OOB;
            } else {
                isString = isString | AnalyseString(BitConverter.ToInt64(data, offset), varying);
            }

            if (distToEnd < 16) {
                isRef = Error.OOB;
                isArray = Error.OOB;
            } else {
                long arrayCount = BitConverter.ToInt64(data, offset);
                long arrayOffset = BitConverter.ToInt64(data, offset + 8);
                int intArrayOffset = (int)arrayOffset;

                Error refResult = AnalyseRef(arrayCount, arrayOffset, maxRows);
                isRef = isRef | refResult;
                if (refResult == Error.NONE && arrayCount > maxRef)
                    maxRef = (int)arrayCount;


                //arrays
                if (arrayCount < 0)
                    isArray = isArray | Error.COUNT_TOO_SMALL;
                else if (arrayCount > arrayMaxCount)
                    isArray = isArray | Error.COUNT_TOO_BIG;
                else if (arrayOffset < 0)
                    isArray = isArray | Error.OFFSET_TOO_SMALL;
                else if (arrayCount > 0) {
                    long arrayDistToEnd = varying.Length - arrayOffset;
                    if (arrayDistToEnd <= 0)
                        isArray = isArray | Error.OFFSET_TOO_BIG;

                    if (arrayDistToEnd < arrayCount * 4) {
                        isIntArray = isIntArray | Error.OFFSET_TOO_BIG;
                        isFloatArray = isFloatArray | Error.OFFSET_TOO_BIG;
                    } else {
                        for (int i = 0; i < arrayCount; i++) {
                            isFloatArray = isFloatArray | AnalyseFloat(BitConverter.ToSingle(varying, intArrayOffset + i * 4));
                        }
                    }

                    if (arrayDistToEnd < arrayCount * 8) {
                        isStringArray = isStringArray | Error.OFFSET_TOO_BIG;
                    } else {
                        for (int i = 0; i < arrayCount; i++) {
                            isStringArray = isStringArray | AnalyseString(BitConverter.ToInt64(varying, intArrayOffset + i * 8), varying);
                        }
                    }

                    if (arrayDistToEnd < arrayCount * 16) {
                        isRefArray = isRefArray | Error.OFFSET_TOO_BIG;
                    } else {
                        for (int i = 0; i < arrayCount; i++) {
                            long lower = BitConverter.ToInt64(varying, intArrayOffset + i * 16);
                            long upper = BitConverter.ToInt64(varying, intArrayOffset + i * 16 + 8);

                            Error refArrayResult = AnalyseRef(lower, upper, maxRows);
                            isRefArray = isRefArray | refArrayResult;
                            if (refArrayResult == Error.NONE && lower > maxRefArray)
                                maxRefArray = (int)lower;
                        }
                    }
                }
            }

            isIntArray = isIntArray | isArray;
            isFloatArray = isFloatArray | isArray;
            isStringArray = isStringArray | isArray;
            isRefArray = isRefArray | isArray;
        }

    }


}
