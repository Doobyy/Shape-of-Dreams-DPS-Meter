using Mirror;

public class Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack_SubInstance : InstantDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Force);
		}
	}

	private void MirrorProcessed()
	{
	}
}
