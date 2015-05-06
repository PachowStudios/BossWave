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
		if (!Target.enabled)
			return;

		Vector3[] currentNodes = Target.Nodes;

		if (currentNodes.Length > 0)
		{
			Undo.RecordObject(Target, "Adjust Vector Path");

			Handles.Label(currentNodes[0], "'" + Target.pathName + " Begin", style);
			Handles.Label(currentNodes[currentNodes.Length - 1], "'" + Target.pathName + " End", style);

			for (int i = 0; i < currentNodes.Length; i++)
				Target.nodes[i] = Handles.PositionHandle(currentNodes[i], Quaternion.identity) - (Target.isLocal ? Target.transform.position : Vector3.zero);
		}
	}
	#endregion
}
