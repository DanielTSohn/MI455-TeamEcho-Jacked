using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GridSystem))]
public class GridSystemEditor : Editor
{
 public override void OnInspectorGUI()
    {
        GridSystem gridSupport = target as GridSystem;

        base.OnInspectorGUI();

        EditorGUILayout.Separator();

        if (GUILayout.Button("Instantiate Tiles", GUILayout.Width(170)))
        {
            gridSupport.InitializeTiles();
        }
        EditorGUILayout.Separator();

        if (GUILayout.Button("Remove Tiles", GUILayout.Width(170)))
        {
            gridSupport.RemoveTiles();
        }
        EditorGUILayout.Separator();

        if (GUI.changed && !Application.isPlaying)
        {
            EditorUtility.SetDirty(gridSupport);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}