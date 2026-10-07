public class Ai_M_FeatheryDash : Ai_GenericDodge
{
	private float _baseSpeed;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseSpeed = speed;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		speed = _baseSpeed;
	}

	private void MirrorProcessed()
	{
	}
}
