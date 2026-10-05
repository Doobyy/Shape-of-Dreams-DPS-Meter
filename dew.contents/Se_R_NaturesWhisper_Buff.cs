using Mirror;

public class Se_R_NaturesWhisper_Buff : StatusEffect
{
	public ScalingValue hasteAmount;

	public ScalingValue speedAmount;

	public float buffDuration = 3f;

	private ScalingValue _baseHasteAmount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseHasteAmount = hasteAmount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		hasteAmount = _baseHasteAmount;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ShowOnScreenTimer();
			DoHaste(GetValue(hasteAmount));
			DoSpeed(GetValue(speedAmount));
			SetTimer(buffDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
