using System;
using UnityEngine;

public class DewControlSettings_Platform : ICloneable, IInitializableSettings, IValidatableSettings
{
	public float joystick0Dead = 0.1f;

	public float joystick1Dead = 0.1f;

	public float joystick0Max = 0.95f;

	public float joystick1Max = 0.95f;

	public ControllerButtonType controllerButtonType = ControllerButtonType.Xbox;

	public void Initialize()
	{
	}

	public void Validate()
	{
		joystick0Dead = Mathf.Clamp(joystick0Dead, 0.01f, 0.4f);
		joystick1Dead = Mathf.Clamp(joystick1Dead, 0.01f, 0.4f);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
