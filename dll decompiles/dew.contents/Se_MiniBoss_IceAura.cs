using Mirror;
using UnityEngine;

public class Se_MiniBoss_IceAura : MiniBossEffect
{
	public float range;

	public float addedArmorAmount;

	public float startDelay;

	public float aoeInterval;

	public float aoeDelay;

	private float _lastAoETime;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoArmorBoost(addedArmorAmount);
			_lastAoETime = Time.time - aoeInterval + startDelay;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, victim.agentPosition, range, tvDefaultHarmfulEffectTargets))
		{
			if (item.Status.TryGetStatusEffect<Se_MiniBoss_IceAura_Slow>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_MiniBoss_IceAura_Slow>(item, new CastInfo(info.caster, item));
			}
		}
		handle.Return();
		if (victim.Visual.isRendererOff)
		{
			_lastAoETime = Time.time;
		}
		else if (!(Time.time - _lastAoETime < aoeInterval))
		{
			info.caster.Control.StartDaze(aoeDelay);
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			Vector3 end = info.caster.agentPosition;
			if ((Object)(object)closestAliveHero != null)
			{
				end = AbilityTrigger.PredictPoint_Simple(victim, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), closestAliveHero, aoeDelay);
			}
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			CreateAbilityInstance(end, null, new CastInfo(info.caster, end), (Ai_MiniBoss_IceAura_AoE b) =>
			{
				b.delay = aoeDelay;
			});
			_lastAoETime = Time.time;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastAoETime = 0f;
	}

	private void MirrorProcessed()
	{
	}
}
