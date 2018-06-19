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

        int mapWidth = map.MapWidth;
        int mapHeigth = map.MapHeight;
        byte[, ] costField = map.CostField;
        float[, ] inteField = map.IntegrationField;

        for (int y = 0; y < mapHeigth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                inteField[x, y] = maxValue;
            }
        }
        inteField[target.x, target.y] = 0;

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

                endCost = inteField[cur.x, cur.y] + costField[neighborId.x, neighborId.y] + 1;
                if (endCost < inteField[neighborId.x, neighborId.y])
                {
                    if (!openList.Contains (neighborId)) openList.Enqueue (neighborId);
                    inteField[neighborId.x, neighborId.y] = endCost;
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

        int mapWidth = map.MapWidth;
        int mapHeight = map.MapHeight;
        byte[, ] costField = map.CostField;
        float[, ] inteField = map.IntegrationField;
        IntegrateFlag[, ] inteInfos = map.IntegrateInfos;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                inteField[x, y] = maxValue;
                inteInfos[x, y] &= ~IntegrateFlag.Visited; // Init all field visited flag to false
            }
        }

        FastPriorityQueue<PathNode> openList = new FastPriorityQueue<PathNode> (mapWidth * mapHeight / 10);

        // Origin 
        inteField[target.x, target.y] = 0;
        inteInfos[target.x, target.y] |= IntegrateFlag.Visited;
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
                inteField[bestNode.Node.x, bestNode.Node.y]);
        }
    }

    public static void GenerateFlowField (MapColliderInfo map)
    {
        float[] neighboers = new float[8];
        bool[] blocks = new bool[8];

        float[, ] inteField = map.IntegrationField;
        FlowFieldDir[, ] flowField = map.FlowField;

        float maxValue = float.MaxValue;

        for (int y = 1; y < (map.MapHeight - 1); y++)
        {
            for (int x = 1; x < (map.MapWidth - 1); x++)
            {
                if (MapColliderUtils.UnReachable (map, x, y))
                {
                    flowField[x, y] = FlowFieldDir.None;
                    continue;
                }

                blocks[0] = MapColliderUtils.UnReachable (map, x - 1, y + 1);
                blocks[1] = MapColliderUtils.UnReachable (map, x, y + 1);
                blocks[2] = MapColliderUtils.UnReachable (map, x + 1, y + 1);
                blocks[3] = MapColliderUtils.UnReachable (map, x - 1, y);
                blocks[4] = MapColliderUtils.UnReachable (map, x + 1, y);
                blocks[5] = MapColliderUtils.UnReachable (map, x - 1, y - 1);
                blocks[6] = MapColliderUtils.UnReachable (map, x, y - 1);
                blocks[7] = MapColliderUtils.UnReachable (map, x + 1, y - 1);


                neighboers[0] = (blocks[3] && blocks[1]) ? maxValue : inteField[x - 1, y + 1];
                neighboers[1] = (blocks[1]) ? maxValue : inteField[x, y + 1];
                neighboers[2] = (blocks[1] && blocks[4]) ? maxValue : inteField[x + 1, y + 1];
                neighboers[3] = (blocks[3]) ? maxValue : inteField[x - 1, y];
                neighboers[4] = (blocks[4]) ? maxValue : inteField[x + 1, y];
                neighboers[5] = (blocks[3] && blocks[6]) ? maxValue : inteField[x - 1, y - 1];
                neighboers[6] = (blocks[6]) ? maxValue : inteField[x, y - 1];
                neighboers[7] = (blocks[4] && blocks[6]) ? maxValue : inteField[x + 1, y - 1];

                int minIdx = 0;

                for (int i = 1; i < 8; i++)
                {
                    if (neighboers[i] < neighboers[minIdx]) minIdx = i;
                }

                switch (minIdx)
                {
                    case 0:
                        flowField[x, y] = FlowFieldDir.Deg135;
                        break;
                    case 1:
                        flowField[x, y] = FlowFieldDir.Deg90;
                        break;
                    case 2:
                        flowField[x, y] = FlowFieldDir.Deg45;
                        break;
                    case 3:
                        flowField[x, y] = FlowFieldDir.Deg180;
                        break;
                    case 4:
                        flowField[x, y] = FlowFieldDir.Deg0;
                        break;
                    case 5:
                        flowField[x, y] = FlowFieldDir.Deg225;
                        break;
                    case 6:
                        flowField[x, y] = FlowFieldDir.Deg270;
                        break;
                    case 7:
                        flowField[x, y] = FlowFieldDir.Deg315;
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
            IntegrateFlag integrateFlag = map.IntegrateInfos[start.x, start.y];

            if (integrateFlag.HasFlag (IntegrateFlag.Visited))
            {
                if (map.IntegrationField[start.x, start.y] > cost)
                {
                    map.IntegrationField[start.x, start.y] = cost;
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
                    map.IntegrateInfos[start.x, start.y] &= ~IntegrateFlag.Visited;
                }
                map.IntegrationField[start.x, start.y] = math.min (map.IntegrationField[start.x, start.y], cost);
                openList.Enqueue (new PathNode (start, newdir), cost);
                return;
            }
            else
            {
                map.IntegrateInfos[start.x, start.y] |= IntegrateFlag.Visited;
                map.IntegrationField[start.x, start.y] = cost;
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
            IntegrateFlag integrateFlag = map.IntegrateInfos[start.x, start.y];

            if (integrateFlag.HasFlag (IntegrateFlag.Visited))
            {
                if (map.IntegrationField[start.x, start.y] > cost)
                {
                    map.IntegrationField[start.x, start.y] = cost;
                }
                else
                {
                    return;
                }
            }
            else
            {
                map.IntegrateInfos[start.x, start.y] |= IntegrateFlag.Visited;
                map.IntegrationField[start.x, start.y] = cost;
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

        map.IntegrationField[next.x, next.y] = cost;
        openList.Enqueue (new PathNode (next, dir), cost);
    }


    /// <summary>
    /// If node is a jump point, return the new direction. Otherwise return int2(0,0)
    /// </summary>
    private static int2 IsJumpPoint (MapColliderInfo map, int2 child, int2 dir)
    {
        int2 parent = child - dir;
        int2 falseReval = new int2 (0, 0);

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