using Unity.Entities;

public struct BVHNode {
    public AABB   aabb;
    public Entity AssociatedEntity;
    public int    RightmostLeafIndex;
    public int    FirstChildIndex;
    public byte   IsValid;
    public byte   ColliderType;
}