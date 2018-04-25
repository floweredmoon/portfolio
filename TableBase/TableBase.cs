using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class TableBase<Table, Schema>
    where Table : class
    where Schema : class
{
    class FieldCache : Dictionary<long, Schema> { }

    static TableBase<Table, Schema> m_Instance;

    public static TableBase<Table, Schema> instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = Activator.CreateInstance(typeof(Table)) as TableBase<Table, Schema>;
            }

            return m_Instance;
        }
    }

    List<Schema> m_SchemaList = new List<Schema>();

    public List<Schema> schemaList
    {
        get
        {
            return m_SchemaList;
        }
    }

    Dictionary<string, FieldInfo> m_FieldInfoDictionary = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
    Dictionary<string, FieldCache> m_FieldCacheDictionary = new Dictionary<string, FieldCache>(StringComparer.Ordinal);

    public virtual bool FromAssetBundle(AssetBundle assetBundle)
    {
        return false;
    }

    public virtual bool FromAssetBundle(AssetBundle assetBundle, string assetName)
    {
        return false;
    }

    public static Schema Query<Key>(string fieldName, Key value, bool log = true, bool caching = true)
    {
        if (instance != null)
        {
            return instance.InternalQuery(fieldName, value, log, caching, true);
        }

        return null;
    }

    Schema InternalQuery<Key>(string fieldName, Key value, bool queryFailLog, bool caching, bool cachingLog)
    {
        long? key = GetKey(value);
        if (key.HasValue)
        {
            FieldInfo fieldInfo;
            if (TryGetFieldInfo(fieldName, out fieldInfo, caching))
            {
                FieldCache fieldCache;
                if (TryGetFieldCache(fieldName, out fieldCache))
                {
                    Schema schema;
                    if (fieldCache.TryGetValue(key.Value, out schema))
                    {
                        return schema;
                    }
                }
                else if (caching)
                {
                    fieldCache = new FieldCache();
                    this.m_FieldCacheDictionary.Add(fieldName, fieldCache);
                }

                if (cachingLog)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning(string.Format("[{0}] FieldCache was not generated. (Field : {1}, Key : {2})", typeof(Table), fieldName, value));
#else
                    UnityEngine.Debug.LogWarning(string.Format("[{0}] FieldCache was not generated. (Field : {1}, Key : {2})\n({3})", typeof(Table), fieldName, value, Environment.StackTrace));
#endif
                }

                foreach (var item in m_SchemaList)
                {
                    object fieldValue = fieldInfo.GetValue(item);
                    if (fieldValue != null)
                    {
                        long? fieldValueKey = GetKey(fieldValue);
                        if (fieldValueKey.HasValue)
                        {
                            if (Equals(key.Value, fieldValueKey.Value))
                            {
                                if (caching)
                                {
                                    fieldCache.Add(key.Value, item);
                                }

                                return item;
                            }
                        }
                    }
                }
            }
        }

        if (queryFailLog)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(string.Format("[{0}] Query failed. (Field : {1}, Key : {2})", typeof(Table), fieldName, value));
#else
            UnityEngine.Debug.LogError(string.Format("[{0}] Query failed. (Field : {1}, Key : {2})\n({3})", typeof(Table), fieldName, value, Environment.StackTrace));
#endif
        }

        return null;
    }

    public static Schema Query<Key1, Key2>(string fieldName1, Key1 value1, string fieldName2, Key2 value2, bool log = true, bool caching = true)
    {
        if (instance != null)
        {
            return instance.InternalQuery(fieldName1, value1, fieldName2, value2, log, caching, true);
        }

        return null;
    }

    Schema InternalQuery<Key1, Key2>(string fieldName1, Key1 value1, string fieldName2, Key2 value2, bool queryFailLog, bool caching, bool cachingLog)
    {
        long? key = GetCompositeKey(value1, value2);
        if (key.HasValue)
        {
            FieldInfo fieldInfo1, fieldInfo2;
            TryGetFieldInfo(fieldName1, out fieldInfo1, caching);
            TryGetFieldInfo(fieldName2, out fieldInfo2, caching);
            if (fieldInfo1 != null && fieldInfo2 != null)
            {
                string fieldName = GetCompositeFieldName(fieldName1, fieldName2);
                FieldCache fieldCache;
                if (TryGetFieldCache(fieldName, out fieldCache))
                {
                    Schema schema;
                    if (fieldCache.TryGetValue(key.Value, out schema))
                    {
                        return schema;
                    }
                }
                else if (caching)
                {
                    fieldCache = new FieldCache();
                    this.m_FieldCacheDictionary.Add(fieldName, fieldCache);
                }

                if (cachingLog)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning(string.Format("[{0}] FieldCache was not generated. (Field : {1}, Key : {2}, Field : {3}, Key : {4})", typeof(Table), fieldName1, value1, fieldName2, value2));
#else
                    UnityEngine.Debug.LogWarning(string.Format("[{0}] FieldCache was not generated. (Field : {1}, Key : {2}, Field : {3}, Key : {4})\n({5})", typeof(Table), fieldName1, value1, fieldName2, value2, Environment.StackTrace));
#endif
                }

                long? key1 = GetKey(value1);
                long? key2 = GetKey(value2);
                foreach (var item in m_SchemaList)
                {
                    object fieldValue1 = fieldInfo1.GetValue(item);
                    object fieldValue2 = fieldInfo2.GetValue(item);
                    if (fieldValue1 != null && fieldValue2 != null)
                    {
                        long? fieldValueKey1 = GetKey(fieldValue1);
                        long? fieldValueKey2 = GetKey(fieldValue2);
                        if (fieldValueKey1.HasValue && fieldValueKey2.HasValue)
                        {
                            if (Equals(key1.Value, fieldValueKey1.Value) && Equals(key2.Value, fieldValueKey2.Value))
                            {
                                if (caching)
                                {
                                    fieldCache.Add(key.Value, item);
                                }

                                return item;
                            }
                        }
                    }
                }
            }
        }

        if (queryFailLog)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(string.Format("[{0}] Query failed. (Field : {1}, Key : {2}, Field : {3}, Key : {4})", typeof(Table), fieldName1, value1, fieldName2, value2));
#else
            UnityEngine.Debug.LogError(string.Format("[{0}] Query failed. (Field : {1}, Key : {2}, Field : {3}, Key : {4})\n({5})", typeof(Table), fieldName1, value1, fieldName2, value2, Environment.StackTrace));
#endif
        }

        return null;
    }

    bool TryGetFieldCache(string fieldName, out FieldCache fieldCache)
    {
        if (this.m_FieldCacheDictionary.TryGetValue(fieldName, out fieldCache))
        {
            return true;
        }

        fieldCache = null;
        return false;
    }

    bool TryGetFieldInfo(string fieldName, out FieldInfo fieldInfo, bool caching)
    {
        if (m_FieldInfoDictionary.TryGetValue(fieldName, out fieldInfo))
        {
            return true;
        }
        else
        {
            fieldInfo = typeof(Schema).GetField(fieldName);
            if (fieldInfo != null)
            {
                if (caching)
                {
                    m_FieldInfoDictionary.Add(fieldName, fieldInfo);
                }

                return true;
            }
        }

        fieldInfo = null;

        return false;
    }

    public bool SetSchemaList(List<Schema> schemaList)
    {
        m_SchemaList = schemaList;

        return (m_SchemaList != null);
    }

    public bool GenerateCache(bool clearCache = true)
    {
        if (clearCache)
        {
            m_FieldCacheDictionary.Clear();
            m_FieldInfoDictionary.Clear();
        }

        FieldInfo[] fieldInfos = typeof(Schema).GetFields();
        if (fieldInfos != null && fieldInfos.Length > 0)
        {
            return GenerateCache(fieldInfos[0].Name);
        }

        return false;
    }

    public bool GenerateCache(string fieldName, bool clearCache = true)
    {
        if (clearCache)
        {
            m_FieldCacheDictionary.Clear();
            m_FieldInfoDictionary.Clear();
        }

        FieldInfo fieldInfo = typeof(Schema).GetField(fieldName);
        if (fieldInfo != null)
        {
            foreach (var item in m_SchemaList)
            {
                object fieldValue = fieldInfo.GetValue(item);
                if (fieldValue != null)
                {
                    InternalQuery(fieldName, fieldValue, true, true, false);
                }
            }

            return true;
        }

        return false;
    }

    public bool GenerateCache(string fieldName1, string fieldName2, bool clearCache = true)
    {
        if (clearCache)
        {
            m_FieldCacheDictionary.Clear();
            m_FieldInfoDictionary.Clear();
        }

        FieldInfo fieldInfo1 = typeof(Schema).GetField(fieldName1);
        FieldInfo fieldInfo2 = typeof(Schema).GetField(fieldName2);
        if (fieldInfo1 != null && fieldInfo2 != null)
        {
            foreach (var item in m_SchemaList)
            {
                object fieldValue1 = fieldInfo1.GetValue(item);
                object fieldValue2 = fieldInfo2.GetValue(item);
                if (fieldValue1 != null && fieldValue2 != null)
                {
                    InternalQuery(fieldName1, fieldValue1, fieldName2, fieldValue2, true, true, false);
                }
            }

            return true;
        }

        return false;
    }

    long? GetKey<Key>(Key value)
    {
        if (typeof(string) != value.GetType())
        {
            return Convert.ToInt64(value);
        }
        else
        {
            return (value as string).ToLower().GetHashCode();
        }
    }

    long? GetCompositeKey<Key1, Key2>(Key1 value1, Key2 value2)
    {
        long? key1 = GetKey(value1);
        long? key2 = GetKey(value2);

        if (key1.HasValue && key2.HasValue)
        {
            return ((long)key1.Value.GetHashCode() << 32) + key2.Value.GetHashCode();
        }

        return null;
    }

    string GetCompositeFieldName(string fieldName1, string fieldName2)
    {
        return string.Format("{0}&{1}", fieldName1, fieldName2);
    }
}
