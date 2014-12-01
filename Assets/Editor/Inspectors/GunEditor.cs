using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Gun))]
public class GunEditor : Editor
{
	private AnimBool showSecondaryShot;
	private AnimBool showSecondaryGUI;
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
		showSecondaryShot = new AnimBool(Target.secondaryShot);
		showSecondaryGUI = new AnimBool(Target.showSecondaryGUI);
		showContinuousFire = new AnimBool(Target.continuousFire);
		showCanOverheat = new AnimBool(Target.canOverheat);

		serializedTarget = new SerializedObject(Target);
		overheatGradient = serializedTarget.FindProperty("overheatGradient");
	}

	public override void OnInspectorGUI()
	{
		serializedTarget.Update();

		Target.gunName = EditorGUILayout.TextField("Gun Name", Target.gunName);
		Target.rarity = (Gun.RarityLevel)EditorGUILayout.EnumPopup("Rarity", Target.rarity);
		Target.projectile = (Projectile)EditorGUILayout.ObjectField("Projectile", Target.projectile, typeof(Projectile), false);

		if (Target.projectile == null)
		{
			EditorGUILayout.HelpBox("No projectile selected!", MessageType.Error);
		}

		EditorGUILayout.Space();

		Target.continuousFire = EditorGUILayout.Toggle("Continuous Fire", Target.continuousFire);
		showContinuousFire.target = !Target.continuousFire;

		if (EditorGUILayout.BeginFadeGroup(showContinuousFire.faded))
		{
			EditorGUI.indentLevel++;

			Target.shootCooldown = EditorGUILayout.FloatField("Shot Cooldown", Target.shootCooldown);
			EditorGUILayout.Space();

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();

		Target.secondaryShot = EditorGUILayout.Toggle("Secondary Shot", Target.secondaryShot);
		showSecondaryShot.target = Target.secondaryShot;

		if (EditorGUILayout.BeginFadeGroup(showSecondaryShot.faded))
		{
			EditorGUI.indentLevel++;

			Target.secondaryProjectile = (Projectile)EditorGUILayout.ObjectField("Projectile", Target.secondaryProjectile, typeof(Projectile), false);

			if (Target.secondaryProjectile == null)
			{
				EditorGUILayout.HelpBox("No secondary projectile selected!", MessageType.Error);
			}

			Target.secondaryCooldown = EditorGUILayout.FloatField("Cooldown", Target.secondaryCooldown);
			Target.showSecondaryGUI = EditorGUILayout.Toggle("Show GUI", Target.showSecondaryGUI);
			showSecondaryGUI.target = Target.showSecondaryGUI;

			if (EditorGUILayout.BeginFadeGroup(showSecondaryGUI.faded))
			{
				EditorGUI.indentLevel++;

				Target.secondaryIcon = (Sprite)EditorGUILayout.ObjectField("Icon", Target.secondaryIcon, typeof(Sprite), false);

				if (Target.secondaryIcon == null)
				{
					EditorGUILayout.HelpBox("No icon selected!", MessageType.Error);
				}

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.Space();

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
