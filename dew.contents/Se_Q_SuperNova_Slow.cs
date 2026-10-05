using Mirror;

public class Se_Q_SuperNova_Slow : StatusEffect
{
	public ScalingValue slowAmount;

	public float duration;

	private ScalingValue _baseSlowAmount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseSlowAmount = slowAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		slowAmount = _baseSlowAmount;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(GetValue(slowAmount));
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
