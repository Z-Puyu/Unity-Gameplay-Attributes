using System.Collections.Generic;
using System.Linq;
using DataStructuresForUnity.Runtime.Tree;
using NUnit.Framework;
using UnityEngine;

namespace DataStructuresForUnity.Tests {
    public class UnionFindTest {
        // A Test behaves as an ordinary method
        [Test]
        public void UnionFind_UnionAndQuery_AllCorrect() {
            List<int> cases = UnionFindTest.GenerateTestCases();
            UnionFind<int> ufds = new UnionFind<int>(cases);
            List<HashSet<int>> sets = new List<HashSet<int>>();
            foreach (int i in cases) {
                HashSet<int> set = new HashSet<int>();
                set.Add(i);
                sets.Add(set);
            }

            for (int i = 0; i < 100; i += 1) {
                int x = cases[Random.Range(0, cases.Count)];
                int y = cases[Random.Range(0, cases.Count)];
                HashSet<int> setX = sets.Find(s => s.Contains(x));
                HashSet<int> setY = sets.Find(s => s.Contains(y));
                bool isSameSet = setX == setY;
                if (isSameSet) {
                    Assert.True(ufds.ContainsInSameSet(x, y));
                    Assert.False(ufds.Union(x, y));
                } else {
                    Assert.False(ufds.ContainsInSameSet(x, y));
                    Assert.True(ufds.Union(x, y));
                }
                
                Assert.True(ufds.ContainsInSameSet(x, y));
                setX.UnionWith(setY);
                setY.Clear();
            }
        }

        private static List<int> GenerateTestCases() {
            return Enumerable.Range(0, 1000).ToList();
        }
    }
}
