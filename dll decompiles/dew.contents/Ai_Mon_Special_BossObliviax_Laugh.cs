using System.Collections;
using Mirror;

public class Ai_Mon_Special_BossObliviax_Laugh : AbilityInstance
{
	public float duration;

	public DewAnimationClip clip;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDaze(duration);
			info.caster.Animation.PlayAbilityAnimation(clip);
			yield return new SI.WaitForSeconds(duration);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
