using System.Collections.Generic;
using Unity.Mathematics;


public static class NavUtils
{
    public static void GenerateDijkstraIntegrationField (MapColliderInfo[] map, int2 mapSize, int2 target)
    {
        int maxValue = int.MaxValue;

        for (int i = 0; i < map.Length; i++)
        {
            map[i].IntegrationField = maxValue;
        }

        int tarId = target.y * mapSize.x + target.x;
        map[tarId].IntegrationField = 0;

        Queue<int> openList = new Queue<int> (mapSize.x * mapSize.y / 10);
        openList.Enqueue (tarId);

        int[] neighborIds = new int[4];

        while (openList.Count > 0)
        {
            int curId = openList.Dequeue ();

            int curY = curId / mapSize.x;
            int curX = curId % mapSize.x;

            int endCost = 0;
            int neighborLength = 0;

            if (curY > 0) neighborIds[neighborLength++] = curId - mapSize.x;
            if (curY < (mapSize.y - 1)) neighborIds[neighborLength++] = curId + mapSize.x;
            if (curX > 0) neighborIds[neighborLength++] = curId - 1;
            if (curX < (mapSize.x - 1)) neighborIds[neighborLength++] = curId + 1;

            int neighborId = 0;
            for (int i = 0; i < neighborLength; i++)
            {
                neighborId = neighborIds[i];
                if (map[neighborId].CostField == 255) continue;
                endCost = map[curId].IntegrationField + map[neighborId].CostField + 1;
                if (endCost < map[neighborId].IntegrationField)
                {
                    if (!openList.Contains (neighborId)) openList.Enqueue (neighborId);
                    map[neighborId].IntegrationField = endCost;
                }
            }
        }
    } 
}