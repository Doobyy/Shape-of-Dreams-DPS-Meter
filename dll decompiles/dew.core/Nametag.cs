using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DewResourceLink(ResourceLinkBy.Name)]
[RequireComponent(typeof(LayoutElement))]
public class Nametag : MonoBehaviour, ICosmetic
{
	public string category;

	public float customOrder;

	public bool generatedFromServer;

	public string[] dlcIds;

	public CosmeticPurchasePrerequisite condition;

	public string conditionType;

	public int conditionValue;

	[Space(20f)]
	public float iconNormalizedOffsetY;

	public float iconWidthOffset;

	public Vector2 nametagHorizontalMargin;

	public int price = 250;

	bool ICosmetic.generatedFromServer => generatedFromServer;

	string[] ICosmetic.dlcIds => dlcIds;

	public void Setup(RectTransform target, bool isIcon)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		GetComponent<LayoutElement>().ignoreLayout = true;
		RectTransform rectTransform = (RectTransform)transform;
		rectTransform.SetParent(target);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = Vector2.zero;
		float height = target.GetScreenSpaceRect().height;
		if (isIcon)
		{
			rectTransform.sizeDelta = rectTransform.sizeDelta.WithX(iconWidthOffset);
			transform.position += Vector3.up * (height * iconNormalizedOffsetY);
		}
		ShortcutExtensions.DOPunchScale(transform, Vector3.one * 0.1f, 0.1f, 10, 1f);
	}

	private void OnDrawGizmos()
	{
		RectTransform component = GetComponent<RectTransform>();
		if (!(component == null))
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			Vector3[] array = new Vector3[4];
			component.GetWorldCorners(array);
			for (int i = 0; i < 4; i++)
			{
				Gizmos.DrawLine(array[i], array[(i + 1) % 4]);
			}
		}
	}

	public void CheckPrerequisites(out bool canPurchase, out int currentProgress, out int maxProgress, out string conditionText)
	{
		Dew.CheckCosmeticPrerequisites(name, category, category, condition, conditionType, conditionValue, out canPurchase, out currentProgress, out maxProgress, out conditionText);
	}
}
