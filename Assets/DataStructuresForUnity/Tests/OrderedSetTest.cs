using System.Collections.Generic;
using System.Linq;
using DataStructuresForUnity.Runtime.Tree;
using NUnit.Framework;
using UnityEngine;

namespace DataStructuresForUnity.Tests {
    public class OrderedSetTest {
        [Test]
        public void OrderedSet_Insertion_NoDuplicates() {
            OrderedSet<int> set = new OrderedSet<int>();
            List<int> cases = OrderedSetTest.GenerateTestCases();
            HashSet<int> inserted = new HashSet<int>();
            foreach (int i in cases) {
                if (!inserted.Add(i)) {
                    Assert.True(set.Contains(i));
                    Assert.False(set.Add(i));
                } else {
                    Assert.False(set.Contains(i));
                    Assert.True(set.Add(i));
                    Assert.True(set.Contains(i));
                }
            }

            foreach (int i in cases) {
                Assert.False(set.Add(i));
            }

            int count = set.Count;
            Assert.AreEqual(cases.Distinct().Count(), count);
            HashSet<int> removed = new HashSet<int>();
            foreach (int i in cases) {
                if (!removed.Add(i)) {
                    Assert.False(set.Contains(i));
                    Assert.False(set.Remove(i));
                } else {
                    Assert.True(set.Contains(i));
                    Assert.True(set.Remove(i));
                }
            }
            
            Assert.AreEqual(set.Count, 0);
            Assert.AreEqual(cases.Distinct().Count(), removed.Count);
        }
        
        [Test]
        public void OrderedSet_Ordering_NonDecreasing() {
            OrderedSet<int> set = new OrderedSet<int>();
            List<int> cases = OrderedSetTest.GenerateTestCases();
            foreach (int i in cases) {
                set.Add(i);
            }

            int count = set.Count;
            Assert.AreEqual(set.Last(), cases.Max(), $"Expect: {cases.Max()} but got {set.Last()}");
            Assert.AreEqual(set.First(), cases.Min());
            Assert.AreEqual(set.Last(), set.PollLast());
            int max = cases.Max();
            cases.RemoveAll(x => x == max);
            Assert.AreEqual(count - 1, set.Count);
            Assert.AreEqual(set.Count, cases.Distinct().Count());
            Assert.AreEqual(set.Last(), cases.Max());
            Assert.AreEqual(set.First(), cases.Min());
            Assert.AreEqual(set.First(), set.PollFirst());
            int min = cases.Min();
            cases.RemoveAll(x => x == min);
            Assert.AreEqual(count - 2, set.Count);
            Assert.AreEqual(set.Count, cases.Distinct().Count());
            Assert.AreEqual(set.Last(), cases.Max());
            Assert.AreEqual(set.First(), cases.Min());

            cases = cases.Distinct().ToList();
            cases.Sort();
            for (int i = 0; i < 100; i += 1) {
                int index = Random.Range(0, cases.Count);
                if (set.HasStrictPredecessorOf(cases[index], out int p1)) {
                    Assert.AreEqual(cases[index - 1], p1, $"Expect: {cases[index - 1]} but got {p1}");
                } else if (set.HasStrictSuccessorOf(cases[index], out int s1)) {
                    Assert.AreEqual(cases[index + 1], s1, $"Expect: {cases[index + 1]} but got {s1}");
                } else if (set.HasWeakPredecessorOf(cases[index], out int p2)) {
                    Assert.AreEqual(cases[index], p2);
                } else if (set.HasWeakSuccessorOf(cases[index], out int s2)) {
                    Assert.AreEqual(cases[index], s2);
                } else {
                    Assert.Fail("No predecessor or successor found for index " + index);
                }
            }

            cases = cases.Select(i => i * 2).ToList();
            cases.Sort();
            set.Clear();
            foreach (int i in cases) {
                set.Add(i);
            }
            
            for (int i = 0; i < 100; i += 1) {
                int index = Random.Range(1, cases.Count);
                int test = (cases[index] + cases[index - 1]) / 2;
                Assert.True(set.HasStrictPredecessorOf(test, out int p1), 
                    $"{cases[index - 1]} < {test} but is not a strict predecessor! Instead {p1} is found.");
                Assert.True(set.HasWeakPredecessorOf(test, out int p2),
                    $"{cases[index - 1]} <= {test} but is not a weak predecessor! Instead {p2} is found.");
                Assert.AreEqual(p1, p2);
                Assert.True(cases.Contains(p1));
                Assert.AreEqual(p1, cases[index - 1]);
                Assert.True(set.HasStrictSuccessorOf(test, out int s1),
                    $"{cases[index]} > {test} but is not a strict successor! Instead {s1} is found.");
                Assert.True(set.HasWeakSuccessorOf(test, out int s2),
                    $"{cases[index]} >= {test} but is not a weak successor! Instead {s2} is found.");
                Assert.AreEqual(s1, s2);
                Assert.True(cases.Contains(s1));
                Assert.AreEqual(s1, cases[index]);
            }
        }

        private static List<int> GenerateTestCases() {
            List<int> cases = new List<int>();
            for (int i = 0; i < 1000; i++) {
                cases.Add(Random.Range(-1000, 1000));
            }
        
            return cases;
        }
    }
}
