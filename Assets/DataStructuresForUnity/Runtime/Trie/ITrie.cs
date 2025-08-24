using System.Collections.Generic;

namespace DataStructuresForUnity.Runtime.Trie {
    public interface ITrie<K, in T> : ICollection<K> where K : IEnumerable<T> {
        public bool ContainsPrefix(IEnumerable<T> prefix);
        public IEnumerable<K> EnumerateWithPrefix(IEnumerable<T> prefix);
        public bool RemoveAllWithPrefix(IEnumerable<T> prefix);
        public bool Remove(IEnumerable<T> key);
        public IEnumerable<K> Enumerate();
    }
}
