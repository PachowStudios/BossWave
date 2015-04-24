using UnityEngine;
using UnityEditor;
using System.Collections;
using Rotorz.ReorderableList;

[CustomEditor(typeof(VectorPath))]
public class VectorPathEditor : Editor
{
	#region Fields
	private SerializedProperty nodes;
	private GUIStyle style = new GUIStyle();
	#endregion

	#region Internal Properties
	private VectorPath Target
	{ get { return (VectorPath)target; } }
	#endregion

	#region Editor Methods
	private void OnEnable()
	{
		nodes = serializedObject.FindProperty("nodes");

		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "nodes" } );

		ReorderableListGUI.Title("Nodes");
		ReorderableListGUI.ListField(nodes);

		if (GUI.changed)
			EditorUtility.SetDirty(Target);

		serializedObject.ApplyModifiedProperties();
		Repaint();
	}

	private void OnSceneGUI()
	{
		if (Target.enabled && Target.nodes.Length> 0)
		{
			Undo.RecordObject(Target, "Adjust Vector Path");

			Handles.Label(Target.nodes[0], "'" + Target.pathName + " Begin", style);
			Handles.Label(Target.nodes[Target.nodes.Length - 1], "'" + Target.pathName + " End", style);

			for (int i = 0; i < Target.nodes.Length; i++)
			{
				Target.nodes[i] = Handles.PositionHandle(Target.nodes[i], Quaternion.identity);
			}
		}
	}
	#endregion
}
