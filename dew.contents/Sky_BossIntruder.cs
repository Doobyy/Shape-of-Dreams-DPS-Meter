using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Sky_BossIntruder : Actor
{
	public DewCutsceneDirector cutscene;

	public GameObject fxMapProps;

	public FxNetworkedEffect fxIntrude;

	public float delay;

	public float cutsceneDuration;

	public float postDelay;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && Ge_TheConsortOfNight.EnableSpawnErebos())
		{
			FxPlayNetworked(fxMapProps);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null))
		{
			FxStopNetworked(fxMapProps);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
	}

	private void OnEntityAdd(Entity entity)
	{
		if (!entity.IsNullInactiveDeadOrKnockedOut() && entity is Mon_Sky_BossNyx)
		{
			entity.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (obj.victim.normalizedHealth < 0.51f)
		{
			if (obj.victim.Status.TryGetStatusEffect<Se_Mon_Sky_BossNyx_PhaseChange>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_Mon_Sky_BossNyx_IntrudeErebos>(obj.victim, new CastInfo(obj.victim));
			Dew.CallDelayed(() =>
			{
				obj.victim.Status.SetHealth(obj.victim.maxHealth * 0.5f);
			});
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			fxIntrude.PlayNetworked();
			yield return new WaitForSeconds(delay);
			bool flag = true;
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (!allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity is Hero)
				{
					flag = false;
				}
			}
			if (flag)
			{
				Destroy();
			}
			else
			{
				cutscene.PlayNetworked();
				yield return new WaitForSeconds(cutsceneDuration);
				CreateAbilityInstance<Ai_Sky_BossIntruder_Instance>(SingletonBehaviour<Sky_BossRoomCenter>.instance.transform.position, Quaternion.identity, new CastInfo(obj.victim));
				Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
