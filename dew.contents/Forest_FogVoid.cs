using System;
using System.Collections;
using UnityEngine;

public class Forest_FogVoid : MonoBehaviour
{
	public float interval;

	public float increment;

	public float maxSize;

	public GameObject ppVolume;

	private void Start()
	{
		GameManager.CallOnReady(() =>
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(OnEntityDeath);
		});
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(OnEntityDeath);
		}
	}

	private void OnEntityDeath(EventInfoKill i)
	{
		if (i.victim is Monster { type: Monster.MonsterType.Boss })
		{
			FogVoidSequenceStart(i.victim);
		}
	}

	private void FogVoidSequenceStart(Entity e)
	{
		transform.position = new Vector3(((Component)(object)e).transform.position.x, transform.position.y, ((Component)(object)e).transform.position.z);
		UnityEngine.Object.Destroy(ppVolume);
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (transform.localScale.x < maxSize)
			{
				transform.localScale = new Vector3(transform.localScale.x + increment, transform.localScale.y, transform.localScale.z + increment);
				yield return new WaitForSeconds(interval);
			}
		}
	}
}
