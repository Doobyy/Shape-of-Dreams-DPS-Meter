using Mirror;
using UnityEngine;

public class At_Mon_Special_BossErebos_SpawnTormentor : AbilityTrigger
{
	public GameObject fxSpawnAttacker;

	public GameObject fxSpawnSlow;

	private bool _isSlowSpawn = true;

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		_isSlowSpawn = Random.value > 0.5f;
		TriggerConfig triggerConfig = configs[configIndex];
		if (!_isSlowSpawn)
		{
			triggerConfig.effectOnCast = fxSpawnAttacker;
		}
		else
		{
			triggerConfig.effectOnCast = fxSpawnSlow;
		}
		base.OnCastStart(configIndex, info);
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		if (((NetworkBehaviour)this).isServer && cast.instance is Ai_Mon_Special_BossErebos_SpawnTormentor_Spawner ai_Mon_Special_BossErebos_SpawnTormentor_Spawner)
		{
			ai_Mon_Special_BossErebos_SpawnTormentor_Spawner.isSlowSpawn = _isSlowSpawn;
		}
	}

	private void MirrorProcessed()
	{
	}
}
