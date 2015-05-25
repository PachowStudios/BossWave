using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using Rotorz.ReorderableList;

[CustomEditor(typeof(Gun))]
public class GunEditor : Editor
{
	#region Fields
	private AnimBool showSecondaryShot;
	private AnimBool showSecondaryGUI;
	private AnimBool showContinuousFire;
	private AnimBool showCanOverheat;
	private AnimBool showMuzzleFlash;
	private AnimBool showShakeOnFire;

	private SerializedProperty overheatGradient;
	private SerializedProperty muzzleFlashes;
	#endregion

	#region Internal Properties
	private Gun Target
	{ get { return (Gun)target; } }
	#endregion

	#region Editor Methods
	private void OnEnable()
	{
		showSecondaryShot = new AnimBool(Target.hasSecondaryShot);
		showSecondaryGUI = new AnimBool(Target.showSecondaryGUI);
		showContinuousFire = new AnimBool(Target.continuousFire);
		showCanOverheat = new AnimBool(Target.canOverheat);
		showMuzzleFlash = new AnimBool(Target.hasMuzzleFlash);
		showShakeOnFire = new AnimBool(Target.shakeOnFire);

		overheatGradient = serializedObject.FindProperty("overheatGradient");
		muzzleFlashes = serializedObject.FindProperty("muzzleFlashes");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		Target.gunName = EditorGUILayout.TextField("Gun Name", Target.gunName);
		Target.rarity = (Gun.RarityLevel)EditorGUILayout.EnumPopup("Rarity", Target.rarity);
		Target.projectile = (Projectile)EditorGUILayout.ObjectField("Projectile", Target.projectile, typeof(Projectile), false);

		if (Target.projectile == null)
			EditorGUILayout.HelpBox("No projectile selected!", MessageType.Error);

		Target.firePoint = (Transform)EditorGUILayout.ObjectField("Fire Point", Target.firePoint, typeof(Transform), true);

		if (Target.firePoint == null)
			EditorGUILayout.HelpBox("No fire point selected!", MessageType.Error);

		EditorGUILayout.Space();

		Target.continuousFire = EditorGUILayout.Toggle("Continuous Fire", Target.continuousFire);
		showContinuousFire.target = !Target.continuousFire;

		if (EditorGUILayout.BeginFadeGroup(showContinuousFire.faded))
		{
			EditorGUI.indentLevel++;

			Target.shootCooldown = EditorGUILayout.FloatField("Shot Cooldown", Target.shootCooldown);

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();
		EditorGUILayout.Space();

		Target.hasSecondaryShot = EditorGUILayout.Toggle("Secondary Shot", Target.hasSecondaryShot);
		showSecondaryShot.target = Target.hasSecondaryShot;

		if (EditorGUILayout.BeginFadeGroup(showSecondaryShot.faded))
		{
			EditorGUI.indentLevel++;

			Target.secondaryProjectile = (Projectile)EditorGUILayout.ObjectField("Projectile", Target.secondaryProjectile, typeof(Projectile), false);

			if (Target.secondaryProjectile == null)
				EditorGUILayout.HelpBox("No secondary projectile selected!", MessageType.Error);

			Target.secondaryCooldown = EditorGUILayout.FloatField("Cooldown", Target.secondaryCooldown);
			Target.showSecondaryGUI = EditorGUILayout.Toggle("Show GUI", Target.showSecondaryGUI);
			showSecondaryGUI.target = Target.showSecondaryGUI;

			if (EditorGUILayout.BeginFadeGroup(showSecondaryGUI.faded))
			{
				EditorGUI.indentLevel++;

				Target.secondaryIcon = (Sprite)EditorGUILayout.ObjectField("Icon", Target.secondaryIcon, typeof(Sprite), false);

				if (Target.secondaryIcon == null)
					EditorGUILayout.HelpBox("No icon selected!", MessageType.Error);

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFadeGroup();

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();
		EditorGUILayout.Space();

		Target.canOverheat = EditorGUILayout.Toggle("Overheat", Target.canOverheat);
		showCanOverheat.target = Target.canOverheat;

		if (EditorGUILayout.BeginFadeGroup(showCanOverheat.faded))
		{
			EditorGUI.indentLevel++;

			Target.overheatTime = EditorGUILayout.FloatField("Time", Target.overheatTime);
			Target.overheatDamage = EditorGUILayout.FloatField("Damage", Target.overheatDamage);
			Target.overheatThreshold = EditorGUILayout.Slider("Threshold", Target.overheatThreshold, 0f, 1f);

			EditorGUILayout.PropertyField(overheatGradient, new GUIContent("Gradient"));

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();
		EditorGUILayout.Space();

		Target.hasMuzzleFlash = EditorGUILayout.Toggle("Muzzle Flash", Target.hasMuzzleFlash);
		showMuzzleFlash.target = Target.hasMuzzleFlash;

		if (EditorGUILayout.BeginFadeGroup(showMuzzleFlash.faded))
		{
			EditorGUI.indentLevel++;

			Target.muzzleFlashRenderer = (SpriteRenderer)EditorGUILayout.ObjectField("Renderer", Target.muzzleFlashRenderer, typeof(SpriteRenderer), true);

			if (Target.muzzleFlashRenderer == null)
				EditorGUILayout.HelpBox("No renderer selected!", MessageType.Error);

			ReorderableListGUI.Title("Sprites");
			ReorderableListGUI.ListField(muzzleFlashes);

			if (Target.muzzleFlashes.Count <= 0 || Target.muzzleFlashes.HasNullElements())
				EditorGUILayout.HelpBox("No sprites selected!", MessageType.Error);

			Target.muzzleFlashDuration = EditorGUILayout.FloatField("Duration", Target.muzzleFlashDuration);

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();
		EditorGUILayout.Space();

		Target.shakeOnFire = EditorGUILayout.Toggle("Shake On Fire", Target.shakeOnFire);
		showShakeOnFire.target = Target.shakeOnFire;

		if (EditorGUILayout.BeginFadeGroup(showShakeOnFire.faded))
		{
			EditorGUI.indentLevel++;

			Target.shakeDuration = EditorGUILayout.FloatField("Duration", Target.shakeDuration);
			Target.shakeIntensity = EditorGUILayout.Vector3Field("Intensity", Target.shakeIntensity);

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.EndFadeGroup();

		if (GUI.changed)
			EditorUtility.SetDirty(Target);

		serializedObject.ApplyModifiedProperties();
		Repaint();
	}
	#endregion
}
