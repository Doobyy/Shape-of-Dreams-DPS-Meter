using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Atk : AbilityInstance
{
	public ChannelData firstAtkChannel;

	public GameObject fxTelegraphFirst;

	public ChannelData secondAtkChannel;

	public GameObject fxTelegraphSecond;

	public float postDelay;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraphFirst, info.caster);
		firstAtkChannel.Get().AddOnComplete(() =>
		{
			CreateAbilityInstance<Ai_Mon_Special_BossMaw_Atk_First>(position, rotation, new CastInfo(info.caster, info.angle));
			FxStopNetworked(fxTelegraphFirst);
			FxPlayNetworked(fxTelegraphSecond, info.caster);
			secondAtkChannel.Get().AddOnComplete(() =>
			{
				CreateAbilityInstance<Ai_Mon_Special_BossMaw_Atk_Second>(position, rotation, new CastInfo(info.caster, info.angle));
				info.caster.Control.StartDaze(postDelay);
				Destroy();
			}).AddOnCancel(Destroy)
				.Dispatch(info.caster);
		}).AddOnCancel(Destroy)
			.Dispatch(info.caster);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraphFirst);
			FxStopNetworked(fxTelegraphSecond);
		}
	}

	private void MirrorProcessed()
	{
	}
}
