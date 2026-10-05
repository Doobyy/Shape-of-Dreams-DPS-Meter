using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_BackButtonHandler : MonoBehaviour
{
	public int priority;

	public bool consume = true;

	public bool checkUIOcclusion;

	public bool checkInteractable = true;

	public bool activateOnRightClickAnywhere;

	public UnityEvent onHandle;

	private DewInputTrigger it_rightClick;

	private Selectable _selectable;

	private BackHandler _handle;

	private void Awake()
	{
		_selectable = GetComponent<Selectable>();
		it_rightClick = new DewInputTrigger
		{
			owner = this,
			isValidCheck = () => isActiveAndEnabled && activateOnRightClickAnywhere && !ManagerBase<MessageManager>.instance.isShowingMessage && !ManagerBase<GlobalUIManager>.instance.isTutorialHighlighting && (!checkInteractable || !(Object)(object)_selectable || _selectable.IsInteractable()),
			canConsume = true,
			binding = () => DewBinding.KeyboardAndMouseOnly(MouseButton.Right),
			priority = -priority
		};
	}

	private void OnEnable()
	{
		if (!(ManagerBase<GlobalUIManager>.instance != null))
		{
			return;
		}
		_handle = ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, priority, () =>
		{
			if (checkUIOcclusion && !ManagerBase<GlobalUIManager>.instance.IsUIElementClickable((RectTransform)transform))
			{
				return false;
			}
			return (!checkInteractable || !(Object)(object)_selectable || _selectable.IsInteractable()) && Callback();
		});
	}

	private void Update()
	{
		if (activateOnRightClickAnywhere && it_rightClick.down)
		{
			Callback();
		}
	}

	private void OnDisable()
	{
		if (_handle != null)
		{
			_handle.Remove();
			_handle = null;
		}
	}

	private bool Callback()
	{
		Button component = GetComponent<Button>();
		if ((Object)(object)component != null)
		{
			((UnityEvent)(object)component.onClick)?.Invoke();
		}
		onHandle?.Invoke();
		return consume;
	}
}
