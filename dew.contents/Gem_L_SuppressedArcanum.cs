using System.Collections;
using Mirror;
using UnityEngine;

public class Gem_L_SuppressedArcanum : Gem
{
	public float hpThresholdRatio = 0.6f;

	public float shieldAmpOnLowHp = 0.5f;

	public float delay = 0.25f;

	public float waitForDashMaxTime = 1f;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtShieldProcessor.Add(ShieldProcessor);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (Object)(object)oldSkill != null)
		{
			oldSkill.dealtShieldProcessor.Remove(ShieldProcessor);
		}
	}

	private void ShieldProcessor(ref HealData data, Actor actor, Entity target)
	{
		if (isValid && !(owner.normalizedHealth > hpThresholdRatio) && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(shieldAmpOnLowHp);
			NotifyUse();
		}
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.instance.LockDestroy();
			yield return new WaitForSeconds(delay);
			if (!isValid)
			{
				info.instance.UnlockDestroy();
			}
			else
			{
				float startTime = Time.time;
				while ((Object)(object)owner != null && owner.Control.isDashing && Time.time - startTime < waitForDashMaxTime)
				{
					yield return null;
				}
				info.instance.UnlockDestroy();
				if (isValid)
				{
					CreateAbilityInstanceWithSource<Ai_Gem_L_SuppressedArcanum>(info.instance, owner.position, owner.rotation, new CastInfo(owner));
					NotifyUse();
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
