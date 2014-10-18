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

	private CrossPlatformInputManager.VirtualAxis horizontalAxis;
	private CrossPlatformInputManager.VirtualAxis verticalAxis;
	private CrossPlatformInputManager.VirtualAxis gunAxis;

	private bool tripleTapLastFrame = false;
	private bool tripleTap = false;

	void OnEnable()
	{
		horizontalAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
		verticalAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
		gunAxis = new CrossPlatformInputManager.VirtualAxis(gunAxisName);
		new CrossPlatformInputManager.VirtualButton(pauseButtonName);
	}

	void Update()
	{
		horizontalAxis.Update(moveJoystick.JoystickAxis.x);
		verticalAxis.Update(moveJoystick.JoystickAxis.y);
		gunAxis.Update(Mathf.Atan2(gunJoystick.JoystickAxis.y, gunJoystick.JoystickAxis.x) * Mathf.Rad2Deg);

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
