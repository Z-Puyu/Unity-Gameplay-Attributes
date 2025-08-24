namespace GameplayAttributes.Runtime.ModificationRules {
    public interface IAttributeModificationRule {
        /// <summary>
        /// Apply a modification rule to the current value of an attribute.
        /// </summary>
        /// <param name="value">The attribute value before applying the rule.</param>
        /// <param name="root">The attribute set from which the rule is applied.</param>
        /// <returns>The attribute value after applying the rule.</returns>
        /// <remarks>
        /// Usually, this should be applied after a group of modifier effects are calculated.
        /// </remarks>
        public float Apply(float value, AttributeSet root);
    }
}
