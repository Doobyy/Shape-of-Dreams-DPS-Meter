using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_E_Thunder : Gem
{
	public GameObject chargeEffect;

	public GameObject startEffect;

	public float delay;

	public float randomMagnitude;

	public float range;

	public float interval;

	public int noTargetGraceCount;

	public ScalingValue maxCharge;

	private int _currentCharge;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			}
			_currentCharge = 0;
			numberDisplay = 0;
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (isValid && !((UnityEngine.Object)(object)obj.skill == (UnityEngine.Object)(object)skill))
		{
			int num = Mathf.RoundToInt(GetValue(maxCharge));
			if (_currentCharge < num)
			{
				_currentCharge = Mathf.Clamp(_currentCharge + 1, 0, num);
				numberDisplay = _currentCharge;
				NotifyUse();
				FxPlayNewNetworked(chargeEffect, owner);
			}
		}
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		int count;
		if (_currentCharge > 0)
		{
			FxPlayNewNetworked(startEffect, owner);
			count = _currentCharge;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
			_currentCharge = 0;
			numberDisplay = 0;
			NotifyUse();
		}
		IEnumerator Routine()
		{
			info.instance.LockDestroy();
			yield return new WaitForSeconds(delay);
			int currentStrikes = 0;
			for (int i = 0; i < count; i++)
			{
				if (!isValid)
				{
					info.instance.UnlockDestroy();
					yield break;
				}
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, owner.position, range, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.Random
				});
				if (list.Count > 0)
				{
					Vector3 positionOnGround = Dew.GetPositionOnGround(list[UnityEngine.Random.Range(0, list.Count)].agentPosition + UnityEngine.Random.insideUnitSphere * randomMagnitude);
					CreateAbilityInstanceWithSource<Ai_Gem_E_Thunder>(info.instance, positionOnGround, null, new CastInfo(owner));
					currentStrikes = 0;
				}
				else
				{
					currentStrikes++;
					if (currentStrikes <= noTargetGraceCount)
					{
						i--;
					}
				}
				handle.Return();
				NotifyUse();
				yield return new WaitForSeconds(interval);
			}
			info.instance.UnlockDestroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
