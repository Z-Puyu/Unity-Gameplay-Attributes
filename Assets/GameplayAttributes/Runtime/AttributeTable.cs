using System.Collections;
using System.Collections.Generic;
using SaintsField;
using UnityEngine;

namespace GameplayAttributes.Runtime {
    [CreateAssetMenu(fileName = "Attribute Table", menuName = "Gameplay Attributes/Attribute Table")]
    public class AttributeTable : ScriptableObject, IEnumerable<KeyValuePair<AttributeTypeDefinition, int>> {
        [field: SerializeField, SaintsDictionary("Attribute Type", "Initial Value")]
        private SaintsDictionary<AttributeTypeDefinition, int> Attributes { get; set; } =
            new SaintsDictionary<AttributeTypeDefinition, int>();

        public IEnumerator<KeyValuePair<AttributeTypeDefinition, int>> GetEnumerator() {
            return this.Attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}
