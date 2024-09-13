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
                r.SeekRow(i);
                row.Read(this, r);
                table[i] = row;
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
                    r.SeekRow(i);
                    row.Read(this, r);
                    table[i] = row;
                }
            }

            return Unsafe.As<T[]>(table);
        }

    }

    public class Row {
        public Row() { }
        public virtual void Read(Database d, DatReader r) { }
    }

    public class RowId : Row {
        public string id;

        public override void Read(Database d, DatReader r) {
            id = r.String();
        }
    }

    public class SanctumRoomType : Row {
        public string id;
        public bool unk1;
        public bool unk2;
        //2 refs, skip
        public bool unk3;
        public string icon;
        public bool unk4;
        public string description;
        public string[] names;
        public RowId[] rooms;
        public string unk5;
        public bool unk6;

        public override void Read(Database d, DatReader r) {
            id = r.String();
            unk1 = r.Bool();
            unk2 = r.Bool();
            r.Ref();
            r.Ref();
            unk3 = r.Bool();
            icon = r.String();
            unk4 = r.Bool();
            description = r.String();
            names = r.StringArray();
            rooms = d.GetArray<RowId>(r.RefArray(), "sanctumrooms");
            unk5 = r.String();
            unk6 = r.Bool();
        }
    }

    public class MonsterResistance : Row {
        public string id;
        public int fireNormal;
        public int coldNormal;
        public int lightningNormal;
        public int chaosNormal;
        public int fireCruel;
        public int coldCruel;
        public int lightningCruel;
        public int chaosCruel;
        public int fireMerciless;
        public int coldMerciless;
        public int lightningMerciless;
        public int chaosMerciless;

        public override void Read(Database d, DatReader r) {
            id = r.String();
            fireNormal = r.Int();
            coldNormal = r.Int();
            lightningNormal = r.Int();
            chaosNormal = r.Int();
            fireCruel = r.Int();
            coldCruel = r.Int();
            lightningCruel = r.Int();
            chaosCruel = r.Int();
            fireMerciless = r.Int();
            coldMerciless = r.Int();
            lightningMerciless = r.Int();
            chaosMerciless = r.Int();
        }
    }

    public class MonsterType : Row {
        public string id;
        public int unk1;
        public bool isSummoned;
        public int armour;
        public int evasion;
        public int energyShield;
        public int damageSpread;
        public MonsterResistance monsterResistancesKey;
        public bool isLargeAbyssMonster;
        public bool isSmallAbyssMonster;
        public bool unk2;

        public override void Read(Database d, DatReader r) {
            id = r.String();
            unk1 = r.Int();
            isSummoned = r.Bool();
            armour = r.Int();
            evasion = r.Int();
            energyShield = r.Int();
            damageSpread = r.Int();
            monsterResistancesKey = d.Get<MonsterResistance>(r.Ref());
            isLargeAbyssMonster = r.Bool();
            isSmallAbyssMonster = r.Bool();
            unk2 = r.Bool();
        }
    }

}
