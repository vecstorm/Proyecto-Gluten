using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneExit))]
public class SceneExitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Scene Exit ID
        SerializedProperty idProperty =
            serializedObject.FindProperty("sceneExitId");

        EditorGUILayout.PropertyField(
            idProperty,
            new GUIContent("Scene Exit ID")
        );

        // Destination Scene Exit ID
        SerializedProperty destinationExitIdProperty =
            serializedObject.FindProperty("destinationSceneExitId");

        EditorGUILayout.PropertyField(
            destinationExitIdProperty,
            new GUIContent("Destination Scene Exit ID")
        );

        // Scene manager
        ChangeSceneManager manager =
            FindFirstObjectByType<ChangeSceneManager>();

        if (manager == null)
        {
            EditorGUILayout.HelpBox(
                "No se encuentra ChangeSceneManager en la escena.",
                MessageType.Warning
            );

            serializedObject.ApplyModifiedProperties();
            return;
        }

        var scenes = manager.GetSceneNames();

        if (scenes.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No hay escenas registradas en el manager.",
                MessageType.Info
            );

            serializedObject.ApplyModifiedProperties();
            return;
        }

        // Destination Scene
        string[] options = scenes.ToArray();

        SerializedProperty indexProperty =
            serializedObject.FindProperty("destinationIndex");

        indexProperty.intValue = EditorGUILayout.Popup(
            "Destination Scene",
            Mathf.Clamp(indexProperty.intValue, 0, options.Length - 1),
            options
        );

        serializedObject.ApplyModifiedProperties();
    }
}