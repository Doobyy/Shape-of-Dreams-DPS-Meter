using System.Collections;
using Mirror;
using UnityEngine;

public class FxEntityInvoke : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public float delay;

	[Space(8f)]
	public string playMethodName;

	public bool playMethodCallOnClients = true;

	[Space(8f)]
	public string stopMethodName;

	public bool stopMethodCallOnClients = true;

	private Entity _target;

	public bool isPlaying => false;

	public void Play()
	{
		if ((playMethodCallOnClients || NetworkServer.active) && !string.IsNullOrEmpty(playMethodName) && !_target.IsNullOrInactive())
		{
			StopAllCoroutines();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(delay);
			if (!_target.IsNullOrInactive())
			{
				((MonoBehaviour)(object)_target).Invoke(playMethodName, 0f);
			}
		}
	}

	public void Stop()
	{
		if ((stopMethodCallOnClients || NetworkServer.active) && !string.IsNullOrEmpty(stopMethodName) && !_target.IsNullOrInactive())
		{
			StopAllCoroutines();
			((MonoBehaviour)(object)_target).Invoke(stopMethodName, 0f);
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		_target = target;
	}
}
