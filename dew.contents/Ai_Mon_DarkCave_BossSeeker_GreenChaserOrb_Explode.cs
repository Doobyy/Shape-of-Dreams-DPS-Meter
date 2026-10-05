public class Ai_Mon_DarkCave_BossSeeker_GreenChaserOrb_Explode : InstantDamageInstance
{
	public FxCameraShake shake;

	public DewAudioSource explodeAudio;

	protected override void OnCreate()
	{
		range.transform.localScale *= strengthMultiplier;
		if (shake != null)
		{
			shake.amplitude *= strengthMultiplier;
		}
		if (explodeAudio != null)
		{
			explodeAudio.pitchMultiplier *= 2f - strengthMultiplier;
			explodeAudio.volumeMultiplier *= strengthMultiplier;
		}
		startEffectNoStop.transform.localScale *= strengthMultiplier;
		InvalidateInstance();
		base.OnCreate();
	}

	private void MirrorProcessed()
	{
	}
}
