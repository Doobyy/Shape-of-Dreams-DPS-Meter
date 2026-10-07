using UnityEngine;

public class Mon_RedMan : Monster
{
	public GameObject deathEffects;

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		Visual.DisableRenderersLocal();
		Quaternion quaternion = ((Component)(object)this).transform.rotation;
		if ((Object)(object)info.actor.firstEntity != null)
		{
			quaternion = Quaternion.LookRotation(((Component)(object)this).transform.position - ((Component)(object)info.actor.firstEntity).transform.position).Flattened();
		}
		deathEffects.transform.rotation = quaternion;
		deathEffects.transform.parent = null;
		deathEffects.SetActive(value: true);
		Rigidbody[] componentsInChildren = deathEffects.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody obj in componentsInChildren)
		{
			obj.AddForce(Random.Range(0f, 1f) * deathEffects.transform.forward, (ForceMode)2);
			((Component)(object)obj).transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);
			((Component)(object)obj).transform.rotation = Random.rotation;
		}
		Object.Destroy(deathEffects, 10f);
	}

	private void MirrorProcessed()
	{
	}
}
