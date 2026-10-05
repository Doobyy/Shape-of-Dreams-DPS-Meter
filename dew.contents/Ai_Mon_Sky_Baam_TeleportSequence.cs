using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_Baam_TeleportSequence : AbilityInstance
{
	public float postDelay;

	public DewAnimationClip startAnimation;

	public GameObject telegraph;

	public ChannelData channel;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		Ai_Mon_Sky_Baam_Teleport byType = DewResources.GetByType<Ai_Mon_Sky_Baam_Teleport>(default(ResourceLoadSettings));
		Vector3 end = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, channel.duration + byType.appearDuration);
		Vector3 dest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
		channel.Get().AddOnCancel(DestroyIfActive).AddOnComplete(() =>
		{
			CreateAbilityInstance(info.caster.agentPosition, info.caster.rotation, info, (Ai_Mon_Sky_Baam_Teleport s) =>
			{
				s.targetPos = dest;
			});
			info.caster.Control.StartDaze(postDelay);
			info.caster.Animation.StopAbilityAnimation(startAnimation);
			DestroyIfActive();
		})
			.Dispatch(info.caster);
		info.caster.Animation.PlayAbilityAnimation(startAnimation);
		FxPlayNetworked(telegraph, end, null);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (info.caster.Status.currentHealth <= 0.1f || info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			FxStop(telegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
