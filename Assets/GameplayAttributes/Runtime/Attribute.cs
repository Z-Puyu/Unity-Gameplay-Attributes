namespace GameplayAttributes.Runtime {
    public readonly struct Attribute {
        public string Name { get; }
        public int Value { get; }
        
        public Attribute(string name, int value) {
            this.Name = name;
            this.Value = value;
        }
    }
}
