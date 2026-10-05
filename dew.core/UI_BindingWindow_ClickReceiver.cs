using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BindingWindow_ClickReceiver : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0)
		{
			GetComponentInParent<UI_BindingWindow>().DoLeftClick();
		}
	}
}
