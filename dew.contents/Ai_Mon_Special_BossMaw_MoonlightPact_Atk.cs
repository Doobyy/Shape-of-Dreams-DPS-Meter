public class Ai_Mon_Special_BossMaw_MoonlightPact_Atk : InstantDamageInstance
{
	protected override void OnCreate()
	{
		if (startEffectNoStop != null && DewAnimationClip.GetEntryIndex(info.animSelectValue, 2) == 1)
		{
			startEffectNoStop.transform.localScale = startEffectNoStop.transform.localScale.WithX(-1f);
		}
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
