using TMPro;
using UnityEngine;

public class FontTest_ChineseCrashRepro : MonoBehaviour
{
	private void Start()
	{
		((TMP_Text)GetComponentInChildren<TextMeshProUGUI>()).text = "文括号（时并且它的左边有任意字符";
	}
}
