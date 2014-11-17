using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Gun))]
public class GunEditor : Editor
{
	private AnimBool showContinuousFire;
	private AnimBool showCanOverheat;
	private SerializedObject serializedTarget;
	private SerializedProperty overheatGradient;

	private Gun Target
	{
		get { return (Gun)target; }
	}

	void OnEnable()
	{
		showContinuousFire = new AnimBool(Target.continuousFire);
		showCanOverheat = new AnimBool(Target.canOverheat);

		serializedTarget = new SerializedObject(Target);
		overheatGradient = serializedTarget.FindProperty("overheatGradient");
	}

	public override void OnInspectorGUI()
	{
		Target.gunName = EditorGUILayout.TextField("Gun Name", Target.gunName);
		Target.rarity = (Gun.RarityLevel)EditorGUILayout.EnumPopup("Rarity", Target.rarity);
		Target.projectile = (Projectile)EditorGUILayout.ObjectField("Projectile", Target.projectile, typeof(Projectile), false);

		Target.continuousFire = EditorGUILayout.Toggle("Continuous Fire", Target.continuousFire);
		showContinuousFire.target = !Target.continuousFire;

		if (EditorGUILayout.BeginFadeGroup(showContinuousFire.faded))
		{
			EditorGUI.indentLevel++;

			Target.shootCooldown = EditorGUILayout.FloatField("Shot Cooldown", Target.shootCooldown);

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();

		Target.canOverheat = EditorGUILayout.Toggle("Overheat", Target.canOverheat);
		showCanOverheat.target = Target.canOverheat;

		if (EditorGUILayout.BeginFadeGroup(showCanOverheat.faded))
		{
			EditorGUI.indentLevel++;

			Target.overheatTime = EditorGUILayout.FloatField("Time", Target.overheatTime);
			Target.overheatDamage = EditorGUILayout.FloatField("Damage", Target.overheatDamage);
			Target.overheatThreshold = EditorGUILayout.Slider("Threshold", Target.overheatThreshold, 0f, 1f);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(overheatGradient, new GUIContent("Gradient"));
			if (EditorGUI.EndChangeCheck())
			{
				serializedTarget.ApplyModifiedProperties();
			}

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(Target);
		}

		Repaint();
	}
}
