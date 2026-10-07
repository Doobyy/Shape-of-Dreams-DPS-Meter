using UnityEngine;
using UnityEngine.Events;

namespace VolFx;

public class IfMovePresented : MonoBehaviour
{
	public UnityEvent _onInvoke;

	private void OnEnable()
	{
		if ((bool)(Object)(object)Object.FindObjectOfType<Move>())
		{
			_onInvoke.Invoke();
		}
	}
}
