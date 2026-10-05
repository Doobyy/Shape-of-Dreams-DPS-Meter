using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Primus_BossPrimusAeron_Adapt_Doom_Ongoing : StatusEffect
{
	public float initDelay = 2f;

	public int meteorCount = 4;

	public float meteorInterval = 3f;

	public float postDelay = 2f;

	public float postDaze = 2f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DoInvulnerable();
		DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false);
		yield return new SI.WaitForSeconds(initDelay);
		for (int i = 0; i < meteorCount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				Vector3 randomPathablePosition = SingletonBehaviour<Room_BossArena>.instance.GetRandomPathablePosition();
				CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_Meteor>(randomPathablePosition, Quaternion.Euler(0f, ManagerBase<CameraManager>.instance.entityCamAngle + 120f, 0f), new CastInfo(info.caster));
				yield return new SI.WaitForSeconds(0.25f);
			}
			if (meteorCount != i - 1)
			{
				yield return new SI.WaitForSeconds(meteorInterval);
			}
		}
		Actor[] array = children.ToArray();
		foreach (Actor actor in array)
		{
			if (actor is Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_PyranasFireball)
			{
				actor.Destroy();
			}
		}
		yield return new SI.WaitForSeconds(postDelay);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false, postDaze);
		info.caster.Control.StartDaze(postDaze);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((Object)(object)info.caster != null)
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
		Actor[] array = children.ToArray();
		foreach (Actor actor in array)
		{
			if (actor is Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_PyranasFireball)
			{
				actor.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
