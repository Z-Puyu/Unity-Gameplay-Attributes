using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructuresForUnity.Runtime.Trie {
    /// <summary>
    /// Represents a generic dictionary-like data structure implemented as a Trie.
    /// Allows efficient storage and retrieval of key-value pairs where keys are sequences of elements.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary. Must implement <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="T">The type of elements in the key sequences.</typeparam>
    /// <typeparam name="V">The type of values stored in the dictionary.</typeparam>
    public class TrieDictionary<K, T, V> : ITrie<K, T>, IDictionary<K, V> where K : IEnumerable<T> {
        private sealed class Entry {
            public SortedList<T, Entry> Children { get; } = new SortedList<T, Entry>(Comparer<T>.Default);
            public bool IsEndOfKey { get; private set; }
            public bool IsEndOfToken { get; set; }
            public bool IsLeaf => this.Children.Count == 0;
            public K Key { get; private set; }
            public V Value { get; private set; }

            public void Empty() {
                this.IsEndOfKey = false;
                this.IsEndOfToken = false;
                this.Children.Clear();
                this.Key = default;
                this.Value = default;
            }

            public void Put(K key, V value) {
                this.Key = key;
                this.Value = value;
                this.IsEndOfKey = true;
                this.IsEndOfToken = true;
            }

            public void EraseEntry() {
                this.Key = default;
                this.Value = default;
                this.IsEndOfKey = false;
            }

            public IEnumerable<KeyValuePair<K, V>> AllChildEntries() {
                Stack<Entry> stack = new Stack<Entry>();
                stack.Push(this);
                while (stack.TryPop(out Entry curr)) {
                    if (curr.IsEndOfKey) {
                        yield return new KeyValuePair<K, V>(curr.Key, curr.Value);
                    }

                    for (int i = this.Children.Count - 1; i >= 0; i -= 1) {
                        stack.Push(this.Children.Values[i]);
                    }
                }
            }
        }

        private Entry Root { get; } = new Entry();
        private T Separator { get; }
        private bool HasSeparator { get; }

        public V this[K key] {
            get {
                if (this.TryGetValue(key, out V value)) {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set => this.Add(key, value);
        }

        public int Count { get; private set; }
        public bool IsReadOnly => false;
        public ICollection<K> Keys => this.Root.AllChildEntries().Select(kv => kv.Key).ToArray();
        public ICollection<V> Values => this.Root.AllChildEntries().Select(kv => kv.Value).ToArray();

        public TrieDictionary() {
            this.HasSeparator = false;
        }

        /// <summary>
        /// Create a TrieDictionary with a pre-defined separator token.
        /// Elements in prefix sequences of a key are produced by splitting at separators.
        /// </summary>
        /// <typeparam name="K">The type of keys in the dictionary.
        /// Must implement <see cref="IEnumerable{T}"/>.</typeparam>
        /// <typeparam name="T">The type of elements in the key sequences.</typeparam>
        /// <typeparam name="V">The type of values stored in the dictionary.</typeparam>
        public TrieDictionary(T separator) {
            this.Separator = separator;
            this.HasSeparator = true;
        }

        IEnumerator<K> IEnumerable<K>.GetEnumerator() {
            return this.Root.AllChildEntries().Select(kv => kv.Key).GetEnumerator();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            return this.Root.AllChildEntries().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified key-value pair to the TrieDictionary.
        /// If the key already exists, its associated value is updated.
        /// </summary>
        /// <param name="item">The pair to be added</param>
        public void Add(KeyValuePair<K, V> item) {
            this.Add(item.Key, item.Value);
        }

        public void Add(K item) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all key-value pairs from the TrieDictionary.
        /// </summary>
        public void Clear() {
            this.Root.Empty();
            this.Count = 0;
        }

        public bool Contains(K item) {
            throw new NotImplementedException();
        }

        public void CopyTo(K[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the specified key-value pair exists in the TrieDictionary.
        /// </summary>
        /// <param name="item">The key-value pair to locate in the dictionary.</param>
        /// <returns>True if the specified key-value pair is found; otherwise, false.</returns>
        public bool Contains(KeyValuePair<K, V> item) {
            return this.TryGetValue(item.Key, out V value) && EqualityComparer<V>.Default.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
            foreach (KeyValuePair<K, V> kv in this) {
                if (arrayIndex >= array.Length) {
                    break;
                }

                array[arrayIndex] = kv;
                arrayIndex += 1;
            }
        }

        /// <summary>
        /// Removes the specified key-value pair from the TrieDictionary.
        /// </summary>
        /// <param name="item">The key-value pair to remove from the TrieDictionary.</param>
        /// <returns>True if the specified key-value pair was successfully removed; otherwise, false.</returns>
        public bool Remove(KeyValuePair<K, V> item) {
            if (this.TryGetValue(item.Key, out V value) && EqualityComparer<V>.Default.Equals(value, item.Value)) {
                return this.Remove(item.Key);
            }

            return false;
        }

        /// <summary>
        /// Adds a key-value pair to the TrieDictionary. If the key already exists, its value will be updated.
        /// </summary>
        /// <param name="key">The key to add to the TrieDictionary.
        /// Must be a sequence type implementing <see cref="IEnumerable{T}"/>.</param>
        /// <param name="value">The value associated with the specified key.</param>
        public void Add(K key, V value) {
            Entry curr = this.Root;
            foreach (T element in key) {
                if (!this.HasSeparator) {
                    curr.IsEndOfToken = true;
                } else if (EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    curr.IsEndOfToken = true;
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    next = new Entry();
                    curr.Children.Add(element, next);
                }

                curr = next;
            }

            if (!curr.IsEndOfKey) {
                this.Count += 1;
            }

            curr.Put(key, value);
        }

        /// <summary>
        /// Determines whether the TrieDictionary contains a specified key.
        /// Note that this is different from <see cref="ContainsPrefix"/>.
        /// A proper prefix of an existing key is not considered as present!
        /// </summary>
        /// <param name="key">The key to locate in the TrieDictionary.</param>
        /// <returns>True if the specified key exists in the TrieDictionary; otherwise, false.</returns>
        public bool ContainsKey(K key) {
            Entry curr = this.Root;
            foreach (T element in key) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfKey;
        }

        /// <summary>
        /// Removes the entry with the specified key from the TrieDictionary.
        /// </summary>
        /// <param name="key">The key of the entry to be removed.</param>
        /// <returns>True if the key was successfully found and removed; otherwise, false.</returns>
        public bool Remove(K key) {
            return this.RemoveFrom(this.Root, key.GetEnumerator());
        }

        private bool RemoveFrom(Entry entry, IEnumerator<T> it) {
            if (!it.MoveNext()) {
                if (!entry.IsEndOfKey) {
                    return false;
                }

                entry.EraseEntry();
                this.Count -= 1;
                return true;
            }

            T token = it.Current;
            if (token == null || (this.HasSeparator && EqualityComparer<T>.Default.Equals(token, this.Separator))) {
                return this.RemoveFrom(entry, it);
            }

            if (!entry.Children.TryGetValue(token, out Entry next)) {
                return false;
            }

            bool hasRemoved = this.RemoveFrom(next, it);
            if (next.IsLeaf && !next.IsEndOfKey) {
                entry.Children.Remove(token);
            }

            return hasRemoved;
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key in the Trie dictionary.
        /// </summary>
        /// <param name="key">The key whose associated value is to be retrieved.
        /// The key must be a sequence of elements compatible with the Trie structure.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key is found in the Trie dictionary; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(K key, out V value) {
            Entry curr = this.Root;
            foreach (T element in key) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    value = default;
                    return false;
                }

                curr = next;
            }

            if (curr.IsEndOfKey) {
                value = curr.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Determines whether the trie set contains any keys with the given prefix.
        /// Unlike <see cref="ContainsKey"/>, this method checks if the specified prefix matches the beginning
        /// of at least one key in the trie set. 
        /// </summary>
        /// <param name="prefix">The sequence of elements representing the prefix to search for in the trie set.</param>
        /// <returns>True if the prefix exists at the start of any key in the trie set; otherwise, false.</returns>
        /// <remarks>
        /// Only prefixes stored in the trie whose last element is marked to be the end of a token are considered valid.
        /// </remarks>
        /// <example>
        /// If the trie uses <c>'.'</c> for the separator and <c>"Item.Weapon.Sword"</c> is added as a key,
        /// then only prefixes <c>"Item.Weapon.Sword"</c>, <c>"Item.Weapon"</c> and <c>"Item"</c> are considered
        /// present in the trie, whereas <c>"Item.Weap"</c> is not present in the trie.
        /// If the trie does not define a separator, any prefix string of an existing key is considered present.
        /// </example>
        public bool ContainsPrefix(IEnumerable<T> prefix) {
            if (prefix == null) {
                return false;
            }

            T[] array = prefix.ToArray();
            if (array[^1].Equals(this.Separator)) {
                return false;
            }

            Entry curr = this.Root;
            foreach (T element in array) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Entry next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfToken;
        }

        public IEnumerable<K> EnumerateWithPrefix(IEnumerable<T> prefix) {
            return this.HasEntry(prefix, out Entry entry) ? entry.AllChildEntries().Select(kv => kv.Key) : Enumerable.Empty<K>();
        }

        public bool RemoveAllWithPrefix(IEnumerable<T> prefix) {
            return prefix is not null && removeFrom(this.Root, prefix.GetEnumerator());
            
            bool removeFrom(Entry entry, IEnumerator<T> it) {
                if (!it.MoveNext()) {
                    if (!entry.IsEndOfToken) {
                        return false;
                    }
                    
                    this.Count -= entry.AllChildEntries().Count();
                    entry.Empty();
                    return true;
                }

                T token = it.Current;
                if (token == null || (this.HasSeparator && EqualityComparer<T>.Default.Equals(token, this.Separator))) {
                    return removeFrom(entry, it); // skip separator
                }

                if (!entry.Children.TryGetValue(token, out Entry next)) {
                    return false;
                }

                bool hasRemoved = removeFrom(next, it);
                if (next.IsLeaf && !next.IsEndOfKey) {
                    entry.Children.Remove(token);
                }

                return hasRemoved;
            }
        }

        public bool Remove(IEnumerable<T> key) {
            return this.RemoveFrom(this.Root, key.GetEnumerator());
        }

        public IEnumerable<K> Enumerate() {
            return this.Root.AllChildEntries().Select(kv => kv.Key);
        }

        public IEnumerable<KeyValuePair<K, V>> CollectAllWithPrefix(IEnumerable<T> prefix) {
            return this.HasEntry(prefix, out Entry entry)
                    ? entry.AllChildEntries()
                    : Enumerable.Empty<KeyValuePair<K, V>>();
        }

        public S Aggregate<S>(IEnumerable<T> prefix, S seed, Func<S, V, S> aggregator) {
            return this.HasEntry(prefix, out Entry entry) 
                    ? entry.AllChildEntries().Aggregate(seed, (current, kv) => aggregator(current, kv.Value))
                    : seed;
        }
        
        public void ForEachWithPrefix(IEnumerable<T> prefix, Action<K, V> action) {
            if (prefix is null || !this.HasEntry(prefix, out Entry entry)) {
                return;
            }

            foreach (KeyValuePair<K, V> kv in entry.AllChildEntries()) {
                action(kv.Key, kv.Value);
            }
        }
        
        private bool HasEntry(IEnumerable<T> prefix, out Entry entry) {
            if (prefix == null) {
                entry = null;
                return false;
            }
            
            T[] array = prefix.ToArray();
            if (this.HasSeparator && array[^1].Equals(this.Separator)) {
                entry = null;
                return false;
            }
            
            entry = this.Root;
            foreach (T element in array) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!entry.Children.TryGetValue(element, out Entry next)) {
                    return false;
                }

                entry = next;
            }
            
            return entry != this.Root && entry.IsEndOfToken;
        }
    }
}
