using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Sirenix.Utilities;
using UnityEngine;

[SaveActor(true)]
public class StarEffect : StatusEffect
{
	public StarType type;

	public int requiredLevel;

	public Sprite starIcon;

	public int maxStarLevel = 1;

	public int[] price = new int[1];

	[CompilerGenerated]
	[SyncVar]
	private float strength__BackingField = 1f;

	public virtual Type heroType => null;

	public virtual Type skillType => null;

	public virtual bool isMovementSkillType => false;

	public Hero hero => victim as Hero;

	public DewPlayer player
	{
		get
		{
			if (!((UnityEngine.Object)(object)victim == null))
			{
				return victim.owner;
			}
			return null;
		}
	}

	public SkillTrigger skill
	{
		get
		{
			if ((UnityEngine.Object)(object)hero == null)
			{
				return null;
			}
			if (skillType != null)
			{
				return hero.Ability.abilities.Values.FirstOrDefault((AbilityTrigger t) => skillType.IsInstanceOfType(t)) as SkillTrigger;
			}
			if (isMovementSkillType)
			{
				return hero.Skill.Movement;
			}
			return null;
		}
	}

	public SkillTrigger[] skills
	{
		get
		{
			if ((UnityEngine.Object)(object)hero == null)
			{
				return Array.Empty<SkillTrigger>();
			}
			if (skillType != null)
			{
				return LinqExtensions.FilterCast<SkillTrigger>((IEnumerable)hero.Ability.abilities.Values.Where((AbilityTrigger t) => skillType.IsInstanceOfType(t))).ToArray();
			}
			if (isMovementSkillType && (UnityEngine.Object)(object)hero.Skill.Movement != null)
			{
				return new SkillTrigger[1] { hero.Skill.Movement };
			}
			return Array.Empty<SkillTrigger>();
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public float strength
	{
		[CompilerGenerated]
		get
		{
			return strength__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cstrength_003Ek__BackingField = value;
		}
	}

	public float Network_003Cstrength_003Ek__BackingField
	{
		get
		{
			return strength__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref strength__BackingField, 4096uL, (Action<float, float>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		parentActor = victim;
	}

	public bool IsIncompatibleWith(Hero h, HeroLoadoutData l)
	{
		HeroSkill component = ((Component)(object)h).GetComponent<HeroSkill>();
		SkillTrigger[] loadoutSkills = component.GetLoadoutSkills(HeroSkillLocation.Q);
		SkillTrigger[] loadoutSkills2 = component.GetLoadoutSkills(HeroSkillLocation.R);
		SkillTrigger[] loadoutSkills3 = component.GetLoadoutSkills(HeroSkillLocation.Identity);
		if (skillType != null && (l.skillQ < 0 || l.skillQ >= loadoutSkills.Length || ((object)loadoutSkills[l.skillQ]).GetType() != skillType) && (l.skillR < 0 || l.skillR >= loadoutSkills2.Length || ((object)loadoutSkills2[l.skillR]).GetType() != skillType))
		{
			if (l.skillTrait >= 0 && l.skillTrait < loadoutSkills3.Length)
			{
				return ((object)loadoutSkills3[l.skillTrait]).GetType() != skillType;
			}
			return true;
		}
		return false;
	}

	public bool IsSkillRelated(SkillTrigger s)
	{
		if (!isMovementSkillType || !((object)s).GetType().Name.Contains("_M_"))
		{
			if (skillType != null)
			{
				return ((object)s).GetType() == skillType;
			}
			return false;
		}
		return true;
	}

	public bool IsSkillRelated(string s)
	{
		if (!isMovementSkillType || !s.Contains("_M_"))
		{
			if (skillType != null)
			{
				return s == skillType.Name;
			}
			return false;
		}
		return true;
	}

	public SafeAction DoSkill(Func<SkillTrigger, Action> callback)
	{
		return DoAbility((AbilityTrigger at) =>
		{
			if (isMovementSkillType && (UnityEngine.Object)(object)at == (UnityEngine.Object)(object)hero.Skill.Movement)
			{
				return true;
			}
			return skillType != null && skillType.IsInstanceOfType(at);
		}, (AbilityTrigger at) => callback((SkillTrigger)at));
	}

	public SafeAction DoSkill(Action<SkillTrigger> callback)
	{
		return DoAbility((AbilityTrigger at) =>
		{
			if (isMovementSkillType && (UnityEngine.Object)(object)at == (UnityEngine.Object)(object)hero.Skill.Movement)
			{
				return true;
			}
			return skillType != null && skillType.IsInstanceOfType(at);
		}, (AbilityTrigger at) =>
		{
			callback((SkillTrigger)at);
			return (Action)null;
		});
	}

	[Server]
	public SafeAction DoSkillBonusAll(SkillBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SafeAction StarEffect::DoSkillBonusAll(SkillBonus)' called when server was not active");
			return null;
		}
		if (bonus == null)
		{
			bonus = new SkillBonus();
		}
		return DoSkill((SkillTrigger s) =>
		{
			SkillBonus newBonus = bonus.Clone();
			s.AddSkillBonus(newBonus);
			return () =>
			{
				newBonus.Stop();
			};
		});
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, strength__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, strength__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref strength__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref strength__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
