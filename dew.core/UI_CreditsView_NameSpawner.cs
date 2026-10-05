using System;
using TMPro;
using UnityEngine;

public class UI_CreditsView_NameSpawner : MonoBehaviour
{
	public UI_CreditsView_Name prefab;

	[TextArea(10, 10)]
	public string namesStr;

	public void UpdateNames()
	{
		UI_CreditsView_Name[] componentsInChildren = GetComponentsInChildren<UI_CreditsView_Name>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		string[] array = namesStr.Trim().Split("\n", StringSplitOptions.None);
		foreach (string text in array)
		{
			try
			{
				string[] array2 = text.Split("|", StringSplitOptions.None);
				if (array2.Length == 2)
				{
					UI_CreditsView_Name uI_CreditsView_Name = UnityEngine.Object.Instantiate(prefab, transform);
					if (array2[0].Contains(":"))
					{
						string[] array3 = array2[0].Split(":", StringSplitOptions.None);
						((TMP_Text)uI_CreditsView_Name.nameText).text = ((DewSave.profileMain.language == "ko-KR") ? array3[1] : array3[0]);
					}
					else
					{
						((TMP_Text)uI_CreditsView_Name.nameText).text = array2[0].Trim();
					}
					string text2 = array2[1].Trim().Replace(" ", "").Replace("&", "")
						.Replace("-", "");
					if (DewLocalization.TryGetUIValue("Credits_Role_" + text2, out var value))
					{
						((TMP_Text)uI_CreditsView_Name.roleText).text = value;
					}
					else
					{
						((TMP_Text)uI_CreditsView_Name.roleText).text = array2[1].Trim();
					}
					if (((TMP_Text)uI_CreditsView_Name.nameText).text.Length == 0 && uI_CreditsView_Name.arrowObject != null)
					{
						uI_CreditsView_Name.arrowObject.SetActive(value: false);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
