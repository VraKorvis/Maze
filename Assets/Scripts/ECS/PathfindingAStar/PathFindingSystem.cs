using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(PathMovementSystem))]
public class PathFindingSystem : SystemBase {
    private EntityQuery pathRequestGroup;
    private EntityQuery gridBufferGroup;

    // private NativeList<PathRequestAgent> pathList;
    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    //TODO hardcode
    private int dimX = 26;
    private int dimY = 13;

    private NativeArray<int2> Neighbours;

    private const int NeighborCount = 4;
    private const int WorkerCount = 1;
    private const int IterationLimit = 1000;
    private const int InnerLoopBatchSize = 1;

    private NativeArray<float> CostSoFar;
    private NativeArray<int2> CameFrom;

    private NativeMinHeap OpenSet;

    // TODO for implement custom IBootStrap
//    public PathFindingSystem(int dimX = 26, int dimY = 13 ) {
//        this.dimX = dimX;
//        this.dimY = dimY;
//    }

    protected override void OnCreate() {
        pathRequestGroup = GetEntityQuery(new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadWrite<PathRequestAgent>(),
                ComponentType.ReadOnly<PathAgentStatusFindTag>(),
                ComponentType.ReadOnly<PathAgentStatusProcessTag>(),
                ComponentType.ReadOnly<HauntingTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        // pathRequestGroup.SetChangedVersionFilter(ComponentType.ReadWrite<PathRequestAgent>());
        // pathRequestGroup.SetChangedVersionFilter(
        //     new ComponentType[] {typeof(PathRequestAgent), typeof(PathAgentStatus)});
        gridBufferGroup = GetEntityQuery(new EntityQueryDesc {
            All = new[] {
                ComponentType.ReadOnly<GridBuffer>(),
                ComponentType.ReadOnly<GridTag>(),
            },
            Options = EntityQueryOptions.FilterWriteGroup
        });
        RequireForUpdate(gridBufferGroup);
        // RequireForUpdate(pathRequestGroup);
        Neighbours = new NativeArray<int2>(4, Allocator.Persistent) {
            [0] = new int2(-1, 0),
            [1] = new int2(0, 1),
            [2] = new int2(1, 0),
            [3] = new int2(0, -1)
        };
        // pathList = new NativeList<PathRequestAgent>(Allocator.Persistent);
        var size = dimX * dimY;
        CostSoFar = new NativeArray<float>(size * WorkerCount, Allocator.Persistent);
        CameFrom = new NativeArray<int2>(size * WorkerCount, Allocator.Persistent);
        OpenSet = new NativeMinHeap((IterationLimit + 1) * NeighborCount * WorkerCount, Allocator.Persistent);

        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // TODO rework it to iJobCHunk; ForEach<PathRequestAgent, PathAgentStatus> is obsolete
    [BurstCompile]
    private struct PathRequestToListJob : IJobChunk {
        //ForEach<PathRequestAgent, PathAgentStatus> {
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeList<PathRequestAgent> pathList;

        [ReadOnly]
        public ArchetypeChunkComponentType<PathRequestAgent> PathRequestAgentArchetype;

        [ReadOnly]
        public ArchetypeChunkComponentType<PathAgentStatus> PathAgentStatusArchetype;

        public uint LastSystemVersion;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var pathRequestChunk = chunk.GetNativeArray(PathRequestAgentArchetype);
            var didChangePath = chunk.DidChange(PathRequestAgentArchetype, LastSystemVersion);
            var didChangeStatus = chunk.DidChange(PathAgentStatusArchetype, LastSystemVersion);
            if (!didChangePath && !didChangeStatus) return;
            var statusChunk = chunk.GetNativeArray(PathAgentStatusArchetype);
            for (int i = 0; i < chunk.Count; i++) {
                var path = pathRequestChunk[i];
                var status = statusChunk[i];
                if (status.Value == AgentStatus.Find) {
                    pathList.Add(path);
                }
            }
        }
    }

    [BurstCompile]
    private unsafe struct FindPathAStarJob : IJobParallelFor {
        public int dimX;
        public int dimY;

        [ReadOnly]
        public DynamicBuffer<GridBuffer> grid;

        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Waypoint> Waypoints;

        // [NativeDisableParallelForRestriction] 
        // public ComponentDataFromEntity<PathAgentStatus> agents;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        [DeallocateOnJobCompletion] //doesnt support
        public NativeArray<PathRequestAgent> pathList;

        //SHARED DATA
        [NativeDisableParallelForRestriction]
        public NativeArray<float> CostSoFar; // g cost for each cell of grid

        [NativeDisableParallelForRestriction]
        public NativeArray<int2> CameFrom;

        [NativeDisableParallelForRestriction]
        public NativeMinHeap OpenSet;

        [ReadOnly]
        public NativeArray<int2> neighbours;

        private struct BoxData {
            public DynamicBuffer<float3> waypoints;
            public int dimX;
            public int dimY;
            public int2 startPos;
            public int2 destination;
            public NativeSlice<float> costSoFar;
            public NativeSlice<int2> cameFrom;
            public NativeMinHeap openSet;
        }

        public void Execute(int index) {
            var size = dimX * dimY;
            var costSoFar = CostSoFar.Slice(index * size, size);
            var cameFrom = CameFrom.Slice(index * size, size);
            var openSetSize = (IterationLimit + 1) * NeighborCount;
            var openSet = OpenSet.Slice(index * openSetSize, openSetSize);

            var pathReqInd = index;

            while (pathList.Length > pathReqInd) {
                var request = pathList[pathReqInd];
                pathReqInd += WorkerCount;
                var buffer = costSoFar.GetUnsafePtr();
                UnsafeUtility.MemClear(buffer, (long) costSoFar.Length * UnsafeUtility.SizeOf<float>());
                openSet.Clear();


                var owner = request.owner;
                // Debug.Log(owner);
                if (owner == Entity.Null) continue;
                DynamicBuffer<float3> waypoints = Waypoints[owner].Reinterpret<float3>();
                waypoints.Clear();

                var start = request.startCoord;
                var destination = request.destination;

                var tmpBox = new BoxData() {
                    waypoints = waypoints,
                    dimX = dimX,
                    dimY = dimY,
                    startPos = start,
                    destination = destination,
                    costSoFar = costSoFar,
                    cameFrom = cameFrom,
                    openSet = openSet,
                };

                if (FindPath(ref tmpBox)) {
                    BuildPath(ref tmpBox);
                    // agents[owner] = new PathAgentStatus() {
                    //     Value = AgentStatus.Ready,
                    // };
                }
            }
        }

        private bool FindPath(ref BoxData box) {
            if (box.startPos.Equals(box.destination)) {
                var indexCell = GridUtils.CoordToIndex(box.destination, box.dimX);
                box.waypoints.Add(GridUtils.CoordToWorld(grid, indexCell));
                return false;
            }

            // init first path cell (owned)
            var H = GridUtils.H(box.startPos, box.destination);
            var head = new MinHeapNode(box.startPos, H, H);
            box.openSet.Push(head);
            var closest = head;

            while (box.openSet.HasNext()) {
                var current = box.openSet.Pop();
                if (current.DistanceToGoal < closest.DistanceToGoal) {
                    closest = current;
                }

                if (current.Position.Equals(box.destination)) {
                    return true;
                }

                var fromIndex = GridUtils.CoordToIndex(current.Position, box.dimX);
                var initialCost = box.costSoFar[fromIndex];

                for (int i = 0; i < neighbours.Length; i++) {
                    var neighbour = neighbours[i];
                    var nextPosition = current.Position + neighbour;

                    if (nextPosition.x < 0 || nextPosition.x >= box.dimX || nextPosition.y < 0 ||
                        nextPosition.y >= box.dimY) {
                        continue;
                    }

                    var toIndex = GridUtils.CoordToIndex(nextPosition, box.dimX);
                    var nextCellCost = Cost(toIndex);
                    if (float.IsInfinity(nextCellCost)) {
                        continue;
                    }

                    var newCost = initialCost + nextCellCost;
                    var oldCost = box.costSoFar[toIndex];

                    // its not a new cell and cost less then new
                    if (oldCost > 0 && oldCost <= newCost) {
                        continue;
                    }

                    //put new cost
                    box.costSoFar[toIndex] = newCost;
                    box.cameFrom[toIndex] = current.Position;

                    var h = GridUtils.H(nextPosition, box.destination);
                    var expectedCost = newCost + h;
                    var newNode = new MinHeapNode(nextPosition, expectedCost, h);
                    box.openSet.Push(newNode);
                }
            }

            return false;
        }

        private void BuildPath(ref BoxData box) {
            var ind = GridUtils.CoordToIndex(box.destination, box.dimX);
            var destCoord = GridUtils.CoordToWorld(grid, ind);
            box.waypoints.Add(destCoord);

            var currCoord = box.cameFrom[ind];
            ind = GridUtils.CoordToIndex(currCoord, box.dimX);
            var next = GridUtils.CoordToWorld(grid, ind);

            while (!currCoord.Equals(box.startPos)) {
                var prev = box.cameFrom[GridUtils.CoordToIndex(currCoord, box.dimX)];
                currCoord = prev;
                var tmp = next;
                ind = GridUtils.CoordToIndex(currCoord, box.dimX);
                next = GridUtils.CoordToWorld(grid, ind);
                box.waypoints.Add(tmp);
            }

            box.waypoints.Reverse();
        }

        private float Cost(int index) {
            var cell = grid[index];
            if (cell.Type == CellType.Wall)
                return float.PositiveInfinity;
            return 1;
        }
    }

    protected override void OnUpdate() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var dimX = MazeBootstrap.Instance.dimX;
        var dimY = MazeBootstrap.Instance.dimY;

        var gridEntity = gridBufferGroup.GetSingletonEntity();
        var gridBuffer = em.GetBuffer<GridBuffer>(gridEntity);
        // pathList.Clear();
        // var pathList = new NativeList<PathRequestAgent>(Allocator.TempJob);

        //TODO fps down, used Entities.ForEach instead
        // var pathReqToListJob = new PathRequestToListJob() {
        //     LastSystemVersion = LastSystemVersion,
        //     PathAgentStatusArchetype = GetArchetypeChunkComponentType<PathAgentStatus>(true),
        //     PathRequestAgentArchetype = GetArchetypeChunkComponentType<PathRequestAgent>(true),
        //     pathList = pathList,
        // }.Schedule();
        // pathReqToListJob.Complete();
        // var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        // // var addPathToPathList =
        // Entities.WithName("Add_path_toPathList")
        //     .WithoutBurst()
        //     .WithStructuralChanges()
        //     .WithAll<Spirit, HauntingTag, PathAgentStatusFindTag>()
        //     .WithNone<PathAgentStatusNoneTag>()
        //     .ForEach((Entity entity, int entityInQueryIndex, ref PathRequestAgent path) => {
        //         pathList1.Add(path);
        //         // em.RemoveComponent<PathAgentStatusFindTag>(entity);
        //         // em.AddComponent<PathAgentStatusProcessTag>(entity);
        //         Debug.Log(pathList.Length);
        //         Debug.Log(entity);
        //     }).Run();

        // m_EndSimulationEcbSystem.AddJobHandleForProducer(addPathToPathList);

        int count = pathRequestGroup.CalculateEntityCount();
        var pathArray = new NativeArray<PathRequestAgent>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory );

        // TODO need optimization
        var addPathToPathListJobHandle = 
        Entities.WithName("Add_path_toPathList")
            .WithBurst()
            // .WithStructuralChanges()
            .WithAll<HauntingTag, PathAgentStatusFindTag, PathAgentStatusProcessTag>()
            //.WithChangeFilter<PathRequestAgent>()
            .ForEach((int entityInQueryIndex, in PathRequestAgent path) => {
                pathArray[entityInQueryIndex] = path;
            }).ScheduleParallel(Dependency);
        Dependency = addPathToPathListJobHandle;

        var findPathAStarJob = new FindPathAStarJob {
            // agents = GetComponentDataFromEntity<PathAgentStatus>(),
            grid = gridBuffer,
            dimX = dimX,
            dimY = dimY,
            neighbours = Neighbours,
            Waypoints = GetBufferFromEntity<Waypoint>(),
            pathList = pathArray,
            OpenSet = OpenSet,
            CostSoFar = CostSoFar,
            CameFrom = CameFrom,
        }.Schedule(WorkerCount, InnerLoopBatchSize, addPathToPathListJobHandle);
        Dependency = findPathAStarJob;
        // pathList.Dispose(findPathAStarJob);

    }

    //TODO add optimization quadrant NativeMultiHashMap

    protected override void OnDestroy() {
        CostSoFar.Dispose();
        CameFrom.Dispose();
        OpenSet.Dispose();
        Neighbours.Dispose();
        //pathList.Dispose();
    }
}