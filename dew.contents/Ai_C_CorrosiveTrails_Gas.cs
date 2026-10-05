using Mirror;
using UnityEngine;

public class Ai_C_CorrosiveTrails_Gas : AbilityInstance
{
	public float lingerDuration;

	public float tickInterval;

	public DewCollider range;

	private float _lastTickTime;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastTickTime = 0f;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (Time.time - creationTime > lingerDuration)
		{
			Destroy();
		}
		else
		{
			if (!(Time.time - _lastTickTime > tickInterval))
			{
				return;
			}
			_lastTickTime = Time.time;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				Se_C_CorrosiveTrails_Poisoned se_C_CorrosiveTrails_Poisoned = entity.Status.FindStatusEffect((Se_C_CorrosiveTrails_Poisoned se) => (Object)(object)se.parentActor.parentActor == (Object)(object)parentActor);
				if ((Object)(object)se_C_CorrosiveTrails_Poisoned != null)
				{
					se_C_CorrosiveTrails_Poisoned.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_C_CorrosiveTrails_Poisoned>(entity);
				}
			}
			handle.Return();
		}
	}

	private void MirrorProcessed()
	{
	}
}
