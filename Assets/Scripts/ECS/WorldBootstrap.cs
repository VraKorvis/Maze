using System;
using System.Collections.Generic;
using Unity.Entities;

//public class WorldBootstrap : ICustomBootstrap {
//
//    // Returns the systems which should be handled by the default bootstrap process.
//    // If null is returned the default world will not be created at all.
//    // Empty list creates default world and entrypoints
//    public List<Type> Initialize(List<Type> systems) {
//        var world = World.DefaultGameObjectInjectionWorld; // World.DefaultGameObjectInjectionWorld is always set before ICustomBootstrap.Initialize is called
//
//        
//        //AddSystem(new PathFindingSystem(), world, systems);
//      
//        return systems;
//    }
//
//    private static void AddSystem(ComponentSystemBase system, World world, ICollection<Type> systems) {
//        world.AddSystem(system);
//        systems.Add(system.GetType());
//    }
//}