using UnityEngine;

[DewResourceLink(ResourceLinkBy.Guid)]
[CreateAssetMenu(fileName = "mus", menuName = "Dew Music Item")]
public class DewMusicItem : ScriptableObject, ILinkedByGuid
{
	public AudioClip clip;

	public float delay = 1.5f;

	public float fadeInTime = 2f;

	public float volumeMultiplier = 1f;

	public float pitch = 1f;

	public float startTime;

	[field: HideInInspector]
	[field: SerializeField]
	public string resourceId { get; set; }

	public void Play()
	{
		if (!Application.isPlaying || ManagerBase<MusicManager>.instance == null)
		{
			Debug.Log("MusicManager is not present.");
		}
		else
		{
			ManagerBase<MusicManager>.instance.Play(this);
		}
	}
}
