#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// defines a MenuItem to create the runtime DeveloperMenu scene object.
public static class DeveloperMenuEditor
{
    [MenuItem("GameObject/Developer Tools/Create Developer Menu", false, 10)]
    public static void CreateDeveloperMenu(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("DeveloperMenu");
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create DeveloperMenu");
        go.AddComponent<DeveloperMenu>();
        Selection.activeObject = go;
    }
}
#endif
