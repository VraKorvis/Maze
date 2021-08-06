using System;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class CharacterFactory {
    public static Entity CreateCharacter() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
//        switch (type) {
//            case ArchetypeCharacter.Shaman:
//                var player = em.CreateArchetype(typeof(PlayerData),
//                    typeof(LocalToWorld),
//                    typeof(Scale),
//                    typeof(RenderMesh),
//                    typeof(MoveSettings),
//                    typeof(Heading),
//                    typeof(CheckWallTag),
//                    typeof(CellIndex),
//                    typeof(AABB),
//                    typeof(TurnTag),
//                    typeof(RespawnData));
//                return em.CreateEntity(player);
//
//            case ArchetypeCharacter.BackForth:    break;
//            case ArchetypeCharacter.Circle:       break;
//            case ArchetypeCharacter.Circumential: break;
//            case ArchetypeCharacter.Boss:         break;
//        }

        return Entity.Null;
    }

    public static GameObject LoadShaman() {
        return Resources.Load(StringConstant.SHAMAN) as GameObject;
    }
}