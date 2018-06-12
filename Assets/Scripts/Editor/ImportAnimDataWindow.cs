using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImportAnimDataWindow : EditorWindow
{
    private SimpleSpriteAnimCollection _simpelSpriteCollection;

    private int _clipIndex;

    [MenuItem ("Tools/Import Anim Data")]
    private static void Init ()
    {
        var window = (ImportAnimDataWindow) EditorWindow.GetWindow (typeof (ImportAnimDataWindow));
        window.Show ();
    }

    private void OnGUI ()
    {
        _simpelSpriteCollection = EditorGUILayout.ObjectField ("Simple Animation",
            _simpelSpriteCollection, typeof (SimpleSpriteAnimCollection), true) as SimpleSpriteAnimCollection;

        _clipIndex = EditorGUILayout.IntField ("Anim Clip Index", _clipIndex);

        if (GUILayout.Button ("Import"))
        {
            if (_simpelSpriteCollection == null || _clipIndex < 0 ||
                _clipIndex >= _simpelSpriteCollection.AnimClips.Count)
            {
                return;
            }

            var sprites = Selection.objects.Where (x => x is Sprite)
                .Select (x => x as Sprite);

            if (sprites == null || sprites.Count () == 0) return;

            _simpelSpriteCollection.AnimClips[_clipIndex] = new SimpleAnimClip
            {
                AnimFrames = new List<Sprite> (sprites)
            };
        }
    }
}