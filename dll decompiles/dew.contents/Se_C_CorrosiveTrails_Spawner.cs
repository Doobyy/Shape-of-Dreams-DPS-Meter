using Mirror;
using UnityEngine;

public class Se_C_CorrosiveTrails_Spawner : StatusEffect
{
	public float duration;

	public ScalingValue speedAmount;

	public float minDistanceBetweenInstances;

	private float _lastSpawnGasTimer;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastSpawnGasTimer = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(GetValue(speedAmount));
			SetTimer(duration);
			ShowOnScreenTimer();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastSpawnGasTimer < 0.05f)
		{
			return;
		}
		foreach (Actor child in children)
		{
			if (child is Ai_C_CorrosiveTrails_Gas && Vector2.Distance(victim.agentPosition.ToXY(), child.position.ToXY()) < minDistanceBetweenInstances)
			{
				return;
			}
		}
		CreateAbilityInstance<Ai_C_CorrosiveTrails_Gas>(victim.agentPosition, null, new CastInfo(info.caster));
	}

	private void MirrorProcessed()
	{
	}
}
