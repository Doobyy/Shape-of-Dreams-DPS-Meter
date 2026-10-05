using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_D_BonusOnDash : StarEffect
{
	public StarScalingValue grantedAd;

	public StarScalingValue grantedCritChance;

	public float duration = 2.5f;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			victim.Control.ClientEvent_OnTeleport += new Action<Vector3, Vector3>(ClientEventOnTeleport);
			victim.Control.ClientEvent_OnDisplacementStarted += new Action<Displacement>(ClientEventOnDisplacementStarted);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StopAllCoroutines();
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.Control.ClientEvent_OnTeleport -= new Action<Vector3, Vector3>(ClientEventOnTeleport);
				victim.Control.ClientEvent_OnDisplacementStarted -= new Action<Displacement>(ClientEventOnDisplacementStarted);
			}
		}
	}

	private void ClientEventOnTeleport(Vector3 arg1, Vector3 arg2)
	{
		StartBonus();
	}

	private void ClientEventOnDisplacementStarted(Displacement obj)
	{
		if (obj.isFriendly)
		{
			StartBonus();
		}
	}

	private void StartBonus()
	{
		((MonoBehaviour)(object)this).StopAllCoroutines();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_bonus.attackDamageFlat = GetValueInt(grantedAd);
			_bonus.critChanceFlat = GetValue(grantedCritChance);
			yield return new WaitForSeconds(duration);
			_bonus.attackDamageFlat = 0f;
			_bonus.critChanceFlat = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
