using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_NightOlm_Atk : AbilityInstance
{
	public int spawnCount;

	public float spawnInterval;

	public float postDaze;

	public bool cancelable;

	public Vector2 angleRange;

	public float angleSpeed;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			float num = (float)spawnCount * spawnInterval;
			if (!cancelable)
			{
				CreateBasicEffect(info.caster, new UnstoppableEffect(), num + postDaze);
			}
			info.caster.Control.StartChannel(new Channel
			{
				duration = num + postDaze,
				blockedActions = Channel.BlockedAction.Everything,
				onCancel = () =>
				{
					info.caster.Animation.StopAbilityAnimation();
					Destroy();
				},
				onComplete = () =>
				{
					info.caster.Animation.StopAbilityAnimation();
					Destroy();
				}
			});
			for (int i = 0; i < spawnCount; i++)
			{
				float x = angleRange.x;
				float y = angleRange.y;
				float num2 = Mathf.Lerp(x, y, (Mathf.Sin(Time.time * angleSpeed) + 1f) * 0.5f);
				CreateAbilityInstance<Ai_Mon_DarkCave_NightOlm_Atk_Projectile>(info.caster.position, Quaternion.identity, new CastInfo(info.caster, num2 + info.angle));
				yield return new SI.WaitForSeconds(spawnInterval);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
