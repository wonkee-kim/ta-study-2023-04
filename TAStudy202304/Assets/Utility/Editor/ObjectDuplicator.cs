using UnityEngine;
using UnityEditor;

public class ObjectDuplicator : MonoBehaviour
{
    public static void DuplicateMultipleObject(GameObject[] targetObjects, int count, Vector3 positionOffset)
    {
        for (int i = 0; i < count; i++)
        {
            foreach (var go in targetObjects)
            {
                GameObject g = PrefabUtility.GetCorrespondingObjectFromSource(go);
                GameObject d;
                if (g != null)
                {
                    d = PrefabUtility.InstantiatePrefab(g) as GameObject;
                }
                else
                {
                    d = Instantiate(g);
                }
                d.transform.SetPositionAndRotation(go.transform.position + positionOffset * (i + 1), go.transform.rotation);
                d.transform.parent = go.transform.parent;
            }
        }
    }

    public static void DuplicateMultipleObjectMatrix(GameObject[] targetObjects, int[] count, Vector3 positionOffset)
    {
        for (int z = 0; z < count[2]; z++)
        {
            for (int y = 0; y < count[1]; y++)
            {
                for (int x = 0; x < count[0]; x++)
                {
                    // Ignore myself
                    if (x == 0 && y == 0 && z == 0)
                    {
                        continue;
                    }
                    foreach (var go in targetObjects)
                    {
                        GameObject g = PrefabUtility.GetCorrespondingObjectFromSource(go);
                        GameObject d;
                        if (g != null)
                        {
                            d = PrefabUtility.InstantiatePrefab(g) as GameObject;
                        }
                        else
                        {
                            d = Instantiate(g);
                        }
                        d.transform.SetPositionAndRotation(go.transform.position + Vector3.Scale(positionOffset, new Vector3(x, y, z)), go.transform.rotation);
                        d.transform.parent = go.transform.parent;
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
public class ObjectDuplicatorWindow : EditorWindow
{
    public enum DuplicateType
    {
        Single,
        Matrix,
    }
    private DuplicateType _duplicateType = DuplicateType.Single;

    private GameObject[] _selectedGameObjects;
    private int _count = 10;
    private int[] _countMatrix = new int[] { 2, 2, 2 };
    private Vector3 _positionOffset = new Vector3(0f, 0.1f, 0f);

    private Vector2 _scrollPosition;

    [MenuItem("Edit/Duplicate Multiple")]
    private static void ShowWindow()
    {
        GetWindow<ObjectDuplicatorWindow>().Show();
    }

    private void OnSelectionChange()
    {
        _selectedGameObjects = Selection.gameObjects;
        Repaint();
    }

    private void OnGUI()
    {
        // If any window is not opened
        if (!EditorWindow.HasOpenInstances<ObjectDuplicatorWindow>())
        {
            return;
        }

        // Scroll
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        {
            // Title
            int selectedCount = (_selectedGameObjects == null) ? 0 : _selectedGameObjects.Length;
            EditorGUILayout.LabelField($"Selected Objects({selectedCount})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5f);

            // Type
            _duplicateType = (DuplicateType)EditorGUI.EnumPopup(new Rect(3, 20, position.width - 6, 15), "Type", _duplicateType);
            EditorGUILayout.Space(20f);

            // Input - Count
            switch (_duplicateType)
            {
                case DuplicateType.Single:
                    _count = EditorGUILayout.IntField("Count", _count);
                    break;
                case DuplicateType.Matrix:
                    EditorGUILayout.LabelField("Count");
                    EditorGUILayout.BeginHorizontal();
                    _countMatrix[0] = EditorGUILayout.IntField(_countMatrix[0]);
                    _countMatrix[1] = EditorGUILayout.IntField(_countMatrix[1]);
                    _countMatrix[2] = EditorGUILayout.IntField(_countMatrix[2]);
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            // Input - Position offset
            _positionOffset = EditorGUILayout.Vector3Field("Position Offset", _positionOffset);
            EditorGUILayout.Space(5f);

            // Availability
            bool anyObjectSelected = selectedCount > 0;
            bool countIsNotZero;
            switch (_duplicateType)
            {
                case DuplicateType.Single:
                default:
                    countIsNotZero = _count > 0;
                    break;
                case DuplicateType.Matrix:
                    countIsNotZero = _countMatrix[0] + _countMatrix[1] + _countMatrix[2] > 0;
                    break;
            }

            // Duplicate Button
            GUI.enabled = anyObjectSelected && countIsNotZero;
            if (GUILayout.Button("Duplicate Multiple"))
            {
                switch (_duplicateType)
                {
                    case DuplicateType.Single:
                    default:
                        ObjectDuplicator.DuplicateMultipleObject(_selectedGameObjects, _count, _positionOffset);
                        break;
                    case DuplicateType.Matrix:
                        ObjectDuplicator.DuplicateMultipleObjectMatrix(_selectedGameObjects, _countMatrix, _positionOffset);
                        break;
                }
            }

            // HelpBox
            if (!anyObjectSelected)
            {
                EditorGUILayout.HelpBox("GameObject is not selected.", MessageType.Info);
            }
            if (!countIsNotZero)
            {
                EditorGUILayout.HelpBox("Duplication count is zero.", MessageType.Info);
            }
            EditorGUILayout.Space(5f);

            // Selected Object List
            if (selectedCount > 0)
            {
                foreach (var go in _selectedGameObjects)
                {
                    // EditorGUILayout.LabelField(go.name, EditorStyles.objectFieldThumb);
                    EditorGUILayout.ObjectField(go, typeof(GameObject), allowSceneObjects: true);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
#endif