using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

public class SimplePathGenerator : MonoBehaviour
{
    public GameObject CostUIPrefab;

    public bool CostField;

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
        if (Input.GetKeyDown (KeyCode.C)) CostField = !CostField;
        if (Input.GetKeyDown (KeyCode.G)) Generate ();
    }

    public void Generate ()
    {
        NavUtils.GenerateDijkstraIntegrationField (MapCollidersSingleton.Infos,
            new int2 (GameSetting.MAP_WIDTH, GameSetting.MAP_HEIGHT), Target);

        for (int y = 0; y < GameSetting.MAP_HEIGHT; y++)
        {
            for (int x = 0; x < GameSetting.MAP_WIDTH; x++)
            {
                int idx = y * GameSetting.MAP_WIDTH + x;

                int value = CostField ? MapCollidersSingleton.Infos[idx].CostField :
                    MapCollidersSingleton.Infos[idx].IntegrationField;

                if (value == int.MaxValue)
                {
                    _texts[idx].text = "INF";
                }
                else
                {
                    _texts[idx].text = value.ToString ();
                }
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