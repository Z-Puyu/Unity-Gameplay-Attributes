using System;
using System.Collections.Generic;

namespace DataStructuresForUnity.Runtime.Tree {
    public class RedBlackTreeEntry<K, V> where K : IComparable<K> {
        public static readonly RedBlackTreeEntry<K, V> Void = new RedBlackTreeEntry<K, V>(default, default);
        
        internal K Key { get; }
        internal V Element { get; set; }
        internal RedBlackTreeEntry<K, V> Left { get; set; }
        internal RedBlackTreeEntry<K, V> Right { get; set; }
        internal RedBlackTreeEntry<K, V> Parent { get; set; }
        internal bool IsRed { get; set; }
        internal bool IsBlack => !this.IsRed;

        internal RedBlackTreeEntry(K key, V element, RedBlackTreeEntry<K, V> parent = null) {
            this.Key = key;
            this.Element = element;
            this.Parent = parent;
            this.IsRed = true;
            this.Left = RedBlackTreeEntry<K, V>.Void;
            this.Right = RedBlackTreeEntry<K, V>.Void;
        }

        internal RedBlackTreeEntry<K, V> LeastChild() {
            RedBlackTreeEntry<K, V> min = this;
            while (min.Left is not null && min.Left != RedBlackTreeEntry<K, V>.Void) {
                min = min.Left;
            }

            return min;
        }

        internal RedBlackTreeEntry<K, V> GreatestChild() {
            RedBlackTreeEntry<K, V> max = this;
            while (max.Right is not null && max.Right != RedBlackTreeEntry<K, V>.Void) {
                max = max.Right;
            }

            return max;
        }

        internal IEnumerable<KeyValuePair<K, V>> InOrderTraversal() {
            if (this.Left is not null) {
                foreach (KeyValuePair<K, V> item in this.Left.InOrderTraversal()) {
                    yield return item;
                }
            }

            yield return this;
            if (this.Right is null) {
                yield break;
            }

            foreach (KeyValuePair<K, V> item in this.Right.InOrderTraversal()) {
                yield return item;
            }
        }

        public override string ToString() {
            return $"{{{this.Key}: {this.Element}}}";
        }
        
        public static implicit operator KeyValuePair<K, V>(RedBlackTreeEntry<K, V> entry) {
            return new KeyValuePair<K, V>(entry.Key, entry.Element); 
        }
        
        public static implicit operator (K key, V value)(RedBlackTreeEntry<K, V> entry) {
            return (entry.Key, entry.Element); 
        }
    }
}
