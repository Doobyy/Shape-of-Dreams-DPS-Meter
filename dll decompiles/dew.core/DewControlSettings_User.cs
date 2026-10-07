using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class DewControlSettings_User : ICloneable, IValidatableSettings
{
	public enum PresetType
	{
		MOBA,
		WASD
	}

	public AimAssistType targetAssist = AimAssistType.High;

	public bool attackByIssuingMoveOnEnemy;

	public bool autoTargetSelfIfPossible = true;

	public bool clickToCastInsteadOfHoldToCast;

	public int mouseSensitivity = -1;

	public DashDirection dashDirectionWhenDirectionalMovement;

	public DewBinding confirmCast = DewBinding.KeyboardAndMouseOnly(MouseButton.Left);

	public DewBinding move = DewBinding.KeyboardAndMouseOnly(MouseButton.Right);

	public DewBinding attackMoveNormal = DewBinding.KeyboardAndMouseOnly((object)(Key)15);

	public DewBinding attackMoveImmediately = DewBinding.KeyboardAndMouseOnly(MouseButton.Left);

	public DewBinding attackMoveOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding attackInPlace = DewBinding.PCAndGamepad(GamepadButtonEx.Square);

	public DewBinding back = DewBinding.PCAndGamepad(GamepadButtonEx.B, (object)(Key)60);

	public DewBinding scoreboard = DewBinding.PCAndGamepad(GamepadButtonEx.DpadDown, (object)(Key)3);

	public DewBinding worldMap = DewBinding.PCAndGamepad(GamepadButtonEx.DpadRight, (object)(Key)27);

	public bool turnOffAimAssistMeleeDirectionalAttack;

	public bool attackMoveUseDistanceFromDestination = true;

	public DewBinding stop = DewBinding.PCAndGamepad((object)(Key)33);

	public bool enableDirMoveKeys;

	public DewBinding moveUp = DewBinding.KeyboardOnly((object)(Key)37);

	public DewBinding moveLeft = DewBinding.KeyboardOnly((object)(Key)15);

	public DewBinding moveDown = DewBinding.KeyboardOnly((object)(Key)33);

	public DewBinding moveRight = DewBinding.KeyboardOnly((object)(Key)18);

	public DewBinding skillQ = DewBinding.PCAndGamepad(GamepadButtonEx.LeftTrigger, (object)(Key)31);

	public DewBinding skillW = DewBinding.PCAndGamepad(GamepadButtonEx.LeftShoulder, (object)(Key)37);

	public DewBinding skillE = DewBinding.PCAndGamepad(GamepadButtonEx.RightShoulder, (object)(Key)19);

	public DewBinding skillR = DewBinding.PCAndGamepad(GamepadButtonEx.RightTrigger, (object)(Key)32);

	public DewBinding skillMovement = DewBinding.PCAndGamepad(GamepadButtonEx.A, (object)(Key)1);

	public DewBinding skillQNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillWNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillENormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillRNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillMovementNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillQImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillWImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillEImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillRImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillMovementImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillQOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillWOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillEOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillROnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillMovementOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillQSelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		(Key)53,
		(Key)31
	});

	public DewBinding skillWSelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		(Key)53,
		(Key)37
	});

	public DewBinding skillESelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		(Key)53,
		(Key)19
	});

	public DewBinding skillRSelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		(Key)53,
		(Key)32
	});

	public DewBinding skillQEdit = DewBinding.KeyboardAndMouseOnly((object)(Key)41);

	public DewBinding skillWEdit = DewBinding.KeyboardAndMouseOnly((object)(Key)42);

	public DewBinding skillEEdit = DewBinding.KeyboardAndMouseOnly((object)(Key)43);

	public DewBinding skillREdit = DewBinding.KeyboardAndMouseOnly((object)(Key)44);

	public DewBinding skillIdentityEdit = DewBinding.KeyboardAndMouseOnly((object)(Key)45);

	public CastConfirmType defaultHeroAbilityCastType = CastConfirmType.Immediately;

	public CastConfirmType movementHeroAbilityCastType = CastConfirmType.Immediately;

	public DewBinding interact = DewBinding.PCAndGamepad(GamepadButtonEx.North, (object)(Key)20);

	public DewBinding interactAlt = DewBinding.PCAndGamepad(GamepadButtonEx.B, (object)(Key)21);

	public DewBinding editSkillHold = DewBinding.KeyboardAndMouseOnly((object)(Key)55);

	public DewBinding editSkillToggle = DewBinding.PCAndGamepad(GamepadButtonEx.DpadUp);

	public DewBinding showDetails = DewBinding.PCAndGamepad(GamepadButtonEx.Select, (object)(Key)53);

	public DewBinding zoomOut = DewBinding.PCAndGamepad(MouseButton.ScrollDown);

	public DewBinding zoomIn = DewBinding.PCAndGamepad(MouseButton.ScrollUp);

	public DewBinding chat = DewBinding.KeyboardAndMouseOnly((object)(Key)2);

	public DewBinding ping = DewBinding.KeyboardAndMouseOnly(MouseButton.Middle);

	public DewBinding emote = DewBinding.KeyboardAndMouseOnly((object)(Key)34);

	public DewBinding travelVote = DewBinding.PCAndGamepad(GamepadButtonEx.Select, (object)(Key)3);

	public DewBinding travelVoteCancel = DewBinding.PCAndGamepad(GamepadButtonEx.Start, (object)(Key)60);

	public DewBinding spectatorNextTarget = DewBinding.PCAndGamepad(GamepadButtonEx.A, (object)(Key)1, MouseButton.Left);

	public DewBinding skip = DewBinding.PCAndGamepad(GamepadButtonEx.A, (object)(Key)1);

	public DewBinding nextCategory = DewBinding.GamepadOnly(GamepadButtonEx.RightShoulder);

	public DewBinding prevCategory = DewBinding.GamepadOnly(GamepadButtonEx.LeftShoulder);

	public DewBinding gamepadTextInputLeftShift = DewBinding.GamepadOnly(GamepadButtonEx.LeftShoulder);

	public DewBinding gamepadTextInputRightShift = DewBinding.GamepadOnly(GamepadButtonEx.RightShoulder);

	public DewBinding gamepadTextInputBackspace = DewBinding.GamepadOnly(GamepadButtonEx.Square);

	public DewBinding menu = DewBinding.GamepadOnly(GamepadButtonEx.Start);

	public DewBinding selectInfoBar = DewBinding.GamepadOnly(GamepadButtonEx.DpadLeft);

	public DewBinding confirm = DewBinding.PCAndGamepad(GamepadButtonEx.A, MouseButton.Left);

	public DewBinding gamepadApply = DewBinding.GamepadOnly(GamepadButtonEx.Start);

	public DewBinding gamepadSecondary = DewBinding.GamepadOnly(GamepadButtonEx.North);

	public DewBinding gamepadTertiary = DewBinding.GamepadOnly(GamepadButtonEx.Square);

	public bool lockCursorInGame = true;

	public AimAssistType attackTargetAssistGamepad = AimAssistType.High;

	public JoystickClickAction leftJoystickClickAction = JoystickClickAction.Dodge;

	public JoystickClickAction rightJoystickClickAction = JoystickClickAction.PingAndEmotes;

	public bool gamepadAttackByAiming = true;

	public DewBinding GetSkillBinding(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => skillQ, 
			HeroSkillLocation.W => skillW, 
			HeroSkillLocation.E => skillE, 
			HeroSkillLocation.R => skillR, 
			HeroSkillLocation.Identity => DewBinding.MockBinding, 
			HeroSkillLocation.Movement => skillMovement, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}

	public DewBinding GetSkillEditBinding(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => skillQEdit, 
			HeroSkillLocation.W => skillWEdit, 
			HeroSkillLocation.E => skillEEdit, 
			HeroSkillLocation.R => skillREdit, 
			HeroSkillLocation.Identity => skillIdentityEdit, 
			HeroSkillLocation.Movement => DewBinding.MockBinding, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}

	public object Clone()
	{
		object obj = MemberwiseClone();
		FieldInfo[] fields = typeof(DewControlSettings_User).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!(fieldInfo.FieldType != typeof(DewBinding)))
			{
				DewBinding dewBinding = (DewBinding)fieldInfo.GetValue(obj);
				fieldInfo.SetValue(obj, dewBinding.Clone());
			}
		}
		return obj;
	}

	public string GetSettingsValueText(string keys, out BindingType type)
	{
		string[] array = keys.Split(",", StringSplitOptions.None);
		foreach (string name in array)
		{
			FieldInfo field = typeof(DewControlSettings_User).GetField(name);
			if (!(field == null) && field.GetValue(DewSave.profileMain.controls) is DewBinding dewBinding && dewBinding.HasAssignedForCurrentMode())
			{
				return DewInput.GetReadableTextForCurrentMode(dewBinding, out type);
			}
		}
		type = BindingType.None;
		return DewLocalization.GetUIValue("Key_None");
	}

	public object GetSettingsValue(string key)
	{
		FieldInfo field = typeof(DewControlSettings_User).GetField(key);
		if (field == null)
		{
			return null;
		}
		return field.GetValue(DewSave.profileMain.controls);
	}

	public string GetSettingsValueText(string keys)
	{
		BindingType type;
		return GetSettingsValueText(keys, out type);
	}

	public void Validate()
	{
		if (mouseSensitivity < 0)
		{
			mouseSensitivity = InputManager.GetMouseSensitivity();
		}
		mouseSensitivity = Mathf.Clamp(mouseSensitivity, 1, 20);
		FieldInfo[] fields = typeof(DewControlSettings_User).GetFields();
		DewControlSettings_User obj = new DewControlSettings_User();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo.FieldType != typeof(DewBinding))
			{
				continue;
			}
			DewBinding dewBinding = (DewBinding)fieldInfo.GetValue(obj);
			if (!(fieldInfo.GetValue(this) is DewBinding dewBinding2))
			{
				fieldInfo.SetValue(this, dewBinding);
				continue;
			}
			dewBinding2.canAssignKeyboard = dewBinding.canAssignKeyboard;
			dewBinding2.canAssignGamepad = dewBinding.canAssignGamepad;
			dewBinding2.canAssignMouse = dewBinding.canAssignMouse;
			dewBinding2.gamepad = dewBinding.gamepad;
			dewBinding2.gamepadBinds = new List<GamepadButtonEx>();
			foreach (GamepadButtonEx gamepadBind in dewBinding.gamepadBinds)
			{
				dewBinding2.gamepadBinds.Add(gamepadBind);
			}
		}
	}

	public void ApplyPreset(PresetType type)
	{
		DewControlSettings_User dewControlSettings_User = new DewControlSettings_User();
		move = dewControlSettings_User.move;
		skillQ = dewControlSettings_User.skillQ;
		skillW = dewControlSettings_User.skillW;
		skillE = dewControlSettings_User.skillE;
		skillR = dewControlSettings_User.skillR;
		skillQNormal = dewControlSettings_User.skillQNormal;
		skillWNormal = dewControlSettings_User.skillWNormal;
		skillENormal = dewControlSettings_User.skillENormal;
		skillRNormal = dewControlSettings_User.skillRNormal;
		skillQSelf = dewControlSettings_User.skillQSelf;
		skillWSelf = dewControlSettings_User.skillWSelf;
		skillESelf = dewControlSettings_User.skillESelf;
		skillRSelf = dewControlSettings_User.skillRSelf;
		attackMoveImmediately = dewControlSettings_User.attackMoveImmediately;
		attackMoveNormal = dewControlSettings_User.attackMoveNormal;
		attackInPlace = dewControlSettings_User.attackInPlace;
		moveUp = dewControlSettings_User.moveUp;
		moveLeft = dewControlSettings_User.moveLeft;
		moveDown = dewControlSettings_User.moveDown;
		moveRight = dewControlSettings_User.moveRight;
		stop = dewControlSettings_User.stop;
		attackByIssuingMoveOnEnemy = false;
		clickToCastInsteadOfHoldToCast = false;
		switch (type)
		{
		case PresetType.MOBA:
			enableDirMoveKeys = false;
			break;
		case PresetType.WASD:
			enableDirMoveKeys = true;
			move.pcBinds.Clear();
			stop.pcBinds.Clear();
			attackMoveImmediately.pcBinds.Clear();
			attackMoveNormal.pcBinds.Clear();
			attackInPlace.pcBinds = new List<PCBind> { MouseButton.Left };
			skillQ.pcBinds = new List<PCBind> { MouseButton.Right };
			skillW.pcBinds = new List<PCBind> { (Key)31 };
			skillE.pcBinds = new List<PCBind> { (Key)19 };
			skillR.pcBinds = new List<PCBind> { (Key)32 };
			skillQNormal.pcBinds = new List<PCBind>
			{
				new PCBind(MouseButton.Right, (Key)51)
			};
			skillWNormal.pcBinds = new List<PCBind>
			{
				new PCBind((Key)31, (Key)51)
			};
			skillENormal.pcBinds = new List<PCBind>
			{
				new PCBind((Key)19, (Key)51)
			};
			skillRNormal.pcBinds = new List<PCBind>
			{
				new PCBind((Key)32, (Key)51)
			};
			skillQSelf.pcBinds = new List<PCBind>
			{
				new PCBind(MouseButton.Right, (Key)53)
			};
			skillWSelf.pcBinds = new List<PCBind>
			{
				new PCBind((Key)31, (Key)53)
			};
			skillESelf.pcBinds = new List<PCBind>
			{
				new PCBind((Key)19, (Key)53)
			};
			skillRSelf.pcBinds = new List<PCBind>
			{
				new PCBind((Key)32, (Key)53)
			};
			clickToCastInsteadOfHoldToCast = true;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}
}
