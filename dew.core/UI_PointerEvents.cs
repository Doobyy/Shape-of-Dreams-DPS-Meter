using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_PointerEvents : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	public UnityEvent onPointerClick = new UnityEvent();

	public UnityEvent onPointerDown = new UnityEvent();

	public UnityEvent onPointerUp = new UnityEvent();

	public UnityEvent onPointerEnter = new UnityEvent();

	public UnityEvent onPointerExit = new UnityEvent();

	public void OnPointerClick(PointerEventData eventData)
	{
		onPointerClick?.Invoke();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		onPointerDown?.Invoke();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		onPointerUp?.Invoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		onPointerEnter?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		onPointerExit?.Invoke();
	}
}
