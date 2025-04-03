using System.Collections.Generic;
using System;

namespace ManagedDoom {
    public static class Ext {
        public static int Clamp(int val, int min, int max) {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
            if (dictionary == null) {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (!dictionary.ContainsKey(key)) {
                dictionary.Add(key, value);
                return true;
            }
            return false;
        }
    }
}