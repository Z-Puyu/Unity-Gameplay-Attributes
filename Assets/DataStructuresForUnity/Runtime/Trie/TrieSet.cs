using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructuresForUnity.Runtime.Trie {
    /// <summary>
    /// A set abstract data type implemented using a trie.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="T">The token type, i.e. the type of the element stored at each trie node.</typeparam>
    public sealed class TrieSet<K, T> : ITrie<K, T>, ISet<K> where K : IEnumerable<T> {
        private sealed class Node {
            public SortedList<T, Node> Children { get; } = new SortedList<T, Node>(Comparer<T>.Default);
            public bool IsEndOfKey { get; private set; }
            public bool IsEndOfToken { get; set; }
            public bool IsLeaf => this.Children.Count == 0;
            public K Value { get; private set; }

            public void Empty() {
                this.IsEndOfKey = false;
                this.IsEndOfToken = false;
                this.Children.Clear();
            }

            public void Put(K value) {
                this.Value = value;
                this.IsEndOfKey = true;
                this.IsEndOfToken = true;
            }

            public void EraseKey() {
                this.Value = default;
                this.IsEndOfKey = false;
            }

            public IEnumerable<K> AllChildKeys() {
                Stack<Node> stack = new Stack<Node>();
                stack.Push(this);
                while (stack.TryPop(out Node curr)) {
                    if (curr.IsEndOfKey) {
                        yield return curr.Value;
                    }

                    for (int i = this.Children.Count - 1; i >= 0; i -= 1) {
                        stack.Push(this.Children.Values[i]);
                    }
                }
            }
        }

        private Node Root { get; } = new Node();
        private T Separator { get; }
        private bool HasSeparator { get; }
        public int Count { get; private set; }
        public bool IsReadOnly => false;

        public TrieSet() {
            this.HasSeparator = false;
        }

        /// <summary>
        /// Create a TrieSet with a pre-defined separator token.
        /// Elements in prefix sequences of a key are produced by splitting at separators.
        /// </summary>
        /// <typeparam name="K">The type of keys stored in the trie set,
        /// must implement <see cref="IEnumerable{T}"/>.</typeparam>
        /// <typeparam name="T">The type of elements used to form the keys.</typeparam>
        public TrieSet(T separator) {
            this.Separator = separator;
            this.HasSeparator = true;
        }

        public IEnumerator<K> GetEnumerator() {
            return enumerate(this.Root).GetEnumerator();
            
            IEnumerable<K> enumerate(Node node) {
                if (node.IsEndOfKey) {
                    yield return node.Value;
                }

                foreach (Node child in node.Children.Values) {
                    foreach (K key in enumerate(child)) {
                        yield return key;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds a key to the TrieSet. If the key already exists in the set, it is not added again.
        /// </summary>
        /// <param name="key">The key to be added to the set.
        /// Each key is represented as a sequence of tokens of type <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        public void Add(K key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            Node curr = this.Root;
            foreach (T element in key) {
                if (!this.HasSeparator) {
                    curr.IsEndOfToken = true;
                } else if (EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    curr.IsEndOfToken = true;
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Node next)) {
                    next = new Node(); 
                    curr.Children.Add(element, next);
                }

                curr = next;
            }

            if (curr.IsEndOfKey) {
                return; // The key already exists.
            }

            curr.Put(key);
            this.Count += 1;
        }

        /// <summary>
        /// Removes all elements in the current set that are also in the specified collection.
        /// </summary>
        /// <param name="other">The collection of elements to remove from the set.
        /// Each element from <paramref name="other"/> will be removed if it exists in the current set.</param>
        public void ExceptWith(IEnumerable<K> other) {
            foreach (K key in other) {
                this.Remove(key);
            }
        }

        /// <summary>
        /// Modifies the current TrieSet to contain only elements that are also in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current TrieSet.</param>
        public void IntersectWith(IEnumerable<K> other) {
            HashSet<K> set = other.ToHashSet();
            foreach (K key in this.ToArray()) {
                if (!set.Contains(key)) {
                    this.Remove(key);
                }
            }
        }

        /// <summary>
        /// Determines whether the current set is a proper subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a proper subset of the specified collection;
        /// otherwise, false.
        /// </returns>
        public bool IsProperSubsetOf(IEnumerable<K> other) {
            return other.ToHashSet().IsProperSupersetOf(this);
        }

        /// <summary>
        /// Determines whether the current TrieSet is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current TrieSet.</param>
        /// <returns>True if the current TrieSet is a proper superset of the specified collection; otherwise, false.</returns>
        public bool IsProperSupersetOf(IEnumerable<K> other) {
            return other.ToHashSet().IsProperSubsetOf(this);
        }

        /// <summary>
        /// Determines whether the current set is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// True if the current set is a subset of the specified collection; otherwise, false.
        /// </returns>
        public bool IsSubsetOf(IEnumerable<K> other) {
            return other.ToHashSet().IsSupersetOf(this);
        }

        /// <summary>
        /// Determines whether the current <see cref="TrieSet{K, T}"/> object is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection of keys to compare with the current set.</param>
        /// <returns>
        /// true if the current set is a superset of the specified collection; otherwise, false.
        /// </returns>
        public bool IsSupersetOf(IEnumerable<K> other) {
            return other.ToHashSet().IsSubsetOf(this);
        }

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> containing the elements to compare with the current set.</param>
        /// <returns>True if the current set and the specified collection share at least one common element; otherwise, false.</returns>
        public bool Overlaps(IEnumerable<K> other) {
            return other.ToHashSet().Overlaps(this);
        }

        /// <summary>
        /// Determines whether the current set and a specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>True if the current set is equal to the specified collection; otherwise, false.</returns>
        public bool SetEquals(IEnumerable<K> other) {
            return other.ToHashSet().SetEquals(this);
        }

        /// <summary>
        /// Modifies the current TrieSet to contain only elements that are in either the current TrieSet
        /// or the specified collection, but not both. This operation effectively represents
        /// the symmetric difference of two sets.
        /// </summary>
        /// <param name="other">The collection to compare to the current trie set. Any elements present in both sets
        /// will be removed from the current TrieSet, and elements in the collection
        /// but not in the TrieSet will be added.</param>
        public void SymmetricExceptWith(IEnumerable<K> other) {
            HashSet<K> set = other.ToHashSet();
            foreach (K key in set) {
                if (!this.Contains(key)) {
                    this.Add(key);
                } else {
                    this.Remove(key);
                }
            }
        }

        /// <summary>
        /// Modifies the current set to include all elements that are present in
        /// either the current set or the specified collection.
        /// </summary>
        /// <param name="other">The collection of keys to union with the current set.</param>
        public void UnionWith(IEnumerable<K> other) {
            foreach (K key in other) {
                this.Add(key);
            }
        }

        /// <summary>
        /// Adds the specified key to the trie set. If the key is successfully added,
        /// the trie set's count will increase, and the key will be available for retrieval.
        /// </summary>
        /// <param name="item">The key to be added to the trie set. The key must be non-null and iterable.</param>
        /// <returns>True if the key was successfully added; otherwise, false
        /// (e.g., if the key already exists in the trie set).</returns>
        bool ISet<K>.Add(K item) {
            if (this.Contains(item)) {
                return false;
            }
            
            this.Add(item);
            return true;
        }

        void ICollection<K>.Add(K item) {
            if (item == null) {
                return;
            }
            
            this.Add(item);
        }

        /// <summary>
        /// Removes all keys and elements from the TrieSet, resetting it to an empty state.
        /// </summary>
        public void Clear() {
            this.Root.Empty();
            this.Count = 0;
        }

        /// <summary>
        /// Checks whether the specified key exists in the trie set.
        /// Note that this is different from <see cref="ContainsPrefix"/>.
        /// A proper prefix of an existing key is not considered as present!
        /// </summary>
        /// <param name="key">The key to search for in the trie set.</param>
        /// <returns>True if the key exists in the trie set; otherwise, false.</returns>
        public bool Contains(K key) {
            if (key == null) {
                return false;
            }

            Node curr = this.Root;
            foreach (T element in key) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Node next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfKey;
        }

        /// <summary>
        /// Determines whether the trie set contains any keys with the given prefix.
        /// Unlike <see cref="Contains"/>, this method checks if the specified prefix matches the beginning
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
            if (this.HasSeparator && array[^1].Equals(this.Separator)) {
                return false;
            }
            
            Node curr = this.Root;
            foreach (T element in array) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!curr.Children.TryGetValue(element, out Node next)) {
                    return false;
                }

                curr = next;
            }

            return curr.IsEndOfToken;
        }

        public IEnumerable<K> EnumerateWithPrefix(IEnumerable<T> prefix) {
            return this.HasNode(prefix, out Node node) ? node.AllChildKeys() : Enumerable.Empty<K>();
        }

        public bool RemoveAllWithPrefix(IEnumerable<T> prefix) {
            return prefix is not null && removeFrom(this.Root, prefix.GetEnumerator());
            
            bool removeFrom(Node node, IEnumerator<T> it) {
                if (!it.MoveNext()) {
                    if (!node.IsEndOfToken) {
                        return false;
                    }
                    
                    this.Count -= node.AllChildKeys().Count();
                    node.Empty();
                    return true;
                }

                T token = it.Current;
                if (token == null || (this.HasSeparator && EqualityComparer<T>.Default.Equals(token, this.Separator))) {
                    return removeFrom(node, it); // skip separator
                }

                if (!node.Children.TryGetValue(token, out Node next)) {
                    return false;
                }

                bool hasRemoved = removeFrom(next, it);
                if (next.IsLeaf && !next.IsEndOfKey) {
                    node.Children.Remove(token);
                }

                return hasRemoved;
            }
        }
        
        public bool Remove(IEnumerable<T> key) {
            return key is not null && this.RemoveFrom(this.Root, key.GetEnumerator());
        }

        public IEnumerable<K> Enumerate() {
            return this.Root.AllChildKeys();
        }

        public void CopyTo(K[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length) {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (this.Count > array.Length - arrayIndex) {
                throw new ArgumentException("Insufficient space in target array to copy.");
            }
            
            foreach (K key in this) {
                array[arrayIndex] = key;
                arrayIndex += 1;
            }
        }

        /// <summary>
        /// Removes the specified key from the TrieSet if it exists.
        /// </summary>
        /// <param name="key">The key to be removed from the TrieSet.</param>
        /// <returns>Returns true if the key was successfully removed, otherwise false.</returns>
        public bool Remove(K key) {
            return key != null && this.RemoveFrom(this.Root, key.GetEnumerator());
        }

        private bool RemoveFrom(Node node, IEnumerator<T> it) {
            if (!it.MoveNext()) {
                if (!node.IsEndOfKey) {
                    return false;
                }

                node.EraseKey();
                this.Count -= 1;
                return true;
            }

            T token = it.Current;
            if (token == null || (this.HasSeparator && EqualityComparer<T>.Default.Equals(token, this.Separator))) {
                return this.RemoveFrom(node, it); // skip separator
            }

            if (!node.Children.TryGetValue(token, out Node next)) {
                return false;
            }

            bool hasRemoved = this.RemoveFrom(next, it);
            if (next.IsLeaf && !next.IsEndOfKey) {
                node.Children.Remove(token);
            }

            return hasRemoved;
        }
        
        public void ForEachWithPrefix(IEnumerable<T> prefix, Action<K> action) {
            if (prefix is null || !this.HasNode(prefix, out Node node)) {
                return;
            }
            
            foreach (K key in node.AllChildKeys()) {
                action(key);
            }
        }

        private bool HasNode(IEnumerable<T> prefix, out Node node) {
            if (prefix == null) {
                node = null;
                return false;
            }
            
            T[] array = prefix.ToArray();
            if (this.HasSeparator && array[^1].Equals(this.Separator)) {
                node = null;
                return false;
            }
            
            node = this.Root;
            foreach (T element in array) {
                if (this.HasSeparator && EqualityComparer<T>.Default.Equals(element, this.Separator)) {
                    continue;
                }

                if (!node.Children.TryGetValue(element, out Node next)) {
                    return false;
                }

                node = next;
            }
            
            return node != this.Root && node.IsEndOfToken;
        }
    }
}
