using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructuresForUnity.Runtime.Tree {
    /// <summary>
    /// An ordered dictionary based on a red-black tree. Support common successor and predecessor queries.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="T">The value type.</typeparam>
    public class OrderedDictionary<K, T> : IDictionary<K, T> where K : IComparable<K> {
        protected RedBlackTreeEntry<K, T> Root { get; set; } = RedBlackTreeEntry<K, T>.Void;

        #region Tree semantics

        /// <summary>
        /// Replaces one subtree with another.
        /// </summary>
        /// <param name="oldRoot">The root of the replaced tree.</param>
        /// <param name="newRoot">The root of the other tree</param>
        private void Transplant(RedBlackTreeEntry<K, T> oldRoot, RedBlackTreeEntry<K, T> newRoot) {
            if (oldRoot is null || newRoot is null) {
                return;
            }

            RedBlackTreeEntry<K, T> parent = oldRoot.Parent;
            if (parent is null || parent == RedBlackTreeEntry<K, T>.Void) {
                this.Root = newRoot;
            } else if (oldRoot == parent.Left) {
                parent.Left = newRoot;
            } else {
                parent.Right = newRoot;
            }

            newRoot.Parent = parent;
        }

        private bool RemoveNode(RedBlackTreeEntry<K, T> entry) {
            if (entry is null || entry == RedBlackTreeEntry<K, T>.Void) {
                return false;
            }
            
            RedBlackTreeEntry<K, T> successorChild;
            RedBlackTreeEntry<K, T> successor = entry;
            bool isSuccessorRed = entry.IsRed;

            if (entry.Left is null || entry.Left == RedBlackTreeEntry<K, T>.Void) {
                successorChild = entry.Right;
                this.Transplant(entry, entry.Right);
            } else if (entry.Right is null || entry.Right == RedBlackTreeEntry<K, T>.Void) {
                successorChild = entry.Left;
                this.Transplant(entry, entry.Left);
            } else {
                successor = entry.Right.LeastChild();
                isSuccessorRed = successor.IsRed;
                successorChild = successor.Right;

                if (successor != entry.Right) {
                    this.Transplant(successor, successor.Right);
                    successor.Right = entry.Right;
                    successor.Right.Parent = successor;
                } else {
                    successorChild.Parent = successor;
                }

                this.Transplant(entry, successor);
                successor.Left = entry.Left;
                successor.Left.Parent = successor;
                successor.IsRed = entry.IsRed;
            }

            if (!isSuccessorRed) {
                this.RebalanceAfterRemoval(successorChild);
            }
            
            return true;
        }

        private void RebalanceAfterRemoval(RedBlackTreeEntry<K, T> entry) {
            while (entry != this.Root && entry.IsBlack) {
                RedBlackTreeEntry<K, T> parent = entry.Parent;
                if (entry == parent.Left) {
                    RedBlackTreeEntry<K, T> sibling = parent.Right;
                    if (sibling is null || sibling == RedBlackTreeEntry<K, T>.Void) {
                        break;
                    }
                    
                    if (sibling.IsRed) {
                        sibling.IsRed = false;
                        parent.IsRed = true;
                        this.RotateLeft(parent);
                        sibling = parent.Right;
                    }

                    if ((sibling.Left is null || sibling.Left.IsBlack) &&
                        (sibling.Right is null || sibling.Right.IsBlack)) {
                        sibling.IsRed = true;
                        entry = parent;
                    } else {
                        if (sibling.Right is null || sibling.Right.IsBlack) {
                            if (sibling.Left is not null) {
                                sibling.Left.IsRed = false;
                            }
                            
                            sibling.IsRed = true;
                            this.RotateRight(sibling);
                            sibling = parent.Right;
                        }

                        sibling.IsRed = parent.IsRed;
                        parent.IsRed = false;
                        sibling.Right.IsRed = false;
                        this.RotateLeft(parent);
                        entry = this.Root;
                    }
                } else {
                    RedBlackTreeEntry<K, T> sibling = parent.Left;
                    if (sibling is null || sibling == RedBlackTreeEntry<K, T>.Void) {
                        break;
                    }
                    
                    if (sibling.IsRed) {
                        sibling.IsRed = false;
                        parent.IsRed = true;
                        this.RotateRight(parent);
                        sibling = parent.Left;
                    }

                    if ((sibling.Right is null || sibling.Right.IsBlack) &&
                        (sibling.Left is null || sibling.Left.IsBlack)) {
                        sibling.IsRed = true;
                        entry = parent;
                    } else {
                        if (sibling.Left is null || sibling.Left.IsBlack) {
                            if (sibling.Right is not null) {
                                sibling.Right.IsRed = false;
                            }
                            
                            sibling.IsRed = true;
                            this.RotateLeft(sibling);
                            sibling = parent.Left;
                        }

                        sibling.IsRed = parent.IsRed;
                        parent.IsRed = false;
                        sibling.Left.IsRed = false;
                        this.RotateRight(parent);
                        entry = this.Root;
                    }
                }
            }

            entry.IsRed = false;
        }

        private void RebalanceAfterInsertion(RedBlackTreeEntry<K, T> entry) {
            RedBlackTreeEntry<K, T> parent = entry.Parent;
            while (parent is not null && parent != RedBlackTreeEntry<K, T>.Void && parent.IsRed) {
                RedBlackTreeEntry<K, T> grandParent = parent.Parent;
                if (parent == grandParent.Left) {
                    RedBlackTreeEntry<K, T> uncle = grandParent.Right;
                    if (uncle.IsRed) {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandParent.IsRed = true;
                        entry = grandParent;
                    } else {
                        if (entry == parent.Right) {
                            entry = parent;
                            this.RotateLeft(entry);
                            parent = entry.Parent;
                            grandParent = parent.Parent;
                        }

                        parent.IsRed = false;
                        grandParent.IsRed = true;
                        this.RotateRight(grandParent);
                    }
                } else {
                    RedBlackTreeEntry<K, T> uncle = grandParent.Left;
                    if (uncle.IsRed) {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandParent.IsRed = true;
                        entry = grandParent;
                    } else {
                        if (entry == parent.Left) {
                            entry = parent;
                            this.RotateRight(entry);
                            parent = entry.Parent;
                            grandParent = parent.Parent;       
                        }

                        parent.IsRed = false;
                        grandParent.IsRed = true;
                        this.RotateLeft(grandParent);
                    }
                }
                
                parent = entry.Parent;
            }

            this.Root.IsRed = false;
        }

        private void RotateLeft(RedBlackTreeEntry<K, T> entry) {
            RedBlackTreeEntry<K, T> rightChild = entry.Right;
            entry.Right = rightChild.Left;

            if (rightChild.Left != RedBlackTreeEntry<K, T>.Void) {
                rightChild.Left.Parent = entry;
            }

            rightChild.Parent = entry.Parent;
            if (entry.Parent == RedBlackTreeEntry<K, T>.Void) {
                this.Root = rightChild;
            } else if (entry == entry.Parent.Left) {
                entry.Parent.Left = rightChild;
            } else {
                entry.Parent.Right = rightChild;
            }

            rightChild.Left = entry;
            entry.Parent = rightChild;
            this.PostprocessRotation(entry, rightChild);
        }

        private void RotateRight(RedBlackTreeEntry<K, T> entry) {
            RedBlackTreeEntry<K, T> leftChild = entry.Left;
            entry.Left = leftChild.Right;

            if (leftChild.Right != RedBlackTreeEntry<K, T>.Void) {
                leftChild.Right.Parent = entry;
            }

            leftChild.Parent = entry.Parent;
            if (entry.Parent == RedBlackTreeEntry<K, T>.Void) {
                this.Root = leftChild;
            } else if (entry == entry.Parent.Right) {
                entry.Parent.Right = leftChild;
            } else {
                entry.Parent.Left = leftChild;
            }

            leftChild.Right = entry;
            entry.Parent = leftChild;
            this.PostprocessRotation(entry, leftChild);
        }

        protected RedBlackTreeEntry<K, T> MakeNode(K key, T value, RedBlackTreeEntry<K, T> parent) {
            return new RedBlackTreeEntry<K, T>(key, value, parent);
        }

        private bool ContainsNode(K key, out RedBlackTreeEntry<K, T> entry) {
            RedBlackTreeEntry<K, T> curr = this.Root;
            entry = RedBlackTreeEntry<K, T>.Void;
            while (curr is not null && curr != RedBlackTreeEntry<K, T>.Void) {
                entry = curr;
                switch (key.CompareTo(curr.Key)) {
                    case 0:
                        return true;
                    case < 0:
                        curr = curr.Left;
                        break;
                    default:
                        curr = curr.Right;
                        break;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Insert a key-value pair into the tree and return the insertion point.
        /// </summary>
        /// <param name="key">The key of the node.</param>
        /// <param name="value">The value of the node.</param>
        /// <param name="replace">Whether to replace existing values.</param>
        /// <returns>The node at which the pair has been inserted.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <c>null</c>.</exception>
        private void Insert(K key, T value, bool replace = true) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }
            
            if (!this.ContainsNode(key, out RedBlackTreeEntry<K, T> node)) {
                RedBlackTreeEntry<K, T> entry = this.MakeNode(key, value, node);
                if (node is null || node == RedBlackTreeEntry<K, T>.Void) {
                    // This means that the traversal above did not happen, i.e., the tree is empty.
                    this.Root = entry;
                } else if (key.CompareTo(node.Key) < 0) {
                    node.Left = entry;
                } else {
                    node.Right = entry;
                }

                this.PostprocessInsertion(entry);
                this.RebalanceAfterInsertion(entry);
                this.Count += 1;
            } else if (replace) {
                node.Element = value;
            }
            
            this.Keys.Add(key);
        }

        #endregion

        #region BST features

        /// <summary>
        /// Search for the smallest key that is strictly greater than a given key.
        /// </summary>
        /// <param name="key">The key to find a strict successor for.</param>
        /// <param name="successor">The key strictly greater than the given key, if it exists.</param>
        /// <returns>True if the strict successor is found; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        public bool HasStrictSuccessorOf(K key, out KeyValuePair<K, T> successor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            successor = default;
            RedBlackTreeEntry<K, T> entry = this.Root;
            bool found = false;
            while (entry is not null && entry != RedBlackTreeEntry<K, T>.Void) {
                if (key.CompareTo(entry.Key) < 0) {
                    // node.Value < key -> possible successor, but keep searching left for a larger one
                    successor = entry;
                    found = true;
                    entry = entry.Left;
                } else {
                    // node.Value >= key -> a successor, if any, is in the right subtree
                    entry = entry.Right;
                }
            }
            
            return found;
        }

        public bool HasStrictPredecessorOf(K key, out KeyValuePair<K, T> predecessor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }
            
            predecessor = default;
            RedBlackTreeEntry<K, T> entry = this.Root;
            bool found = false;
            while (entry is not null && entry != RedBlackTreeEntry<K, T>.Void) {
                if (key.CompareTo(entry.Key) > 0) {
                    // node.Value > key -> possible predecessor, but keep searching right for a smaller one
                    predecessor = entry;
                    found = true;
                    entry = entry.Right;
                } else {
                    // node.Value <= key -> a predecessor, if any, is in the left subtree
                    entry = entry.Left;
                }
            }

            return found;
        }

        /// <summary>
        /// Searches for the smallest key that is greater than or equal to a given key.
        /// </summary>
        /// <param name="key">The key to find a weak successor for.</param>
        /// <param name="successor">The key greater than or equal to the given key, if it exists.</param>
        /// <returns>True if a weak successor is found; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        public bool HasWeakSuccessorOf(K key, out KeyValuePair<K, T> successor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            successor = default;
            RedBlackTreeEntry<K, T> entry = this.Root;
            bool found = false;
            while (entry is not null) {
                int cmp = key.CompareTo(entry.Key);
                switch (cmp) {
                    case < 0:
                        // node.Value > key -> possible successor, but keep searching left for a smaller one
                        successor = entry;
                        found = true;
                        entry = entry.Left;
                        break;
                    case > 0:
                        // node.Value <= key -> a successor, if any, is in the right subtree
                        entry = entry.Right;
                        break;
                    case 0:
                        successor = entry;
                        return true;
                }
            }

            return found;
        }

        public bool HasWeakPredecessorOf(K key, out KeyValuePair<K, T> predecessor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            predecessor = default;
            RedBlackTreeEntry<K, T> entry = this.Root;
            bool found = false;
            while (entry is not null) {
                int cmp = key.CompareTo(entry.Key);
                switch (cmp) {
                    case > 0:
                        // node.Value < key -> possible predecessor, but keep searching right for a larger one
                        predecessor = entry;
                        found = true;
                        entry = entry.Right;
                        break;
                    case < 0:
                        // node.Value >= key -> a predecessor, if any, is in the left subtree
                        entry = entry.Left;
                        break;
                    case 0:
                        predecessor = entry;
                        return true;
                }
            }

            return found;
        }

        /// <summary>
        /// Returns the first (smallest) element in the set, or default if the set is empty.
        /// </summary>
        /// <returns>The smallest element in the set, or default if empty.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the set is empty.</exception>
        public KeyValuePair<K, T> First() {
            if (this.Root is null) {
                throw new InvalidOperationException("Cannot get first element of empty set");
            }

            return this.Root.LeastChild();
        }

        /// <summary>
        /// Returns the last (largest) element in the set, or default if the set is empty.
        /// </summary>
        /// <returns>The largest element in the set, or default if empty.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the set is empty</exception>
        public KeyValuePair<K, T> Last() {
            if (this.Root is null) {
                throw new InvalidOperationException("Cannot get last element of empty set");
            }

            return this.Root.GreatestChild();
        }

        /// <summary>
        /// Removes and returns the first (smallest) element in the set, or default if the set is empty.
        /// </summary>
        /// <returns>The smallest element which was removed, or default if empty.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the set is empty</exception>
        public KeyValuePair<K, T> PollFirst() {
            if (this.Count == 0) {
                throw new InvalidOperationException("Cannot poll first element of empty set");
            }

            KeyValuePair<K, T> first = this.First();
            this.Remove(first.Key);
            return first;
        }

        /// <summary>
        /// Removes and returns the last (largest) element in the set, or default if the set is empty.
        /// </summary>
        /// <returns>The largest element which was removed, or default if empty.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the set is empty</exception>
        public KeyValuePair<K, T> PollLast() {
            if (this.Count == 0) {
                throw new InvalidOperationException("Cannot poll last element of empty set");
            }

            KeyValuePair<K, T> last = this.Last();
            this.Remove(last.Key);
            return last;
        }

        #endregion

        #region Template methods

        protected virtual void PostprocessInsertion(RedBlackTreeEntry<K, T> insertedEntry) { }
        protected virtual void PrepareForRemoval(RedBlackTreeEntry<K, T> entryToRemove) { }

        protected virtual void PostprocessRotation(
            RedBlackTreeEntry<K, T> oldRoot, RedBlackTreeEntry<K, T> newRoot
        ) { }

        #endregion

        #region Dictionary semantics
        
        public IEnumerator<KeyValuePair<K, T>> GetEnumerator() {
            return this.Root.InOrderTraversal().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
        
        public void Add(KeyValuePair<K, T> item) {
            this.Insert(item.Key, item.Value, false);
        }
        
        public void Clear() {
            this.Root = RedBlackTreeEntry<K, T>.Void;
            this.Count = 0;
        }
        
        public bool Contains(KeyValuePair<K, T> item) {
            return item.Key != null && this.ContainsNode(item.Key, out RedBlackTreeEntry<K, T> entry) &&
                   entry.Element.Equals(item.Value);
        }
        
        public void CopyTo(KeyValuePair<K, T>[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length) {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Length - arrayIndex < this.Count) {
                throw new ArgumentException("Insufficient space to copy all the items to the array.");
            }
            
            foreach (KeyValuePair<K, T> item in this) {
                array[arrayIndex] = item;
                arrayIndex += 1;
            }
        }
        
        public bool Remove(KeyValuePair<K, T> item) {
            return item.Key != null && this.Remove(item.Key);
        }
        
        public int Count { get; private set; }
        public bool IsReadOnly => false;
        
        /// <summary>
        /// Removes a key from the tree.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns><c>true</c> if the key was present and has been successfully removed,
        /// <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <c>null</c>.</exception>
        public bool Remove(K key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            if (!this.ContainsNode(key, out RedBlackTreeEntry<K, T> node)) {
                return false;
            }

            this.PrepareForRemoval(node);
            if (!this.RemoveNode(node)) {
                return false;
            }
            
            this.Count -= 1;
            this.Keys.Remove(key);
            return true;
        }
        
        public void Add(K key, T value) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.Insert(key, value, false);
        }

        public bool ContainsKey(K key) {
            RedBlackTreeEntry<K, T> entry = this.Root;
            while (entry is not null && entry != RedBlackTreeEntry<K, T>.Void) {
                switch (key.CompareTo(entry.Key)) {
                    case 0:
                        return true;
                    case < 0:
                        entry = entry.Left;
                        break;
                    default:
                        entry = entry.Right;
                        break;
                }
            }
            
            return false;
        }

        public bool TryGetValue(K key, out T value) {
            RedBlackTreeEntry<K, T> entry = this.Root;
            while (entry is not null && entry != RedBlackTreeEntry<K, T>.Void) {
                switch (key.CompareTo(entry.Key)) {
                    case 0:
                        value = entry.Element;
                        return true;
                    case < 0:
                        entry = entry.Left;
                        break;
                    default:
                        entry = entry.Right;
                        break;
                }
            }
            
            value = default;
            return false;
        }

        public T this[K key] {
            get => this.TryGetValue(key, out T value) ? value : throw new KeyNotFoundException(key.ToString());
            set => this.Insert(key, value);
        }

        public ICollection<K> Keys { get; } = new SortedSet<K>();
        public ICollection<T> Values => this.Root.InOrderTraversal().Select(entry => entry.Value).ToArray();
        
        #endregion
    }
}
