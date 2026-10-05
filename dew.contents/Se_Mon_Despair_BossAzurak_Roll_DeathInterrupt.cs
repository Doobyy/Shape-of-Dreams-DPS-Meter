using Mirror;

public class Se_Mon_Despair_BossAzurak_Roll_DeathInterrupt : StatusEffect
{
	public float armorAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoUnstoppable();
		DoArmorBoost(armorAmount);
		DoDeathInterrupt((EventInfoKill _) =>
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			if (victim.Control.isDisplacing && (int)Dew.GetNavMeshPathStatus(Dew.GetPositionOnGround(parentActor.position), victim.agentPosition) != 0)
			{
				victim.Status.SetHealth(1f);
			}
		}, -100);
	}

	private void MirrorProcessed()
	{
	}
}
