using System;
using System.Collections.Generic;
using UnityEngine;

public class MapTerrainInfo
{
    public MapTerrainInfo (int width, int height)
    {
        Terrains = new TerrainType[height, width];
        BarrierColliders = new Dictionary<int, List<GameObject>> ();

        MapWidth = width;
        MapHeigth = height;
    }

    public static MapTerrainInfo GameMap;

    public TerrainType[, ] Terrains;

    public Dictionary<int, List<GameObject>> BarrierColliders;

    public readonly int MapWidth;

    public readonly int MapHeigth;

}

[Flags]
public enum TerrainType
{
    Normal = 0,
    Barrier
}