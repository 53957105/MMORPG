using Malee.List;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player), true)]
public class PlayerEditor : Editor
{
    private SerializedProperty _characterProperty;
    private SerializedProperty _statesProperty;
    private ReorderableList _statesList;
    private ReorderableList _anyStateTransitionsList;
    private Player _player => (Player)target;

    private void OnEnable()
    {
        _characterProperty = serializedObject.FindProperty("Character");
        _statesProperty = serializedObject.FindProperty("States");
        _statesList = new(_statesProperty);
        _anyStateTransitionsList = new(serializedObject.FindProperty("AnyStateTransitions"));

        _statesList.drawElementCallback += (rect, index, element, label, selected, focused) =>
        {
            EditorGUI.PropertyField(rect, element, label, true);

            var leftBoardRect = new Rect(rect.xMin - 32, rect.yMin, 4f, rect.height + 4f);
            // var bottomBoardRect = new Rect(rect.xMin - 32, rect.yMax, rect.width + 32, 3f);

            var boardColor = index % 3 == 0 ?
                Color.yellow :
                (index % 3 == 1 ? Color.green : Color.cyan);

            EditorGUI.DrawRect(leftBoardRect, boardColor);
            // EditorGUI.DrawRect(bottomBoardRect, boardColor * 0.7f);

            if (element.isExpanded)
            {
                var iconRect = new Rect(rect)
                {
                    x = rect.x - 15,
                    y = rect.y + rect.height / 2 - 10,
                    width = 32,
                    height = 32
                };
                EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("d_BlendTree Icon"));
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_characterProperty);
        _statesList.DoLayoutList();
        _anyStateTransitionsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
