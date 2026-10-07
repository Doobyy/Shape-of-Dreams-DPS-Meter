using System;

namespace UnityEngine.UI;

[AddComponentMenu("Layout/Dew Content Size Fitter", 141)]
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class DewContentSizeFitter : ContentSizeFitter
{
	[NonSerialized]
	private RectTransform m_Rect;

	[SerializeField]
	private float m_MaxWidth = -1f;

	[SerializeField]
	private float m_MaxHeight = -1f;

	private DrivenRectTransformTracker m_DewTracker;

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = ((Component)this).GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	public float maxWidth
	{
		get
		{
			return m_MaxWidth;
		}
		set
		{
			m_MaxWidth = value;
		}
	}

	public float maxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			m_MaxHeight = value;
		}
	}

	private void Fit(int axis, FitMode fitting, float max)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		RectTransform rectTransform = this.rectTransform;
		if ((int)fitting == 0)
		{
			m_DewTracker.Add((Object)(object)this, rectTransform, DrivenTransformProperties.None);
			return;
		}
		m_DewTracker.Add((Object)(object)this, rectTransform, (axis == 0) ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY);
		float num = (((int)fitting == 1) ? LayoutUtility.GetMinSize(rectTransform, axis) : LayoutUtility.GetPreferredSize(rectTransform, axis));
		if (max > 0f)
		{
			num = Mathf.Min(num, max);
		}
		if (Mathf.Abs(((axis == 0) ? rectTransform.rect.width : rectTransform.rect.height) - num) > 0.01f)
		{
			rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, num);
		}
	}

	public override void SetLayoutHorizontal()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		m_DewTracker.Clear();
		Fit(0, ((ContentSizeFitter)this).horizontalFit, maxWidth);
	}

	public override void SetLayoutVertical()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Fit(1, ((ContentSizeFitter)this).verticalFit, maxHeight);
	}

	protected override void OnDisable()
	{
		m_DewTracker.Clear();
		((ContentSizeFitter)this).OnDisable();
	}
}
