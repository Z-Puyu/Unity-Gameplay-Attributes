using System;
using SaintsField;
using UnityEngine;

namespace GameplayAttributes.Runtime.ModificationRules {
    [Serializable]
    public class MaximumByAttributeRule : IAttributeModificationRule {
        [field: SerializeField, Required] private AttributeTypeDefinition Max { get; set; }
        
        public float Apply(float value, AttributeSet root) {
            if (!this.Max) {
                Debug.LogError("Max Attribute is not set");
                return value;
            }
            
            return value;
        }
    }
}
