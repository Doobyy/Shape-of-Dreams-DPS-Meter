using UnityEngine;
using UnityEngine.UI;

public class UI_Window : MonoBehaviour
{
	public UI_Window_DragArea dragArea;

	public GameObject backdropPrefab;

	private bool _enableBackdrop;

	private GameObject _backdrop;

	public bool isDraggable
	{
		get
		{
			return dragArea.gameObject.activeSelf;
		}
		set
		{
			dragArea.gameObject.SetActive(value);
		}
	}

	public bool enableBackdrop
	{
		get
		{
			return _enableBackdrop;
		}
		set
		{
			if (_enableBackdrop != value)
			{
				if (_backdrop != null)
				{
					Object.Destroy(_backdrop);
					_backdrop = null;
				}
				_enableBackdrop = value;
				if (value)
				{
					_backdrop = Object.Instantiate(backdropPrefab, transform.parent);
					transform.SetAsLastSibling();
				}
			}
		}
	}

	private void Awake()
	{
		if (TryGetComponent<DewContentSizeFitter>(out var component) && component.maxHeight < 0f)
		{
			if (Mathf.Approximately(component.maxHeight, -2f))
			{
				component.maxHeight = (float)Screen.height / transform.lossyScale.y - 100f;
			}
			if (Mathf.Approximately(component.maxWidth, -2f))
			{
				component.maxWidth = (float)Screen.width / transform.lossyScale.x - 100f;
			}
		}
	}

	private void OnDestroy()
	{
		if (_backdrop != null)
		{
			Object.Destroy(_backdrop);
			_backdrop = null;
		}
	}
}
