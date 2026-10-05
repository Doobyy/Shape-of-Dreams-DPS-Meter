using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Curse_DarkUrge : CurseStatusEffect
{
	public float[] damageMultipliers;

	public float lowHpRatio;

	public float lowHpMultiplier;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)victim.owner == null))
		{
			victim.dealtDamageProcessor.Add(DamageBetweenPlayersProcessor);
			victim.takenDamageProcessor.Add(DamageBetweenPlayersProcessor);
			DewPlayer.onGamePlayerAdded += new Action<DewPlayer>(UpdateRelations);
			DewPlayer.onGamePlayerRemoved += new Action<DewPlayer>(UpdateRelations);
			yield return new SI.WaitForSeconds(3f);
			UpdateRelations();
		}
	}

	private void UpdateRelations(DewPlayer _)
	{
		UpdateRelations();
	}

	private void UpdateRelations()
	{
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			gamePlayer.PurgeRelationsList();
			if (!((UnityEngine.Object)(object)gamePlayer == (UnityEngine.Object)(object)victim.owner))
			{
				if (!victim.owner.neutrals.Contains(gamePlayer))
				{
					victim.owner.neutrals.Add(gamePlayer);
				}
				if (!gamePlayer.neutrals.Contains(victim.owner))
				{
					gamePlayer.neutrals.Add(victim.owner);
				}
			}
		}
	}

	private void DamageBetweenPlayersProcessor(ref DamageData data, Actor actor, Entity to)
	{
		if (data.IsAmountModifiedBy(this))
		{
			return;
		}
		Hero hero = actor.FindFirstOfType<Hero>();
		if (!((UnityEngine.Object)(object)hero == null) && !((UnityEngine.Object)(object)to == null) && !((UnityEngine.Object)(object)hero == (UnityEngine.Object)(object)to) && !((UnityEngine.Object)(object)hero.owner == null) && !((UnityEngine.Object)(object)to.owner == null) && hero.owner.isHumanPlayer && to.owner.isHumanPlayer && !((UnityEngine.Object)(object)hero.owner == (UnityEngine.Object)(object)to.owner))
		{
			float num = GetValue(damageMultipliers);
			if (to.normalizedHealth < lowHpRatio)
			{
				num *= lowHpMultiplier;
			}
			data.ApplyRawMultiplier(num);
			data.SetAmountModifiedBy(this);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DewPlayer.onGamePlayerAdded -= new Action<DewPlayer>(UpdateRelations);
		DewPlayer.onGamePlayerRemoved -= new Action<DewPlayer>(UpdateRelations);
		if (!((UnityEngine.Object)(object)victim != null))
		{
			return;
		}
		victim.dealtDamageProcessor.Remove(DamageBetweenPlayersProcessor);
		victim.takenDamageProcessor.Remove(DamageBetweenPlayersProcessor);
		if (!((UnityEngine.Object)(object)victim.owner != null))
		{
			return;
		}
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			victim.owner.neutrals.Remove(gamePlayer);
			gamePlayer.neutrals.Remove(victim.owner);
		}
	}

	public override bool IsViable(Entity target)
	{
		if (DewPlayer.gamePlayers.Count((DewPlayer h) => !h.hero.IsNullInactiveDeadOrKnockedOut()) > 1)
		{
			return (UnityEngine.Object)(object)Dew.FindActorOfType<LucidDream_TheDarkestUrge>() == null;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
