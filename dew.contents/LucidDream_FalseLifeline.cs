using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class LucidDream_FalseLifeline : LucidDream
{
	public GameObject fxBlessing;

	public float missingHealthHealRatio = 0.25f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		NetworkedManagerBase<ActorManager>.instance.onActorBeforePrepare += new Action<Actor>(OnActorBeforePrepare);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			OnActorBeforePrepare(allActor);
		}
	}

	private void OnActorBeforePrepare(Actor obj)
	{
		if (obj is Se_HeroKnockedOut se_HeroKnockedOut)
		{
			se_HeroKnockedOut.disableQuest = true;
		}
		if (obj is CurseStatusEffect curseStatusEffect)
		{
			curseStatusEffect.dontRemoveOnKnockOut = true;
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (obj.isTraveling && !SingletonDewNetworkBehaviour<Room>.instance.isRevisit)
		{
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(() =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			});
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.4f);
			if (!((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null))
			{
				Hero[] array = NetworkedManagerBase<ActorManager>.instance.allHeroes.ToArray();
				foreach (Hero h in array)
				{
					if (NetworkedManagerBase<GameManager>.instance.isGameConcluded)
					{
						break;
					}
					if (h.isKnockedOut)
					{
						Hero closestAliveHero = Dew.GetClosestAliveHero(h.agentPosition);
						if ((UnityEngine.Object)(object)closestAliveHero != null)
						{
							Vector3 vector = closestAliveHero.agentPosition + UnityEngine.Random.onUnitSphere * 5f;
							vector = Dew.GetPositionOnGround(vector);
							vector = Dew.GetValidAgentDestination_Closest(closestAliveHero.agentPosition, vector);
							Teleport(h, vector);
						}
						CreateAbilityInstance(h.agentPosition, null, new CastInfo(h, h), (Ai_ReviveHero ai) =>
						{
							ai.reviveHealthMultiplier = missingHealthHealRatio;
						});
					}
					else
					{
						Heal(h.Status.missingHealth * missingHealthHealRatio).Dispatch(h);
					}
					yield return new WaitForSeconds(0.1f);
					FxPlayNewNetworked(fxBlessing, h);
				}
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.onActorBeforePrepare -= new Action<Actor>(OnActorBeforePrepare);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
