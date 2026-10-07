using Mirror;

public class Se_HunterBuff_ShadowWalk_Disappear : StatusEffect
{
	public float duration;

	private float _prefabDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_prefabDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		duration = _prefabDuration;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Visual.DisableRenderers();
			DoUnstoppable();
			SetTimer(duration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
