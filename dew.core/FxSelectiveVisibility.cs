using UnityEngine;

[DisallowMultipleComponent]
public class FxSelectiveVisibility : MonoBehaviour, ISerializationCallbackReceiver
{
	public static bool forceFail;

	public EffectVisibility visibleTo;

	public bool hideOnSummons;

	public bool invertVisibility;

	[HideInInspector]
	public Actor parent;

	[HideInInspector]
	public bool wasDisabled;

	private string _helpText => "Parent: " + (((Object)(object)parent == null) ? ((object)"null") : ((object)parent));

	public bool IsVisibleLocally()
	{
		return invertVisibility ^ IsVisibleLocally_Imp();
	}

	private bool IsVisibleLocally_Imp()
	{
		if ((Object)(object)parent == null && visibleTo != EffectVisibility.Everyone && !TryResolveParentAtRuntime())
		{
			Debug.LogWarning("Selectively visible effect '" + transform.GetScenePath() + "' has no proper parent");
			return false;
		}
		if (forceFail && visibleTo != EffectVisibility.Everyone)
		{
			return false;
		}
		Entity entity = ((ManagerBase<CameraManager>.instance != null) ? ManagerBase<CameraManager>.instance.focusedEntity : null);
		DewPlayer dewPlayer = (((Object)(object)entity != null) ? entity.owner : DewPlayer.local);
		switch (visibleTo)
		{
		case EffectVisibility.Everyone:
			return true;
		case EffectVisibility.Caster:
		{
			Entity caster = ((AbilityInstance)parent).info.caster;
			if ((Object)(object)caster == null)
			{
				return false;
			}
			if ((Object)(object)caster.owner != (Object)(object)dewPlayer)
			{
				return false;
			}
			if (caster is Summon && hideOnSummons)
			{
				return false;
			}
			return true;
		}
		case EffectVisibility.Target:
		{
			Entity target = ((AbilityInstance)parent).info.target;
			if ((Object)(object)target == null)
			{
				return false;
			}
			if ((Object)(object)target.owner != (Object)(object)dewPlayer)
			{
				return false;
			}
			if (target is Summon && hideOnSummons)
			{
				return false;
			}
			return true;
		}
		case EffectVisibility.Victim:
		{
			Entity victim = ((StatusEffect)parent).victim;
			if ((Object)(object)victim == null)
			{
				return false;
			}
			if ((Object)(object)victim.owner != (Object)(object)dewPlayer)
			{
				return false;
			}
			if (victim is Summon && hideOnSummons)
			{
				return false;
			}
			return true;
		}
		case EffectVisibility.Owner:
		{
			Entity firstEntity = parent.firstEntity;
			if ((Object)(object)firstEntity.owner != (Object)(object)dewPlayer)
			{
				return false;
			}
			if (firstEntity is Summon && hideOnSummons)
			{
				return false;
			}
			return true;
		}
		default:
			return true;
		}
	}

	private bool TryResolveParentAtRuntime()
	{
		switch (visibleTo)
		{
		case EffectVisibility.Caster:
		case EffectVisibility.Target:
			parent = GetComponentInParent<AbilityInstance>(includeInactive: true);
			break;
		case EffectVisibility.Victim:
			parent = GetComponentInParent<StatusEffect>(includeInactive: true);
			break;
		case EffectVisibility.Owner:
			parent = GetComponentInParent<Actor>(includeInactive: true);
			break;
		}
		return (Object)(object)parent != null;
	}

	private void OnValidate()
	{
		if (Application.IsPlaying(this) && (Object)(object)parent == null && visibleTo != EffectVisibility.Everyone)
		{
			Debug.LogWarning("Selectively visible effect '" + name + "' has no proper parent");
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
