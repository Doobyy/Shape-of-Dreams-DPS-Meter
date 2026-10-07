using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Mon_LavaLand_InfernusPillar : Monster
{
	public NavMeshObstacle obstacle;

	public float projectileInterval;

	private float _lastProjectileTime;

	private Mon_LavaLand_BossInfernus _infernus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			AddData(new LavaLand_Lava.Ad_Lava
			{
				isImmune = true
			});
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity, "PillarUnstoppable");
			((Behaviour)(object)obstacle).enabled = true;
			_infernus = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !Visual.isSpawning)
		{
			if (_infernus.IsNullOrInactive())
			{
				_infernus = Dew.FindActorOfType<Mon_LavaLand_BossInfernus>();
			}
			if (Time.time - _lastProjectileTime > projectileInterval && !_infernus.IsNullOrInactive())
			{
				_lastProjectileTime = Time.time;
				CreateAbilityInstance<Ai_Mon_LavaLand_InfernusPillar_Projectile>(position, Quaternion.identity, new CastInfo(this, _infernus));
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		((Behaviour)(object)obstacle).enabled = false;
	}

	private void MirrorProcessed()
	{
	}
}
