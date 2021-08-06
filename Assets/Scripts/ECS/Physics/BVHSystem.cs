using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class BVHSystem : JobComponentSystem {
    private EntityQuery aabbGroup;

    private NativeArray<int> mortonCodesA;
    private NativeArray<int> mortonCodesB;
    private NativeArray<int> indexConverterB;
    private NativeArray<int> indexConverterA;
    private NativeArray<int> radixSortBitValues;
    private NativeArray<int> radixSortOffsets;
    private NativeArray<int> sortResultsArrayIsA;

    protected override void OnCreate() {
        aabbGroup = GetEntityQuery(new EntityQueryDesc() {
            All = new[] {
                ComponentType.ReadWrite<AABB>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadWrite<CollisionInfo>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        aabbGroup.SetChangedVersionFilter(typeof(Translation));
        mortonCodesA = new NativeArray<int>(10, Allocator.Persistent);
        mortonCodesB = new NativeArray<int>(10, Allocator.Persistent);
        indexConverterB = new NativeArray<int>(10, Allocator.Persistent);
        indexConverterA = new NativeArray<int>(10, Allocator.Persistent);
        radixSortBitValues = new NativeArray<int>(10, Allocator.Persistent);
        radixSortOffsets = new NativeArray<int>(10, Allocator.Persistent);
        sortResultsArrayIsA = new NativeArray<int>(1, Allocator.Persistent);
    }

    //[BurstCompile]
//    private struct BVHJob : IJob {
//
//        [ReadOnly]
//        public NativeArray<AABB> aabbs;
//
//        public NativeArray<int> mortonCodesA;
//        public NativeArray<int> mortonCodesB;
//        public NativeArray<int> indexConverterA;
//        public NativeArray<int> indexConverterB;
//        public NativeArray<int> radixSortBitValues;
//        public NativeArray<int> radixSortOffsets;
//
//        [WriteOnly]
//        public NativeArray<int> sortResultsArrayIsA;
//
//        private int zeroesHistogramCounter;
//        private int onesHistogramCounter;
//        private int zeroesPrefixSum;
//        private int onesPrefixSum;
//
//        public void Execute() {
//            for (int i = 0; i < aabbs.Length; i++) {
//                mortonCodesA[i] = MortonZCurve.WorldPosToMortonIndex(aabbs[i]);
//
//                indexConverterA[i] = i;
//                indexConverterB[i] = i;
//            }
//
//            for (int bitPosition = 0; bitPosition < 31; bitPosition++) {
//                bool isEvenIteration = bitPosition % 2 == 0;
//
//                // init histogram counts
//                zeroesHistogramCounter = 0;
//                onesHistogramCounter   = 0;
//                zeroesPrefixSum        = 0;
//                onesPrefixSum          = 0;
//
//                // Compute histograms and offsets
//                for (int i = 0; i < aabbs.Length; i++) {
//                    int bitVal = 0;
//                    if (isEvenIteration) {
//                        bitVal = (mortonCodesA[i] & (1 << bitPosition)) >> bitPosition;
//                    } else {
//                        bitVal = (mortonCodesB[i] & (1 << bitPosition)) >> bitPosition;
//                    }
//
//                    radixSortBitValues[i] = bitVal;
//
//                    if (bitVal == 0) {
//                        radixSortOffsets[i]    =  zeroesHistogramCounter;
//                        zeroesHistogramCounter += 1;
//                    } else {
//                        radixSortOffsets[i]  =  onesHistogramCounter;
//                        onesHistogramCounter += 1;
//                    }
//                }
//
//                // calc prefix sum from histogram
//                zeroesPrefixSum = 0;
//                onesPrefixSum   = zeroesHistogramCounter;
//
//                // Reorder array
//                for (int i = 0; i < aabbs.Length; i++) {
//                    int newIndex = 0;
//                    if (radixSortBitValues[i] == 0) {
//                        newIndex = zeroesPrefixSum + radixSortOffsets[i];
//                    } else {
//                        newIndex = onesPrefixSum + radixSortOffsets[i];
//                    }
//
//                    if (isEvenIteration) {
//                        mortonCodesB[newIndex]    = mortonCodesA[i];
//                        indexConverterB[newIndex] = indexConverterA[i];
//                    } else {
//                        mortonCodesA[newIndex]    = mortonCodesB[i];
//                        indexConverterA[newIndex] = indexConverterB[i];
//                    }
//                }
//
//                sortResultsArrayIsA[0] = isEvenIteration ? 0 : 1; // it's the A array only for odd number iterations
//            }
//        }
//    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        return inputDeps;
    }

    protected override void OnDestroy() {
        mortonCodesA.Dispose();
        mortonCodesB.Dispose();
        indexConverterB.Dispose();
        indexConverterA.Dispose();
        radixSortBitValues.Dispose();
        radixSortOffsets.Dispose();
        sortResultsArrayIsA.Dispose();
    }
}