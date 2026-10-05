using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEngine;

public abstract class PlayTutorial_Tutorial : MonoBehaviour
{
	public readonly List<Entity> spawnedEntities = new List<Entity>();

	private List<TutorialArrow> _spawnedArrows = new List<TutorialArrow>();

	public Hero h
	{
		get
		{
			if (!((UnityEngine.Object)(object)DewPlayer.local != null))
			{
				return null;
			}
			return DewPlayer.local.hero;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Debug.Log("Start: " + name);
			yield return OnStart();
			Debug.Log("End: " + name);
		}
	}

	public TutorialArrow CreateTutorialArrow()
	{
		TutorialArrow tutorialArrow = ManagerBase<InGameTutorialManager>.instance.CreateArrow();
		_spawnedArrows.Add(tutorialArrow);
		return tutorialArrow;
	}

	private void OnDisable()
	{
		Debug.Log("Cleanup: " + name);
		try
		{
			foreach (TutorialArrow spawnedArrow in _spawnedArrows)
			{
				if (spawnedArrow != null)
				{
					UnityEngine.Object.Destroy(spawnedArrow.gameObject);
				}
			}
			_spawnedArrows.Clear();
			foreach (Entity spawnedEntity in spawnedEntities)
			{
				if (!spawnedEntity.IsNullInactiveDeadOrKnockedOut())
				{
					spawnedEntity.Destroy();
				}
			}
			spawnedEntities.Clear();
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.FieldType == typeof(UI_ShowAndHideObject))
				{
					UI_ShowAndHideObject uI_ShowAndHideObject = (UI_ShowAndHideObject)fieldInfo.GetValue(this);
					if (uI_ShowAndHideObject != null)
					{
						uI_ShowAndHideObject.Hide();
					}
				}
			}
			OnCleanup();
		}
		catch (Exception)
		{
		}
	}

	public void AddToSpawnedEntity(Entity entity)
	{
		spawnedEntities.Add(entity);
	}

	public IEnumerator WaitForSpawnedEntityDeathRoutine()
	{
		yield return new WaitWhile(() => spawnedEntities.Any((Entity e) => !e.IsNullInactiveDeadOrKnockedOut()));
		spawnedEntities.Clear();
	}

	protected abstract IEnumerator OnStart();

	protected abstract void OnCleanup();

	protected Coroutine StartSkillHighlightCoroutine(Func<HeroSkillLocation> skillLocGetter)
	{
		return StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (!(this == null) && isActiveAndEnabled)
			{
				HeroSkillLocation type = skillLocGetter();
				ShortcutExtensions.DOKill((Component)PlayTutorialManager.instance.GetSkillButton(type), true);
				if (h.Skill.TryGetSkill(type, out var skill))
				{
					skill.ClientTriggerEvent_OnCurrentConfigCooldownReduced.Invoke();
				}
				yield return new WaitForSeconds(0.85f);
			}
		}
	}
}
