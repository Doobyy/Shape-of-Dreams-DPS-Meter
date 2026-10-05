using Mirror;

public class Se_ArcticTerritory : StatusEffect
{
	public float duration;

	public float slowAmount;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoSlow(slowAmount);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			ApplyElemental(ElementalType.Cold, victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
