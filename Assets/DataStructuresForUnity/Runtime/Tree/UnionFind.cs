using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructuresForUnity.Runtime.Tree {
    /// <summary>
    /// Union-Find Disjoint Set data structure with path compression and union-by-rank optimisations.
    /// Supports generic types and provides additional utility operations for game development.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the disjoint set</typeparam>
    public class UnionFind<T> : IEnumerable<T> where T : IEquatable<T> {
        private Dictionary<T, HashSet<T>> Children { get; } 
        private Dictionary<T, T> Parents { get; }
        private Dictionary<T, int> SetSizes { get; }
        private Dictionary<T, int> NodeRanks { get; }
        private int Size { get; set; } = 0;
        private int NumberOfSets { get; set; } = 0;

        public UnionFind() {
            this.Children = new Dictionary<T, HashSet<T>>();
            this.Parents = new Dictionary<T, T>();
            this.SetSizes = new Dictionary<T, int>();
            this.NodeRanks = new Dictionary<T, int>();
        }

        public UnionFind(int capacity) {
            this.Children = new Dictionary<T, HashSet<T>>(capacity);
            this.Parents = new Dictionary<T, T>(capacity);
            this.SetSizes = new Dictionary<T, int>(capacity);
            this.NodeRanks = new Dictionary<T, int>(capacity);
        }

        public UnionFind(IEnumerable<T> elements) {
            T[] elems = elements.Distinct().ToArray();
            this.Size = this.NumberOfSets = elems.Length;
            this.Children = new Dictionary<T, HashSet<T>>(this.NumberOfSets);
            this.Parents = new Dictionary<T, T>(this.NumberOfSets);
            this.SetSizes = new Dictionary<T, int>(this.NumberOfSets);
            this.NodeRanks = new Dictionary<T, int>(this.NumberOfSets);
            foreach (T element in elems) {
                this.Children.Add(element, new HashSet<T>());
                this.Parents.Add(element, element);
                this.SetSizes.Add(element, 1);
                this.NodeRanks.Add(element, 0);
            }
        }

        /// <summary>
        /// Creates a new set containing the specified element as its only member.
        /// </summary>
        /// <param name="element">The element to create a new set for</param>
        /// <returns>True if the set is successfully created,
        /// false if a set containing <paramref name="element"/> already exists</returns>
        public bool MakeNewSet(T element) {
            if (!this.Parents.TryAdd(element, element)) {
                return false;
            }
            
            this.Children.Add(element, new HashSet<T>());
            this.NodeRanks.Add(element, 0);
            this.SetSizes.Add(element, 1);
            this.Size += 1;
            this.NumberOfSets += 1;
            return true;
        }

        /// <summary>
        /// Adds a new element to an existing set or creates a new set if <paramref name="target"/> does not exist.
        /// </summary>
        /// <param name="element">New element to add</param>
        /// <param name="target">Element whose set the new element should join</param>
        /// <returns>True if successful, false if <paramref name="element"/> already exists</returns>
        public bool AddToSet(T element, T target) {
            if (!this.MakeNewSet(element) || !this.Parents.ContainsKey(target)) {
                return false;
            }
            
            return this.Union(element, target);
        }

        /// <summary>
        /// Finds the representative (root) of the set containing the element.
        /// Uses path compression for optimisation.
        /// </summary>
        /// <param name="element">Element to find the set representative for</param>
        /// <returns>The representative element, or <c>default(T)</c> if <paramref name="element"/> is not found</returns>
        public T FindSet(T element) {
            if (!this.Parents.TryGetValue(element, out T parent)) {
                return default;
            }
            
            if (element.Equals(parent)) {
                return element;
            }

            T root = this.FindSet(parent);
            this.Children[this.Parents[element]].Remove(element);
            this.Parents[element] = root;
            this.Children[root].Add(element);
            return root;
        }

        /// <summary>
        /// Checks if two elements are in the same set.
        /// </summary>
        /// <param name="first">First element</param>
        /// <param name="second">Second element</param>
        /// <returns>True if both elements are in the same set</returns>
        public bool ContainsInSameSet(T first, T second) {
            return this.Parents.ContainsKey(first) && this.Parents.ContainsKey(second) &&
                   this.FindSet(first).Equals(this.FindSet(second));
        }

        /// <summary>
        /// Unions two sets containing the given elements.
        /// Uses union-by-rank heuristic for optimisation.
        /// </summary>
        /// <param name="first">Element from the first set</param>
        /// <param name="second">Element from the second set</param>
        /// <returns>True if union was performed, false if elements don't exist or are already in the same set.</returns>
        public bool Union(T first, T second) {
            T firstRoot = this.FindSet(first);
            T secondRoot = this.FindSet(second);
            if (firstRoot.Equals(secondRoot)) {
                return false;
            }

            this.NumberOfSets -= 1;
            int firstRank = this.NodeRanks[firstRoot];
            int secondRank = this.NodeRanks[secondRoot];
            if (firstRank > secondRank) {
                this.Children[firstRoot].Add(secondRoot);
                this.Parents[secondRoot] = firstRoot;
                this.SetSizes[firstRoot] += this.SetSizes[secondRoot];
            } else {
                this.Children[secondRoot].Add(firstRoot);
                this.Parents[firstRoot] = secondRoot;
                this.SetSizes[secondRoot] += this.SetSizes[firstRoot];
                if (firstRank == secondRank) {
                    this.NodeRanks[secondRoot] += 1;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the size of the set containing the given element
        /// </summary>
        /// <param name="element">Element to find the set size for</param>
        /// <returns>Size of the set, or 0 if the element not found</returns>
        public int SizeOf(T element) {
            return this.SetSizes.GetValueOrDefault(this.FindSet(element), 0);
        }

        /// <summary>
        /// Performs an action on every element in the set containing the given element
        /// </summary>
        /// <param name="element">Element whose set to operate on</param>
        /// <param name="action">Action to perform on each element</param>
        public void ForEachInSet(T element, Action<T> action) {
            foreach (T child in this.DenumerateElementsInSet(element)) {
                action(child);
            }
        }

        /// <summary>
        /// Gets all elements in the set containing the given element
        /// </summary>
        /// <param name="element">Element whose set to retrieve</param>
        /// <returns>List of all elements in the set, or empty list if the element is not found</returns>
        public ISet<T> DenumerateElementsInSet(T element) {
            if (!this.Parents.ContainsKey(element)) {
                return Enumerable.Empty<T>().ToHashSet();
            }
            
            HashSet<T> result = new HashSet<T>(this.SizeOf(element));
            Stack<T> stack = new Stack<T>();
            stack.Push(this.FindSet(element));
            while (stack.TryPop(out T curr)) {
                result.Add(curr);
                if (!this.Children.TryGetValue(curr, out HashSet<T> children)) {
                    continue;
                }

                foreach (T child in children.Where(c => !result.Contains(c))) {
                    stack.Push(child);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Aggregates elements in the set containing the specified element using the provided aggregator function.
        /// </summary>
        /// <param name="element">The element used to identify the set to aggregate.</param>
        /// <param name="aggregator">A function that takes two elements and returns the aggregated result.</param>
        /// <returns>The aggregated result of all elements in the set containing <paramref name="element"/>.</returns>
        public T Aggregate(T element, Func<T, T, T> aggregator) {
            return this.DenumerateElementsInSet(element).Aggregate(aggregator);
        }

        /// <summary>
        /// Aggregates all elements in the set containing the specified element using the provided aggregation function.
        /// </summary>
        /// <param name="element">The element whose set will be aggregated.</param>
        /// <param name="seed">The initial seed value for the aggregation.</param>
        /// <param name="aggregator">The aggregation function to apply.</param>
        /// <typeparam name="S">The type of the aggregated result.</typeparam>
        /// <returns>The aggregated result after applying the aggregation function to all elements in the set.</returns>
        public S Aggregate<S>(T element, S seed, Func<S, T, S> aggregator) {
            return this.DenumerateElementsInSet(element).Aggregate(seed, aggregator);
        }

        /// <summary>
        /// Gets all disjoint sets.
        /// </summary>
        /// <returns>List of sets, where each set is a list of elements.</returns>
        public IEnumerable<IEnumerable<T>> DenumerateAllSets() {
            return this.Parents.Where(pair => pair.Key.Equals(pair.Value))
                       .Select(pair => this.DenumerateElementsInSet(pair.Key));
        }

        /// <summary>
        /// Checks if an element exists in the data structure
        /// </summary>
        /// <param name="element">Element to check</param>
        /// <returns>True if the element exists</returns>
        public bool Contains(T element) {
            return this.Parents.ContainsKey(element);
        }

        /// <summary>
        /// Removes all elements and sets
        /// </summary>
        public void Clear() {
            this.Parents.Clear();
            this.Children.Clear();
            this.SetSizes.Clear();
            this.NodeRanks.Clear();
            this.Size = this.NumberOfSets = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through all elements in the data structure
        /// </summary>
        /// <returns>An enumerator for all elements</returns>
        public IEnumerator<T> GetEnumerator() {
            return this.Parents.Keys.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through all elements in the data structure
        /// </summary>
        /// <returns>An enumerator for all elements</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}
