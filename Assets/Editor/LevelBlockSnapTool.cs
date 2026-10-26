using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class LevelBlockSnapTool
{
    private static bool snapEnabled = false;

    static LevelBlockSnapTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Level Blocks/Snap On/On")]
    private static void ToggleSnap()
    {
        snapEnabled = !snapEnabled;
        Menu.SetChecked("Tools/Level Blocks/Snap On", snapEnabled);

        Debug.Log("Level Block Snap: " + (snapEnabled ? "ON" : "OFF"));
    }

    [MenuItem("Tools/Level Blocks/Snap On", true)]
    private static bool ValidateToggleSnap()
    {
        Menu.SetChecked("Tools/Level Blocks/Snap On", snapEnabled);
        return true;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!snapEnabled)
            return;

        Event e = Event.current;

        // Solo nos interesa cuando soltamos algo arrastrado
        if (e.type != EventType.DragPerform)
            return;

        // Comprobamos que estamos arrastrando un GameObject
        if (DragAndDrop.objectReferences.Length == 0)
            return;

        GameObject prefab = DragAndDrop.objectReferences[0] as GameObject;

        if (prefab == null)
            return;

        // Comprobamos que sea un prefab
        if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
            return;

        // Convertimos la posición del ratón en una posición 3D
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        // Plano XY, con Z = 0
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (!plane.Raycast(ray, out float distance))
            return;

        Vector3 position = ray.GetPoint(distance);

        // Snap de 1 metro
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        position.z = 0;

        // Creamos el prefab manteniendo su vínculo con el prefab original
        GameObject instance =
            (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        Undo.RegisterCreatedObjectUndo(instance, "Place Level Block");

        instance.transform.position = position;

        // Lo seleccionamos
        Selection.activeGameObject = instance;

        // Confirmamos el Drag & Drop
        DragAndDrop.AcceptDrag();

        e.Use();
    }
}