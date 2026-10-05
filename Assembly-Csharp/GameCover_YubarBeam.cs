using System.Collections;
using UnityEngine;

public class GameCover_YubarBeam : MonoBehaviour
{
	public Transform targetPosition;

	private void Start()
	{
		Shoot();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Shoot();
		}
	}

	private void Shoot()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Vector3 lastPos = transform.position;
			yield return new WaitForSeconds(0.5f);
			ParticleSystem ps = GetComponent<ParticleSystem>();
			ps.Stop(true, (ParticleSystemStopBehavior)0);
			yield return null;
			ps.Play(true);
			yield return null;
			transform.position = targetPosition.position;
			yield return null;
			yield return null;
			yield return null;
			ps.Pause(true);
			yield return null;
			transform.position = lastPos;
		}
	}
}
