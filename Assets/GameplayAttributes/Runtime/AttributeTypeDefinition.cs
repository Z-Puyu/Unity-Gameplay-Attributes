using System;
using System.Collections.Generic;
using GameplayAttributes.Runtime.ModificationRules;
using SaintsField;
using UnityEngine;

namespace GameplayAttributes.Runtime {
    [CreateAssetMenu(fileName = "Attribute Type Definition", menuName = "Gameplay Attributes")]
    public class AttributeTypeDefinition : ScriptableObject {
        [field: SerializeField] private string Name { get; set; }

        [field: SerializeReference]
        public List<IAttributeModificationRule> ModificationRules { get; private set; } =
            new List<IAttributeModificationRule>();

        [field: SerializeField, ReadOnly] public string FullName { get; private set; }
        [field: SerializeField, ReadOnly] private AttributeTypeDefinition Parent { get; set; }
        
        [field: SerializeField, OnValueChanged(nameof(this.OnSubtypesChanged))]
        public List<AttributeTypeDefinition> SubTypes { get; private set; } = new List<AttributeTypeDefinition>();

        private void OnSubtypesChanged() {
            foreach (AttributeTypeDefinition def in this.SubTypes) {
                if (def) {
                    def.Parent = this;
                }
            }
        }

        private void OnValidate() {
            LinkedList<string> names = new LinkedList<string>();
            AttributeTypeDefinition curr = this;
            while (curr) {
                names.AddFirst(curr.Name);
                curr = curr.Parent;
            }
            
            this.FullName = string.Join(".", names);
        }
    }
}
