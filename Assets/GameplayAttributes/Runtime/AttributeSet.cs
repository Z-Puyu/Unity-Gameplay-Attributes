using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStructuresForUnity.Runtime.Trie;
using GameplayAttributes.Runtime.ModificationRules;
using log4net.Filter;
using UnityEngine;

namespace GameplayAttributes.Runtime {
    [DisallowMultipleComponent]
    public sealed class AttributeSet : MonoBehaviour, IEnumerable<Attribute> {
        private TrieDictionary<string, char, AttributeData> Attributes { get; } =
            new TrieDictionary<string, char, AttributeData>();

        public void Initialise(AttributeTable table) {
            foreach (KeyValuePair<AttributeTypeDefinition, int> attribute in table) {
                this.Attributes.Add(attribute.Key.FullName, AttributeData.From(attribute.Key, attribute.Value, this));
            }

            foreach (AttributeData data in this.Attributes.Values) {
                data.ExecuteModificationRules();
            }
        }

        public IEnumerator<Attribute> GetEnumerator() {
            foreach (KeyValuePair<string, AttributeData> entry in this.Attributes) {
                yield return new Attribute(entry.Key, entry.Value.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public void AddModifier(Modifier modifier) {
            this.Attributes.ForEachWithPrefix(modifier.Target, (_, data) => data.AddModifier(modifier));
        }

        public int GetCurrent(string key) {
            if (this.Attributes.TryGetValue(key, out AttributeData data)) {
                return data.Value;
            }

            Debug.LogWarning($"Trying to access non-existing attribute {key}", this);
            return 0;
        }

        public int GetMax(string key) {
            if (this.Attributes.TryGetValue(key, out AttributeData data)) {
                return data.MaxValue;
            }

            Debug.LogWarning($"Trying to access non-existing attribute {key}", this);
            return int.MaxValue;
        }

        public int GetMin(string key) {
            if (this.Attributes.TryGetValue(key, out AttributeData data)) {
                return data.MinValue;
            }

            Debug.LogWarning($"Trying to access non-existing attribute {key}", this);
            return int.MinValue;
        }

        public Attribute GetAttribute(string key) {
            if (this.Attributes.TryGetValue(key, out AttributeData data)) {
                return new Attribute(key, data.Value);
            }

            Debug.LogWarning($"Trying to access non-existing attribute {key}", this);
            return new Attribute(key, 0);
        }
    }
}
