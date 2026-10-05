using System;
using Mirror;
using UnityEngine;

public class Mon_DarkCave_CaveSpider : Mon_Forest_SpiderWarrior
{
	[NonSerialized]
	public GameObject fxRockObject;

	public GameObject fxArmorBroken;

	public GameObject fxHit;

	public float fxHitPlayInterval;

	public float armorAmount;

	public float armorBreakHealthThreshold;

	private StatBonus _armorBonus;

	private float _lastHitTime;

	private bool _enablePlayFxHit = true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_enablePlayFxHit = true;
			_armorBonus = new StatBonus
			{
				armorFlat = armorAmount
			};
			Status.AddStatBonus(_armorBonus);
			EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		fxRockObject = Visual.model.GetCustomMapping<GameObject>("fxRockObject");
		FxPlay(fxRockObject, this);
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (_enablePlayFxHit)
		{
			if (Time.time - _lastHitTime > fxHitPlayInterval)
			{
				FxPlayNetworked(fxHit, this);
				_lastHitTime = Time.time;
			}
			if (!(Status.normalizedHealth > armorBreakHealthThreshold))
			{
				FxPlayNetworked(fxArmorBroken, this);
				FxStopNetworked(fxRockObject);
				Status.RemoveStatBonus(_armorBonus);
				Stagger(obj.damage.direction);
				_enablePlayFxHit = false;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
			FxStopNetworked(fxRockObject);
		}
	}

	protected override Monster GetMiniBossSpawnedMonster()
	{
		return DewResources.GetByType<Mon_DarkCave_CaveBat>(default(ResourceLoadSettings));
	}

	public override void LoadEntityModelLocal()
	{
		if (DewSave.profileMain.gameplay.enableArachnophobia)
		{
			Visual.LoadModelLocal(null);
		}
		else
		{
			base.LoadEntityModelLocal();
		}
	}

	private void MirrorProcessed()
	{
	}
}
