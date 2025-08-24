using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataStructuresForUnity.Runtime.Tree {
    public class RedBlackTreeNode<T> where T : IComparable<T> {
        public static readonly RedBlackTreeNode<T> Void = new RedBlackTreeNode<T>(default);
        
        internal T Key { get; }
        internal RedBlackTreeNode<T> Left { get; set; }
        internal RedBlackTreeNode<T> Right { get; set; }
        internal RedBlackTreeNode<T> Parent { get; set; }
        internal bool IsRed { get; set; }
        internal bool IsBlack => !this.IsRed;

        internal RedBlackTreeNode(T key, RedBlackTreeNode<T> parent = null) {
            this.Key = key;
            this.Parent = parent;
            this.IsRed = true;
            this.Left = RedBlackTreeNode<T>.Void;
            this.Right = RedBlackTreeNode<T>.Void;
        }

        internal RedBlackTreeNode<T> LeastChild() {
            RedBlackTreeNode<T> min = this;
            while (min.Left is not null && min.Left != RedBlackTreeNode<T>.Void) {
                min = min.Left;
            }

            return min;
        }

        internal RedBlackTreeNode<T> GreatestChild() {
            RedBlackTreeNode<T> max = this;
            while (max.Right is not null && max.Right != RedBlackTreeNode<T>.Void) {
                max = max.Right;
            }
            
            return max;
        }

        internal IEnumerable<T> InOrderTraversal() {
            if (this.Left is not null) {
                foreach (T item in this.Left.InOrderTraversal()) {
                    yield return item;
                }
            }

            yield return this;
            if (this.Right is null) {
                yield break;
            }

            foreach (T item in this.Right.InOrderTraversal()) {
                yield return item;
            }
        }

        public override string ToString() {
            return $"{{{this.Key}}}";
        }

        public static implicit operator T(RedBlackTreeNode<T> node) {
            return node.Key;
        }
    }
}
