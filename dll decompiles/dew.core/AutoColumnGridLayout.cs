using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Layout/Auto Column Grid Layout")]
[ExecuteAlways]
public class AutoColumnGridLayout : GridLayoutGroup
{
	[SerializeField]
	private bool _autoAdjustColumns = true;

	[SerializeField]
	private int _minColumns = 1;

	[SerializeField]
	private int _maxColumns = int.MaxValue;

	public bool AutoAdjustColumns
	{
		get
		{
			return _autoAdjustColumns;
		}
		set
		{
			_autoAdjustColumns = value;
			if (_autoAdjustColumns)
			{
				UpdateColumnCount();
			}
		}
	}

	public int MinColumns
	{
		get
		{
			return _minColumns;
		}
		set
		{
			_minColumns = Mathf.Max(1, value);
			if (_autoAdjustColumns)
			{
				UpdateColumnCount();
			}
		}
	}

	public int MaxColumns
	{
		get
		{
			return _maxColumns;
		}
		set
		{
			_maxColumns = Mathf.Max(_minColumns, value);
			if (_autoAdjustColumns)
			{
				UpdateColumnCount();
			}
		}
	}

	protected override void OnEnable()
	{
		((LayoutGroup)this).OnEnable();
		UpdateColumnCount();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		((LayoutGroup)this).OnRectTransformDimensionsChange();
		UpdateColumnCount();
	}

	protected override void OnTransformParentChanged()
	{
		((UIBehaviour)this).OnTransformParentChanged();
		UpdateColumnCount();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		((LayoutGroup)this).OnDidApplyAnimationProperties();
		UpdateColumnCount();
	}

	private void Update()
	{
	}

	public void UpdateColumnCount()
	{
		if (!_autoAdjustColumns || !((Component)this).gameObject.activeInHierarchy)
		{
			return;
		}
		RectTransform rectTransform = ((Component)this).transform.parent as RectTransform;
		if (!(rectTransform == null))
		{
			int b = Mathf.FloorToInt((rectTransform.rect.width - (float)((LayoutGroup)this).padding.left - (float)((LayoutGroup)this).padding.right + ((GridLayoutGroup)this).spacing.x) / (((GridLayoutGroup)this).cellSize.x + ((GridLayoutGroup)this).spacing.x));
			b = Mathf.Max(1, b);
			int num = Mathf.Clamp(b, _minColumns, _maxColumns);
			if (((GridLayoutGroup)this).constraintCount != num)
			{
				((GridLayoutGroup)this).constraint = (Constraint)1;
				((GridLayoutGroup)this).constraintCount = num;
				LayoutRebuilder.MarkLayoutForRebuild(((LayoutGroup)this).rectTransform);
			}
		}
	}
}
