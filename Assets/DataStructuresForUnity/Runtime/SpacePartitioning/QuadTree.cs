using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataStructuresForUnity.Runtime.SpacePartitioning {
    /// <summary>
    /// Represents a data structure for spatial partitioning using a quadtree system.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the quadtree.</typeparam>
    public sealed class QuadTree<T> : IDictionary<Vector2, T> {
        private Quadrant<T> Root { get; }
        
        public int Count => this.Root.Count;
        public bool IsReadOnly => false;
        
        public T this[Vector2 key] { get => this.Root[key]; set => this.Root[key] = value; }
        public ICollection<Vector2> Keys => this.Root.Keys;
        public ICollection<T> Values => this.Root.Values;

        /// <summary>
        /// Represents a quadtree data structure designed for efficient spatial partitioning and
        /// querying of two-dimensional point-data associations.
        /// </summary>
        /// <typeparam name="T">The type of data stored within the quadtree at each point.</typeparam>
        /// <remarks>
        /// The extent of the quadtree is the half-size of its bounding square.
        /// 
        /// The quadtree divides a rectangular space into smaller rectangles or "quadrants"
        /// when a quadrant exceeds the maximum bucket size. The granularity of subdivisions is
        /// limited by a specified minimum rectangle size.
        /// </remarks>
        public QuadTree(float extent, int maxBucketSize, float minRectSize) {
            this.Root = new Quadrant<T>(Vector2.zero, extent, maxBucketSize, minRectSize);
        }

        /// <summary>
        /// Collects all points within the specified bounds and returns them as a dictionary.
        /// </summary>
        /// <param name="bounds">The rectangular bounds within which points should be collected.</param>
        /// <returns>A dictionary containing points as keys and their associated values
        /// that fall within the specified bounds.</returns>
        public Dictionary<Vector2, T> CollectPointsIn(Rect bounds) {
            return this.Root.CollectPointsIn(bounds);
        }

        /// <summary>
        /// Finds the nearest point and its associated data to the specified position within a maximum distance.
        /// </summary>
        /// <param name="position">The position to search from.</param>
        /// <param name="nearest">
        /// When the method returns, contains a tuple of the nearest point and its associated data, if a point is found.
        /// </param>
        /// <param name="maxDistance">
        /// The maximum distance within which to search for the nearest point. Defaults to <c>float.MaxValue</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if any point was found within the specified distance; otherwise, <c>false</c>.
        /// </returns>
        public bool FindNearest(
            Vector2 position, out (Vector2, T) nearest, float maxDistance = float.MaxValue
        ) {
            return this.Root.FindNearest(position, out nearest, maxDistance);
        }

        /// <summary>
        /// Inserts a point and its associated data into the quadtree structure.
        /// </summary>
        /// <param name="position">The two-dimensional position where the data should be inserted.</param>
        /// <param name="value">The data to associate with the specified position.</param>
        /// <returns>
        /// Returns true if the data was successfully inserted into the quadtree;
        /// otherwise, false if the insertion failed (e.g., the point already exists in the quadtree).
        /// </returns>
        public bool Insert(Vector2 position, T value) {
            return this.Root.Insert(position, value);
        }
        
        public IEnumerator<KeyValuePair<Vector2, T>> GetEnumerator() {
            return this.Root.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified key-value pair to the quadtree.
        /// </summary>
        /// <param name="item">The key-value pair representing a spatial point and
        /// its associated data to be added to the quadtree.</param>
        public void Add(KeyValuePair<Vector2, T> item) {
            this.Root.Add(item);
        }

        /// <summary>
        /// Removes all entries from the quadtree, effectively resetting its state
        /// and clearing all spatially partitioned data.
        /// </summary>
        /// <remarks>
        /// Invoking this method clears all stored points and their associated data
        /// within the quadtree, including all subdivided quadrants, if any. After this
        /// operation, the quadtree will be empty and ready to accept new data.
        /// </remarks>
        public void Clear() {
            this.Root.Clear();
        }

        /// <summary>
        /// Determines whether the quadtree contains a specific key-value pair.
        /// </summary>
        /// <param name="item">The key-value pair to locate in the quadtree.</param>
        /// <returns>
        /// true if the specified key-value pair is found in the quadtree; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<Vector2, T> item) {
            return this.Root.Contains(item);
        }

        public void CopyTo(KeyValuePair<Vector2, T>[] array, int arrayIndex) {
            this.Root.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a specified key-value pair from the quadtree if it exists.
        /// </summary>
        /// <param name="item">The key-value pair to remove from the quadtree.</param>
        /// <returns>True if the specified key-value pair was successfully removed; otherwise, false.</returns>
        public bool Remove(KeyValuePair<Vector2, T> item) {
            return this.Root.Remove(item);
        }

        /// <summary>
        /// Adds a new key-value pair to the quadtree at a specified position.
        /// </summary>
        /// <param name="key">The position in 2D space where the value will be stored.</param>
        /// <param name="value">The data associated with the specified position.</param>
        /// <remarks>
        /// Inserting a key-value pair may trigger further spatial partitioning if the current quadrant
        /// exceeds its maximum capacity.
        /// </remarks>
        public void Add(Vector2 key, T value) {
            this.Root.Add(key, value);
        }

        /// <summary>
        /// Determines whether the quadtree contains an element with the specified point.
        /// </summary>
        /// <param name="key">The key to locate in the quadtree, represented as a two-dimensional point.</param>
        /// <returns>True if the quadtree contains an element with the specified point; otherwise, false.</returns>
        public bool ContainsKey(Vector2 key) {
            return this.Root.ContainsKey(key);
        }

        /// <summary>
        /// Removes the item associated with the specified point from the quadtree.
        /// </summary>
        /// <param name="key">The key representing the position of the item to remove.</param>
        /// <returns>
        /// True if the item was successfully removed from the quadtree; otherwise, false
        /// if the key was not found.
        /// </returns>
        public bool Remove(Vector2 key) {
            return this.Root.Remove(key);
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified point in the quadtree.
        /// </summary>
        /// <param name="key">The key representing the position in the two-dimensional space.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>True if the quadtree contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(Vector2 key, out T value) {
            return this.Root.TryGetValue(key, out value);
        }
    }
}
