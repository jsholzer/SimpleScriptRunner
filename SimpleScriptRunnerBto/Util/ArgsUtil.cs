using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleScriptRunnerBto.Util;

public static class ArgsUtil
{
    public static DateTime? parseDate(this String[] args, params String[] keys)
    {
            String match = args.FirstOrDefault(x => TextHelper.startsWithAny(x, keys));
            if (match == null || !match.Contains("="))
                return null;

            String value = match.Split('=')[1];
            return Convert.ToDateTime(value);
        }

    public static String[] parseSwitches(ref String[] args)
    {
            String[] switches = args.Where(x => x.StartsWith("-")).ToArray();
            args = args.Where(x => !x.StartsWith("-")).ToArray();
            return switches;
        }

    public static Dictionary<String, String> parseDictionary(ref String[] args)
    {
            return asDictionary(parseSwitches(ref args));
        }

    public static Dictionary<String, String> asDictionary(String[] args, StringComparer comparer = null)
    {
            comparer = comparer ?? StringComparer.OrdinalIgnoreCase;
            Dictionary<String, String> result = new Dictionary<String, String>(comparer);
            foreach (String theKey in args)
            {
                String key = theKey;
                String value = null;
                if (key.Contains("="))
                {
                    String[] parts = key.Split('=');
                    key = parts[0];
                    value = parts[1];
                }

                // Sets value
                if (!result.ContainsKey(key))
                    result.Add(key, value);
                else
                    result[key] = value;
            }
            return result;
        }

    public static String value(this Dictionary<String, String> switchDictionary, params String[] keys)
    {
            foreach (String key in keys)
            {
                if (switchDictionary.ContainsKey(key))
                    return switchDictionary[key];
            }
            return null;
        }

    public static int? valueInt(this Dictionary<String, String> switchDictionary, params String[] keys)
    {
            return TextHelper.parseIntNullable(value(switchDictionary, keys));
        }

    public static long? valueLong(this Dictionary<String, String> switchDictionary, params String[] keys)
    {
            return TextHelper.parseLongNullable(value(switchDictionary, keys));
        }

    public static double? valueDouble(this Dictionary<String, String> switchDictionary, params String[] keys)
    {
            return TextHelper.parseDoubleNullable(value(switchDictionary, keys));
        }

    public static bool? valueBool(this Dictionary<String, String> switchDictionary, params String[] keys)
    {
            foreach (String key in keys)
            {
                if (!switchDictionary.ContainsKey(key))
                    continue;

                String val = switchDictionary[key];
                if (val == null)
                    return true;                                // treats as a switch

                return TextHelper.parseBool(val);
            }

            return null;
        }

    public static bool hasAny(this Dictionary<String, String> switchDictionary, params String[] keys)
    {
            return keys.Any(switchDictionary.ContainsKey);
        }
}