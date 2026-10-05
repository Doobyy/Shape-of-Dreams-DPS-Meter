using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_LittleBaam_Reposition : AbilityInstance
{
	public float dazeDuration;

	public Vector2 distanceFromTargetPoint;

	public float randomMagnitude;

	public Vector2 delay;

	public float landYOffset;

	public float landYVelocity;

	public float landEffectDelay;

	public GameObject landEffect;

	private bool _renderersDisabled;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_renderersDisabled = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _renderersDisabled)
		{
			_renderersDisabled = false;
			if ((Object)(object)info.caster != null)
			{
				info.caster.Visual.EnableRenderers();
			}
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartDaze(dazeDuration);
			Vector2 normalized = (info.point - info.caster.position).ToXY().normalized;
			Vector3 dest = Dew.GetValidAgentDestination_LinearSweep(end: info.point + normalized.ToXZ() * Random.Range(distanceFromTargetPoint.x, distanceFromTargetPoint.y) + Random.insideUnitSphere * randomMagnitude, start: info.caster.agentPosition);
			info.caster.Visual.DisableRenderers();
			_renderersDisabled = true;
			yield return new SI.WaitForSeconds(Random.Range(delay.x, delay.y));
			Teleport(info.caster, dest);
			info.caster.Control.RotateTowards(info.point, immediately: true);
			info.caster.Visual.EnableRenderers();
			_renderersDisabled = false;
			info.caster.Visual.SetYOffset(landYOffset);
			info.caster.Visual.SetYVelocity(landYVelocity);
			yield return new SI.WaitForSeconds(landEffectDelay);
			FxPlayNewNetworked(landEffect, Dew.GetPositionOnGround(dest), Quaternion.identity);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
