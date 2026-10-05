using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_L_ArmorWithPenalty : StarEffect
{
	public StarScalingValue armorStrength;

	public float disableDuration = 3f;

	private ArmorBoostEffect _armor;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_armor = DoArmorBoost(GetValueInt(armorStrength));
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
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
				victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			}
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		((MonoBehaviour)(object)this).StopAllCoroutines();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_armor.strength = 0f;
			yield return new WaitForSeconds(disableDuration);
			_armor.strength = GetValueInt(armorStrength);
		}
	}

	private void MirrorProcessed()
	{
	}
}
