using System.Collections;
using Mirror;
using UnityEngine;

public class Se_HarmlessWhispers_GrantShield : StatusEffect
{
	public float delay;

	public GameObject fxAfterDelay;

	public Transform moveDownwardsTransform;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxAfterDelay, victim);
			float ratio = 0.14f + NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true) * 0.02f;
			victim.Status.SetHealth(Mathf.Clamp(victim.Status.currentHealth - victim.Status.maxHealth * ratio, victim.Status.maxHealth * 0.05f, victim.Status.maxHealth));
			CreateStatusEffect(victim, (Se_MirageSkin_Armor m) =>
			{
				m.customAmount = victim.Status.maxHealth * ratio;
			});
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float num = DewEase.EaseInQuart.Get((Time.time - creationTime) / delay);
		moveDownwardsTransform.localPosition = ((1f - num) * 5f - 0.5f) * Vector3.up;
	}

	private void MirrorProcessed()
	{
	}
}
