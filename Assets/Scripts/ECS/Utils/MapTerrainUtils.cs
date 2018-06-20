using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MapTerrainUtils
{
    public static int Hash (float2 position)
    {
        int2 quantized = new int2 (position);
        return quantized.x + GameSetting.MAP_WIDTH / 2 + (quantized.y + GameSetting.MAP_HEIGHT / 2) * GameSetting.MAP_WIDTH;
    }

    public static void AddCollider (MapTerrainInfo map, GameObject collider)
    {
        int hash = Hash (new float2 (collider.transform.position.x, collider.transform.position.z));

        List<GameObject> colliders;
        map.BarrierColliders.TryGetValue (hash, out colliders);

        if (colliders == null)
        {
            colliders = new List<GameObject> (1);
            colliders.Add (collider);
            map.BarrierColliders[hash] = colliders;
        }
        else if (!colliders.Contains (collider))
        {
            colliders.Add (collider);
            map.BarrierColliders[hash] = colliders;
        }
    }

    public static GameObject GetCollider (MapTerrainInfo map, float2 pos)
    {
        int hash = Hash (pos);

        List<GameObject> colliders;
        map.BarrierColliders.TryGetValue (hash, out colliders);

        if (colliders != null && colliders.Count > 0)
        {
            return colliders[0];
        }

        return null;
    }

    public static void RemoveColliders (MapTerrainInfo map, float2 pos, bool destroy = false)
    {
        int hash = Hash (pos);

        List<GameObject> colliders;
        map.BarrierColliders.TryGetValue (hash, out colliders);

        if (colliders != null)
        {
            map.BarrierColliders.Remove (hash);
            
            if (colliders.Count > 0 && destroy)
            {
                foreach (var collider in colliders)
                {
                    if (collider != null) Object.Destroy (collider);
                }
            }
        }
    }
}