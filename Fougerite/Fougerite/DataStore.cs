﻿namespace Fougerite
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public class DataStore
    {
        public Hashtable datastore = new Hashtable();
        private static DataStore instance;
        public static string PATH = Path.Combine(Config.GetPublicFolder(), "FougeriteDatastore.ds");

        public void ToIni(string tablename, IniParser ini)
        {
            string nullref = "__NullReference__";
            Hashtable ht = (Hashtable)this.datastore[tablename];
            if (ht == null || ini == null)
                return;

            foreach (object key in ht.Keys)
            {
                string setting = key.ToString();
                string val = nullref;
                if (ht[setting] != null)
                {
                    float tryfloat;
                    if (float.TryParse((string)ht[setting], out tryfloat))
                    {
                        val = ((float)ht[setting]).ToString("G9");
                    } else
                    {
                        val = ht[setting].ToString();
                    }
                }
                ini.AddSetting(tablename, setting, val);
            }
            ini.Save();
        }

        public void FromIni(IniParser ini)
        {
            foreach (string section in ini.Sections)
            {
                foreach (string key in ini.EnumSection(section))
                {
                    string setting = ini.GetSetting(section, key);
                    float valuef;
                    int valuei;
                    if (float.TryParse(setting, out valuef))
                    {
                        Add(section, key, valuef);
                    } else if (int.TryParse(setting, out valuei))
                    {
                        Add(section, key, valuei);
                    } else if (ini.GetBoolSetting(section, key))
                    {
                        Add(section, key, true);
                    } else if (setting.ToUpperInvariant() == "FALSE")
                    {
                        Add(section, key, false);
                    } else if (setting == "__NullReference__")
                    {
                        Add(section, key, null);
                    } else
                    {
                        Add(section, key, ini.GetSetting(section, key));
                    }
                }
            }
        }

        public void Add(string tablename, object key, object val)
        {
            if (key == null)
                return;

            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
            {
                hashtable = new Hashtable();
                this.datastore.Add(tablename, hashtable);
            }
            hashtable[key] = val;
        }

        public bool ContainsKey(string tablename, object key)
        {
            if (key == null)
                return false;

            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
                return false;

            return hashtable.ContainsKey(key);
        }

        public bool ContainsValue(string tablename, object val)
        {
            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
                return false;

            return hashtable.ContainsValue(val);
        }

        public int Count(string tablename)
        {
            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
            {
                return 0;
            }
            return hashtable.Count;
        }

        public void Flush(string tablename)
        {
            if ((this.datastore[tablename] as Hashtable) != null)
            {
                this.datastore.Remove(tablename);
            }
        }

        public object Get(string tablename, object key)
        {
            if (key == null)
                return null;

            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
                return null;

            return hashtable[key];
        }

        public static DataStore GetInstance()
        {
            if (instance == null)
            {
                instance = new DataStore();
            }
            return instance;
        }

        public Hashtable GetTable(string tablename)
        {
            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
                return null;

            return hashtable;
        }

        public object[] Keys(string tablename)
        {
            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
                return null;

            return (hashtable.Keys as object[]).ToArray<object>();
        }

        public void Load()
        {
            if (File.Exists(PATH))
            {
                try
                {
                    Hashtable hashtable = Util.HashtableFromFile(PATH);
                    this.datastore = hashtable;
                    Util.GetUtil().ConsoleLog("Fougerite DataStore Loaded", false);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        public void Remove(string tablename, object key)
        {
            if (key == null)
                return;

            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable != null)
            {
                hashtable.Remove(key);
            }
        }

        public void Save()
        {
            if (this.datastore.Count != 0)
            {
                Util.HashtableToFile(this.datastore, PATH);
                Util.GetUtil().ConsoleLog("Fougerite DataStore Saved", false);
            }
        }

        public object[] Values(string tablename)
        {
            Hashtable hashtable = this.datastore[tablename] as Hashtable;
            if (hashtable == null)
                return null;

            return (hashtable.Values as object[]).ToArray<object>();
        }
    }
}