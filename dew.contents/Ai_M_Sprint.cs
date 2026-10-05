using Mirror;

public class Ai_M_Sprint : Ai_GenericDodge
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (info.caster.Status.TryGetStatusEffect<Se_M_Sprint>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_M_Sprint>(info.caster);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		minDistance = 2f;
	}

	private void MirrorProcessed()
	{
	}
}
