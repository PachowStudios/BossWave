using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public class MobileInputToggle
{
	private static BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[]
	{
		BuildTargetGroup.Standalone,
		BuildTargetGroup.WebPlayer,
		BuildTargetGroup.Android,
		BuildTargetGroup.iPhone,
		BuildTargetGroup.WP8,
		BuildTargetGroup.BlackBerry,
	};

	private static BuildTargetGroup[] mobileBuildTargetGroups = new BuildTargetGroup[]
	{
		BuildTargetGroup.Android,
		BuildTargetGroup.iPhone,
		BuildTargetGroup.WP8,
		BuildTargetGroup.BlackBerry,
	};

	static MobileInputToggle()
	{
		List<string> defines = GetDefinesList(buildTargetGroups[0]);

		if (!defines.Contains("CROSS_PLATFORM_INPUT"))
		{
			SetEnabled("CROSS_PLATFORM_INPUT", true, false);
			SetEnabled("MOBILE_INPUT", true, true);
		}
	}

	[MenuItem("Mobile Input/Enable")]
	private static void Enable()
	{
		SetEnabled("MOBILE_INPUT", true, true);

		switch (EditorUserBuildSettings.activeBuildTarget)
		{
			case BuildTarget.Android:
			case BuildTarget.iPhone:
			case BuildTarget.WP8Player:
			case BuildTarget.BlackBerry:
				EditorUtility.DisplayDialog("Mobile Input",
											"Mobile input rigs have been enabled. Use the Unity Remote App to control them.",
											"OK");
				break;
			default:
				EditorUtility.DisplayDialog("Mobile Input",
											"Please switch to a mobile build target to use mobile input rigs.",
											"OK");
				break;
		}
	}

	[MenuItem("Mobile Input/Enable", true)]
	private static bool EnableValidate()
	{
		List<string> defines = GetDefinesList(mobileBuildTargetGroups[0]);
		
		return !defines.Contains("MOBILE_INPUT");
	}

	[MenuItem("Mobile Input/Disable")]
	private static void Disable()
	{
		SetEnabled("MOBILE_INPUT", false, true);

		switch (EditorUserBuildSettings.activeBuildTarget)
		{
			case BuildTarget.Android:
			case BuildTarget.iPhone:
			case BuildTarget.WP8Player:
			case BuildTarget.BlackBerry:
				EditorUtility.DisplayDialog("Mobile Input",
											"Mobile input rigs have been disabled",
											"OK");
				break;
		}
	}

	[MenuItem("Mobile Input/Disable", true)]
	private static bool DisableValidate()
	{
		List<string> defines = GetDefinesList(mobileBuildTargetGroups[0]);

		return defines.Contains("MOBILE_INPUT");
	}

	private static void SetEnabled(string defineName, bool enable, bool mobile)
	{
		foreach (BuildTargetGroup group in mobile ? mobileBuildTargetGroups : buildTargetGroups)
		{
			List<string> defines = GetDefinesList(group);

			if (enable)
			{
				if (defines.Contains(defineName))
				{
					return;
				}

				defines.Add(defineName);
			}
			else
			{
				if (!defines.Contains(defineName))
				{
					return;
				}
				while (defines.Contains(defineName))
				{
					defines.Remove(defineName);
				}
			}

			string definesString = string.Join(";", defines.ToArray());
			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
		}
	}

	private static List<string> GetDefinesList(BuildTargetGroup group)
	{
		return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
	}
}
