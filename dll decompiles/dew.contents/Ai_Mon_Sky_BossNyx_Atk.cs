using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_Atk : AbilityInstance
{
	public GameObject fxExplode;

	public ChannelData channel;

	public float maxRange;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<Entity> list = new List<Entity>();
		list.Add(info.target);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && !list.Contains(gamePlayer.hero) && Vector2.Distance(gamePlayer.hero.GetAIPosition(info.caster).ToXY(), info.caster.position.ToXY()) < maxRange)
			{
				list.Add(gamePlayer.hero);
			}
		}
		float initDelay = DewResources.GetByType<Ai_Mon_Sky_BossNyx_PillarOfStars>(default(ResourceLoadSettings)).initDelay;
		foreach (Entity item in list)
		{
			Vector3 point = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), item, initDelay);
			CreateAbilityInstance<Ai_Mon_Sky_BossNyx_PillarOfStars>(point, null, new CastInfo(info.caster, point));
		}
		Channel channel = this.channel.Get();
		channel.duration = initDelay;
		channel.onCancel = (Action)Delegate.Combine(channel.onCancel, (Action)(() =>
		{
			Actor[] array = children.ToArray();
			foreach (Actor actor in array)
			{
				if (actor is Ai_Mon_Sky_BossNyx_PillarOfStars)
				{
					actor.DestroyIfActive();
				}
			}
			DestroyIfActive();
			if ((UnityEngine.Object)(object)firstTrigger != null)
			{
				firstTrigger.SetCooldownTime(0, 1f);
			}
		}));
		channel.onComplete = (Action)Delegate.Combine(channel.onComplete, (Action)(() =>
		{
			DewEffect.Play(fxExplode);
			DestroyIfActive();
		}));
		channel.Dispatch(info.caster);
	}

	private void MirrorProcessed()
	{
	}
}
