using UnityEngine;

public class Ai_Atk_LacertaRifle_Crit : AttackProjectile
{
	public override bool reuseInRoom => true;

	protected override bool detachEntityHitEffect => false;

	protected override void OnPrepare()
	{
		Quaternion quaternion = (((Object)(object)info.target != null) ? Quaternion.LookRotation(info.target.agentPosition - info.caster.agentPosition).Flattened() : info.rotation);
		SetCustomStartPosition(GetLacertaMuzzlePosition(info.caster, quaternion));
		base.OnPrepare();
	}

	public static Vector3 GetLacertaMuzzlePosition(Entity lacerta, Quaternion rotation)
	{
		Vector3 centerPosition = lacerta.Visual.GetCenterPosition();
		Vector3 forward = lacerta.Visual.GetMuzzlePosition() - centerPosition;
		Vector3 eulerAngles = Quaternion.LookRotation(forward).eulerAngles;
		eulerAngles.y = rotation.eulerAngles.y + 10f;
		return centerPosition + Quaternion.Euler(eulerAngles) * Vector3.forward * forward.magnitude;
	}

	private void MirrorProcessed()
	{
	}
}
