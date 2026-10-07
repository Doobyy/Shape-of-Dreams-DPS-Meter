using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Rage_Atk : AbilityInstance
{
	public ChannelData firstAtkChannel;

	public GameObject fxTelegraphFirst;

	public ChannelData secondAtkChannel;

	public GameObject fxTelegraphSecond;

	public float postDaze = 0.4f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxPlayNetworked(fxTelegraphFirst, info.caster);
		firstAtkChannel.Get().AddOnComplete(() =>
		{
			CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Rage_Atk_FirstSwipe>(position, rotation, new CastInfo(info.caster, info.angle));
			FxStopNetworked(fxTelegraphFirst);
			FxPlayNetworked(fxTelegraphSecond, info.caster);
			secondAtkChannel.Get().AddOnComplete(() =>
			{
				CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Rage_Atk_SecondSwipe>(position, rotation, new CastInfo(info.caster, info.angle));
				info.caster.Control.StartDaze(postDaze);
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
