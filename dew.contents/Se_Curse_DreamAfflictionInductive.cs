using Mirror;
using UnityEngine;

public class Se_Curse_DreamAfflictionInductive : CurseStatusEffect
{
	public float[] damageHpRatio;

	public Vector2[] interval;

	private float _nextActivateTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Vector2 value = GetValue(interval);
			_nextActivateTime = Time.time + Random.Range(value.x, value.y);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		Vector2 value = GetValue(interval);
		if (Se_Curse_IntermittentExplosion.ShouldBeSuppressed(victim))
		{
			_nextActivateTime = Time.time + Random.Range(value.x, value.y);
		}
		else if (!(Time.time < _nextActivateTime))
		{
			_nextActivateTime = Time.time + Random.Range(value.x, value.y);
			CreateAbilityInstance(AbilityTrigger.PredictPoint_Simple(null, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), victim, 1f), null, new CastInfo(victim), (Ai_Curse_DreamAfflictionInductive_Smite ai) =>
			{
				ai.dmgFactor = (ScalingValue)GetValue(damageHpRatio);
				ai.multiplyDamageByMaxHp = true;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
