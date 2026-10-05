using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_AltSkill_LineAtk_Spawner : AbilityInstance
{
	public int waves;

	public int perWaveCount;

	public float startDelay;

	public float waveInterval;

	public float postDelay;

	public DewAnimationClip eachWaveAnim;

	public GameObject fxSpawn;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		yield return new SI.WaitForSeconds(startDelay);
		float delay = DewResources.GetByType<Ai_Mon_Forest_BossDemon_AltSkill_LineAtk>(default(ResourceLoadSettings)).damageDelay;
		RoomSection section = info.caster.section;
		if (section == null)
		{
			section = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
		}
		if (section == null)
		{
			Destroy();
			yield break;
		}
		for (int i = 0; i < waves; i++)
		{
			info.caster.Animation.PlayAbilityAnimation(eachWaveAnim);
			FxPlayNewNetworked(fxSpawn, info.caster);
			for (int j = 0; j < perWaveCount; j++)
			{
				Vector3 vector;
				if (j < DewPlayer.gamePlayers.Count && !DewPlayer.gamePlayers[j].hero.isKnockedOut)
				{
					vector = DewPlayer.gamePlayers[j].hero.GetAIPosition(info.caster);
				}
				else
				{
					vector = ((j < perWaveCount - DewPlayer.gamePlayers.Count || DewPlayer.gamePlayers[j - perWaveCount + DewPlayer.gamePlayers.Count].hero.isKnockedOut) ? (section.GetAnyRandomNode() + Random.insideUnitSphere.Flattened() * 1.5f) : AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.7f, 1f), DewPlayer.gamePlayers[j - perWaveCount + DewPlayer.gamePlayers.Count].hero, delay));
				}
				vector = Dew.GetPositionOnGround(vector);
				int num = ((Random.Range(0, 2) == 0) ? 90 : (-90));
				int num2 = (((float)j < (float)perWaveCount * 0.5f) ? num : 0);
				CreateAbilityInstance<Ai_Mon_Forest_BossDemon_AltSkill_LineAtk>(vector, info.caster.rotation * Quaternion.Euler(0f, num2, 0f), new CastInfo(info.caster, vector));
			}
			yield return new SI.WaitForSeconds(waveInterval);
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	private void MirrorProcessed()
	{
	}
}
