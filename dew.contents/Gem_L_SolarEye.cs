using System.Collections;
using UnityEngine;

public class Gem_L_SolarEye : Gem
{
	public DewCollider range;

	public GameObject fxCast;

	public GameObject fxStart;

	public float delay;

	public int tickCount;

	public ScalingValue totalStack;

	public int calTotalStack => Mathf.CeilToInt(GetValue(totalStack));

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (IsReady() && isValid)
		{
			FxPlayNewNetworked(fxCast, owner);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			NotifyUse();
			StartCooldown();
			AbilityInstance source = info.instance;
			source.LockDestroyFor(delay + 0.1f);
			yield return new WaitForSeconds(delay);
			bool checkValidation = false;
			range.transform.position = owner.position;
			Entity[] ents = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets).ToArray();
			source.LockDestroyFor((float)ents.Length * 0.03f + 0.1f);
			for (int i = 0; i < ents.Length; i++)
			{
				Entity entity = ents[i];
				if (entity is Monster && entity.Status.HasElemental(ElementalType.Fire))
				{
					CreateAbilityInstanceWithSource(source, owner.position, null, new CastInfo(owner, entity), (Ai_Gem_L_SolarEye b) =>
					{
						b.totalStack = calTotalStack;
						b.maxTickCount = tickCount;
					});
					checkValidation = true;
					if (i < 2)
					{
						FxPlayNewNetworked(fxStart, entity);
					}
					yield return new WaitForSeconds(0.03f);
				}
			}
			handle.Return();
			if (!checkValidation)
			{
				ResetCooldown();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
