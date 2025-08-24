using System;
using UnityEngine;

namespace GameplayAttributes.Runtime.ModificationRules {
    [Serializable]
    public class MinimumByConstantRule : IAttributeModificationRule {
        [field: SerializeField] private int Min { get; set; }
        
        public float Apply(float value, AttributeSet root) {
            return Math.Max(value, this.Min);
        }
    }
}
