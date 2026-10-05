using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine_PrimusDoor : Shrine, ICustomInteractable
{
	public DewCutsceneDirector director;

	public Knockback knockback;

	public float delay;

	public DewCollider range;

	private bool _didUse;

	public string nameRawText => "";

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Knock");

	public Vector3 worldOffset => new Vector3(0f, 2.3f, 0f);

	public override bool ShouldBeSavedWithRoom()
	{
		return !_didUse;
	}

	protected override bool OnUse(Entity entity)
	{
		if (_didUse)
		{
			return true;
		}
		_didUse = true;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			List<Hero> heros = NetworkedManagerBase<ActorManager>.instance.allHeroes;
			foreach (Hero item in heros)
			{
				if (!item.IsNullInactiveDeadOrKnockedOut())
				{
					item.Control.StartDaze(delay + 2f);
				}
			}
			yield return new WaitForSeconds(delay);
			foreach (Hero item2 in heros)
			{
				if (!item2.IsNullInactiveDeadOrKnockedOut())
				{
					CreateBasicEffect(item2, new StunEffect(), 10f);
				}
			}
			range.transform.position = ((Component)(object)this).transform.position;
			List<Entity> entities = range.GetEntities(out var handle);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity2 = entities[i];
				if (entity2.owner.isHumanPlayer && !entity2.IsNullInactiveDeadOrKnockedOut())
				{
					knockback.ApplyWithDirection(entity2.position - ((Component)(object)this).transform.position, entity2);
				}
			}
			handle.Return();
			yield return new WaitForSeconds(1f);
			director.PlayNetworked();
			DewCutsceneDirector dewCutsceneDirector = director;
			dewCutsceneDirector.onFinish = (Action)Delegate.Combine(dewCutsceneDirector.onFinish, (Action)(() =>
			{
				foreach (Hero item3 in heros)
				{
					if (!item3.IsNullInactiveDeadOrKnockedOut())
					{
						StatusEffect[] array = item3.Status.statusEffects.ToArray();
						foreach (StatusEffect statusEffect in array)
						{
							if (!statusEffect.IsNullOrInactive() && statusEffect is Se_GenericEffectContainer se_GenericEffectContainer && se_GenericEffectContainer.effect is StunEffect)
							{
								statusEffect.Destroy();
							}
						}
					}
				}
				NetworkedManagerBase<ZoneManager>.instance.LoadNode(new LoadNodeSettings
				{
					from = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex,
					to = 2,
					advanceTurn = true,
					isWhiteTransition = false,
					dontDoRiftTransition = true,
					dontStopLoading = false,
					isTravelingRoom = true
				});
			}));
		}
	}

	private void MirrorProcessed()
	{
	}
}
