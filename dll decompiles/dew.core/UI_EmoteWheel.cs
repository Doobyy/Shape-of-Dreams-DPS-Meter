using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI_EmoteWheel : SingletonBehaviour<UI_EmoteWheel>, ISettingsChangedCallback
{
	public bool listenForGamepadInput;

	public UI_EmoteWheel_Item[] wheelItems;

	public DewInputTrigger it_emote;

	public DewInputTrigger it_emoteConfirm;

	public DewInputTrigger it_emoteCancel;

	public UILineRenderer lineRenderer;

	public GameObject fxFocusChanged;

	public GameObject fxShow;

	public GameObject fxHide;

	private CanvasGroup _cg;

	private int _previousFocused = -1;

	private bool _isUsingRightStick;

	private float _lastCloseUnscaledTime;

	public bool isShown => _cg.alpha > 0.05f;

	public float GetElapsedTimeAfterLastClose()
	{
		return Time.unscaledTime - _lastCloseUnscaledTime;
	}

	private void Start()
	{
		_cg = GetComponent<CanvasGroup>();
		it_emote = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profileMain.controls.emote,
			isValidCheck = CheckValid,
			checkGameAreaForMouse = true,
			priority = 0
		};
		it_emoteConfirm = new DewInputTrigger
		{
			owner = this,
			binding = () => DewBinding.KeyboardAndMouseOnly(MouseButton.Left),
			isValidCheck = () => isShown,
			checkGameAreaForMouse = false,
			priority = -10
		};
		it_emoteCancel = new DewInputTrigger
		{
			owner = this,
			binding = () => DewBinding.KeyboardAndMouseOnly(MouseButton.Right),
			isValidCheck = () => isShown,
			checkGameAreaForMouse = false,
			priority = -10
		};
		DewInput.onCurrentModeChanged += new Action<InputMode, InputMode>(OnCurrentModeChanged);
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 5000, () =>
		{
			if (!isShown)
			{
				return false;
			}
			Hide();
			return true;
		});
		SetupItems();
		_cg.alpha = 0f;
		Hide();
	}

	private void OnDestroy()
	{
		DewInput.onCurrentModeChanged -= new Action<InputMode, InputMode>(OnCurrentModeChanged);
		if ((bool)ManagerBase<GlobalUIManager>.instance)
		{
			ManagerBase<GlobalUIManager>.instance.disableGamepadUiInputs = false;
		}
	}

	private void OnCurrentModeChanged(InputMode arg1, InputMode arg2)
	{
		if (isShown)
		{
			Hide();
		}
	}

	public bool CheckValid()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance != null)
		{
			if (ManagerBase<ControlManager>.instance.shouldProcessCharacterInputAllowKnockedOut)
			{
				return ManagerBase<GlobalUIManager>.instance.focused == null;
			}
			return false;
		}
		if (LobbyUIManager.softInstance != null)
		{
			if (LobbyUIManager.instance.IsState("Lobby"))
			{
				return !ControlManager.IsInputFieldFocused();
			}
			return false;
		}
		return false;
	}

	private void Update()
	{
		TryShowWheel();
		if (isShown)
		{
			UpdateWheelVisuals();
			TryCloseWheel();
		}
	}

	private void TryShowWheel()
	{
		if (it_emote.down)
		{
			Show(InputMode.KeyboardAndMouse, useRightStick: false);
		}
		else if (listenForGamepadInput && DewInput.currentMode == InputMode.Gamepad)
		{
			if (DewSave.profileMain.controls.leftJoystickClickAction == JoystickClickAction.PingAndEmotes && DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.LeftStick))
			{
				Show(InputMode.Gamepad, useRightStick: false);
			}
			if (DewSave.profileMain.controls.rightJoystickClickAction == JoystickClickAction.PingAndEmotes && DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.RightStick))
			{
				Show(InputMode.Gamepad, useRightStick: true);
			}
		}
	}

	private void UpdateWheelVisuals()
	{
		Vector2 wheelCursorPosition = GetWheelCursorPosition();
		lineRenderer.Points = new Vector2[2]
		{
			Vector2.zero,
			transform.InverseTransformPoint(wheelCursorPosition)
		};
		int closestEmoteIndex = GetClosestEmoteIndex(wheelCursorPosition);
		if (_previousFocused != closestEmoteIndex)
		{
			_previousFocused = closestEmoteIndex;
			DewEffect.Play(fxFocusChanged);
			for (int i = 0; i < wheelItems.Length; i++)
			{
				wheelItems[i].SetHighlight(i == closestEmoteIndex);
			}
		}
	}

	private void TryCloseWheel()
	{
		if (it_emote.up || it_emoteConfirm.down)
		{
			ConfirmAndHide();
			return;
		}
		if (it_emoteCancel.down)
		{
			Hide();
			return;
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			if (DewInput.GetButtonUp((GamepadButtonEx?)(_isUsingRightStick ? GamepadButtonEx.RightStick : GamepadButtonEx.LeftStick)))
			{
				ConfirmAndHide();
				return;
			}
			if (DewInput.GetButtonDown((GamepadButtonEx?)GamepadButtonEx.B))
			{
				Hide();
				return;
			}
		}
		if ((bool)ManagerBase<ControlManager>.instance && GetElapsedTimeAfterLastClose() > 0.1f)
		{
			ManagerBase<ControlManager>.instance.CancelGamepadEmote();
		}
	}

	public void Show(InputMode mode, bool useRightStick)
	{
		StopAllCoroutines();
		ManagerBase<GlobalUIManager>.instance.disableGamepadUiInputs = true;
		_isUsingRightStick = useRightStick;
		DewEffect.Play(fxShow);
		SetupItems();
		ShortcutExtensions.DOKill((Component)(object)_cg, true);
		DOTweenModuleUI.DOFade(_cg, 1f, 0.2f);
		ShortcutExtensions.DOKill((Component)transform, true);
		transform.localScale = Vector3.one * 0.9f;
		ShortcutExtensions.DOScale(transform, 1f, 0.2f);
		_cg.blocksRaycasts = true;
		if (mode == InputMode.KeyboardAndMouse)
		{
			transform.position = Input.mousePosition;
		}
		else
		{
			transform.position = new Vector2((float)Screen.width / 2f, (float)Screen.height / 7f * 4f);
		}
		((Behaviour)(object)lineRenderer).enabled = true;
		lineRenderer.Points = new Vector2[2]
		{
			Vector2.zero,
			Vector2.zero
		};
		_previousFocused = -1;
	}

	public void Hide()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		DewEffect.Play(fxHide);
		ShortcutExtensions.DOKill((Component)(object)_cg, false);
		_cg.alpha = 0f;
		_cg.blocksRaycasts = false;
		((Behaviour)(object)lineRenderer).enabled = false;
		_lastCloseUnscaledTime = Time.unscaledTime;
		static IEnumerator Routine()
		{
			yield return new WaitForSecondsRealtime(0.25f);
			ManagerBase<GlobalUIManager>.instance.disableGamepadUiInputs = false;
		}
	}

	public void ConfirmAndHide()
	{
		int closestEmoteIndex = GetClosestEmoteIndex(GetWheelCursorPosition());
		if (!string.IsNullOrEmpty(DewSave.profileMain.equippedEmotes[closestEmoteIndex]))
		{
			NetworkedManagerBase<ChatManager>.instance.CmdSendEmote(DewSave.profileMain.equippedEmotes[closestEmoteIndex]);
		}
		Hide();
	}

	private Vector2 GetWheelCursorPosition()
	{
		if (DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			return Input.mousePosition;
		}
		float num = ((RectTransform)transform).GetScreenSpaceRect().size.x * 0.65f;
		Vector2 vector = (_isUsingRightStick ? DewInput.GetRightJoystick() : DewInput.GetLeftJoystick());
		return (Vector2)transform.position + num * vector;
	}

	private void SetupItems()
	{
		for (int i = 0; i < wheelItems.Length; i++)
		{
			if (i >= DewSave.profileMain.equippedEmotes.Count)
			{
				wheelItems[i].Setup(null);
			}
			else
			{
				wheelItems[i].Setup(DewSave.profileMain.equippedEmotes[i]);
			}
		}
	}

	public void OnSettingsChanged()
	{
		SetupItems();
	}

	private int GetClosestEmoteIndex(Vector2 cursorPos)
	{
		return Dew.SelectBestIndexWithScore((IList<UI_EmoteWheel_Item>)wheelItems, (Func<UI_EmoteWheel_Item, int, float>)((UI_EmoteWheel_Item item, int _) => 1f / Vector2.Distance(cursorPos, item.transform.position)), 0f, (DewRandom)null);
	}
}
