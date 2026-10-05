using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_NeedleAtk : AbilityInstance
{
	public float postDelay;

	public float startDelay;

	public GameObject fxTelegraph;

	public GameObject fxExplode;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraph, info.caster);
		info.caster.Animation.PlayAbilityAnimation(startAnim);
		info.caster.Control.StartDaze(startDelay);
		yield return new SI.WaitForSeconds(startDelay);
		FxPlayNetworked(fxExplode, info.caster);
		CreateAbilityInstance<Ai_Mon_Special_BossObliviax_NeedleAtk_AtkInstance>(info.caster.agentPosition, null, new CastInfo(info.caster));
		info.caster.Animation.StopAbilityAnimation(startAnim);
		info.caster.Animation.PlayAbilityAnimation(endAnim);
		info.caster.Control.StartDaze(postDelay);
		foreach (KeyValuePair<int, AbilityTrigger> ability in info.caster.Ability.abilities)
		{
			if (!(ability.Value is At_Mon_Special_BossObliviax_NeedleAtk) && !(ability.Value is At_Mon_Special_BossLightElemental_BeamAtk))
			{
				ResetCooldown(ability.Value);
			}
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
