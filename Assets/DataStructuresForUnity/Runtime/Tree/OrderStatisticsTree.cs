using System;
using System.Collections.Generic;

namespace DataStructuresForUnity.Runtime.Tree {
    // TODO: Not ready for release. Beta only.
    internal sealed class OrderStatisticsTree<K, T> : OrderedDictionary<K, T> where K : IComparable<K> {
        private Dictionary<K, int> SubtreeSizes { get; } = new Dictionary<K, int>();

        private int SubtreeSizeOf(RedBlackTreeEntry<K, T> entry) {
            return entry is null || entry == RedBlackTreeEntry<K, T>.Void || entry.Key is null
                    ? 0
                    : this.SubtreeSizes.GetValueOrDefault(entry.Key, 0);
        }
        
        #region Tree utilities
        
        protected override void PostprocessInsertion(RedBlackTreeEntry<K, T> insertedEntry) {
            base.PostprocessInsertion(insertedEntry);
            this.UpdateSubtreeSizesUpward(insertedEntry.Parent);
        }
    
        protected override void PrepareForRemoval(RedBlackTreeEntry<K, T> entryToRemove) {
            base.PrepareForRemoval(entryToRemove);
            this.UpdateSubtreeSizesUpward(entryToRemove.Parent);
        }
    
        protected override void PostprocessRotation(RedBlackTreeEntry<K, T> oldRoot, RedBlackTreeEntry<K, T> newRoot) {
            base.PostprocessRotation(oldRoot, newRoot);
            this.UpdateSubtreeSize(oldRoot);
            this.UpdateSubtreeSize(newRoot);
        }
    
        private void UpdateSubtreeSizesUpward(RedBlackTreeEntry<K, T> entry) {
            while (entry is not null && entry != RedBlackTreeEntry<K, T>.Void) {
                this.UpdateSubtreeSize(entry);
                entry = entry.Parent;
            }
        }
    
        private void UpdateSubtreeSize(RedBlackTreeEntry<K, T> entry) {
            if (entry == RedBlackTreeEntry<K, T>.Void) {
                return;
            }
        
            int leftSize = this.SubtreeSizeOf(entry.Left);
            int rightSize = this.SubtreeSizeOf(entry.Right);
            this.SubtreeSizes[entry.Key] = leftSize + rightSize + 1;
        }
        
        #endregion

        #region Order Statistics Methods

        /// <summary>
        /// Selects the k-th smallest element in the order statistics tree (0-based index).
        /// </summary>
        /// <param name="k">The 0-based index of the element to be retrieved,
        /// where k must be within the bounds of the collection size.</param>
        /// <returns>The k-th smallest element in the tree.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified index k is less than 0
        /// or greater than or equal to the number of elements in the tree.</exception>
        public KeyValuePair<K, T> Select(int k) {
            if (k < 0 || k >= this.Count) {
                throw new ArgumentOutOfRangeException(nameof(k));
            }

            return select(this.Root, k);

            KeyValuePair<K, T> select(RedBlackTreeEntry<K, T> entry, int rank) {
                if (entry == null) {
                    throw new InvalidOperationException();
                }

                int leftSize = entry.Left is null ? 0 : this.SubtreeSizes[entry.Left.Key];
                if (rank < leftSize) {
                    return select(entry.Left, rank);
                }

                const int multiplicity = 1;
                return rank >= leftSize + multiplicity
                        ? select(entry.Right, rank - leftSize - multiplicity)
                        : new KeyValuePair<K, T>(entry.Key, entry.Element);
            }
        }

        /// <summary>
        /// Determines the rank of a specified element in the order statistics tree.
        /// The rank represents the number of elements in the tree that are less than the specified value.
        /// </summary>
        /// <param name="key">The key whose rank is to be determined. The value must be comparable
        /// to the elements in the tree.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <returns>The 0-based rank of the specified value in the tree.</returns>
        public int Rank(K key, out T value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            return rank(this.Root, out value);

            int rank(RedBlackTreeEntry<K, T> entry, out T value) {
                if (entry == null) {
                    value = default;
                    return 0;
                }
                
                value = entry.Element;
                return key.CompareTo(entry.Key) switch {
                    < 0 => rank(entry.Left, out value),
                    > 0 => this.SubtreeSizeOf(entry.Left) +
                           1 + rank(entry.Right, out value),
                    var _ => this.SubtreeSizeOf(entry.Left)
                };
            }
        }

        #endregion
    }
}
