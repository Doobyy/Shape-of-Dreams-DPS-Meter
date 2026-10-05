using System.Collections.Generic;
using Mirror;

public class Ai_Mon_SnowMountain_VikingWarrior_Atk : InstantDamageInstance
{
	public float stunDuration;

	private TriggerConfig _config;

	private float _postDelay;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			AbilityTrigger abilityTrigger = firstTrigger;
			_config = abilityTrigger.currentConfig;
			_postDelay = _config.postDelay;
		}
	}

	protected override void OnHit(Entity entity)
	{
		_config.postDelay = 0.5f;
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "viking_stun");
		foreach (KeyValuePair<int, AbilityTrigger> ability in info.caster.Ability.abilities)
		{
			if (ability.Key != info.caster.Ability.attackAbility.abilityIndex)
			{
				ResetCooldown(ability.Value);
			}
		}
		base.OnHit(entity);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _config != null)
		{
			_config.postDelay = _postDelay;
		}
	}

	private void MirrorProcessed()
	{
	}
}
