using System.Collections;
using UnityEngine;

public class FxChangeMusic : MonoBehaviour, IEffectComponent
{
	public DewMusicItem music;

	public float delay;

	public bool disableLoop;

	public bool isPlaying => false;

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(delay);
			ManagerBase<MusicManager>.instance.Play(music, !disableLoop);
		}
	}

	public void Stop()
	{
	}
}
