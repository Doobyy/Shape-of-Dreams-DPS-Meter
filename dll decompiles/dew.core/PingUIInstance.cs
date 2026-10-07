using System;
using DG.Tweening;
using PixelPlay.OffScreenIndicator;
using TMPro;
using UnityEngine;

public class PingUIInstance : MonoBehaviour
{
	public Vector2 screenMargin;

	public Transform rotationTransform;

	public float angleOffset;

	public CanvasGroup arrowCanvasGroup;

	public TextMeshProUGUI playerNameText;

	public Vector3 scalePunch;

	public float punchDuration;

	public bool useAlwaysOnTopUIParent;

	internal PingManager.Ping _ping;

	private Func<Vector3> _worldPosGetter;

	private Func<Vector2> _uiPosGetter;

	private void Start()
	{
		switch (_ping.type)
		{
		case PingManager.PingType.Move:
			_worldPosGetter = () => _ping.position;
			break;
		case PingManager.PingType.Entity:
		case PingManager.PingType.ShopItem:
			_worldPosGetter = () => ((Entity)(object)_ping.target).Visual.GetCenterPosition();
			break;
		case PingManager.PingType.Interactable:
			_worldPosGetter = () => ((IInteractable)_ping.target).interactPivot.position;
			break;
		case PingManager.PingType.EquippedItem:
			UnityEngine.Object.Destroy(gameObject);
			break;
		case PingManager.PingType.WorldNode:
			_uiPosGetter = GetUIPosOfPing;
			InGameUIManager.instance.IncrementWorldNodePingCounter();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		TweenSettingsExtensions.SetUpdate<Tweener>(ShortcutExtensions.DOPunchScale(transform, scalePunch, punchDuration, 10, 1f), true);
		((TMP_Text)playerNameText).text = _ping.sender.playerName;
		UpdatePosition();
	}

	private Vector2 GetUIPosOfPing()
	{
		return InGameUIManager.instance.GetWorldNodeUIPos(_ping.itemIndex);
	}

	private void LateUpdate()
	{
		if (!_ping.IsValid())
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			UpdatePosition();
		}
	}

	private void OnDestroy()
	{
		if (_ping.type == PingManager.PingType.WorldNode && InGameUIManager.instance != null)
		{
			InGameUIManager.instance.DecrementWorldNodePingCounter();
		}
	}

	private void UpdatePosition()
	{
		if (_worldPosGetter != null)
		{
			Vector3 position = _worldPosGetter();
			Vector3 vector = Dew.mainCamera.WorldToScreenPoint(position);
			bool flag = OffScreenIndicatorCore.IsTargetVisible(vector);
			arrowCanvasGroup.alpha = ((!flag) ? 1 : 0);
			if (flag)
			{
				transform.position = vector.Quantitized();
				return;
			}
			float num = 0f;
			Vector3 vector2 = new Vector3(Screen.width, Screen.height, 0f) / 2f;
			Vector3 vector3 = vector2 - new Vector3(screenMargin.x, screenMargin.y, 0f);
			OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref vector, ref num, vector2, vector3);
			transform.position = vector.Quantitized();
			rotationTransform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f + angleOffset);
		}
		else if (_uiPosGetter != null)
		{
			Vector2 v = _uiPosGetter();
			transform.position = v.Quantitized();
		}
	}
}
