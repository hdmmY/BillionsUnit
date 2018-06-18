using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Priority_Queue;


public static class NavUtils
{

    #region Public Methods

    public static void GenerateDijkstraIntegratField (MapColliderInfo map, int2 target)
    {
        float maxValue = float.MaxValue;

        TileColliderInfo[, ] tiles = map.Infos;
        int mapWidth = map.MapWidth;
        int mapHeigth = map.MapHeight;

        for (int y = 0; y < mapHeigth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tiles[x, y].IntegrationField = maxValue;
            }
        }

        tiles[target.x, target.y].IntegrationField = 0;

        Queue<int2> openList = new Queue<int2> (mapWidth * mapHeigth / 10);
        openList.Enqueue (target);

        int2[] neighborIds = new int2[4];

        while (openList.Count > 0)
        {
            int2 cur = openList.Dequeue ();

            float endCost = 0;
            int neighborLength = 0;

            if (cur.y > 0) neighborIds[neighborLength++] = new int2 (cur.x, cur.y - 1);
            if (cur.y < (mapHeigth - 1)) neighborIds[neighborLength++] = new int2 (cur.x, cur.y + 1);
            if (cur.x > 0) neighborIds[neighborLength++] = new int2 (cur.x - 1, cur.y);
            if (cur.x < (mapWidth - 1)) neighborIds[neighborLength++] = new int2 (cur.x + 1, cur.y);

            int2 neighborId;
            for (int i = 0; i < neighborLength; i++)
            {
                neighborId = neighborIds[i];
                if (MapColliderUtils.UnReachable (map, neighborId.x, neighborId.y)) continue;

                endCost = tiles[cur.x, cur.y].IntegrationField + tiles[neighborId.x, neighborId.y].CostField + 1;
                if (endCost < tiles[neighborId.x, neighborId.y].IntegrationField)
                {
                    if (!openList.Contains (neighborId)) openList.Enqueue (neighborId);
                    tiles[neighborId.x, neighborId.y].IntegrationField = endCost;
                }
            }
        }
    }

    /// <summary>
    /// Use the uniform cost field to generate the integration field.
    /// It is more efficient than <see cref = "GenerateDijkstraIntegratField" />
    /// </summary>
    public static void GenerateCanonicalDijkstraIntegratField (MapColliderInfo map, int2 target)
    {
        if (MapColliderUtils.UnReachable (map, target.x, target.y)) return;

        float maxValue = float.MaxValue;

        TileColliderInfo[, ] tiles = map.Infos;
        int mapWidth = map.MapWidth;
        int mapHeight = map.MapHeight;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tiles[x, y].IntegrationField = maxValue;
                tiles[x, y].IntegrateInfo &= ~IntegrateFlag.Visited; // Init all field visited flag to false
            }
        }

        FastPriorityQueue<PathNode> openList = new FastPriorityQueue<PathNode> (tiles.Length / 10);

        // Origin 
        tiles[target.x, target.y].IntegrationField = 0;
        tiles[target.x, target.y].IntegrateInfo |= IntegrateFlag.Visited;
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (1, 0));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (1, 1));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (0, 1));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (-1, 1));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (-1, 0));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (-1, -1));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (0, -1));
        SetOriginOpenListForCanonicalDijkstra (openList, map, target, new int2 (1, -1));

        while (openList.Count > 0)
        {
            var bestNode = openList.Dequeue ();

            CanonicalOrdering (openList, map, bestNode.Node, bestNode.Dir,
                tiles[bestNode.Node.x, bestNode.Node.y].IntegrationField);
        }
    }

    public static void GenerateFlowField (MapColliderInfo map)
    {
        float[] neighboers = new float[8];
        bool[] blocks = new bool[8];

        TileColliderInfo[, ] info = map.Infos;

        float maxValue = float.MaxValue;

        for (int y = 1; y < (map.MapHeight - 1); y++)
        {
            for (int x = 1; x < (map.MapWidth - 1); x++)
            {
                if (MapColliderUtils.UnReachable (map, x, y))
                {
                    info[x, y].FlowField = FlowFieldDir.None;
                    continue;
                }

                blocks[0] = MapColliderUtils.UnReachable (map, x - 1, y + 1);
                blocks[1] = MapColliderUtils.UnReachable (map, x    , y + 1);
                blocks[2] = MapColliderUtils.UnReachable (map, x + 1, y + 1);
                blocks[3] = MapColliderUtils.UnReachable (map, x - 1, y    );
                blocks[4] = MapColliderUtils.UnReachable (map, x + 1, y    );
                blocks[5] = MapColliderUtils.UnReachable (map, x - 1, y - 1);
                blocks[6] = MapColliderUtils.UnReachable (map, x    , y - 1);
                blocks[7] = MapColliderUtils.UnReachable (map, x + 1, y - 1);


                neighboers[0] = (blocks[3] && blocks[1]) ? maxValue : info[x - 1, y + 1].IntegrationField;
                neighboers[1] = (blocks[1]             ) ? maxValue : info[x    , y + 1].IntegrationField;
                neighboers[2] = (blocks[1] && blocks[4]) ? maxValue : info[x + 1, y + 1].IntegrationField;
                neighboers[3] = (blocks[3]             ) ? maxValue : info[x - 1, y    ].IntegrationField;
                neighboers[4] = (blocks[4]             ) ? maxValue : info[x + 1, y    ].IntegrationField;
                neighboers[5] = (blocks[3] && blocks[6]) ? maxValue : info[x - 1, y - 1].IntegrationField;
                neighboers[6] = (blocks[6]             ) ? maxValue : info[x    , y - 1].IntegrationField;
                neighboers[7] = (blocks[4] && blocks[6]) ? maxValue : info[x + 1, y - 1].IntegrationField;

                int minIdx = 0;

                for (int i = 1; i < 8; i++)
                {
                    if (neighboers[i] < neighboers[minIdx]) minIdx = i;
                }

                switch (minIdx)
                {
                    case 0:
                        info[x, y].FlowField = FlowFieldDir.Deg135;
                        break;
                    case 1:
                        info[x, y].FlowField = FlowFieldDir.Deg90;
                        break;
                    case 2:
                        info[x, y].FlowField = FlowFieldDir.Deg45;
                        break;
                    case 3:
                        info[x, y].FlowField = FlowFieldDir.Deg180;
                        break;
                    case 4:
                        info[x, y].FlowField = FlowFieldDir.Deg0;
                        break;
                    case 5:
                        info[x, y].FlowField = FlowFieldDir.Deg225;
                        break;
                    case 6:
                        info[x, y].FlowField = FlowFieldDir.Deg270;
                        break;
                    case 7:
                        info[x, y].FlowField = FlowFieldDir.Deg315;
                        break;
                }
            }
        }
    }

    #endregion


    #region Private Helper

    private class PathNode : FastPriorityQueueNode
    {
        public int2 Node;

        public int2 Dir;

        public PathNode (int2 node, int2 direction)
        {
            Node = node;
            Dir = direction;
        }
    }

    private static void CanonicalOrdering (FastPriorityQueue<PathNode> openList, MapColliderInfo map, int2 node, int2 dir, float cost)
    {
        if (dir.x == 0 || dir.y == 0)
        {
            MoveCardinal (openList, map, node, dir, cost);
        }
        else
        {
            MoveDiagonal (openList, map, node, dir, cost);
        }
    }

    private static void MoveCardinal (FastPriorityQueue<PathNode> openList, MapColliderInfo map, int2 start, int2 dir, float cost)
    {
        start += dir;
        cost += 1;

        while (!MapColliderUtils.UnReachable (map, start.x, start.y))
        {
            IntegrateFlag integrateFlag = map.Infos[start.x, start.y].IntegrateInfo;

            if (integrateFlag.HasFlag (IntegrateFlag.Visited))
            {
                if (map.Infos[start.x, start.y].IntegrationField > cost)
                {
                    map.Infos[start.x, start.y].IntegrationField = cost;
                }
                else
                {
                    return;
                }
            }

            int2 newdir = IsJumpPoint (map, start, dir);
            if (newdir.x != 0 || newdir.y != 0)
            {
                if (integrateFlag.HasFlag (IntegrateFlag.Visited))
                {
                    map.Infos[start.x, start.y].IntegrateInfo &= ~IntegrateFlag.Visited;
                }
                map.Infos[start.x, start.y].IntegrationField = math.min (
                    map.Infos[start.x, start.y].IntegrationField, cost);
                openList.Enqueue (new PathNode (start, newdir), cost);
                return;
            }
            else
            {
                map.Infos[start.x, start.y].IntegrateInfo |= IntegrateFlag.Visited;
                map.Infos[start.x, start.y].IntegrationField = cost;
                start += dir;
                cost += 1;
            }
        }
    }

    private static void MoveDiagonal (FastPriorityQueue<PathNode> openList, MapColliderInfo map, int2 start, int2 dir, float cost)
    {
        if (dir.x == 0 || dir.y == 0) return;

        while (!MapColliderUtils.UnReachable (map, start.x, start.y))
        {
            IntegrateFlag integrateFlag = map.Infos[start.x, start.y].IntegrateInfo;

            if (integrateFlag.HasFlag (IntegrateFlag.Visited))
            {
                if (map.Infos[start.x, start.y].IntegrationField > cost)
                {
                    map.Infos[start.x, start.y].IntegrationField = cost;
                }
                else
                {
                    return;
                }
            }
            else
            {
                map.Infos[start.x, start.y].IntegrateInfo |= IntegrateFlag.Visited;
                map.Infos[start.x, start.y].IntegrationField = cost;
            }

            MoveCardinal (openList, map, start, new int2 (dir.x, 0), cost);
            MoveCardinal (openList, map, start, new int2 (0, dir.y), cost);

            if (!MapColliderUtils.UnReachable (map, start + new int2 (dir.x, 0)) ||
                !MapColliderUtils.UnReachable (map, start + new int2 (0, dir.y)))
            {
                start += dir;
                cost += 1.5f;
            }
            else
            {
                return;
            }
        }
    }

    private static void SetOriginOpenListForCanonicalDijkstra (FastPriorityQueue<PathNode> openList,
        MapColliderInfo map, int2 origin, int2 dir)
    {
        int2 next = origin + dir;

        if (MapColliderUtils.UnReachable (map, next.x, next.y)) return;

        float cost = 1f;

        if (dir.x != 0 && dir.y != 0)
        {
            if (MapColliderUtils.UnReachable (map, origin + new int2 (dir.x, 0)) &&
                MapColliderUtils.UnReachable (map, origin + new int2 (0, dir.y)))
            {
                return;
            }

            cost = 1.5f;
        }

        map.Infos[next.x, next.y].IntegrationField = cost;
        openList.Enqueue (new PathNode (next, dir), cost);
    }


    /// <summary>
    /// If node is a jump point, return the new direction. Otherwise return int2(0,0)
    /// </summary>
    private static int2 IsJumpPoint (MapColliderInfo map, int2 child, int2 dir)
    {
        int2 parent = child - dir;
        int2 falseReval = new int2 (0, 0);

        TileColliderInfo[, ] tiles = map.Infos;

        if (dir.x == 0 && dir.y != 0)
        {
            if (parent.x > 0)
            {
                if (MapColliderUtils.IsWall (map, parent.x - 1, parent.y) &&
                    !MapColliderUtils.IsWall (map, child.x - 1, child.y))
                    return new int2 (-1, dir.y);
            }
            if (parent.x < (map.MapWidth - 1))
            {
                if (MapColliderUtils.IsWall (map, parent.x + 1, parent.y) &&
                    !MapColliderUtils.IsWall (map, child.x + 1, child.y))
                    return new int2 (1, dir.y);
            }
        }
        else if (dir.x != 0 && dir.y == 0)
        {
            if (parent.y > 0)
            {
                if (MapColliderUtils.IsWall (map, parent.x, parent.y - 1) &&
                    !MapColliderUtils.IsWall (map, child.x, child.y - 1))
                    return new int2 (dir.x, -1);
            }

            if (parent.y < (map.MapHeight - 1))
            {
                if (MapColliderUtils.IsWall (map, parent.x, parent.y + 1) &&
                    !MapColliderUtils.IsWall (map, child.x, child.y + 1))
                    return new int2 (dir.x, 1);
            }
        }

        return falseReval;
    }

    #endregion
}