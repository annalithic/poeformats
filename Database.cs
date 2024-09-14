using System.Runtime.CompilerServices;

namespace PoeFormats {
    public class Database {
        string datDirectory;
        Dictionary<string, DatReader> readers;
        Dictionary<string, Object[]> tables;
        Dictionary<string, string> typeDats;

        public Database(string dir) {
            datDirectory = dir;
            readers = new Dictionary<string, DatReader>();
            tables = new Dictionary<string, object[]>();

            typeDats = new Dictionary<string, string>();
            foreach(string line in File.ReadAllLines(@"F:\Extracted\PathOfExile\datclassnames.txt")) {
                string[] words = line.Split('\t');
                typeDats[words[1]] = words[0];
            }
        }

        public T[] GetArray<T>(int[] indices, string dat = null) where T : Row, new() {
            T[] rows = new T[indices.Length];
            for(int i = 0; i < indices.Length; i++) {
                rows[i] = Get<T>(indices[i], dat);
            }
            return rows;
        }


        public T Get<T>(int i, string dat = null) where T : Row, new() {
            if (i == -16843010) return null;
            if(dat == null) dat = typeDats[typeof(T).Name];


            DatReader r;
            object[] table;

            if (tables.ContainsKey(dat)) {
                r = readers[dat];
                table = tables[dat];
            }
            else {
                r = new DatReader(Path.Combine(datDirectory, dat) + ".dat64");
                readers[dat] = r;
                table = new Object[r.rowCount];
                tables[dat] = table;
            }

            if (table[i] == null) {

                T row = new T();
                table[i] = row;
                row.Read(this, r, i);
                return row;
            }
            return (T)table[i];
        }

        public T[] GetAll<T>() where T : Row, new() {
            string dat = typeDats[typeof(T).Name];

            DatReader r;
            object[] table;

            if (tables.ContainsKey(dat)) {
                r = readers[dat];
                table = tables[dat];
            }
            else {
                r = new DatReader(Path.Combine(datDirectory, dat) + ".dat64");
                readers[dat] = r;
                table = new Object[r.rowCount];
                tables[dat] = table;
            }
            for(int i = 0; i < table.Length; i++) {
                if (table[i] == null) {
                    T row = new T();
                    table[i] = row;
                    row.Read(this, r, i);
                }
            }

            return Unsafe.As<T[]>(table);
        }

    }

    public class Row {
        public Row() { }
        public virtual void Read(Database d, DatReader r) { }

        public void Read(Database d, DatReader r, int i) {
            long tempOffset = r.r.BaseStream.Position; //TODO jank
            r.SeekRow(i);
            Read(d, r);
            r.r.BaseStream.Seek(tempOffset, SeekOrigin.Begin);
        }
    }

    public class RowId : Row {
        public string id;

        public override void Read(Database d, DatReader r) {
            id = r.String();
        }
    }
}
