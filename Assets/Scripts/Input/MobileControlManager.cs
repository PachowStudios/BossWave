using UnityEngine;
using System.Collections;

public class MobileControlManager : MonoBehaviour
{
	public EasyJoystick moveJoystick;
	public string horizontalAxisName = "Horizontal";
	public string verticalAxisName = "Vertical";
	public EasyJoystick gunJoystick;
	public string gunAxisName = "GunRotation";
	public EasyButton pauseButton;
	public string pauseButtonName = "Pause";

	private bool tripleTapLastFrame = false;
	private bool tripleTap = false;

	void OnEnable()
	{
		if (CrossPlatformInputManager.VirtualAxisReference(horizontalAxisName) == null)
		{
			new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
			new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
			new CrossPlatformInputManager.VirtualAxis(gunAxisName);
			new CrossPlatformInputManager.VirtualButton(pauseButtonName);
		}
	}

	void Update()
	{
		CrossPlatformInputManager.SetAxis(horizontalAxisName, moveJoystick.JoystickAxis.x);
		CrossPlatformInputManager.SetAxis(verticalAxisName, moveJoystick.JoystickAxis.y);
		CrossPlatformInputManager.SetAxis(gunAxisName, Mathf.Atan2(gunJoystick.JoystickAxis.y, gunJoystick.JoystickAxis.x) * Mathf.Rad2Deg);

		tripleTap = Input.touchCount >= 3;

		if (pauseButton.buttonState == EasyButton.ButtonState.Down || (tripleTap && !tripleTapLastFrame))
		{
			CrossPlatformInputManager.SetButtonDown(pauseButtonName);
		}
		else
		{
			CrossPlatformInputManager.SetButtonUp(pauseButtonName);
		}

		tripleTapLastFrame = tripleTap;
	}
}
