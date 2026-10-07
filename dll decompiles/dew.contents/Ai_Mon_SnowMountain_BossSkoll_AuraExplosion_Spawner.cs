using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_AuraExplosion_Spawner : InstantDamageInstance
{
	public int afterAtkCount;

	public float distEachAtk;

	public float firstDelay;

	public float interval;

	public float endAnimDelay;

	public float postDelay;

	public GameObject fxTelegraph;

	public GameObject fxCast;

	public DewAnimationClip clip;

	private bool _blocksIncremented;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxCast, info.caster);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_blocksIncremented = false;
	}

	protected override void OnAfterDelay()
	{
		base.OnAfterDelay();
		DestroyOnDeath(info.caster);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			_blocksIncremented = true;
			yield return new WaitForSeconds(firstDelay);
			for (int i = 0; i < afterAtkCount; i++)
			{
				Vector3 vector = ((Component)(object)info.caster).transform.right;
				for (int j = 0; j < 2; j++)
				{
					if (j == 1)
					{
						vector = -vector;
					}
					Vector3 point = info.caster.agentPosition + vector * (distEachAtk * (float)(i + 1));
					FxPlayNewNetworked(fxTelegraph, point, rotation);
					CreateAbilityInstance<Ai_Mon_SnowMountain_BossSkoll_AuraExplosion_AfterAtk>(point, rotation, new CastInfo(info.caster, point));
				}
				yield return new WaitForSeconds(interval);
			}
			yield return new WaitForSeconds(endAnimDelay);
			info.caster.Animation.PlayAbilityAnimation(clip);
			yield return new WaitForSeconds(0.05f);
			DestroyIfActive();
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_blocksIncremented && (Object)(object)info.caster != null)
			{
				_blocksIncremented = false;
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
			if (!info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				info.caster.Control.StartDaze(postDelay);
			}
			FxStopNetworked(fxCast);
		}
	}

	private void MirrorProcessed()
	{
	}
}
