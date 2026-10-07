using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_ShortDash_Atk : AbilityInstance
{
	public float dashDuration;

	public float dashDistance;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewCollider range;

	public DewEase ease;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		range.transform.position = info.caster.position;
		range.transform.rotation = info.caster.rotation;
		Vector3 dir = ((Component)(object)info.caster).transform.forward;
		Vector3 end = info.caster.position + dir * dashDistance;
		end = Dew.GetValidAgentDestination_LinearSweep(info.caster.position, end);
		info.caster.Control.StartDaze(dashDuration);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = false,
			destination = end,
			duration = dashDuration,
			ease = ease,
			isCanceledByCC = true,
			isFriendly = true,
			onCancel = () =>
			{
				if ((Object)(object)firstTrigger != null)
				{
					firstTrigger.SetCooldownTime(0, 1.5f);
				}
				DestroyIfActive();
			}
		});
		yield return new SI.WaitForSeconds(0.05f);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int num = 0; num < entities.Count; num++)
		{
			Entity entity = entities[num];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(dir).SetOriginPosition(info.caster.position).Dispatch(entity);
			knockback.ApplyWithDirection(dir, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
