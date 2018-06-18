using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

public class SimplePathGenerator : MonoBehaviour
{
    public GameObject CostUIPrefab;

    public bool UseCanonicalDij;

    public int2 Target;

    private List<TextMesh> _texts;

    private void OnEnable ()
    {
        _texts = new List<TextMesh> (GameSetting.MAP_HEIGHT * GameSetting.MAP_WIDTH);

        Quaternion rot = CostUIPrefab.transform.rotation;
        Vector3 offset = CostUIPrefab.transform.position;

        for (int y = 0; y < GameSetting.MAP_HEIGHT; y++)
        {
            for (int x = 0; x < GameSetting.MAP_WIDTH; x++)
            {
                _texts.Add (Instantiate (CostUIPrefab, new Vector3 (x, 0, y) + offset, rot, transform)
                    .GetComponent<TextMesh> ());
            }
        }
    }

    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.C)) UseCanonicalDij = !UseCanonicalDij;
        if (Input.GetKeyDown (KeyCode.G)) Generate ();
    }

    public void Generate ()
    {
        float startTime = DateTime.Now.Millisecond;

        if (UseCanonicalDij)
        {
            NavUtils.GenerateCanonicalDijkstraIntegratField (MapColliderInfo.GameMap, Target);
        }
        else
        {
            NavUtils.GenerateDijkstraIntegratField (MapColliderInfo.GameMap, Target);
        }
        NavUtils.GenerateFlowField (MapColliderInfo.GameMap);

        Debug.LogFormat ("Generate Cost Time : {0}", DateTime.Now.Millisecond - startTime);

        for (int y = 0; y < GameSetting.MAP_HEIGHT; y++)
        {
            for (int x = 0; x < GameSetting.MAP_WIDTH; x++)
            {
                int idx = y * GameSetting.MAP_WIDTH + x;

                float value = MapColliderInfo.GameMap.Infos[x, y].IntegrationField;

                if (value == float.MaxValue)
                {
                    _texts[idx].text = "INF";
                }
                else
                {
                    _texts[idx].text = string.Format ("{0:F1}", value);
                }
            }
        }
    }

    private void OnDrawGizmos ()
    {
        Gizmos.color = Color.black;

        TileColliderInfo[, ] tiles = MapColliderInfo.GameMap.Infos;

        int mapWidth = MapColliderInfo.GameMap.MapWidth;
        int mapHeight = MapColliderInfo.GameMap.MapHeight;

        Vector3 origin = new Vector3 (0, 0f, 0);

        for (int y = 1; y < (mapHeight - 1); y++)
        {
            for (int x = 1; x < (mapWidth - 1); x++)
            {
                origin.x = x + 0.5f;
                origin.z = y + 0.5f;
                float2 dir = 0.5f * TileColliderInfo.FlowFieldVector[(int) tiles[x, y].FlowField];
                Gizmos.DrawLine (origin, origin + new Vector3 (dir.x, 0, dir.y));
            }
        }
    }
}

[CustomEditor (typeof (SimplePathGenerator))]
public class SimplePathGeneratorEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();

        var tar = target as SimplePathGenerator;

        if (GUILayout.Button ("Generate"))
        {
            tar.Generate ();
        }
    }
}