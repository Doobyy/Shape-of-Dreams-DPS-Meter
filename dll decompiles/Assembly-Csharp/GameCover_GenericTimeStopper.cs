using UnityEngine;

public class GameCover_GenericTimeStopper : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (Time.timeScale > 0.1f)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
	}
}
