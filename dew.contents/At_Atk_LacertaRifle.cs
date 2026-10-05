using UnityEngine;

public class At_Atk_LacertaRifle : AttackTrigger
{
	public static readonly Vector3 MagicNumber = new Vector3(100f, 100f, 100f);

	public AbilityInstance Shoot(Entity target)
	{
		CastInfo info = new CastInfo(owner, target);
		info.point = MagicNumber;
		return OnCastComplete(currentConfigIndex, info);
	}

	public AbilityInstance Shoot(float angle)
	{
		CastInfo info = new CastInfo(owner, angle);
		info.point = MagicNumber;
		return OnCastComplete(currentConfigIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
