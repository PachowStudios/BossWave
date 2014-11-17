﻿using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Powerup), true)]
public class PowerupEditor : Editor
{
	private AnimBool showAutoDestroy;
	private SerializedObject serializedTarget;

	private Powerup Target
	{
		get { return (Powerup)target; }
	}

	void OnEnable()
	{
		showAutoDestroy = new AnimBool(Target.autoDestroy);
		serializedTarget = new SerializedObject(Target);
	}

	public override void OnInspectorGUI()
	{
		serializedTarget.Update();

		Target.autoDestroy = EditorGUILayout.Toggle("Auto Destroy", Target.autoDestroy);
		showAutoDestroy.target = Target.autoDestroy;

		if (EditorGUILayout.BeginFadeGroup(showAutoDestroy.faded))
		{
			EditorGUI.indentLevel++;

			EditorGUILayout.MinMaxSlider(new GUIContent("Lifetime"), ref Target.minLifetime, ref Target.maxLifetime, 0f, 30f);
			EditorGUILayout.LabelField(" ", "Min: " + Target.minLifetime + "    Max: " + Target.maxLifetime);

			EditorGUI.indentLevel--;
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(Target);
		}

		DrawPropertiesExcluding(serializedTarget, new string[] { "m_Script", "autoDestroy", "minLifetime", "maxLifetime" } );
		Repaint();
	}
}
