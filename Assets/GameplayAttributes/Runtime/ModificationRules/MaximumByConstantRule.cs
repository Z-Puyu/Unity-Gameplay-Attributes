using System;
using UnityEngine;

namespace GameplayAttributes.Runtime.ModificationRules {
    [Serializable]
    public class MaximumByConstantRule : IAttributeModificationRule {
        [field: SerializeField] private int Max { get; set; }
        
        public float Apply(float value, AttributeSet root) {
            return Math.Min(value, this.Max);
        }
    }
}
