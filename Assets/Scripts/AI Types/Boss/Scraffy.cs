using UnityEngine;
using System.Collections;
using DG.Tweening;

public sealed class Scraffy : Boss
{
	#region Fields
	public Sprite warningPopup;
	public string spawnPathName;
	public float spawnPathTime;
	#endregion

	#region Internal Helper Methods
	protected override void CheckDeath(bool showDrops = true)
	{
		
	}
	#endregion

	#region Public Methods
	public override void Spawn()
	{
		if (spawned)
			return;

		Sequence spawnSequence = DOTween.Sequence();

		spawnSequence
			.AppendCallback(() =>
				{
					CameraShake.Instance.Shake(1f, new Vector3(0f, 2f, 0f));
					PopupMessage.Instance.CreatePopup(PlayerControl.Instance.PopupMessagePoint, "", warningPopup, true);
					PlayerControl.Instance.DisableInput();
				})
			.AppendInterval(2f)
			.AppendCallback(() =>
				{
					Cutscene.Instance.StartCutscene();
					CameraFollow.Instance.FollowObject(transform, false, 0.5f, true);
					BossIntro.Instance.Show(introName, introDescription, introSprite);
					PlayerControl.Instance.GoToPoint(LevelManager.Instance.BossWaveWaitPoint, false, false);
				})
			.Append(transform.DOPath(VectorPath.GetPath(spawnPathName), spawnPathTime, VectorPath.GetPathType(spawnPathName), PathMode.Sidescroller2D)
				.SetEase(Ease.InOutSine));
	}

	public override void End()
	{

	}
	#endregion
}
