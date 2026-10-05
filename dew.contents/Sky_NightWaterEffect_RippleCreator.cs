using System;
using System.Collections;
using UnityEngine;

public class Sky_NightWaterEffect_RippleCreator : MonoBehaviour
{
	private const int CreatorsLimit = 20;

	private static int CurrentCreatorsCount;

	public Vector3 offset;

	internal Sky_NightWaterEffect _parent;

	internal Entity _entity;

	private float _interval;

	private void Start()
	{
		_entity.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			if (this != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		});
		CurrentCreatorsCount++;
		if (CurrentCreatorsCount > 20)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (!(_parent == null) && !((UnityEngine.Object)(object)_entity == null) && _entity.isActive)
			{
				if (!_parent.onlyWhenWalking || _entity.Control.isWalking)
				{
					Vector3 positionOnGround = Dew.GetPositionOnGround(_entity.agentPosition);
					DewEffect.PlayNew(_parent.rippleEffect, positionOnGround + _entity.rotation * offset, Quaternion.identity);
				}
				yield return new WaitForSeconds(UnityEngine.Random.Range(_parent.interval.x, _parent.interval.y));
			}
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private void OnDestroy()
	{
		CurrentCreatorsCount--;
	}
}
