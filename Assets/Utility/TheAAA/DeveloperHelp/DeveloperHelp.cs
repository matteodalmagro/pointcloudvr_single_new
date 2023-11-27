using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DeveloperHelp 
{
    

    /// <summary>
    /// Clear PlayerPrefs
    /// </summary>
    [MenuItem("Tools / AAA Helper / Clear All PlayerPrefs")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Show.LogGray("Clear All PlayerPrefs.");
    }

    /// <summary>
    /// Create and a object and enable SceneScale system in it.
    /// </summary>
    [MenuItem("Tools / AAA Helper / Measuring Tool")]
    public static void EditorSceneScale()
    {
        GameObject obj = new GameObject("Measuring Tool");
        GameObject obj2 = new GameObject("ScalePoint");
        obj2.transform.parent = obj.transform;
        obj.AddComponent<EditorSceneScaleTool>();
        EditorSceneScaleTool sceneScaleTool = obj.GetComponent<EditorSceneScaleTool>();

        sceneScaleTool.xAxis = true;
        sceneScaleTool.yAxis = true;
        sceneScaleTool.zAxis = true;
        sceneScaleTool.showScale = false;
        sceneScaleTool.sizeType = DistanceType.Meter;

        sceneScaleTool.scalingObject = obj2.transform;
        
    }

    




}
