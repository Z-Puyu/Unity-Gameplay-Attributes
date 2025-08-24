using System;
using System.Collections.Generic;
using System.Linq;
using DataStructuresForUnity.Runtime.Trie;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace DataStructuresForUnity.Tests {
    public class TrieSetTest {
        private struct TestCase : IEquatable<TestCase> {
            public string Key { get; }
            public List<string> Prefixes { get; } 
            public List<string> DotSeparatedPrefixes { get; }
        
            public TestCase(string key) {
                this.Key = key;
                this.Prefixes = new List<string>();
                this.DotSeparatedPrefixes = new List<string>();
                for (int i = 0; i < key.Length; i += 1) {
                    string prefix = key[..(i + 1)];
                    this.Prefixes.Add(prefix);
                }

                List<string> tokens = this.Key.Split('.').ToList();
                for (int i = 0; i < tokens.Count; i += 1) {
                    this.DotSeparatedPrefixes.Add(string.Join('.', tokens.GetRange(0, i + 1)));
                }
            }

            public bool Equals(TestCase other) {
                return this.Key == other.Key;
            }

            public override bool Equals(object obj) {
                return obj is TestCase other && this.Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(this.Key, this.Prefixes, this.DotSeparatedPrefixes);
            }
        }
    
        [Test]
        public void TrieSet_InsertionNoSeparator_Inserted() {
            List<TestCase> cases = TrieSetTest.GenerateTestCases();
            TrieSet<string, char> trie = new TrieSet<string, char>();
            foreach (TestCase testCase in cases) {
                trie.Add(testCase.Key);
                Assert.True(trie.Contains(testCase.Key));
                foreach (string prefix in testCase.Prefixes) {
                    Assert.True(trie.ContainsPrefix(prefix), $"{prefix} is a prefix for {testCase.Key} but is not found in the trie.");
                    if (prefix == testCase.Key) {
                        Assert.True(trie.Contains(prefix));
                    } 
                }
            }
        
            foreach (TestCase testCase in cases) {
                Assert.True(trie.Contains(testCase.Key));
                foreach (string prefix in testCase.Prefixes) {
                    Assert.True(trie.ContainsPrefix(prefix), $"{prefix} is a prefix for {testCase.Key} but is not found in the trie.");
                    if (prefix == testCase.Key) {
                        Assert.True(trie.Contains(prefix));
                    } 
                }
            }
        
            Assert.AreEqual(cases.Distinct().Count(), trie.Count);
        }
    
        [Test]
        public void TrieSet_InsertionWithSeparator_Inserted() {
            List<TestCase> cases = TrieSetTest.GenerateTestCases();
            TrieSet<string, char> trie = new TrieSet<string, char>('.');
            List<string> nonPrefixes = cases.SelectMany(c => c.Prefixes)
                                            .Where(p => p.StartsWith('.') || p.EndsWith('.') || !p.Contains('.'))
                                            .Where(p => p.Length > 6)
                                            .Distinct()
                                            .ToList();
            foreach (TestCase testCase in cases) {
                trie.Add(testCase.Key);
                Assert.True(trie.Contains(testCase.Key));
                foreach (string prefix in testCase.DotSeparatedPrefixes) {
                    Assert.True(trie.ContainsPrefix(prefix), $"{prefix} is a prefix for {testCase.Key} but is not found in the trie.");
                    if (prefix == testCase.Key) {
                        Assert.True(trie.Contains(prefix));
                    } 
                }

                foreach (string test in nonPrefixes) {
                    Assert.False(trie.ContainsPrefix(test), $"{test} is not a prefix for {testCase.Key} but is found in the trie.");
                }
            }
        
            foreach (TestCase testCase in cases) {
                Assert.True(trie.Contains(testCase.Key));
                foreach (string prefix in testCase.DotSeparatedPrefixes) {
                    Assert.True(trie.ContainsPrefix(prefix), $"{prefix} is a prefix for {testCase.Key} but is not found in the trie.");
                    if (prefix == testCase.Key) {
                        Assert.True(trie.Contains(prefix));
                    } 
                }
                
                foreach (string test in nonPrefixes) {
                    Assert.False(trie.ContainsPrefix(test), $"{test} is not a prefix for {testCase.Key} but is found in the trie.");
                }
            }
        
            Assert.AreEqual(cases.Distinct().Count(), trie.Count);
        }

        private static List<TestCase> GenerateTestCases() {
            List<TestCase> cases = new List<TestCase>();
        
            for (int i = 0; i < 1000; i++) {
                string key = Enumerable.Range(1, Random.Range(3, 6))
                                       .Select(_ => TrieSetTest.GetRandomKey())
                                       .Aggregate(TrieSetTest.GetRandomKey(), (a, b) => a + '.' + b);
                cases.Add(new TestCase(key));
            }
    
            return cases;
        }
    
        private static string GetRandomKey() {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return Enumerable.Range(1, Random.Range(3, 6))
                             .Select(_ => chars[Random.Range(0, chars.Length)])
                             .Aggregate("", (a, b) => a + b);
        }
    }
}
