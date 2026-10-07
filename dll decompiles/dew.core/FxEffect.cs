using System;
using System.Collections;
using UnityEngine;

public class FxEffect : MonoBehaviour
{
	public bool playOnEnable;

	public bool delayOnEnablePlayOneFrame;

	public bool playNew;

	public EffectBindEntityType bindToEntity;

	public string typeSubstring;

	private void OnEnable()
	{
		if (playOnEnable)
		{
			if (delayOnEnablePlayOneFrame)
			{
				StartCoroutine(Routine());
			}
			else
			{
				Play();
			}
		}
		IEnumerator Routine()
		{
			yield return null;
			Play();
		}
	}

	public void Play()
	{
		if (bindToEntity == EffectBindEntityType.LocalHero && (UnityEngine.Object)(object)DewPlayer.local != null && (UnityEngine.Object)(object)DewPlayer.local.hero != null)
		{
			if (playNew)
			{
				DewEffect.PlayNew(gameObject, DewPlayer.local.hero, null);
			}
			else
			{
				DewEffect.Play(gameObject, DewPlayer.local.hero, null);
			}
			return;
		}
		if (bindToEntity == EffectBindEntityType.FindType)
		{
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (string.Equals(((object)allEntity).GetType().Name, typeSubstring, StringComparison.CurrentCultureIgnoreCase))
				{
					if (playNew)
					{
						DewEffect.PlayNew(gameObject, allEntity);
					}
					else
					{
						DewEffect.Play(gameObject, allEntity);
					}
					return;
				}
			}
			foreach (Entity allEntity2 in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (((object)allEntity2).GetType().Name.ToLower().Contains(typeSubstring.ToLower()))
				{
					if (playNew)
					{
						DewEffect.PlayNew(gameObject, allEntity2);
					}
					else
					{
						DewEffect.Play(gameObject, allEntity2);
					}
					return;
				}
			}
			Debug.Log("Effect '" + name + "' could not find entity type '" + typeSubstring + "'");
		}
		if (playNew)
		{
			DewEffect.PlayNew(gameObject);
		}
		else
		{
			DewEffect.Play(gameObject);
		}
	}

	public void Stop()
	{
		DewEffect.Stop(gameObject);
	}
}
