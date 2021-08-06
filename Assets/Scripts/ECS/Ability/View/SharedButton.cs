using System;
using Unity.Entities;
using UnityEngine.UI;

public struct SharedButton : ISharedComponentData, IEquatable<SharedButton> {
    public Button button;

    public bool Equals(SharedButton other) {
        return Equals(button, other.button);
    }

    public override bool Equals(object obj) {
        return obj is SharedButton other && Equals(other);
    }

    public override int GetHashCode() {
        return (button != null ? button.GetHashCode() : 0);
    }

    public static bool operator ==(SharedButton left, SharedButton right) { return left.Equals(right); }
    public static bool operator !=(SharedButton left, SharedButton right) { return !left.Equals(right); }
}