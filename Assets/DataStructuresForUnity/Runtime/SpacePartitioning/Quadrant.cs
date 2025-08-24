using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataStructuresForUnity.Runtime.SpacePartitioning {
    internal sealed class Quadrant<T> : IDictionary<Vector2, T> {
        private Vector2 Centre { get; }
        private float Extent { get; }
        private Rect Bounds { get; }
        
        private Quadrant<T>[] Children { get; }
        private Dictionary<Vector2, T> Points { get; }

        private int MaxBucketSize { get; }
        private float MinRectSize { get; }

        private bool IsLeaf => this.Children.All(child => child == null);

        public int Count => this.IsLeaf
                ? this.Points.Count
                : this.Children.Sum(quadrant => quadrant.Count);

        public bool IsReadOnly => false;

        public T this[Vector2 key] {
            get => this.TryGetValue(key, out T value) ? value : throw new KeyNotFoundException();
            set => this.Add(key, value);
        }

        public ICollection<Vector2> Keys => this.IsLeaf
                ? this.Points.Keys
                : this.Children.SelectMany(child => child.Keys).ToArray();

        public ICollection<T> Values => this.IsLeaf
                ? this.Points.Values
                : this.Children.SelectMany(child => child.Values).ToArray();

        public Quadrant(Vector2 centre, float extent, int maxBucketSize, float minRectSize) {
            this.Centre = centre;
            this.Extent = extent;
            Vector2 anchor = centre - new Vector2(extent, extent);
            Vector2 size = new Vector2(extent * 2, extent * 2);
            this.Bounds = new Rect(anchor, size);
            this.MaxBucketSize = maxBucketSize;
            this.MinRectSize = minRectSize;
            this.Children = new Quadrant<T>[4];
            this.Points = new Dictionary<Vector2, T>();
        }

        /// <summary>
        /// Determines whether the specified point is within the bounds of the quadrant.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is within the bounds, otherwise false.</returns>
        /// <remarks>Note that this does not check if the point has been added to the quadrant.</remarks>
        private bool Surrounds(Vector2 point) {
            return point.x >= this.Centre.x - this.Extent && point.x <= this.Centre.x + this.Extent &&
                   point.y >= this.Centre.y - this.Extent && point.y <= this.Centre.y + this.Extent;
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the bounds of the quadrant.
        /// </summary>
        /// <param name="rect">The rectangle to check for intersection.</param>
        /// <returns>True if the rectangle intersects with the bounds, otherwise false.</returns>
        private bool Intersects(Rect rect) {
            return this.Bounds.Overlaps(rect);
        }

        /// <summary>
        /// Inserts a point with associated data into the quadrant.
        /// If the point already exists, the insertion will replace the associated data.
        /// If the quadrant's points exceed its capacity, it will subdivide itself
        /// and re-distribute the points to child quadrants.
        /// </summary>
        /// <param name="point">The position of the point to insert.</param>
        /// <param name="data">The data to associate with the specified point.</param>
        /// <returns>True if the point was successfully inserted, otherwise false.</returns>
        /// <remarks>Even when a key is not present, it is considered as an existing key
        /// if there is a point in the quadrant that is extremely close to the key.</remarks>
        internal bool Insert(Vector2 point, T data) {
            if (!this.Surrounds(point)) {
                return false;
            }
            
            if (!this.IsLeaf) {
                for (int i = 0; i < 4; i += 1) {
                    if (this.Children[i].Insert(point, data)) {
                        return true;
                    }
                }

                return false;
            }

            if (this.FindNearest(point, out (Vector2 point, T _) nearest, 0.0001f)) {
                this.Points[nearest.point] = data;
                return true;
            }
            
            this.Points.Add(point, data);
            if (this.Points.Count <= this.MaxBucketSize || this.Extent * 2 <= this.MinRectSize) {
                return true;
            }

            this.Subdivide();
            IEnumerable<KeyValuePair<Vector2, T>> points = this.Points.ToArray();
            foreach ((Vector2 p, T t) in points) {
                bool inserted = false;
                for (int i = 0; i < 4; i += 1) {
                    if (!this.Children[i].Insert(p, t)) {
                        continue;
                    }

                    inserted = true;
                    break;
                }

                if (inserted) {
                    this.Points.Remove(p);
                    continue;
                }

                // This shouldn't happen if the subdivision is correct
                Debug.LogError("Failed to redistribute point after subdivision!");
                this.Points.Add(p, t); // Keep it in parent as a fallback
            }

            this.Points.Clear();
            return true;
        }

        /// <summary>
        /// Splits the current quadrant into four child quadrants, dividing its spatial extent into equal sections.
        /// </summary>
        /// <remarks>
        /// This method converts the quadrant into a non-leaf node by creating four child quadrants (NW, NE, SW, SE).
        /// Each child quadrant has half the extent of the parent quadrant. This operation is typically triggered
        /// when the number of points in the quadrant exceeds the maximum allowed bucket size, provided that the
        /// quadrant's size does not fall below the minimum rectangular size. This method should only be called
        /// for leaf quadrants, and attempting to subdivide a non-leaf quadrant will result in a debug error message.
        /// </remarks>
        private void Subdivide() {
            if (!this.IsLeaf) {
                Debug.LogError("Try to subdivide a non-leaf quadrant!");
                return;
            }

            float halfExtent = this.Extent / 2;

            // Create four child quadrants: NW, NE, SW, SE
            Vector2 nwCentre = new Vector2(this.Centre.x - halfExtent, this.Centre.y + halfExtent);
            this.Children[0] =
                    new Quadrant<T>(nwCentre, halfExtent, this.MaxBucketSize, this.MinRectSize);
            Vector2 neCentre = new Vector2(this.Centre.x + halfExtent, this.Centre.y + halfExtent);
            this.Children[1] =
                    new Quadrant<T>(neCentre, halfExtent, this.MaxBucketSize, this.MinRectSize);
            Vector2 swCentre = new Vector2(this.Centre.x - halfExtent, this.Centre.y - halfExtent);
            this.Children[2] =
                    new Quadrant<T>(swCentre, halfExtent, this.MaxBucketSize, this.MinRectSize);
            Vector2 seCentre = new Vector2(this.Centre.x + halfExtent, this.Centre.y - halfExtent);
            this.Children[3] =
                    new Quadrant<T>(seCentre, halfExtent, this.MaxBucketSize, this.MinRectSize);
        }

        /// <summary>
        /// Collects all points within the specified rectangular bounds.
        /// </summary>
        /// <param name="bounds">The rectangular bounds to search within.</param>
        /// <returns>A dictionary containing the points and their associated data
        /// that lie within the specified bounds.</returns>
        internal Dictionary<Vector2, T> CollectPointsIn(Rect bounds) {
            if (!this.Intersects(bounds)) {
                return new Dictionary<Vector2, T>();
            }

            if (this.IsLeaf) {
                return this.Points.Where(entry => bounds.Contains(entry.Key))
                           .ToDictionary(entry => entry.Key, entry => entry.Value);
            }

            return this.Children.SelectMany(child => child.CollectPointsIn(bounds))
                       .ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        /// <summary>
        /// Finds the nearest point and associated data within the specified maximum distance from a given position.
        /// </summary>
        /// <param name="position">The position to search around.</param>
        /// <param name="nearest">An out parameter that will contain the nearest point
        /// and its associated data, if found.</param>
        /// <param name="maxDistance">The maximum distance to search for the nearest point. Defaults to <c>float.MaxValue</c>.</param>
        /// <returns>True if any point is found within the maximum distance, otherwise false.</returns>
        internal bool FindNearest(
            Vector2 position, out (Vector2, T) nearest, float maxDistance = float.MaxValue
        ) {
            float left = position.x - maxDistance;
            float top = position.y + maxDistance;
            Vector2 size = new Vector2(maxDistance * 2, maxDistance * 2);
            Rect range = new Rect(new Vector2(left, top), size);
            Dictionary<Vector2, T> candidates = this.CollectPointsIn(range);
            if (candidates.Count == 0) {
                nearest = default;
                return false;
            }

            float minDist = float.PositiveInfinity;
            KeyValuePair<Vector2, T> first = candidates.First();
            nearest = (first.Key, first.Value);
            foreach ((Vector2 point, T data) in candidates) {
                float dist = Vector2.Distance(point, position);
                if (dist >= minDist) {
                    continue;
                }

                nearest = (point, data);
                minDist = dist;
            }

            return true;
        }

        /// <summary>
        /// Removes the specified point from the quadrant.
        /// </summary>
        /// <param name="point">The point to remove.</param>
        /// <returns>True if the point was successfully removed, otherwise false.</returns>
        public bool Remove(Vector2 point) {
            if (this.IsLeaf) {
                if (this.Points.Remove(point)) {
                    return true;
                }

                return this.FindNearest(point, out (Vector2 point, T _) nearest, 0.0001f) &&
                       this.Points.Remove(nearest.point);
            }

            if (!this.Children.Any(quadrant => quadrant.Remove(point))) {
                return false;
            }

            this.TryMerge();
            return true;
        }

        /// <summary>
        /// Attempts to merge the child quadrants into the current quadrant if they meet the conditions for merging.
        /// </summary>
        /// <remarks>
        /// Merging occurs if all child quadrants are leaf nodes and the total number of points within
        /// the child quadrants does not exceed the maximum bucket size. Upon merging, the points from
        /// the child quadrants are consolidated into the current quadrant, and the children are nullified.
        /// </remarks>
        private void TryMerge() {
            if (this.IsLeaf) {
                return;
            }

            int size = 0;
            foreach (Quadrant<T> quadrant in this.Children) {
                if (!quadrant.IsLeaf) {
                    return;
                }

                size += quadrant.Points.Count;
            }

            if (size > this.MaxBucketSize) {
                return;
            }

            this.Points.Clear();
            for (int i = 0; i < this.Children.Length; i += 1) {
                foreach ((Vector2 point, T data) in this.Children[i].Points) {
                    this.Points.Add(point, data);
                }
                
                this.Children[i] = null;
            }
        }

        /// <summary>
        /// Adds the specified key-value pair to the data structure.
        /// </summary>
        /// <param name="item">The key-value pair to add.</param>
        /// <remarks>If the key already exists, the value is updated.</remarks>
        public void Add(KeyValuePair<Vector2, T> item) {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all elements from the quadrant and all its child quadrants.
        /// </summary>
        /// <remarks>
        /// This method clears the current quadrant, removing all stored points.
        /// If the quadrant has children, it also clears all of its child quadrants
        /// and deallocates their references, effectively resetting the entire subtree.
        /// </remarks>
        public void Clear() {
            this.Points.Clear();
            if (this.IsLeaf) {
                return;
            }

            for (int i = 0; i < this.Children.Length; i += 1) {
                this.Children[i].Clear();
                this.Children[i] = null;
            }
        }

        /// <summary>
        /// Determines whether the specified key-value pair exists in the quadrant.
        /// </summary>
        /// <param name="item">The key-value pair to locate.</param>
        /// <returns>True if the key-value pair exists within the quadrant, otherwise false.</returns>
        /// <remarks>Even when a key is not present, it is considered as an existing key
        /// if there is a point in the quadrant that is extremely close to the key.</remarks>
        public bool Contains(KeyValuePair<Vector2, T> item) {
            return this.TryGetValue(item.Key, out T data) && data.Equals(item.Value);
        }
        
        public void CopyTo(KeyValuePair<Vector2, T>[] array, int arrayIndex) {
            if (this.IsLeaf) {
                this.Points.ToArray().CopyTo(array, arrayIndex);
            }

            this.Children.SelectMany(child => child).ToArray().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified key-value pair from the quad tree.
        /// </summary>
        /// <param name="item">The key-value pair to remove.</param>
        /// <returns>True if the key-value pair was successfully removed, otherwise false.</returns>
        /// <remarks>Even when a key is not present, it is considered as an existing key
        /// if there is a point in the quadrant that is extremely close to the key.</remarks>
        public bool Remove(KeyValuePair<Vector2, T> item) {
            return this.TryGetValue(item.Key, out T data) &&
                   data.Equals(item.Value) && this.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<Vector2, T>> GetEnumerator() {
            return this.IsLeaf ? this.Points.GetEnumerator() : enumerateChildren().GetEnumerator();

            IEnumerable<KeyValuePair<Vector2, T>> enumerateChildren() {
                foreach (KeyValuePair<Vector2, T> point in this.Points) {
                    yield return point;
                }
                
                foreach (Quadrant<T> child in this.Children) {
                    if (child == null) {
                        continue;
                    }

                    foreach (KeyValuePair<Vector2, T> point in child) {
                        yield return point;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds a key-value pair to the quadrant. If the key already exists, its value is updated.
        /// </summary>
        /// <param name="key">The key representing the position to add.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        public void Add(Vector2 key, T value) {
            this.Insert(key, value);
        }

        /// <summary>
        /// Determines whether the specified key exists in the quadrant or its sub-quadrants.
        /// </summary>
        /// <param name="key">The key to locate in the quadrant.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        /// <remarks>Even when a key is not present, it is considered as an existing key
        /// if there is a point in the quadrant that is extremely close to the key.</remarks>
        public bool ContainsKey(Vector2 key) {
            if (!this.Surrounds(key)) {
                return false;
            }

            if (!this.IsLeaf) {
                return this.Children.Any(quadrant => quadrant.ContainsKey(key));
            }

            if (this.Points.ContainsKey(key)) {
                return true;
            }

            if (this.FindNearest(key, out (Vector2 point, T _) nearest, 0.0001f)) {
                return Mathf.Approximately(nearest.point.x, key.x) &&
                       Mathf.Approximately(nearest.point.y, key.y);
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be retrieved.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns>True if the key is found and the value is retrieved successfully; otherwise, false.</returns>
        /// <remarks>Even when a key is not present, it is considered as an existing key
        /// if there is a point in the quadrant that is extremely close to the key.</remarks>
        public bool TryGetValue(Vector2 key, out T value) {
            if (!this.Surrounds(key)) {
                value = default;
                return false;
            }

            if (!this.IsLeaf) {
                foreach (Quadrant<T> quadrant in this.Children) {
                    if (quadrant.TryGetValue(key, out value)) {
                        return true;
                    }
                }

                value = default;
                return false;
            }

            if (this.Points.TryGetValue(key, out value)) {
                return true;
            }

            if (this.FindNearest(key, out (Vector2 point, T value) nearest, 0.0001f)) {
                value = nearest.value;
                return Mathf.Approximately(nearest.point.x, key.x) &&
                       Mathf.Approximately(nearest.point.y, key.y);
            }

            value = default;
            return false;
        }
    }
}
