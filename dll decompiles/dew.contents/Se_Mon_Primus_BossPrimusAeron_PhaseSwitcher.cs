using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Mon_Primus_BossPrimusAeron_PhaseSwitcher : StatusEffect
{
	public GameObject fxExplode;

	public GameObject[] fxExplodeByPhase;

	public GameObject fxExplodeHit;

	public GameObject[] fxExplodeHitByPhase;

	public Knockback explodeKnockback;

	public float explodeStunDuration = 2.5f;

	public DewCollider explodeRange;

	public float afterExplodeDelay = 3f;

	public GameObject fxDownedLoop;

	public GameObject[] fxWeaponDisappearByPhase;

	public GameObject fxDownEnd;

	public float downEndDuration = 1f;

	[Space]
	public GameObject fxPhaseChangePrepare;

	public GameObject[] fxPhaseChangePrepareByPhase;

	public float[] phaseChangePrepareDuration;

	public GameObject fxPhaseChangeEnd;

	public GameObject[] fxPhaseChangeEndByPhase;

	public float postDaze;

	public new Mon_Primus_BossPrimusAeron victim => base.victim as Mon_Primus_BossPrimusAeron;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoDeathInterrupt((EventInfoKill kill) =>
		{
			if (victim.phase == Mon_Primus_BossPrimusAeron.PhaseType.InTransition)
			{
				victim.Status.SetHealth(1f);
			}
			else if (victim.phase == Mon_Primus_BossPrimusAeron.PhaseType.Force)
			{
				victim.Status.SetHealth(1f);
				ChangePhase(Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
			}
			else if (victim.phase == Mon_Primus_BossPrimusAeron.PhaseType.Adapt)
			{
				victim.Status.SetHealth(1f);
				ChangePhase(Mon_Primus_BossPrimusAeron.PhaseType.Rage);
			}
		}, 0);
	}

	private void ChangePhase(Mon_Primus_BossPrimusAeron.PhaseType newPhase)
	{
		Debug.Log("Changed phase to " + newPhase);
		((MonoBehaviour)(object)this).StopAllCoroutines();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator BreakPizzas(int maxCount)
		{
			Primus_Pizza0[] array = Primus_Pizza0.instances.ToArray();
			int broken = 0;
			array.Shuffle();
			Primus_Pizza0 primusPizza = null;
			RaycastHit[] array2 = Physics.RaycastAll(victim.agentPosition + Vector3.up * 5f, Vector3.down, 15f, LayerMasks.Ground);
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit val = array2[i];
				Primus_Pizza0 componentInParent = val.transform.GetComponentInParent<Primus_Pizza0>();
				if ((Object)(object)componentInParent != null)
				{
					primusPizza = componentInParent;
				}
			}
			Debug.Log($"Breaking pizzas in phase switch: {newPhase}. Max count is {maxCount}, and {array.Count((Primus_Pizza0 primus_Pizza) => !primus_Pizza.isBroken)}/{array.Length} pizza(s) are currently alive.");
			Debug.Log($"Just to be sure, ${array.Count((Primus_Pizza0 primus_Pizza) => primus_Pizza.isActive)}/${array.Length} pizzas are active.");
			Primus_Pizza0[] array3 = array;
			foreach (Primus_Pizza0 p in array3)
			{
				if (!p.IsNullOrInactive() && !p.isBroken && !((Object)(object)p == (Object)(object)primusPizza))
				{
					Vector3 positionOnGround = Dew.GetPositionOnGround(p.centerPoint.position);
					CreateAbilityInstance(positionOnGround, null, new CastInfo(info.caster, positionOnGround), (Ai_Mon_Primus_BossPrimusAeron_Force_DropGiantSword ai) =>
					{
						ai.pizza = p;
					});
					broken++;
					if (broken >= maxCount)
					{
						break;
					}
					yield return new WaitForSeconds(0.35f);
				}
			}
		}
		IEnumerator Routine()
		{
			victim.Control.Rotate(180f + ManagerBase<CameraManager>.instance.entityCamAngle, immediately: false);
			Se_GenericEffectContainer invul = CreateBasicEffect(victim, new InvulnerableEffect(), float.PositiveInfinity);
			victim.isArmorBroken = true;
			victim.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			victim.phase = Mon_Primus_BossPrimusAeron.PhaseType.InTransition;
			victim.Control.CancelOngoingChannels();
			victim.Control.CancelOngoingDisplacement();
			victim.Control.Stop();
			int index = (int)(newPhase - 1);
			FxPlayNetworked(fxExplode, victim);
			FxPlayNetworked(fxExplodeByPhase[index], victim);
			List<Entity> entities = explodeRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxExplodeHit, entity);
				FxPlayNewNetworked(fxExplodeHitByPhase[index], entity);
				explodeKnockback.ApplyWithOrigin(victim.agentPosition, entity);
				CreateBasicEffect(entity, new StunEffect(), explodeStunDuration);
			}
			handle.Return();
			if (victim.weapon == Mon_Primus_BossPrimusAeron.WeaponType.GreatSword)
			{
				Vector3 landPos = default;
				for (int j = 0; j < 10; j++)
				{
					landPos = Dew.GetValidAgentDestination_LinearSweep(victim.agentPosition, victim.agentPosition + Random.insideUnitCircle.ToXZ().normalized * 7f);
					if (Vector2.Distance(victim.agentPosition.ToXY(), landPos.ToXY()) > 5f)
					{
						break;
					}
				}
				CreateAbilityInstance(position, Quaternion.LookRotation(landPos - victim.agentPosition), new CastInfo(victim, landPos), (Ai_Mon_Primus_BossPrimusAeron_PhaseSwitcher_GreatSword ai) =>
				{
					ai.initialSpeed = Vector2.Distance(victim.agentPosition.ToXY(), landPos.ToXY()) / 1.5f;
				});
			}
			victim.weapon = Mon_Primus_BossPrimusAeron.WeaponType.None;
			FxPlayNetworked(fxWeaponDisappearByPhase[index], victim);
			yield return new WaitForSeconds(afterExplodeDelay);
			victim.Control.Rotate(180f + ManagerBase<CameraManager>.instance.entityCamAngle, immediately: false);
			FxPlayNetworked(fxDownedLoop, victim);
			if (newPhase == Mon_Primus_BossPrimusAeron.PhaseType.Adapt)
			{
				int num = Primus_Pizza0.instances.Count((Primus_Pizza0 p) => !p.isBroken);
				if (num > 4)
				{
					yield return BreakPizzas(num - 4);
				}
				if (victim.Status.TryGetStatusEffect<Se_Mon_Primus_BossPrimusAeron_Adaptation>(out var effect))
				{
					yield return effect.GiveAdaptationOrbsRoutine();
				}
			}
			else if (newPhase == Mon_Primus_BossPrimusAeron.PhaseType.Rage)
			{
				yield return BreakPizzas(4);
			}
			FxStopNetworked(fxDownedLoop);
			FxPlayNetworked(fxDownEnd, victim);
			yield return new WaitForSeconds(downEndDuration);
			FxPlayNetworked(fxPhaseChangePrepare, victim);
			FxPlayNetworked(fxPhaseChangePrepareByPhase[index], victim);
			yield return new WaitForSeconds(phaseChangePrepareDuration[index]);
			FxStopNetworked(fxPhaseChangePrepare);
			FxStopNetworked(fxPhaseChangePrepareByPhase[index]);
			FxPlayNetworked(fxPhaseChangeEnd, victim);
			FxPlayNetworked(fxPhaseChangeEndByPhase[index], victim);
			victim.phase = newPhase;
			if (newPhase == Mon_Primus_BossPrimusAeron.PhaseType.Adapt)
			{
				victim.weapon = Mon_Primus_BossPrimusAeron.WeaponType.Spells;
				victim.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Dash>().configs[0].maxCharges = 2;
				victim.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Dash>().configs[0].addedCharges = 2;
				CreateStatusEffect(victim, (Se_GenericHealOverTime se) =>
				{
					se.ticks = 5;
					se.tickInterval = postDaze / 10f;
					se.totalAmount = victim.maxHealth;
				});
			}
			else if (newPhase == Mon_Primus_BossPrimusAeron.PhaseType.Rage)
			{
				victim.weapon = Mon_Primus_BossPrimusAeron.WeaponType.DoubleSword;
				victim.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Dash>().configs[0].maxCharges = 2;
				victim.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Dash>().configs[0].addedCharges = 2;
				victim.Status.SetHealth(victim.maxHealth * 0.05f);
				CreateStatusEffect<Se_Mon_Primus_BossPrimusAeron_Rage_DecayingShield>(victim);
			}
			yield return new WaitForSeconds(postDaze);
			victim.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			invul.Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
