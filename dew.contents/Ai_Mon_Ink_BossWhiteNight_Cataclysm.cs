using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_Cataclysm : AbilityInstance
{
	public float startDelay;

	public float startTeleportDuration;

	public float startDaze;

	public float postDelay;

	public GameObject fxTeleportStart;

	public GameObject fxTeleportEnd;

	[Space(15f)]
	[Header("Cataclysm")]
	public float cCastDelay;

	public float cInterval;

	public float cPostDelay;

	public int cWaveCount;

	public Vector2 globalSafeZoneRange;

	public float heroSafeZoneDeviation;

	public float safeZoneRadius;

	public float safeZoneReduceRatio;

	public int safeZoneBaseCount;

	public DewAnimationClip cCastClip;

	public DewAnimationClip cStartClip;

	public DewAnimationClip cEndClip;

	public GameObject cFxStart;

	public GameObject cFxCharging;

	public GameObject cFxTelegraph;

	public GameObject cFxEnd;

	[Header("Eclipse")]
	[Space(15f)]
	public float eStartDelay;

	public float eCastDelay;

	public float eDuration;

	public float ePostDelay;

	public GameObject eFxStart;

	public GameObject eFxCharging;

	public GameObject eFxChargeEnd;

	public GameObject eFxEnd;

	public DewAnimationClip eStartClip;

	public DewAnimationClip eCastClip;

	public DewAnimationClip eLoopClip;

	public DewAnimationClip eEndClip;

	private bool _isRage;

	private bool _isSolo = true;

	private bool _isDarkMoonSpecialAtk;

	private Mon_Ink_BossDarkMoon _darkMoon;

	private Vector3 _centerPos;

	private Channel _channel;

	private Channel _darkMoonChannel;

	private List<Vector3> _safeZonePoints;

	private float _cIntervalPristine;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_cIntervalPristine = cInterval;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		cInterval = _cIntervalPristine;
		_isRage = false;
		_isSolo = true;
		_isDarkMoonSpecialAtk = false;
		_darkMoon = null;
		_channel = null;
		_darkMoonChannel = null;
		_safeZonePoints = null;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			if (!info.caster.Status.HasStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>())
			{
				CreateStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>(info.caster, new CastInfo(info.caster));
			}
			Mon_Ink_BossWhiteNight mon_Ink_BossWhiteNight = (Mon_Ink_BossWhiteNight)info.caster;
			_isRage = mon_Ink_BossWhiteNight._isRage;
			_isSolo = mon_Ink_BossWhiteNight._isSolo;
			_isDarkMoonSpecialAtk = mon_Ink_BossWhiteNight._isNextSpecialAttackDarkMoonMain;
			_darkMoon = mon_Ink_BossWhiteNight._bossDarkMoon;
			_safeZonePoints = new List<Vector3>();
			if (_isDarkMoonSpecialAtk && !_isSolo)
			{
				_darkMoon.Control.Stop();
				_darkMoon.Control.CancelOngoingChannels();
				_darkMoon.Control.CancelOngoingDisplacement();
				_darkMoon.Control.ClearActionQueue();
				_channel = info.caster.Control.StartChannel(new Channel
				{
					blockedActions = Channel.BlockedAction.Everything,
					duration = float.PositiveInfinity,
					isAttack = false,
					onCancel = DestroyIfActive,
					uncancellableTime = eDuration
				});
				_darkMoonChannel = _darkMoon.Control.StartChannel(new Channel
				{
					blockedActions = Channel.BlockedAction.Everything,
					duration = float.PositiveInfinity,
					isAttack = false,
					onCancel = DestroyIfActive,
					uncancellableTime = eDuration
				});
				yield return DarkMoonPattern();
			}
			else
			{
				_channel = info.caster.Control.StartChannel(new Channel
				{
					blockedActions = Channel.BlockedAction.Everything,
					duration = float.PositiveInfinity,
					isAttack = false,
					onCancel = DestroyIfActive,
					uncancellableTime = cInterval * (float)cWaveCount
				});
				yield return WhiteNightPattern();
			}
		}
	}

	private void GetSafeZonePositions()
	{
		_safeZonePoints.Clear();
		int aliveHeroCount = Dew.GetAliveHeroCount();
		int num = Mathf.RoundToInt(NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier * 4f);
		int num2 = aliveHeroCount;
		int num3 = safeZoneBaseCount - num;
		List<Hero> list = new List<Hero>();
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.IsNullInactiveDeadOrKnockedOut())
			{
				list.Add(allHero);
			}
		}
		int num4 = 0;
		for (int i = 0; i < num2; i++)
		{
			Hero hero = list[num4];
			int num5 = 10;
			Vector3 item = Vector3.zero;
			for (int j = 0; j < num5; j++)
			{
				float num6 = Random.Range(heroSafeZoneDeviation * 0.65f, heroSafeZoneDeviation);
				Vector3 end = hero.agentPosition + Random.insideUnitCircle.ToXZ() * num6;
				Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(hero.position, end);
				Vector3 normalized = (hero.agentPosition + validAgentDestination_LinearSweep).normalized;
				Vector2 inDirection = new Vector2(normalized.x, normalized.y);
				float num7 = Vector3.Distance(hero.agentPosition, validAgentDestination_LinearSweep);
				if (num6 - num7 > 1f)
				{
					Vector2 vector = Vector2.Reflect(inDirection, (Dew.GetPositionOnGround(SingletonBehaviour<Ink_BossRoomCenter>.instance.transform.position) - validAgentDestination_LinearSweep).normalized);
					Vector3 vector2 = new Vector3(vector.x, vector.y, 0f);
					validAgentDestination_LinearSweep += vector2 * (num6 - num7);
				}
				float magnitude = (SingletonBehaviour<Ink_BossRoomCenter>.instance.transform.position - validAgentDestination_LinearSweep).magnitude;
				if (!(globalSafeZoneRange.x * globalSafeZoneRange.x > magnitude) && !(globalSafeZoneRange.y * globalSafeZoneRange.y < magnitude))
				{
					item = validAgentDestination_LinearSweep;
					break;
				}
			}
			_safeZonePoints.Add(item);
			num4 = (num4 + 1) % list.Count;
		}
		list.Clear();
		bool flag = false;
		float num8 = 10f;
		int num9 = 0;
		int num10 = Random.Range(0, 360);
		int num11 = 360 / num3;
		Vector3 positionOnGround = Dew.GetPositionOnGround(SingletonBehaviour<Ink_BossRoomCenter>.instance.transform.position);
		for (int k = 0; k < num3; k++)
		{
			int num12 = num10 + (num11 * k + Random.Range(-30, 30));
			Vector3 vector3 = Quaternion.Euler(0f, num12, 0f) * ((Component)(object)info.caster).transform.forward;
			Vector3 vector4 = positionOnGround + vector3 * Random.Range(globalSafeZoneRange.x, globalSafeZoneRange.y);
			if (num9 >= 30)
			{
				num9 = 0;
				_safeZonePoints.Add(vector4);
				continue;
			}
			Hero closestAliveHero = Dew.GetClosestAliveHero(vector4, fallbackToDead: true, info.caster);
			if ((Object)(object)closestAliveHero != null && (closestAliveHero.GetAIAgentPosition(info.caster) - vector4).sqrMagnitude < num8 * num8)
			{
				k--;
				num9++;
				continue;
			}
			foreach (Vector3 safeZonePoint in _safeZonePoints)
			{
				if (Vector3.Distance(safeZonePoint, vector4) < safeZoneRadius)
				{
					k--;
					num9++;
					flag = false;
					break;
				}
				flag = true;
			}
			if (flag)
			{
				_safeZonePoints.Add(vector4);
			}
		}
	}

	private IEnumerator DarkMoonPattern()
	{
		if (_darkMoon.IsNullInactiveDeadOrKnockedOut())
		{
			Destroy();
			yield break;
		}
		CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_Eclipse>(_darkMoon.position, _darkMoon.rotation, new CastInfo(_darkMoon));
		Vector3 dest = SingletonBehaviour<Ink_BossTeleportPosition>.instance.transform.position;
		dest = Dew.GetPositionOnGround(dest);
		FxPlayNetworked(fxTeleportStart, info.caster);
		yield return new SI.WaitForSeconds(startDelay);
		info.caster.Visual.DisableRenderers();
		Teleport(info.caster, dest);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true);
		yield return new SI.WaitForSeconds(startTeleportDuration);
		info.caster.Control.StartDaze(eStartDelay);
		info.caster.Visual.EnableRenderers();
		info.caster.Animation.PlayAbilityAnimation(eStartClip);
		FxPlayNetworked(fxTeleportEnd, info.caster);
		yield return new SI.WaitForSeconds(eStartDelay - 0.1f);
		FxPlayNetworked(eFxCharging, info.caster);
		info.caster.Animation.PlayAbilityAnimation(eCastClip);
		Hero[] array = NetworkedManagerBase<ActorManager>.instance.allHeroes.ToArray();
		float startTime = Time.time + eCastDelay;
		Hero[] array2 = array;
		foreach (Hero hero in array2)
		{
			if (!hero.IsNullOrInactive())
			{
				CreateAbilityInstance(hero.GetAIPosition(info.caster), null, new CastInfo(info.caster, hero), (Ai_Mon_Ink_BossWhiteNight_Cataclysm_Eclipse b) =>
				{
					b._startTime = startTime;
				});
				yield return new SI.WaitForSeconds(0.15f);
			}
		}
		yield return new SI.WaitForCondition(() => startTime - Time.time < 0.001f);
		FxPlayNetworked(eFxChargeEnd, info.caster);
		info.caster.Animation.PlayAbilityAnimation(eLoopClip);
		yield return new SI.WaitForSeconds(eDuration);
		FxPlayNetworked(eFxEnd, info.caster);
		FxStopNetworked(eFxCharging);
		info.caster.Animation.PlayAbilityAnimation(eEndClip);
		yield return new SI.WaitForSeconds(ePostDelay);
		Destroy();
	}

	private IEnumerator WhiteNightPattern()
	{
		if (_isRage)
		{
			cInterval *= 0.5f;
		}
		if ((Object)(object)_darkMoon != null)
		{
			_darkMoon.Control.StartDaze(startDaze);
			_darkMoon.AI.disableAI = false;
		}
		Vector3 dest = SingletonBehaviour<Ink_BossTeleportPosition>.instance.transform.position;
		dest = Dew.GetPositionOnGround(dest);
		FxPlayNetworked(fxTeleportStart, info.caster);
		yield return new SI.WaitForSeconds(startDelay);
		info.caster.Visual.DisableRenderers();
		Teleport(info.caster, dest);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true);
		yield return new SI.WaitForSeconds(startTeleportDuration);
		FxPlayNetworked(cFxStart, info.caster);
		info.caster.Control.StartDaze(startDaze);
		info.caster.Visual.EnableRenderers();
		info.caster.Animation.PlayAbilityAnimation(cStartClip);
		FxPlayNetworked(fxTeleportEnd, info.caster);
		yield return new SI.WaitForSeconds(startDaze - 0.1f);
		for (int i = 0; i < cWaveCount; i++)
		{
			FxPlayNetworked(cFxCharging, info.caster);
			if (i != 0)
			{
				info.caster.Animation.PlayAbilityAnimation(cCastClip);
			}
			GetSafeZonePositions();
			float radius = safeZoneRadius - safeZoneRadius * safeZoneReduceRatio * (float)i;
			float endTime = Time.time + cCastDelay;
			CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster), (Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone b) =>
			{
				b._endTime = endTime;
				b.Network_radius = radius;
				b._points = _safeZonePoints;
			});
			yield return new SI.WaitForCondition(() => endTime - Time.time < 0.001f);
			FxPlayNewNetworked(cFxTelegraph, info.caster);
			Ai_Mon_Ink_BossWhiteNight_Cataclysm_Instance newInstance = CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_Cataclysm_Instance>(info.caster.position, null, new CastInfo(info.caster));
			yield return new SI.WaitForSeconds(newInstance.delay);
			FxStopNetworked(cFxCharging);
			yield return new SI.WaitForSeconds(cInterval - newInstance.delay);
		}
		info.caster.Animation.PlayAbilityAnimation(cEndClip);
		FxPlayNetworked(cFxEnd, info.caster);
		FxStopNetworked(cFxStart);
		yield return new SI.WaitForSeconds(cPostDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_channel != null && _channel.isAlive)
		{
			_channel.Cancel();
			_channel = null;
		}
		if (_darkMoonChannel != null && _darkMoonChannel.isAlive)
		{
			_darkMoonChannel.Cancel();
			_darkMoonChannel = null;
		}
		FxStopNetworked(cFxStart);
		if (info.caster.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>(out var effect))
		{
			effect.Destroy();
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe>(out var effect2))
			{
				effect2.Destroy();
			}
		}
		if ((Object)(object)_darkMoon != null)
		{
			_darkMoon.AI.disableAI = false;
			if (_darkMoon.Status.TryGetStatusEffect<Se_Mon_Ink_BossWhiteNight_Cataclysm>(out var effect3))
			{
				effect3.Destroy();
			}
		}
		info.caster.Control.StartDaze(postDelay);
	}

	private void MirrorProcessed()
	{
	}
}
