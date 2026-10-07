using Mirror;

public class Se_Curse_AmplifiedPain_AtkSpdReduction : StatusEffect
{
	public float[] duration;

	public float reductionAmount = 40f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoCripple(reductionAmount);
			SetTimer(GetValue(duration));
			ShowOnScreenTimer("Se_Curse_AmplifiedPain");
		}
	}

	private void MirrorProcessed()
	{
	}
}
