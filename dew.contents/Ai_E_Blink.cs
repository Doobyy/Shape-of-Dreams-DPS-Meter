using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_E_Blink : AbilityInstance
{
	public bool enableGoBack;

	public GameObject prepareEffect;

	public float blinkDelay;

	public DewCollider range;

	public GameObject explodeEffect;

	public GameObject hitEffect;

	public GameObject goBackPositionEffect;

	public GameObject halfDissolveEffect;

	public float reducedRatioPerEnemy;

	public ScalingValue damageAmount;

	public float backDuration;

	private int _generation;

	private AbilityTrigger.ChangedConfigHandle _configHandle;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		Vector3 teleportPos = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, info.point);
		rotation = Quaternion.LookRotation(teleportPos - position).Flattened();
		FxPlay(prepareEffect);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		FxPlayNetworked(goBackPositionEffect);
		FxPlayNetworked(halfDissolveEffect, info.caster);
		((St_E_Blink)firstTrigger).backLocation = info.caster.agentPosition;
		Hero hero = info.caster as Hero;
		info.caster.Control.StartDaze(blinkDelay);
		yield return new SI.WaitForSeconds(blinkDelay);
		if ((Object)(object)hero == null || hero.isKnockedOut || !hero.isActive)
		{
			Destroy();
			yield break;
		}
		Teleport(info.caster, teleportPos);
		FxPlayNetworked(explodeEffect, teleportPos, Quaternion.identity);
		range.transform.position = teleportPos;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			FxPlayNewNetworked(hitEffect, entities[i]);
			Damage(damageAmount).SetElemental(ElementalType.Fire).SetOriginPosition(teleportPos).Dispatch(entities[i]);
		}
		if ((Object)(object)firstTrigger != null)
		{
			ApplyCooldownReductionByRatio(firstTrigger, reducedRatioPerEnemy * (float)entities.Count);
		}
		handle.Return();
		if (!enableGoBack)
		{
			Destroy();
			yield break;
		}
		firstTrigger.SetCharge(1, 0);
		int gen = _generation;
		_configHandle = firstTrigger.ChangeConfigTimedOnce(1, backDuration, (EventInfoAbilityInstance _) =>
		{
			if (gen == _generation)
			{
				DestroyIfActive();
			}
		}, () =>
		{
			if (gen == _generation)
			{
				DestroyIfActive();
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			_configHandle?.Stop();
			_configHandle = null;
		}
		FxStop(goBackPositionEffect);
		FxStop(halfDissolveEffect);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
	}

	private void MirrorProcessed()
	{
	}
}
