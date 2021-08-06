// https://forum.unity.com/threads/ecs-grid2d-pathfinding-example-and-feedback-on-how-to-improve.591523/#post-3953125

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
///     A native min heap implementation optimized for pathfinding.
/// </summary>
[NativeContainerSupportsDeallocateOnJobCompletion]
[NativeContainer]
public unsafe struct NativeMinHeap : IDisposable {
    private readonly Allocator allocator;

    [NativeDisableUnsafePtrRestriction]
    private void* buffer;

    private int capacity;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private AtomicSafetyHandle m_Safety;

    [NativeSetClassTypeToNullOnSchedule]
    private DisposeSentinel m_DisposeSentinel;
#endif

    private int head;
    private int length;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeMinHeap"/> struct.
    /// </summary>
    /// <param name="capacity"> The capacity of the min heap. </param>
    /// <param name="allocator"> The allocator. </param>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if allocator not set, capacity is negative or the size > maximum integer value. </exception>
    public NativeMinHeap(int capacity, Allocator allocator) {
        var size = (long) UnsafeUtility.SizeOf<MinHeapNode>() * capacity;
        if (allocator <= Allocator.None) {
            throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
        }

        if (capacity < 0) {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Length must be >= 0");
        }

        if (size > int.MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(capacity), $"Length * sizeof(T) cannot exceed {int.MaxValue} bytes");
        }

        buffer    = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<MinHeapNode>(), allocator);
        this.capacity  = capacity;
        this.allocator = allocator;
        head      = -1;
        length    = 0;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
#endif
    }

    /// <summary>
    /// Does the heap still have remaining nodes.
    /// </summary>
    /// <returns>
    /// True if the min heap still has at least one remaining node, otherwise false.
    /// </returns>
    public bool HasNext() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
        return head >= 0;
    }

    /// <summary>
    /// Add a node to the heap which will be sorted.
    /// </summary>
    /// <param name="node"> The node to add. </param>
    /// <exception cref="IndexOutOfRangeException"> Throws if capacity reached. </exception>
    public void Push(MinHeapNode node) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (length == capacity) {
            throw new IndexOutOfRangeException("Capacity Reached");
        }

        AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
        if (head < 0) {
            head = length;
        } else if (node.ExpectedCost < Get(head).ExpectedCost) {
            node.Next = head;
            head = length;
        } else {
            var currentPtr = head;
            var current    = Get(currentPtr);
            
            while (current.Next >= 0 && Get(current.Next).ExpectedCost <= node.ExpectedCost) {
                
                currentPtr = current.Next;
                current    = Get(current.Next);
                
            }
            
            node.Next    = current.Next;
            current.Next = length;
            

            UnsafeUtility.WriteArrayElement(buffer, currentPtr, current);
        }

        UnsafeUtility.WriteArrayElement(buffer, length, node);
        length += 1;
        
    }

    /// <summary>
    /// Take the top node off the heap.
    /// </summary>
    /// <returns>The current node of the heap.</returns>
    public MinHeapNode Pop() {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
        var result = head;
        head = Get(head).Next;
        return Get(result);
    }

    /// <summary>
    /// Clear the heap by resetting the head and length.
    /// </summary>
    /// <remarks>Does not clear memory.</remarks>
    public void Clear() {
        head   = -1;
        length = 0;
    }

    /// <summary>
    /// Dispose of the heap by freeing up memory.
    /// </summary>
    /// <exception cref="InvalidOperationException"> Memory hasn't been allocated. </exception>
    public void Dispose() {
        if (!UnsafeUtility.IsValidAllocator(allocator)) {
            return;
        }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
        UnsafeUtility.Free(buffer, allocator);
        buffer   = null;
        capacity = 0;
    }

    public NativeMinHeap Slice(int start, int length) {
        var stride = UnsafeUtility.SizeOf<MinHeapNode>();

        return new NativeMinHeap {
            buffer   = (byte*) ((IntPtr) buffer + stride * start),
            capacity = length,
            length   = 0,
            head     = -1,
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = m_Safety,
#endif
        };
    }

    private MinHeapNode Get(int index) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (index < 0 || index >= length) {
            FailOutOfRangeError(index);
        }
        AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
        return UnsafeUtility.ReadArrayElement<MinHeapNode>(buffer, index);
    }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private void FailOutOfRangeError(int index) {
        throw new IndexOutOfRangeException($"Index {index} is out of range of '{capacity}' Length.");
    }
#endif
}

/// <summary>
/// The min heap node.
/// </summary>
public struct MinHeapNode {
    /// <summary>
    /// Initializes a new instance of the <see cref="MinHeapNode"/> struct.
    /// </summary>
    /// <param name="position"> The position. </param>
    /// <param name="expectedCost"> The expected cost. </param>
    /// <param name="distanceToGoal">Remaining distance to the goal</param>
    public MinHeapNode(int2 position, float expectedCost, float distanceToGoal) {
        Position       = position;
        ExpectedCost   = expectedCost;
        DistanceToGoal = distanceToGoal;
        Next           = -1;
    }

    /// <summary>
    /// Gets the position.
    /// </summary>
    public int2 Position { get; }

    /// <summary>
    /// Gets the expected cost.
    /// </summary>
    public float ExpectedCost { get; }

    /// <summary>
    /// Gets the expected cost.
    /// </summary>
    public float DistanceToGoal { get; }

    /// <summary>
    /// Gets or sets the next node in the heap.
    /// </summary>
    public int Next { get; set; }
}