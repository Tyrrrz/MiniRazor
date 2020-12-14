using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniRazor.Internal
{
    internal class Cache<TKey, TValue> where TKey : notnull
    {
        private readonly int _maxCount;
        private readonly Dictionary<TKey, TValue> _dictionary = new();

        public Cache(int maxCount = int.MaxValue) => _maxCount = maxCount;

        public TValue GetOrSet(TKey key, Func<TValue> getValue)
        {
            if (_dictionary.TryGetValue(key, out var value))
                return value;

            if (_dictionary.Count > 0 && _dictionary.Count >= _maxCount)
                _dictionary.Remove(_dictionary.Keys.First());

            return _dictionary[key] = getValue();
        }
    }
}