using Mirror;

public class Ai_Star_Yubar_F_EI_DashNoProjectile_Dash : DashAttackInstance, IEtherealPathDamage
{
	private Ai_Q_EtherealInfluence _main;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_main = FindFirstAncestorOfType<Ai_Q_EtherealInfluence>();
			if (!_main.IsNullOrInactive())
			{
				_main._projectilePos = info.caster.position;
			}
			CreateBasicEffect(info.caster, new UnstoppableEffect(), dash.duration);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)this).isServer && !_main.IsNullOrInactive())
		{
			_main._projectilePos = info.caster.position;
		}
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.SetElemental(ElementalType.Light);
		dmg.SetDirection(info.caster.rotation);
	}

	private void MirrorProcessed()
	{
	}
}
