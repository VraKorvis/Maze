using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Maze.Data.Storages {

    [Serializable]
    public struct EnemySpiritsData {
        public int          lvl;
        public List<SpiritData> spirits;
    }

    [Serializable]
    public struct SpiritData {
        public Guise guise;
        public int   count;
    }

    [Serializable]
    public struct BackForthSpiritData {
        public int           cellCountToFirst;
        public int           cellCountToSecond;
        public float         speed;
        public MoveDirection direction;
        public float3        position;
    }

    [Serializable]
    public struct CircleSpiritData {
        public float     speed;
        public float     radius;
        public Clockwise clockwise;
        public float3    position;
    }

    [Serializable]
    public struct FlatCurveSpiritData {
        public float     speed;
        public float     radius;
        public Clockwise clockwise;
        public float3    position;
    }

    [Serializable]
    public struct HauntingSpirit {
        public float  speed;
        public float3 position;
    }

}