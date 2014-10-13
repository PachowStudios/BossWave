using UnityEngine;
using System.Collections;

public class MobileControlManager : MonoBehaviour
{
	public EasyJoystick moveJoystick;
	public string horizontalAxisName = "Horizontal";
	public string verticalAxisName = "Vertical";
	public EasyJoystick gunJoystick;
	public string gunAxisName = "GunRotation";

	private CrossPlatformInputManager.VirtualAxis horizontalAxis;
	private CrossPlatformInputManager.VirtualAxis verticalAxis;
	private CrossPlatformInputManager.VirtualAxis gunAxis;

	void OnEnable()
	{
		horizontalAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
		verticalAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
		gunAxis = new CrossPlatformInputManager.VirtualAxis(gunAxisName);
	}

	void Update()
	{
		horizontalAxis.Update(moveJoystick.JoystickAxis.x);
		verticalAxis.Update(moveJoystick.JoystickAxis.y);
		gunAxis.Update(Mathf.Atan2(gunJoystick.JoystickAxis.y, gunJoystick.JoystickAxis.x) * Mathf.Rad2Deg);
	}
}
