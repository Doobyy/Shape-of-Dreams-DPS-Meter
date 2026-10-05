using Mirror;
using UnityEngine;

public class Se_R_Deception : StatusEffect
{
	public ScalingValue speedAmount;

	public ScalingValue stealthDuration;

	public float addedStealthTime;

	private float _addedStealthTimeDefault;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_addedStealthTimeDefault = addedStealthTime;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		addedStealthTime = _addedStealthTimeDefault;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Se_GenericStealth actor = ApplyStealth(victim, GetValue(stealthDuration) + addedStealthTime);
			DestroyOnDestroy(actor);
			DoSpeed(GetValue(speedAmount));
			Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(victim.agentPosition, info.point);
			Teleport(victim, validAgentDestination_Closest);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			CreateAbilityInstance<Ai_R_Deception_EndExplosion>(victim.agentPosition, null, new CastInfo(info.caster));
		}
	}

	private void MirrorProcessed()
	{
	}
}
