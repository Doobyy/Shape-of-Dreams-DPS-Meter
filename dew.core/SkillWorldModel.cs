using System;
using UnityEngine;

public class SkillWorldModel : ItemWorldModel
{
	private float _appearTime;

	protected override void Awake()
	{
		base.Awake();
		SkillTrigger skillTrigger = (SkillTrigger)item;
		skillTrigger.ClientSkillEvent_OnDismantleProgressChanged += new Action<float, float>(OnDismantleProgressChanged);
		EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
		instance.OnSkillVisibleClientState = (Action<SkillTrigger, bool>)Delegate.Combine(instance.OnSkillVisibleClientState, new Action<SkillTrigger, bool>(SkillVisibleClientState));
		_appearTime = Time.time;
		skillTrigger.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)((DewPlayer _, DewPlayer _) =>
		{
			UpdateVisibility();
		});
		skillTrigger.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)((Hero _, Hero _) =>
		{
			UpdateVisibility();
		});
		skillTrigger.ClientEvent_OnOwnerChanged += (Action<Entity, Entity>)((Entity _, Entity _) =>
		{
			UpdateVisibility();
		});
	}

	protected override void OnAppearInAnimation()
	{
		base.OnAppearInAnimation();
		_appearTime = Time.time;
	}

	private void Update()
	{
		iconQuadTransform.localPosition = Vector3.up * (2f + Mathf.Sin((Time.time - _appearTime) * 2f) * 0.125f);
	}

	private void SkillVisibleClientState(SkillTrigger arg1, bool arg2)
	{
		if (!((UnityEngine.Object)(object)(SkillTrigger)item != (UnityEngine.Object)(object)arg1))
		{
			UpdateVisibilityLocal(!arg2);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<EditSkillManager>.instance != null)
		{
			EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
			instance.OnSkillVisibleClientState = (Action<SkillTrigger, bool>)Delegate.Remove(instance.OnSkillVisibleClientState, new Action<SkillTrigger, bool>(SkillVisibleClientState));
		}
	}

	protected override Texture GetIconTexture()
	{
		Sprite triggerIcon = ((SkillTrigger)item).configs[0].triggerIcon;
		if (!(triggerIcon != null))
		{
			return null;
		}
		return triggerIcon.texture;
	}
}
