using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_AS_NoShockwaveFasterAttack : StarEffect
{
	public float damageReduction = 0.6f;

	public float travelDistanceReduction = 0.8f;

	public int dodgeAddedCharges = 2;

	public float dodgeMemoryHaste = 30f;

	public float dodgeMemoryHastePerBossKill = 30f;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public float bonusHaste;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_R_AnnihilationStance);

	public float NetworkbonusHaste
	{
		get
		{
			return bonusHaste;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref bonusHaste, 8192uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (obj.victim.IsAnyBoss())
		{
			NetworkbonusHaste = bonusHaste + dodgeMemoryHastePerBossKill;
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_AnnihilationStance se_R_AnnihilationStance)
		{
			float haste = dodgeMemoryHaste + bonusHaste;
			se_R_AnnihilationStance.DoAbility((AbilityTrigger trg) => trg is SkillTrigger && trg.abilityIndex == 5, (AbilityTrigger trg) =>
			{
				SkillTrigger st = trg as SkillTrigger;
				if (st == null)
				{
					return (Action)null;
				}
				SkillBonus bonus = st.AddSkillBonus(new SkillBonus
				{
					addedCharge = dodgeAddedCharges,
					cooldownMultiplier = 1f / (1f + haste * 0.01f),
					ignoreReceiveCooldownReductionFlag = true
				});
				return () =>
				{
					st.RemoveSkillBonus(bonus);
				};
			});
			Dew.CallDelayed(() =>
			{
				if (victim.Ability.originalAttackAbility is At_Atk_HuskSword at_Atk_HuskSword)
				{
					at_Atk_HuskSword.ignoreRangeCheck = false;
				}
			});
		}
		if (obj.instance is Ai_R_AnnihilationStance_Projectile ai_R_AnnihilationStance_Projectile)
		{
			ai_R_AnnihilationStance_Projectile.damage *= 1f - damageReduction;
			ai_R_AnnihilationStance_Projectile.endDistance *= 1f - travelDistanceReduction;
			ai_R_AnnihilationStance_Projectile.initialSpeed *= 0.35f;
			ai_R_AnnihilationStance_Projectile.targetSpeed *= 0.35f;
			ai_R_AnnihilationStance_Projectile.acceleration *= 0.35f;
			ai_R_AnnihilationStance_Projectile.startInFrontDistance = 0f;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, bonusHaste);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, bonusHaste);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref bonusHaste, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref bonusHaste, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
