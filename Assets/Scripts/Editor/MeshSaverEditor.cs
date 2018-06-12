using UnityEngine;
using UnityEditor;

internal static class MeshSaverEditor
{
    [MenuItem ("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
    private static void SaveMeshNewInstanceItem (MenuCommand menuCommand)
    {
        MeshFilter mf = menuCommand.context as MeshFilter;
        Mesh m = mf.sharedMesh;

        string path = EditorUtility.SaveFilePanel ("Save Separate Mesh Asset", "Assets/", m.name, "asset");
        if (string.IsNullOrEmpty (path)) return;

        path = FileUtil.GetProjectRelativePath (path);

        Mesh meshToSave = UnityEngine.Object.Instantiate (m) as Mesh;
        MeshUtility.Optimize (meshToSave);

        AssetDatabase.CreateAsset (meshToSave, path);
        AssetDatabase.SaveAssets ();
    }
}