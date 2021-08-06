using Unity.Entities;


[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(InitializationGroup))]
public class InitGroup : ComponentSystemGroup {
    // InitializationMazeGridSystem
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(InitGroup))]
public class InitializationGroup : ComponentSystemGroup {
    // UpdateCellIndexSystem
    // UpdateQuadrantSystem
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(LateSimulationSystemGroup))]
public class PreSimulationSystemGroup : ComponentSystemGroup {
    //- GridMovementSystem:
    //- 
    //- Update Cell Index for each character on scene
    //- Check wall job,
    //- Spirits movement (BackForth.. etc)
    // 
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PreSimulationSystemGroup))]
public class LateSimulationSystemGroup : ComponentSystemGroup {
    // PathFindingSystem
    // CameraTrackSystem
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(LateSimulationSystemGroup))]
public class AbilitySystemGroup : ComponentSystemGroup {
    // AttackAbilitySystem
    //FearSystem
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(AfterSimulationSystemGroup))]
public class CollisionSystemGroup : ComponentSystemGroup {
    //AABBCollision
    //AbsorptionSpirit
    //DestroySystem
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystemGroup))]
public class AfterSimulationSystemGroup : ComponentSystemGroup {
    // PathMovementSystem
    // MoveSystem
    // IndicatorPointerSystem
}