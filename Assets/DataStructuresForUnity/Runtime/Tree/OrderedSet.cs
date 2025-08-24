using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataStructuresForUnity.Runtime.Tree {
    /// <summary>
    /// An ordered dictionary based on a red-black tree. Support common successor and predecessor queries.
    /// </summary>
    /// <typeparam name="T">The key type.</typeparam>
    public class OrderedSet<T> : ISet<T> where T : IComparable<T> {
        protected RedBlackTreeNode<T> Root { get; set; } = RedBlackTreeNode<T>.Void;

        #region Tree semantics

        /// <summary>
        /// Replaces one subtree with another.
        /// </summary>
        /// <param name="oldRoot">The root of the replaced tree.</param>
        /// <param name="newRoot">The root of the other tree</param>
        private void Transplant(RedBlackTreeNode<T> oldRoot, RedBlackTreeNode<T> newRoot) {
            if (oldRoot is null || newRoot is null) {
                return;
            }

            RedBlackTreeNode<T> parent = oldRoot.Parent;
            if (parent is null || parent == RedBlackTreeNode<T>.Void) {
                this.Root = newRoot;
            } else if (oldRoot == parent.Left) {
                parent.Left = newRoot;
            } else {
                parent.Right = newRoot;
            }

            newRoot.Parent = parent;
        }

        private bool RemoveNode(RedBlackTreeNode<T> node) {
            if (node is null || node == RedBlackTreeNode<T>.Void) {
                return false;
            }

            RedBlackTreeNode<T> child = null;
            RedBlackTreeNode<T> next = node;
            bool isNextNodeRed = next.IsRed;

            if (node.Left is null || node.Left == RedBlackTreeNode<T>.Void) {
                child = node.Right;
                this.Transplant(node, node.Right);
            } else if (node.Right is null || node.Right == RedBlackTreeNode<T>.Void) {
                child = node.Left;
                this.Transplant(node, node.Left);
            } else {
                next = node.Right.LeastChild();
                isNextNodeRed = next.IsRed;
                child = next.Right;

                if (next != node.Right) {
                    this.Transplant(next, next.Right);
                    next.Right = node.Right;
                    next.Right.Parent = next;
                } else {
                    child.Parent = next;
                }

                this.Transplant(node, next);
                next.Left = node.Left;
                next.Left.Parent = next;
                next.IsRed = node.IsRed;
            }

            if (!isNextNodeRed) {
                this.RebalanceAfterRemoval(child);
            }

            return true;
        }

        private void RebalanceAfterRemoval(RedBlackTreeNode<T> node) {
            while (node != this.Root && node.IsBlack) {
                RedBlackTreeNode<T> parent = node.Parent;
                if (node == parent.Left) {
                    RedBlackTreeNode<T> sibling = parent.Right;
                    if (sibling is null || sibling == RedBlackTreeNode<T>.Void) {
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
                        node = parent;
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
                        node = this.Root;
                    }
                } else {
                    RedBlackTreeNode<T> sibling = parent.Left;
                    if (sibling is null || sibling == RedBlackTreeNode<T>.Void) {
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
                        node = parent;
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
                        node = this.Root;
                    }
                }
            }

            node.IsRed = false;
        }

        private void RebalanceAfterInsertion(RedBlackTreeNode<T> node) {
            RedBlackTreeNode<T> parent = node.Parent;
            while (parent is not null && parent != RedBlackTreeNode<T>.Void && parent.IsRed) {
                RedBlackTreeNode<T> grandParent = parent.Parent;
                if (parent == grandParent.Left) {
                    RedBlackTreeNode<T> uncle = grandParent.Right;
                    if (uncle.IsRed) {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandParent.IsRed = true;
                        node = grandParent;
                    } else {
                        if (node == parent.Right) {
                            node = parent;
                            this.RotateLeft(node);
                            parent = node.Parent;
                            grandParent = parent.Parent;
                        }

                        parent.IsRed = false;
                        grandParent.IsRed = true;
                        this.RotateRight(grandParent);
                    }
                } else {
                    RedBlackTreeNode<T> uncle = grandParent.Left;
                    if (uncle.IsRed) {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandParent.IsRed = true;
                        node = grandParent;
                    } else {
                        if (node == parent.Left) {
                            node = parent;
                            this.RotateRight(node);
                            parent = node.Parent;
                            grandParent = parent.Parent;
                        }

                        parent.IsRed = false;
                        grandParent.IsRed = true;
                        this.RotateLeft(grandParent);
                    }
                }

                parent = node.Parent;
            }

            this.Root.IsRed = false;
        }

        private void RotateLeft(RedBlackTreeNode<T> node) {
            RedBlackTreeNode<T> rightChild = node.Right;
            node.Right = rightChild.Left;

            if (rightChild.Left != RedBlackTreeNode<T>.Void) {
                rightChild.Left.Parent = node;
            }

            rightChild.Parent = node.Parent;
            if (node.Parent == RedBlackTreeNode<T>.Void) {
                this.Root = rightChild;
            } else if (node == node.Parent.Left) {
                node.Parent.Left = rightChild;
            } else {
                node.Parent.Right = rightChild;
            }

            rightChild.Left = node;
            node.Parent = rightChild;
            this.PostprocessRotation(node, rightChild);
        }

        private void RotateRight(RedBlackTreeNode<T> node) {
            RedBlackTreeNode<T> leftChild = node.Left;
            node.Left = leftChild.Right;

            if (leftChild.Right != RedBlackTreeNode<T>.Void) {
                leftChild.Right.Parent = node;
            }

            leftChild.Parent = node.Parent;
            if (node.Parent == RedBlackTreeNode<T>.Void) {
                this.Root = leftChild;
            } else if (node == node.Parent.Right) {
                node.Parent.Right = leftChild;
            } else {
                node.Parent.Left = leftChild;
            }

            leftChild.Right = node;
            node.Parent = leftChild;
            this.PostprocessRotation(node, leftChild);
        }

        protected RedBlackTreeNode<T> MakeNode(T key, RedBlackTreeNode<T> parent) {
            return new RedBlackTreeNode<T>(key, parent);
        }

        private bool ContainsNode(T key, out RedBlackTreeNode<T> last) {
            RedBlackTreeNode<T> curr = this.Root;
            last = RedBlackTreeNode<T>.Void;
            while (curr is not null && curr != RedBlackTreeNode<T>.Void) {
                last = curr;
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
        /// <returns>true if successfully inserted, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <c>null</c>.</exception>
        private bool Insert(T key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            if (this.ContainsNode(key, out RedBlackTreeNode<T> last)) {
                return false;
            }

            RedBlackTreeNode<T> node = this.MakeNode(key, last);
            if (last is null || last == RedBlackTreeNode<T>.Void) {
                // This means that the traversal above did not happen, i.e., the tree is empty.
                this.Root = node;
            } else if (key.CompareTo(last.Key) < 0) {
                last.Left = node;
            } else {
                last.Right = node;
            }

            this.PostprocessInsertion(node);
            this.RebalanceAfterInsertion(node);
            this.Count += 1;
            return true;
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
        public bool HasStrictSuccessorOf(T key, out T successor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }
            
            successor = default;
            RedBlackTreeNode<T> node = this.Root;
            bool found = false;
            while (node is not null && node != RedBlackTreeNode<T>.Void) {
                if (key.CompareTo(node.Key) < 0) {
                    successor = node;
                    found = true;
                    node = node.Left;
                } else {
                    node = node.Right;
                }
            }

            return found;
        }

        public bool HasStrictPredecessorOf(T key, out T predecessor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }
            
            predecessor = default;
            RedBlackTreeNode<T> node = this.Root;
            bool found = false;
            while (node is not null && node != RedBlackTreeNode<T>.Void) {
                if (key.CompareTo(node.Key) > 0) {
                    predecessor = node;
                    found = true;
                    node = node.Right;
                } else {
                    node = node.Left;
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
        public bool HasWeakSuccessorOf(T key, out T successor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            successor = default;
            RedBlackTreeNode<T> node = this.Root;
            bool found = false;
            while (node is not null && node != RedBlackTreeNode<T>.Void) {
                int cmp = key.CompareTo(node.Key);
                switch (cmp) {
                    case < 0:
                        // node.Value > key -> possible successor, but keep searching left for a smaller one
                        successor = node;
                        found = true;
                        node = node.Left;
                        break;
                    case > 0:
                        // node.Value <= key -> a successor, if any, is in the right subtree
                        node = node.Right;
                        break;
                    case 0:
                        successor = node;
                        return true;
                }
            }

            return found;
        }

        public bool HasWeakPredecessorOf(T key, out T predecessor) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            predecessor = default;
            RedBlackTreeNode<T> node = this.Root;
            bool found = false;
            while (node is not null && node != RedBlackTreeNode<T>.Void) {
                int cmp = key.CompareTo(node.Key);
                switch (cmp) {
                    case > 0:
                        // node.Value < key -> possible predecessor, but keep searching right for a larger one
                        predecessor = node;
                        found = true;
                        node = node.Right;
                        break;
                    case < 0:
                        // node.Value >= key -> a predecessor, if any, is in the left subtree
                        node = node.Left;
                        break;
                    case 0:
                        predecessor = node;
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
        public T First() {
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
        public T Last() {
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
        public T PollFirst() {
            if (this.Count == 0) {
                throw new InvalidOperationException("Cannot poll first element of empty set");
            }

            T first = this.First();
            this.Remove(first);
            return first;
        }

        /// <summary>
        /// Removes and returns the last (largest) element in the set, or default if the set is empty.
        /// </summary>
        /// <returns>The largest element which was removed, or default if empty.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the set is empty</exception>
        public T PollLast() {
            if (this.Count == 0) {
                throw new InvalidOperationException("Cannot poll last element of empty set");
            }

            T last = this.Last();
            this.Remove(last);
            return last;
        }

        #endregion

        #region Template methods

        protected virtual void PostprocessInsertion(RedBlackTreeNode<T> insertedNode) { }
        protected virtual void PrepareForRemoval(RedBlackTreeNode<T> nodeToRemove) { }

        protected virtual void PostprocessRotation(
            RedBlackTreeNode<T> oldRoot, RedBlackTreeNode<T> newRoot
        ) { }

        #endregion

        #region Set semantics

        public IEnumerator<T> GetEnumerator() {
            return this.Root.InOrderTraversal().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        void ICollection<T>.Add(T item) {
            if (item != null) {
                this.Insert(item);
            }
        }

        public void ExceptWith(IEnumerable<T> other) {
            if (this.Count == 0) {
                return;
            }

            HashSet<T> set = other.ToHashSet();
            foreach (T key in this.Where(set.Contains)) {
                this.Remove(key);
            }
        }

        public void IntersectWith(IEnumerable<T> other) {
            if (this.Count == 0) {
                return;
            }

            HashSet<T> set = other.ToHashSet();
            if (set.Count == 0) {
                this.Clear();
            } else {
                foreach (T key in this.Where(x => !set.Contains(x))) {
                    this.Remove(key);
                }
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            if (this.Count == 0) {
                return other.Any();
            }

            HashSet<T> set = other.ToHashSet();
            return this.Count < set.Count && this.All(set.Contains);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            if (this.Count == 0) {
                return false;
            }

            HashSet<T> set = other.ToHashSet();
            return this.Count > set.Count && set.All(this.Contains);
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            if (this.Count == 0) {
                return true;
            }

            HashSet<T> set = other.ToHashSet();
            return this.Count <= set.Count && this.All(set.Contains);
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            if (this.Count == 0) {
                return false;
            }

            HashSet<T> set = other.ToHashSet();
            return this.Count >= set.Count && set.All(this.Contains);
        }

        public bool Overlaps(IEnumerable<T> other) {
            if (this.Count == 0) {
                return false;
            }

            HashSet<T> set = other.ToHashSet();
            return this.Any(set.Contains);
        }

        public bool SetEquals(IEnumerable<T> other) {
            if (this.Count == 0) {
                return !other.Any();
            }

            HashSet<T> set = other.ToHashSet();
            return this.Count == set.Count && this.All(set.Contains);
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            foreach (T value in other) {
                if (!this.Remove(value)) {
                    this.Add(value);
                }
            }
        }

        public void UnionWith(IEnumerable<T> other) {
            foreach (T value in other) {
                this.Insert(value);
            }
        }

        public bool Add(T item) {
            return this.Insert(item);
        }

        public void Clear() {
            this.Root = RedBlackTreeNode<T>.Void;
            this.Count = 0;
        }

        public bool Contains(T item) {
            return item != null && this.ContainsNode(item, out RedBlackTreeNode<T> _);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length) {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Length - arrayIndex < this.Count) {
                throw new ArgumentException("Insufficient space to copy all the items to the array.");
            }

            foreach (T item in this) {
                array[arrayIndex] = item;
                arrayIndex += 1;
            }
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
        public bool Remove(T key) {
            if (key == null) {
                return false;
            }

            if (!this.ContainsNode(key, out RedBlackTreeNode<T> node)) {
                return false;
            }

            this.PrepareForRemoval(node);
            if (!this.RemoveNode(node)) {
                return false;
            }

            this.Count -= 1;
            return true;
        }

        #endregion
    }
}
