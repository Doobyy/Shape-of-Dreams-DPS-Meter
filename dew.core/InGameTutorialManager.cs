using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameTutorialManager : ManagerBase<InGameTutorialManager>
{
	public bool isTutorialDisabled;

	public TutorialArrow arrowPrefab;

	public Transform uiParent;

	public Transform refSkillButtonQ;

	public Transform refSkillButtonW;

	public Transform refSkillButtonE;

	public Transform refSkillButtonR;

	public Transform refSkillButtonTrait;

	public Transform refSkillButtonMovement;

	public Transform refHuntProgress;

	public Transform refHuntImminentProgress;

	public Transform refSkillNotification;

	public Transform refGeneralSkills;

	public Transform refEditKeyDisplay;

	internal List<Action> _logicUpdates = new List<Action>();

	internal List<Action> _frameUpdates = new List<Action>();

	private List<DewInGameTutorialItem> _activeTutorials = new List<DewInGameTutorialItem>();

	public bool isTutorialActive { get; private set; }

	public Transform GetRefSkill(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => refSkillButtonQ, 
			HeroSkillLocation.W => refSkillButtonW, 
			HeroSkillLocation.E => refSkillButtonE, 
			HeroSkillLocation.R => refSkillButtonR, 
			HeroSkillLocation.Identity => refSkillButtonTrait, 
			HeroSkillLocation.Movement => refSkillButtonMovement, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	private void Start()
	{
		if (isTutorialDisabled)
		{
			return;
		}
		NetworkedManagerBase<GameManager>.instance.ClientEvent_OnGameConcluded += (Action)(() =>
		{
			if (isTutorialActive)
			{
				StopTutorials();
			}
		});
		GameManager.CallOnReady(() =>
		{
			StartTutorials();
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)((Hero _, Hero to) =>
			{
				if (isTutorialActive)
				{
					StopTutorials();
					StartTutorials();
				}
			});
		});
	}

	public void StartTutorials()
	{
		if (isTutorialActive)
		{
			Debug.LogWarning("Tutorial already has started");
		}
		else
		{
			if (DewSave.profileMain.gameplay.disableTutorial)
			{
				return;
			}
			isTutorialActive = true;
			if ((UnityEngine.Object)(object)DewPlayer.local == null || (UnityEngine.Object)(object)DewPlayer.local.hero == null)
			{
				return;
			}
			foreach (Type allTutorialItem in Dew.allTutorialItems)
			{
				if (!DewSave.profileMain.doneTutorials.Contains(allTutorialItem.Name) || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
				{
					DewInGameTutorialItem item = (DewInGameTutorialItem)Activator.CreateInstance(allTutorialItem);
					_activeTutorials.Add(item);
				}
			}
			for (int num = _activeTutorials.Count - 1; num >= 0; num--)
			{
				DewInGameTutorialItem dewInGameTutorialItem = _activeTutorials[num];
				try
				{
					dewInGameTutorialItem.mgr = this;
					dewInGameTutorialItem.OnStart();
				}
				catch (Exception exception)
				{
					Debug.LogError("Exception occured while starting " + dewInGameTutorialItem.GetType().Name);
					Debug.LogException(exception, this);
					_activeTutorials.RemoveAt(num);
				}
			}
			if (_activeTutorials.Count > 0)
			{
				Debug.Log($"Started {_activeTutorials.Count} tutorials");
				return;
			}
			isTutorialActive = false;
			_logicUpdates.Clear();
			_frameUpdates.Clear();
		}
	}

	public void StopTutorials()
	{
		if (!isTutorialActive)
		{
			Debug.LogWarning("Tutorial has not started");
			return;
		}
		isTutorialActive = false;
		for (int num = _activeTutorials.Count - 1; num >= 0; num--)
		{
			DewInGameTutorialItem dewInGameTutorialItem = _activeTutorials[num];
			string text = dewInGameTutorialItem.GetType().Name;
			try
			{
				dewInGameTutorialItem.OnStop();
			}
			catch (Exception exception)
			{
				Debug.LogError(text + ".OnStop() failed with exception below");
				Debug.LogException(exception);
			}
		}
		Debug.Log($"Stopped {_activeTutorials.Count} tutorials");
		_activeTutorials.Clear();
		_logicUpdates.Clear();
		_frameUpdates.Clear();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!isTutorialActive)
		{
			return;
		}
		for (int i = 0; i < _frameUpdates.Count; i++)
		{
			try
			{
				_frameUpdates[i]();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!isTutorialActive)
		{
			return;
		}
		for (int i = 0; i < _logicUpdates.Count; i++)
		{
			try
			{
				_logicUpdates[i]();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	internal void CompleteTutorialItem(DewInGameTutorialItem item)
	{
		if (!isTutorialActive)
		{
			Debug.LogWarning("Tutorial has not started");
			return;
		}
		string text = item.GetType().Name;
		int num = _activeTutorials.IndexOf(item);
		if (num < 0)
		{
			Debug.LogWarning(text + " not found in active tutorials");
			return;
		}
		_activeTutorials.RemoveAt(num);
		Debug.Log("Tutorial " + text + " completed.");
		if (DewSave.profileMain.doneTutorials.Contains(text))
		{
			Debug.LogWarning(text + " already in done tutorials");
		}
		else
		{
			DewSave.profileMain.doneTutorials.Add(text);
		}
		try
		{
			item.OnStop();
		}
		catch (Exception exception)
		{
			Debug.LogError(text + ".OnStop() failed with exception below");
			Debug.LogException(exception);
		}
		if (_activeTutorials.Count == 0)
		{
			StopTutorials();
		}
	}

	public TutorialArrow CreateArrow()
	{
		return UnityEngine.Object.Instantiate(arrowPrefab, uiParent);
	}
}
