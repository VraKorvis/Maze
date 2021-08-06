using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// \Library\PackageCache\com.unity.2d.animation@2.2.0-preview.1\Runtime\Components.cs
// 
//struct SpriteComponent : ISharedComponentData, IEquatable<SpriteComponent> {
//    public Sprite Value;
//
//    public bool Equals(SpriteComponent other) {
//        return Equals(Value, other.Value);
//    }
//
//    public override bool Equals(object obj) {
//        return obj is SpriteComponent other && Equals(other);
//    }
//
//    public override int GetHashCode() {
//        return (Value != null ? Value.GetHashCode() : 0);
//    }
//
//    public static bool operator ==(SpriteComponent left, SpriteComponent right) {
//        return left.Equals(right);
//    }
//
//    public static bool operator !=(SpriteComponent left, SpriteComponent right) {
//        return !left.Equals(right);
//    }
//}

public struct LeafElement : ISharedComponentData, IEquatable<LeafElement> {
    public Entity leaf;

    public bool Equals(LeafElement other) {
        return leaf.Equals(other.leaf);
    }

    public override bool Equals(object obj) {
        return obj is LeafElement other && Equals(other);
    }

    public override int GetHashCode() {
        return leaf.GetHashCode();
    }

    public static bool operator ==(LeafElement left, LeafElement right) {
        return left.Equals(right);
    }

    public static bool operator !=(LeafElement left, LeafElement right) {
        return !left.Equals(right);
    }
}
public class LeafElementComponent : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
    public GameObject leaf;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var data = new LeafElement() {
            leaf = conversionSystem.GetPrimaryEntity(leaf),
        };
        dstManager.AddSharedComponentData(entity, data);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add(leaf);
    }
}