using System;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoeFormats {
    public class Database {
        string datDirectory;
        Dictionary<string, DatReader> readers;
        Dictionary<string, Object[]> rows;

        public Database(string dir) {
            datDirectory = dir;
            readers = new Dictionary<string, DatReader>();
            rows = new Dictionary<string, object[]>();
        }

        public T Get<T>(string dat, int i) where T : Row, new() {
            if (!rows.ContainsKey(dat)) {
                DatReader r = new DatReader(Path.Combine(datDirectory, dat) + ".dat64");
                readers[dat] = r;
                rows[dat] = new Object[r.rowCount];
            }
            if (rows[dat][i] == null) {
                T row = new T();
                DatReader r = readers[dat];
                r.SeekRow(i);
                row.Read(this, r);
                rows[dat][i] = row;
                return row;
            }
            return (T)rows[dat][i];
        }
    }

    public class Row {
        public Row() { }
        public virtual void Read(Database d, DatReader r) { }
    }

    public class MonsterResistance : Row {
        public static string dat = "monsterresistances";
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
        public static string dat = "monstertypes";
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
            monsterResistancesKey = d.Get<MonsterResistance>(MonsterResistance.dat, r.Ref());
            isLargeAbyssMonster = r.Bool();
            isSmallAbyssMonster = r.Bool();
            unk2 = r.Bool();
        }
    }

}
