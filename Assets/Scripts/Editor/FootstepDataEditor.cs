using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(FootstepData))]
public class FootstepDataEditor : Editor
{
    private SerializedProperty _groundFootsteps;
    private SerializedProperty _volume;
    private SerializedProperty _pitchVariation;
    private SerializedProperty _terrainLayerMappings;
    private SerializedProperty _objectTagMappings;

    private void OnEnable()
    {
        _groundFootsteps = serializedObject.FindProperty("groundFootsteps");
        _volume = serializedObject.FindProperty("volume");
        _pitchVariation = serializedObject.FindProperty("pitchVariation");
        _terrainLayerMappings = serializedObject.FindProperty("terrainLayerMappings");
        _objectTagMappings = serializedObject.FindProperty("objectTagMappings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Sound Section
        EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_groundFootsteps, true);
        EditorGUILayout.PropertyField(_volume);
        EditorGUILayout.PropertyField(_pitchVariation);

        EditorGUILayout.Space(10);

        // Terrain Layer Mapping Section
        EditorGUILayout.LabelField("Terrain Layer Mapping", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_terrainLayerMappings, true);

        EditorGUILayout.Space(10);

        // Object Tag Mapping Section with Tag Dropdown
        EditorGUILayout.LabelField("Object Tag Mapping", EditorStyles.boldLabel);
        DrawObjectTagMappings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawObjectTagMappings()
    {
        string[] tags = InternalEditorUtility.tags;

        EditorGUI.indentLevel++;

        _objectTagMappings.isExpanded = EditorGUILayout.Foldout(_objectTagMappings.isExpanded, $"Object Tag Mappings ({_objectTagMappings.arraySize})");

        if (_objectTagMappings.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Size field
            int newSize = EditorGUILayout.IntField("Size", _objectTagMappings.arraySize);
            if (newSize != _objectTagMappings.arraySize)
            {
                _objectTagMappings.arraySize = newSize;
            }

            // Draw each element
            for (int i = 0; i < _objectTagMappings.arraySize; i++)
            {
                SerializedProperty element = _objectTagMappings.GetArrayElementAtIndex(i);
                SerializedProperty tagProp = element.FindPropertyRelative("tag");
                SerializedProperty groundTypeProp = element.FindPropertyRelative("groundType");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Tag dropdown
                int currentIndex = System.Array.IndexOf(tags, tagProp.stringValue);
                if (currentIndex < 0) currentIndex = 0;

                int selectedIndex = EditorGUILayout.Popup("Tag", currentIndex, tags);
                if (selectedIndex >= 0 && selectedIndex < tags.Length)
                {
                    tagProp.stringValue = tags[selectedIndex];
                }

                // GroundType enum
                EditorGUILayout.PropertyField(groundTypeProp);

                // Remove button
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    _objectTagMappings.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            // Add button
            if (GUILayout.Button("Add Mapping"))
            {
                _objectTagMappings.InsertArrayElementAtIndex(_objectTagMappings.arraySize);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }
}
