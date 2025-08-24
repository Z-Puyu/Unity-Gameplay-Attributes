using System;
using System.Collections.Generic;
using System.Linq;
using GameplayAttributes.Runtime.ModificationRules;

namespace GameplayAttributes.Runtime {
    internal class AttributeData {
        private AttributeSet Root { get; }
        private List<IAttributeModificationRule> ModificationRules { get; }
        private bool IsDirty { get; set; }
        private float BaseValue { get; }
        private float value;

        internal int Value {
            get {
                if (!this.IsDirty) {
                    return (int)this.value;
                }

                this.value = this.BaseValue;
                foreach (Modifier modifier in this.Modifiers.Values) {
                    this.value = modifier.Modify(this.value);
                    this.ExecuteModificationRules();
                }
                
                this.IsDirty = false;
                return (int)this.value;
            }
        }

        internal int MaxValue {
            get {
                this.IsDirty = true;
                this.value = float.MaxValue;
                int max = this.Value;
                this.IsDirty = false;
                return max;
            }
        }

        internal int MinValue {
            get {
                this.IsDirty = true;
                this.value = float.MinValue;
                int min = this.Value;
                this.IsDirty = false;
                return min;
            }
        }

        private SortedList<Modifier.Operation, Modifier> Modifiers { get; } =
            new SortedList<Modifier.Operation, Modifier>();

        private AttributeData(List<IAttributeModificationRule> modificationRules, float value, AttributeSet root) {
            this.ModificationRules = modificationRules;
            this.BaseValue = this.value = value;
            this.Root = root;
        }

        internal static AttributeData From(AttributeTypeDefinition definition, float initValue, AttributeSet root) {
            List<IAttributeModificationRule> modificationRules = definition.ModificationRules.ToList();
            return new AttributeData(modificationRules, initValue, root);
        }

        internal void ExecuteModificationRules() {
            foreach (IAttributeModificationRule rule in this.ModificationRules) {
                this.value = rule.Apply(this.value, this.Root);
            }
        }

        internal void AddModifier(Modifier modifier) {
            this.IsDirty = true;
            if (this.Modifiers.TryGetValue(modifier.Type, out Modifier curr)) {
                this.Modifiers[modifier.Type] = curr + modifier;
            } else {
                this.Modifiers.Add(modifier.Type, modifier);
            }
        }
    }
}
