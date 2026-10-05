using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_Roar : AbilityInstance
{
	public int count;

	public float delay;

	public float postDelay;

	public DewAnimationClip animCast;

	public DewAnimationClip animEnd;

	private Channel _channel;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		info.caster.Animation.PlayAbilityAnimation(animCast);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = delay,
			onCancel = DestroyIfActive,
			onComplete = () =>
			{
				info.caster.Control.StartDaze(postDelay);
				info.caster.Animation.PlayAbilityAnimation(animEnd);
				float num = Random.Range(0f, 360f);
				int num2 = 360 / count;
				for (int i = 0; i < count; i++)
				{
					float y = num + (float)(num2 * i);
					Quaternion value = Quaternion.Euler(0f, y, 0f);
					CreateAbilityInstance<Ai_Mon_LavaLand_BossInfernus_Roar_Projectile>(info.caster.agentPosition, value, new CastInfo(info.caster, CastInfo.GetAngle(value)));
				}
				DestroyIfActive();
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
