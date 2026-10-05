using System.Collections;
using Mirror;
using UnityEngine;

public class TickDamageInstance : DamageInstance
{
	public float delay = 0.05f;

	public float tickInterval = 0.2f;

	public int ticks = 20;

	public GameObject fxTelegraph;

	public GameObject fxLoop;

	public GameObject fxPerTick;

	private float _lastTickTime;

	private int _tickMs = 1;

	private int _cooldownMs = 1;

	public float duration => (float)ticks * tickInterval + delay;

	public float normalizedDuration => (Time.time - creationTime) / duration;

	public int doneTicks { get; private set; }

	public float normalizedTicks => doneTicks / ticks;

	protected override float DamageTime => doneTicks * _tickMs;

	protected override float DuplicateCooldown => _cooldownMs;

	protected override float GetDuplicateStamp(bool hadPrevious, float previousStamp)
	{
		if (!hadPrevious)
		{
			return DamageTime;
		}
		float num = previousStamp + (float)_cooldownMs;
		if (DamageTime - num >= (float)_cooldownMs)
		{
			num = DamageTime;
		}
		return num;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			doneTicks = 0;
			_lastTickTime = Time.time + delay - tickInterval - 0.001f;
			_tickMs = Mathf.Max(1, Mathf.RoundToInt(tickInterval * 1000f));
			_cooldownMs = ((duplicateCheck == DuplicateCheckType.CooldownPerInstance && cooldownTime > 0f) ? Mathf.Max(1, Mathf.RoundToInt(cooldownTime * 1000f)) : _tickMs);
			FxPlayNetworked(fxTelegraph);
			if (delay > 0f)
			{
				yield return new SI.WaitForSeconds(delay);
			}
			FxPlayNetworked(fxLoop);
			FxStopNetworked(fxTelegraph);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
			FxStopNetworked(fxLoop);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			return;
		}
		while (doneTicks < ticks && Time.time - _lastTickTime >= tickInterval)
		{
			_lastTickTime += tickInterval;
			DoCollisionChecks();
			FxPlayNewNetworked(fxPerTick);
			doneTicks++;
		}
		if (doneTicks >= ticks && destroyWhenDone)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
