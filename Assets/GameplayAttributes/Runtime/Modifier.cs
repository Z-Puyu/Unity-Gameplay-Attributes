using System;
using UnityEngine;

namespace GameplayAttributes.Runtime {
    public readonly struct Modifier {
        public enum Operation { Shift, Multiply, Offset }
        
        public Operation Type { get; }
        public string Target { get; }
        public int Magnitude { get; }

        public Modifier(int magnitude, Operation type, string target) {
            this.Type = type;
            this.Target = target;
            this.Magnitude = magnitude;
        }

        public float Modify(float value) {
            return this.Type switch {
                Operation.Shift or Operation.Offset => value + this.Magnitude,
                Operation.Multiply => value * Math.Min(100 + this.Magnitude, 0) / 100.0f,
                var _ => value
            };
        }
        
        public override string ToString() {
            return $"{this.Target} {this.Type} of magnitude {this.Magnitude}";
        }

        public static Modifier operator -(Modifier m) {
            return new Modifier(-m.Magnitude, m.Type, m.Target);
        }

        public static Modifier operator +(Modifier a, Modifier b) {
            if (a.Target != b.Target || a.Type != b.Type) {
                throw new ArgumentException("Cannot add modifiers with different targets or types");
            }
            
            return new Modifier(a.Magnitude + b.Magnitude, a.Type, a.Target);
        }
        
        public static Modifier operator -(Modifier a, Modifier b) {
            if (a.Target != b.Target || a.Type != b.Type) {
                throw new ArgumentException("Cannot add modifiers with different targets or types");
            }
            
            return new Modifier(a.Magnitude - b.Magnitude, a.Type, a.Target);
        }

        public static Modifier operator *(Modifier a, float k) {
            return new Modifier(Mathf.RoundToInt(k * a.Magnitude), a.Type, a.Target);
        }
        
        public static Modifier operator *(float k, Modifier a) {
            return a * k;
        }
        
        public static Modifier operator /(Modifier a, float k) {
            return new Modifier(Mathf.RoundToInt(a.Magnitude / k), a.Type, a.Target);
        }
    }
}
