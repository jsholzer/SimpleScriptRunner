using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleScriptRunnerBto.Util;

public static class MapHelper
{
    public static VType cache<KType, VType>(this Dictionary<KType, VType> map, KType key, Func<VType> builder)
    {
            if (map.ContainsKey(key))
                return map[key];

            VType value = builder();
            map[key] = value;
            return value;
        }

    public static String toValueString<KType, VType>(this Dictionary<KType, VType> map) where KType : IComparable
    {
            StringBuilder result = new StringBuilder();

            foreach (KType key in map.Keys.OrderBy(x => x))
            {
                VType value = map[key];
                if (result.Length > 0)
                    result.Append(",");
                result.Append(key).Append("=").Append(value);
            }
            return result.ToString();
        }

    public static VType getValue<KType, VType>(this Dictionary<KType, VType> map, KType key) where KType : IComparable
    {
            if (map.ContainsKey(key))
                return map[key];
            return default(VType);
        }

    public static void setValue<KType, VType>(this Dictionary<KType, VType> map, KType key, VType value) where KType : IComparable
    {
            if (map.ContainsKey(key))
                map[key] = value;
            else
                map.Add(key, value);
        }

    public static List<KType> findMissingKeys<KType, VType>(this Dictionary<KType, VType> map, IEnumerable<KType> keys)
    {
            List<KType> results = new List<KType>();
            foreach (KType key in keys)
                if (!map.ContainsKey(key))
                    results.Add(key);

            return results;
        }

    public static List<KType> findExtraneousKeys<KType, VType>(this Dictionary<KType, VType> map, ICollection<KType> keys)
    {
            List<KType> results = new List<KType>();
            foreach (KType key in map.Keys)
                if (!keys.Contains(key))
                    results.Add(key);

            return results;
        }
}