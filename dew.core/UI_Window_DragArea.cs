using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Window_DragArea : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	private UI_Window _parent;

	private bool _isDragging;

	private Vector3 _lastMousePosition;

	private void Awake()
	{
		_parent = GetComponentInParent<UI_Window>(includeInactive: true);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_isDragging = true;
		_lastMousePosition = Input.mousePosition;
	}

	private void Update()
	{
		if (_isDragging && !(_parent == null))
		{
			Vector3 vector = Input.mousePosition - _lastMousePosition;
			_lastMousePosition = Input.mousePosition;
			RectTransform rectTransform = (RectTransform)_parent.transform;
			Vector3 position = rectTransform.position;
			position += vector;
			position.z = 0f;
			rectTransform.position = position;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_isDragging = false;
		RectTransform rectTransform = (RectTransform)_parent.transform;
		Vector3 position = rectTransform.position;
		Rect worldRect = rectTransform.GetWorldRect(rectTransform.lossyScale);
		if (worldRect.xMin < 0f)
		{
			position.x += 0f - worldRect.xMin;
		}
		else if (worldRect.xMax > (float)Screen.width)
		{
			position.x += (float)Screen.width - worldRect.xMax;
		}
		if (worldRect.yMin < 0f)
		{
			position.y += 0f - worldRect.yMin;
		}
		else if (worldRect.yMax > (float)Screen.height)
		{
			position.y += (float)Screen.height - worldRect.yMax;
		}
		rectTransform.position = position;
	}
}
