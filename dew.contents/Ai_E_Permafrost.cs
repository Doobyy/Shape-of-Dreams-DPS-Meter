using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_E_Permafrost : AbilityInstance
{
	public float castDelay;

	public float postDelay;

	public float range;

	public float chilledAmpBonus;

	public float procCoefficient;

	public ScalingValue dmgFactor;

	public GameObject fxInstance;

	public GameObject fxHit;

	public DewAnimationClip castAnim;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.caster.agentPosition, range, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				Se_E_Permafrost_Shield se_E_Permafrost_Shield = item.Status.FindStatusEffect((Se_E_Permafrost_Shield s) => s.isActive && (Object)(object)s.info.caster == (Object)(object)info.caster && (Object)(object)s.firstTrigger == (Object)(object)firstTrigger);
				if ((Object)(object)se_E_Permafrost_Shield != null)
				{
					se_E_Permafrost_Shield.AddStack();
				}
				else
				{
					CreateStatusEffect<Se_E_Permafrost_Shield>(item, new CastInfo(info.caster));
				}
			}
		}
		handle.Return();
		info.caster.Control.StartDaze(castDelay + postDelay);
		yield return new SI.WaitForSeconds(castDelay);
		FxPlayNetworked(fxInstance, info.caster);
		info.caster.Animation.PlayAbilityAnimation(castAnim);
		ListReturnHandle<Entity> handle2;
		foreach (Entity item2 in DewPhysics.OverlapCircleAllEntities(out handle2, info.caster.agentPosition, range, tvDefaultHarmfulEffectTargets))
		{
			if (!item2.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlayNewNetworked(fxHit, item2);
				float num = GetValue(dmgFactor);
				if (item2.Status.hasCold)
				{
					num *= chilledAmpBonus;
				}
				CreateDamage(DamageData.SourceType.Magic, num, procCoefficient).SetElemental(ElementalType.Cold).SetOriginPosition(info.caster.agentPosition).Dispatch(item2);
				if (!item2.Status.hasCrowdControlImmunity && !item2.Status.HasStatusEffect<Se_E_Permafrost_Stun>())
				{
					CreateStatusEffect<Se_E_Permafrost_Stun>(item2, new CastInfo(info.caster, item2));
				}
			}
		}
		handle2.Return();
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
