using System.Collections;
using Mirror;
using UnityEngine;

public class Se_R_ShadowOverdrive_Initial : StatusEffect
{
	public ScalingValue speedAmount;

	public float speedDuration = 1f;

	public int missileCount = 5;

	public int addedEmpoweredMissileCount = 5;

	public float missileRange = 6f;

	public ScalingValue empowerChance;

	public GameObject fxEmpoweredCast;

	private bool _isEmpowered;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(GetValue(speedAmount));
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Ability | Channel.BlockedAction.Attack);
			SetTimer(speedDuration);
			ShowOnScreenTimer(null, default, invertValue: true);
			_isEmpowered = Random.value * 100f < GetValue(empowerChance);
			if (_isEmpowered)
			{
				FxPlayNetworked(fxEmpoweredCast, info.caster);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)info.caster != null)
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Ability | Channel.BlockedAction.Attack);
			}
			if (!info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				CreateStatusEffect<Se_R_ShadowOverdrive_AtkSpd>(info.caster);
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		}
		IEnumerator Routine()
		{
			Entity[] ents = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, missileRange, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.Random
			}).ToArray();
			handle.Return();
			if (ents.Length != 0)
			{
				int count = missileCount;
				if (_isEmpowered)
				{
					count += addedEmpoweredMissileCount;
				}
				for (int i = 0; i < count; i++)
				{
					if ((Object)(object)info.caster == null)
					{
						break;
					}
					Entity entity = ents[i % ents.Length];
					if (!entity.IsNullInactiveDeadOrKnockedOut())
					{
						CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, entity), (Ai_R_ShadowOverdrive_Projectile ai) =>
						{
							ai.isEmpoweredByCrit = _isEmpowered;
						});
					}
					yield return new WaitForSeconds(0.05f);
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
