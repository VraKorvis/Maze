using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Maze.Data.Storages {

    [Serializable]
    public class MazeData {
        public List<LayerTilemap> layers = new List<LayerTilemap>();
    }

    [Serializable]
    public class LayerTilemap {
        public TileD[]        tiles;
        public LayerOrderType order;
    }

    [Serializable]
    public class TileD {
        public Vector3Int coord;
        public string     nameTile;

        public TileD(Vector3Int coord, string nameTile) {
            this.coord        = coord;
            this.nameTile = nameTile;
        }
    }

    public struct MazeElementsData {
        public float3 startPosition;
        public float3 endPosition;
    }

    [Serializable]
    public enum LayerOrderType {
        Ground = -5,
        Wall   = -4,
        Mask   = -1,
        Trap,
    }

}