using Mirror;

public class Se_M_Sprint : StatusEffect
{
	public float duration = 1f;

	public float speedAmount = 50f;

	public bool isSpeedDecay = true;

	public float hasteAmount = 150f;

	public bool isHasteDecay = true;

	private float _baseSpeedAmount;

	private float _baseHasteAmount;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseSpeedAmount = speedAmount;
		_baseHasteAmount = hasteAmount;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			ShowOnScreenTimer();
			DoSpeed(speedAmount).decay = isSpeedDecay;
			DoHaste(hasteAmount).decay = isHasteDecay;
			ResetCooldown(info.caster.Ability.attackAbility);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		speedAmount = _baseSpeedAmount;
		hasteAmount = _baseHasteAmount;
	}

	private void MirrorProcessed()
	{
	}
}
