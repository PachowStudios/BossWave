using UnityEngine;
using System.Collections;

public abstract class Boss : Enemy
{
	#region Fields
	public string introName;
	public string introDescription;
	public Sprite introSprite;
	#endregion

	#region Public Methods
	public abstract void Spawn();
	public abstract void End();
	#endregion
}
