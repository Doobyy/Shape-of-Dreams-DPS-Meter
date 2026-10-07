using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class DewGUI
{
	public delegate bool FieldBuilderCondition(Type type, FieldInfo info);

	public delegate FieldBuildResult FieldBuilder(Type type, FieldInfo info, Transform parent);

	public delegate bool TryParseFunc<T>(string str, out T result);

	private static RectTransform _canvasTransform;

	public static readonly Type[] SupportedBasicTypes = new Type[17]
	{
		typeof(string),
		typeof(bool),
		typeof(float),
		typeof(double),
		typeof(byte),
		typeof(short),
		typeof(int),
		typeof(long),
		typeof(ushort),
		typeof(uint),
		typeof(ulong),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(Vector2Int),
		typeof(Vector3Int),
		typeof(Quaternion)
	};

	public static List<(FieldBuilderCondition, FieldBuilder)> fieldBuilders = new List<(FieldBuilderCondition, FieldBuilder)>();

	public static RectTransform canvasTransform
	{
		get
		{
			if (_canvasTransform == null)
			{
				_canvasTransform = ManagerBase<GlobalLogicPackage>.instance.transform.Find("Global Canvas").Find("DewGUI Parent") as RectTransform;
			}
			return _canvasTransform;
		}
	}

	public static UI_Window widgetWindow => Resources.Load<GameObject>("DewGUI/Widget Window").GetComponent<UI_Window>();

	public static Button widgetButton => Resources.Load<GameObject>("DewGUI/Widget Button").GetComponent<Button>();

	public static UI_Toggle widgetToggleCheck => Resources.Load<GameObject>("DewGUI/Widget Toggle Check").GetComponent<UI_Toggle>();

	public static UI_Toggle widgetToggleButton => Resources.Load<GameObject>("DewGUI/Widget Toggle Button").GetComponent<UI_Toggle>();

	public static TMP_Dropdown widgetDropdown => Resources.Load<GameObject>("DewGUI/Widget Dropdown").GetComponent<TMP_Dropdown>();

	public static TMP_InputField widgetInputField => Resources.Load<GameObject>("DewGUI/Widget Input Field").GetComponent<TMP_InputField>();

	public static Slider widgetSlider => Resources.Load<GameObject>("DewGUI/Widget Slider").GetComponent<Slider>();

	public static TextMeshProUGUI widgetTextHeader => Resources.Load<GameObject>("DewGUI/Widget Text Header").GetComponent<TextMeshProUGUI>();

	public static TextMeshProUGUI widgetTextBody => Resources.Load<GameObject>("DewGUI/Widget Text Body").GetComponent<TextMeshProUGUI>();

	public static TextMeshProUGUI widgetTextLabel => Resources.Load<GameObject>("DewGUI/Widget Text Label").GetComponent<TextMeshProUGUI>();

	public static ScrollRect widgetScrollRect => Resources.Load<GameObject>("DewGUI/Widget Scroll Rect").GetComponent<ScrollRect>();

	public static UI_BindItem widgetBindItem => Resources.Load<GameObject>("DewGUI/Widget Bind Item").GetComponent<UI_BindItem>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		fieldBuilders.Clear();
		fieldBuilders.Add(((Type type, FieldInfo info) => type == typeof(KeyCode), (Type type, FieldInfo _, Transform parent) =>
		{
			RefValue<KeyCode> val = new RefValue<KeyCode>(KeyCode.None);
			FieldBuildResult result = new FieldBuildResult();
			UI_BindItem newBindItem = UnityEngine.Object.Instantiate(widgetBindItem, parent);
			newBindItem.allowedTypes = BindingType.Keyboard | BindingType.Mouse;
			newBindItem.allowModifiers = false;
			UI_BindItem uI_BindItem = newBindItem;
			uI_BindItem.onValueChanged = (Action<DewBinding>)Delegate.Combine(uI_BindItem.onValueChanged, (Action<DewBinding>)((DewBinding o) =>
			{
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				if (o.pcBinds.Count == 0)
				{
					result.onChanged?.Invoke(KeyCode.None);
				}
				else if ((int)o.pcBinds[0].key != 0)
				{
					result.onChanged?.Invoke(o.pcBinds[0].key.ToKeyCode());
				}
				else if (o.pcBinds[0].mouse != MouseButton.None)
				{
					result.onChanged?.Invoke(o.pcBinds[0].mouse.ToKeyCode());
				}
				else
				{
					result.onChanged?.Invoke(KeyCode.None);
				}
			}));
			result.getValue = () => val.value;
			result.setValue = (object obj) =>
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				val.value = (KeyCode)obj;
				Key val2 = val.value.ToKey();
				MouseButton mouseButton = val.value.ToMouseButton();
				if ((int)val2 != 0)
				{
					newBindItem.value = DewBinding.KeyboardAndMouseOnly(val2);
				}
				else if (mouseButton != MouseButton.None)
				{
					newBindItem.value = DewBinding.KeyboardAndMouseOnly(mouseButton);
				}
				else
				{
					newBindItem.value = DewBinding.KeyboardAndMouseOnly();
				}
			};
			result.root = newBindItem.gameObject;
			return result;
		}));
		fieldBuilders.Add(((Type type, FieldInfo info) => type.IsEnum, (Type type, FieldInfo _, Transform parent) =>
		{
			RefValue<object> val = new RefValue<object>();
			FieldBuildResult result = new FieldBuildResult();
			TMP_Dropdown newDropdown = UnityEngine.Object.Instantiate<TMP_Dropdown>(widgetDropdown, parent);
			string[] names = Enum.GetNames(type);
			object[] values = Enum.GetValues(type).Cast<object>().ToArray();
			newDropdown.ClearOptions();
			newDropdown.AddOptions(names.ToList());
			((UnityEvent<int>)(object)newDropdown.onValueChanged).AddListener((UnityAction<int>)((int newIndex) =>
			{
				result.onChanged?.Invoke(values[newIndex]);
			}));
			result.getValue = () => val.value;
			result.setValue = (object obj) =>
			{
				val.value = obj;
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i].Equals(obj))
					{
						newDropdown.value = i;
						break;
					}
				}
			};
			result.root = ((Component)(object)newDropdown).gameObject;
			return result;
		}));
		Type[] supportedBasicTypes = SupportedBasicTypes;
		foreach (Type t in supportedBasicTypes)
		{
			fieldBuilders.Add(((Type type, FieldInfo info) => type == t, (Type _, FieldInfo _, Transform parent) => BuildFieldOfBasicType(t, parent)));
		}
	}

	private static FieldBuildResult BuildFieldOfBasicType(Type type, Transform parent)
	{
		TMP_InputField inputField;
		if (type == typeof(string))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out string reference)
			{
				reference = str;
				return true;
			}, (CharacterValidation)0, out inputField);
		}
		if (type == typeof(bool))
		{
			RefValue<bool> val = new RefValue<bool>(v: false);
			FieldBuildResult result = new FieldBuildResult();
			UI_Toggle newToggle = UnityEngine.Object.Instantiate(widgetToggleCheck, parent);
			newToggle.onIsCheckedChanged.AddListener((bool newVal) =>
			{
				result.onChanged?.Invoke(newVal);
			});
			result.getValue = () => val.value;
			result.setValue = (object obj) =>
			{
				val.value = (bool)obj;
				newToggle.isChecked = val.value;
			};
			result.root = newToggle.gameObject;
			return result;
		}
		if (type == typeof(float))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out float result2)
			{
				return float.TryParse(str, out result2);
			}, (CharacterValidation)3, out inputField);
		}
		if (type == typeof(double))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out double result2)
			{
				return double.TryParse(str, out result2);
			}, (CharacterValidation)3, out inputField);
		}
		if (type == typeof(byte))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out byte result2)
			{
				return byte.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(short))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out short result2)
			{
				return short.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(int))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out int result2)
			{
				return int.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(long))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out long result2)
			{
				return long.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(ushort))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out ushort result2)
			{
				return ushort.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(uint))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out uint result2)
			{
				return uint.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(ulong))
		{
			return GenericInputFieldBuilder(parent, delegate(string str, out ulong result2)
			{
				return ulong.TryParse(str, out result2);
			}, (CharacterValidation)2, out inputField);
		}
		if (type == typeof(Vector2))
		{
			RefValue<Vector2> val2 = new RefValue<Vector2>();
			FieldBuildResult res = new FieldBuildResult();
			HorizontalLayoutGroup val3 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			res.root = ((Component)(object)val3).gameObject;
			res.getValue = () => val2.value;
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val3).transform)).SetText("x");
			FieldBuildResult x = BuildFieldOfBasicType(typeof(float), ((Component)(object)val3).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val3).transform)).SetText("y");
			FieldBuildResult y = BuildFieldOfBasicType(typeof(float), ((Component)(object)val3).transform);
			x.onChanged += (Action<object>)((object obj) =>
			{
				val2.value.x = (float)obj;
				res.onChanged?.Invoke(val2.value);
			});
			y.onChanged += (Action<object>)((object obj) =>
			{
				val2.value.y = (float)obj;
				res.onChanged?.Invoke(val2.value);
			});
			res.setValue = (object obj) =>
			{
				val2.value = (Vector2)obj;
				x.setValue(val2.value.x);
				y.setValue(val2.value.y);
			};
			return res;
		}
		if (type == typeof(Vector3))
		{
			RefValue<Vector3> val4 = new RefValue<Vector3>();
			FieldBuildResult res2 = new FieldBuildResult();
			HorizontalLayoutGroup val5 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			res2.root = ((Component)(object)val5).gameObject;
			res2.getValue = () => val4.value;
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val5).transform)).SetText("x");
			FieldBuildResult x2 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val5).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val5).transform)).SetText("y");
			FieldBuildResult y2 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val5).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val5).transform)).SetText("z");
			FieldBuildResult z = BuildFieldOfBasicType(typeof(float), ((Component)(object)val5).transform);
			x2.onChanged += (Action<object>)((object obj) =>
			{
				val4.value.x = (float)obj;
				res2.onChanged?.Invoke(val4.value);
			});
			y2.onChanged += (Action<object>)((object obj) =>
			{
				val4.value.y = (float)obj;
				res2.onChanged?.Invoke(val4.value);
			});
			z.onChanged += (Action<object>)((object obj) =>
			{
				val4.value.z = (float)obj;
				res2.onChanged?.Invoke(val4.value);
			});
			res2.setValue = (object obj) =>
			{
				val4.value = (Vector3)obj;
				x2.setValue(val4.value.x);
				y2.setValue(val4.value.y);
				z.setValue(val4.value.z);
			};
			return res2;
		}
		if (type == typeof(Vector4))
		{
			RefValue<Vector4> val6 = new RefValue<Vector4>();
			FieldBuildResult res3 = new FieldBuildResult();
			HorizontalLayoutGroup val7 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			res3.root = ((Component)(object)val7).gameObject;
			res3.getValue = () => val6.value;
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val7).transform)).SetText("x");
			FieldBuildResult x3 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val7).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val7).transform)).SetText("y");
			FieldBuildResult y3 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val7).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val7).transform)).SetText("z");
			FieldBuildResult z2 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val7).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val7).transform)).SetText("w");
			FieldBuildResult w = BuildFieldOfBasicType(typeof(float), ((Component)(object)val7).transform);
			x3.onChanged += (Action<object>)((object obj) =>
			{
				val6.value.x = (float)obj;
				res3.onChanged?.Invoke(val6.value);
			});
			y3.onChanged += (Action<object>)((object obj) =>
			{
				val6.value.y = (float)obj;
				res3.onChanged?.Invoke(val6.value);
			});
			z2.onChanged += (Action<object>)((object obj) =>
			{
				val6.value.z = (float)obj;
				res3.onChanged?.Invoke(val6.value);
			});
			w.onChanged += (Action<object>)((object obj) =>
			{
				val6.value.w = (float)obj;
				res3.onChanged?.Invoke(val6.value);
			});
			res3.setValue = (object obj) =>
			{
				val6.value = (Vector4)obj;
				x3.setValue(val6.value.x);
				y3.setValue(val6.value.y);
				z2.setValue(val6.value.z);
				w.setValue(val6.value.w);
			};
			return res3;
		}
		if (type == typeof(Quaternion))
		{
			RefValue<Quaternion> val8 = new RefValue<Quaternion>();
			FieldBuildResult res4 = new FieldBuildResult();
			HorizontalLayoutGroup val9 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			res4.root = ((Component)(object)val9).gameObject;
			res4.getValue = () => val8.value;
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val9).transform)).SetText("x");
			FieldBuildResult x4 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val9).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val9).transform)).SetText("y");
			FieldBuildResult y4 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val9).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val9).transform)).SetText("z");
			FieldBuildResult z3 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val9).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val9).transform)).SetText("w");
			FieldBuildResult w2 = BuildFieldOfBasicType(typeof(float), ((Component)(object)val9).transform);
			x4.onChanged += (Action<object>)((object obj) =>
			{
				val8.value.x = (float)obj;
				res4.onChanged?.Invoke(val8.value);
			});
			y4.onChanged += (Action<object>)((object obj) =>
			{
				val8.value.y = (float)obj;
				res4.onChanged?.Invoke(val8.value);
			});
			z3.onChanged += (Action<object>)((object obj) =>
			{
				val8.value.z = (float)obj;
				res4.onChanged?.Invoke(val8.value);
			});
			w2.onChanged += (Action<object>)((object obj) =>
			{
				val8.value.w = (float)obj;
				res4.onChanged?.Invoke(val8.value);
			});
			res4.setValue = (object obj) =>
			{
				val8.value = (Quaternion)obj;
				x4.setValue(val8.value.x);
				y4.setValue(val8.value.y);
				z3.setValue(val8.value.z);
				w2.setValue(val8.value.w);
			};
			return res4;
		}
		if (type == typeof(Vector2Int))
		{
			RefValue<Vector2Int> val10 = new RefValue<Vector2Int>();
			FieldBuildResult res5 = new FieldBuildResult();
			HorizontalLayoutGroup val11 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			res5.root = ((Component)(object)val11).gameObject;
			res5.getValue = () => val10.value;
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val11).transform)).SetText("x");
			FieldBuildResult x5 = BuildFieldOfBasicType(typeof(int), ((Component)(object)val11).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val11).transform)).SetText("y");
			FieldBuildResult y5 = BuildFieldOfBasicType(typeof(int), ((Component)(object)val11).transform);
			x5.onChanged += (Action<object>)((object obj) =>
			{
				val10.value.x = (int)obj;
				res5.onChanged?.Invoke(val10.value);
			});
			y5.onChanged += (Action<object>)((object obj) =>
			{
				val10.value.y = (int)obj;
				res5.onChanged?.Invoke(val10.value);
			});
			res5.setValue = (object obj) =>
			{
				val10.value = (Vector2Int)obj;
				x5.setValue(val10.value.x);
				y5.setValue(val10.value.y);
			};
			return res5;
		}
		if (type == typeof(Vector3Int))
		{
			RefValue<Vector3Int> val12 = new RefValue<Vector3Int>();
			FieldBuildResult res6 = new FieldBuildResult();
			HorizontalLayoutGroup val13 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			res6.root = ((Component)(object)val13).gameObject;
			res6.getValue = () => val12.value;
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val13).transform)).SetText("x");
			FieldBuildResult x6 = BuildFieldOfBasicType(typeof(int), ((Component)(object)val13).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val13).transform)).SetText("y");
			FieldBuildResult y6 = BuildFieldOfBasicType(typeof(int), ((Component)(object)val13).transform);
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val13).transform)).SetText("z");
			FieldBuildResult z4 = BuildFieldOfBasicType(typeof(int), ((Component)(object)val13).transform);
			x6.onChanged += (Action<object>)((object obj) =>
			{
				val12.value.x = (int)obj;
				res6.onChanged?.Invoke(val12.value);
			});
			y6.onChanged += (Action<object>)((object obj) =>
			{
				val12.value.y = (int)obj;
				res6.onChanged?.Invoke(val12.value);
			});
			z4.onChanged += (Action<object>)((object obj) =>
			{
				val12.value.z = (int)obj;
				res6.onChanged?.Invoke(val12.value);
			});
			res6.setValue = (object obj) =>
			{
				val12.value = (Vector3Int)obj;
				x6.setValue(val12.value.x);
				y6.setValue(val12.value.y);
				z4.setValue(val12.value.z);
			};
			return res6;
		}
		throw new ArgumentOutOfRangeException("type");
	}

	public static FieldBuildResult GenericInputFieldBuilder<T>(Transform parent, TryParseFunc<T> tryParse, CharacterValidation validation, out TMP_InputField inputField)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		RefValue<T> val = new RefValue<T>();
		FieldBuildResult result = new FieldBuildResult();
		TMP_InputField newField = UnityEngine.Object.Instantiate<TMP_InputField>(widgetInputField, parent).SetExpandWidth<TMP_InputField>(1f);
		inputField = newField;
		Image bg = ((Component)(object)newField).GetComponent<Image>();
		Color colorDefault = ((Graphic)bg).color;
		Color colorInvalid = Color.Lerp(((Graphic)bg).color, Color.red, 0.35f);
		((UnityEvent<string>)(object)newField.onValueChanged).AddListener((UnityAction<string>)((string newStrValue) =>
		{
			if (tryParse(newStrValue, out var result2))
			{
				((Graphic)bg).color = colorDefault;
				result.onChanged?.Invoke(result2);
			}
			else
			{
				((Graphic)bg).color = colorInvalid;
			}
		}));
		newField.characterValidation = validation;
		result.getValue = () => val.value;
		result.setValue = (object obj) =>
		{
			val.value = (T)obj;
			newField.text = ((obj != null) ? obj.ToString() : "");
		};
		result.root = ((Component)(object)newField).gameObject;
		return result;
	}

	public static void CreateWidgetsForObject(Type type, object state, Transform parent, out SafeAction onChanged, out SafeAction requestUpdate)
	{
		if (type.IsAbstract)
		{
			throw new InvalidOperationException("Provided ModConfig cannot be abstract");
		}
		SafeAction _onChanged = new SafeAction();
		onChanged = _onChanged;
		SafeAction safeAction = new SafeAction();
		requestUpdate = safeAction;
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			FieldInfo f = fieldInfo;
			if ((!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) || f.GetCustomAttribute<HideInInspector>() != null)
			{
				continue;
			}
			foreach (Attribute customAttribute3 in f.GetCustomAttributes())
			{
				if (customAttribute3 is HeaderAttribute headerAttribute)
				{
					TextMeshProUGUI val = UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextHeader, parent);
					((TMP_Text)val).margin = ((TMP_Text)val).margin.WithY(15f);
					((TMP_Text)val).SetText(headerAttribute.header);
				}
				if (customAttribute3 is SpaceAttribute spaceAttribute)
				{
					GameObject gameObject = new GameObject("Space", typeof(LayoutElement));
					gameObject.GetComponent<LayoutElement>().preferredHeight = spaceAttribute.height;
					gameObject.transform.SetParent(parent);
				}
			}
			FieldBuilder item = fieldBuilders.Find(((FieldBuilderCondition, FieldBuilder) tuple) => tuple.Item1(f.FieldType, f)).Item2;
			if (item == null)
			{
				continue;
			}
			HorizontalLayoutGroup val2 = CreateHorizontalLayoutGroup(parent, (TextAnchor)3);
			ModConfig.LabelTextAttribute customAttribute = f.GetCustomAttribute<ModConfig.LabelTextAttribute>();
			string text = ((customAttribute != null) ? customAttribute.text : f.Name.NicifyVariableName());
			((TMP_Text)UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextLabel, ((Component)(object)val2).transform).SetExpandWidth<TextMeshProUGUI>(true)).SetText(text);
			FieldBuildResult result = item(f.FieldType, f, ((Component)(object)val2).transform);
			requestUpdate.Add(() =>
			{
				object value = f.GetValue(state);
				result.setValue(value);
			});
			result.onChanged += (Action<object>)((object obj) =>
			{
				f.SetValue(state, obj);
				try
				{
					_onChanged.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			});
			ModConfig.DescriptionAttribute customAttribute2 = f.GetCustomAttribute<ModConfig.DescriptionAttribute>();
			if (customAttribute2 != null)
			{
				TextMeshProUGUI val3 = UnityEngine.Object.Instantiate<TextMeshProUGUI>(widgetTextBody, parent);
				((TMP_Text)val3).SetText(customAttribute2.text);
				((Graphic)val3).color = Color.Lerp(((Graphic)val3).color, Color.white, 0.5f);
				((TMP_Text)val3).margin = ((TMP_Text)val3).margin.WithY(-15f);
				((TMP_Text)val3).margin = ((TMP_Text)val3).margin.WithW(10f);
				((TMP_Text)val3).fontSize = ((TMP_Text)val3).fontSize * 0.85f;
			}
		}
		requestUpdate.Invoke();
	}

	public static HorizontalLayoutGroup CreateHorizontalLayoutGroup(Transform parent, TextAnchor alignment = (TextAnchor)3)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = new GameObject("Horizontal Layout Group");
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		HorizontalLayoutGroup val = gameObject.AddComponent<HorizontalLayoutGroup>();
		ApplyLayoutGroupDefaultSettings(val, alignment);
		return val;
	}

	public static VerticalLayoutGroup CreateVerticalLayoutGroup(Transform parent, TextAnchor alignment = (TextAnchor)1)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = new GameObject("Vertical Layout Group");
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		VerticalLayoutGroup val = gameObject.AddComponent<VerticalLayoutGroup>();
		ApplyLayoutGroupDefaultSettings(val, alignment);
		return val;
	}

	public static void ApplyLayoutGroupDefaultSettings(HorizontalLayoutGroup group, TextAnchor alignment = (TextAnchor)3)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		((HorizontalOrVerticalLayoutGroup)group).spacing = 10f;
		((HorizontalOrVerticalLayoutGroup)group).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)group).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)group).childScaleHeight = true;
		((HorizontalOrVerticalLayoutGroup)group).childScaleWidth = true;
		((HorizontalOrVerticalLayoutGroup)group).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)group).childForceExpandWidth = false;
		((LayoutGroup)group).childAlignment = alignment;
	}

	public static void ApplyLayoutGroupDefaultSettings(VerticalLayoutGroup group, TextAnchor alignment = (TextAnchor)1)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		((HorizontalOrVerticalLayoutGroup)group).spacing = 10f;
		((HorizontalOrVerticalLayoutGroup)group).childControlHeight = true;
		((HorizontalOrVerticalLayoutGroup)group).childControlWidth = true;
		((HorizontalOrVerticalLayoutGroup)group).childScaleHeight = true;
		((HorizontalOrVerticalLayoutGroup)group).childScaleWidth = true;
		((HorizontalOrVerticalLayoutGroup)group).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)group).childForceExpandWidth = false;
		((LayoutGroup)group).childAlignment = alignment;
	}

	public static LayoutElement CreateHorizontalFlexibleSpace(Transform parent)
	{
		GameObject gameObject = new GameObject("Horizontal Flexible Space");
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		LayoutElement val = gameObject.AddComponent<LayoutElement>();
		val.flexibleWidth = 1f;
		return val;
	}

	public static LayoutElement CreateVerticalFlexibleSpace(Transform parent)
	{
		GameObject gameObject = new GameObject("Vertical Flexible Space");
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		LayoutElement val = gameObject.AddComponent<LayoutElement>();
		val.flexibleHeight = 1f;
		return val;
	}

	private static LayoutElement GetLayoutElement(this Component c)
	{
		if (!c.TryGetComponent<LayoutElement>(out var component))
		{
			return c.gameObject.AddComponent<LayoutElement>();
		}
		return component;
	}

	private static LayoutElement GetLayoutElement(this GameObject c)
	{
		if (!c.TryGetComponent<LayoutElement>(out var component))
		{
			return c.AddComponent<LayoutElement>();
		}
		return component;
	}

	public static T SetText<T>(this T c, string text) where T : Component
	{
		DewLocalizedText componentInChildren = c.GetComponentInChildren<DewLocalizedText>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.Destroy(componentInChildren);
		}
		((TMP_Text)c.GetComponentInChildren<TextMeshProUGUI>()).text = text;
		return c;
	}

	public static GameObject SetText(this GameObject c, string text)
	{
		DewLocalizedText componentInChildren = c.GetComponentInChildren<DewLocalizedText>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.Destroy(componentInChildren);
		}
		((TMP_Text)c.GetComponentInChildren<TextMeshProUGUI>()).text = text;
		return c;
	}

	public static T SetTextLocalized<T>(this T c, string key) where T : Component
	{
		((TMP_Text)c.GetComponentInChildren<TextMeshProUGUI>()).text = DewLocalization.GetUIValue(key);
		return c;
	}

	public static GameObject SetTextLocalized(this GameObject c, string key)
	{
		((TMP_Text)c.GetComponentInChildren<TextMeshProUGUI>()).text = DewLocalization.GetUIValue(key);
		return c;
	}

	public static T SetWidth<T>(this T c, float width) where T : Component
	{
		c.GetLayoutElement().preferredWidth = width;
		return c;
	}

	public static T SetHeight<T>(this T c, float height) where T : Component
	{
		c.GetLayoutElement().preferredHeight = height;
		return c;
	}

	public static GameObject SetWidth(this GameObject c, float width)
	{
		c.GetLayoutElement().preferredWidth = width;
		return c;
	}

	public static GameObject SetHeight(this GameObject c, float height)
	{
		c.GetLayoutElement().preferredHeight = height;
		return c;
	}

	public static GameObject SetExpandWidth(this GameObject c, bool expand)
	{
		c.GetLayoutElement().flexibleWidth = (expand ? 1 : 0);
		return c;
	}

	public static GameObject SetExpandHeight(this GameObject c, bool expand)
	{
		c.GetLayoutElement().flexibleHeight = (expand ? 1 : 0);
		return c;
	}

	public static GameObject SetExpandWidth(this GameObject c, float value)
	{
		c.GetLayoutElement().flexibleWidth = value;
		return c;
	}

	public static GameObject SetExpandHeight(this GameObject c, float value)
	{
		c.GetLayoutElement().flexibleHeight = value;
		return c;
	}

	public static T SetExpandWidth<T>(this T c, bool expand) where T : Component
	{
		c.GetLayoutElement().flexibleWidth = (expand ? 1 : 0);
		return c;
	}

	public static T SetExpandHeight<T>(this T c, bool expand) where T : Component
	{
		c.GetLayoutElement().flexibleHeight = (expand ? 1 : 0);
		return c;
	}

	public static T SetExpandWidth<T>(this T c, float value) where T : Component
	{
		c.GetLayoutElement().flexibleWidth = value;
		return c;
	}

	public static T SetExpandHeight<T>(this T c, float value) where T : Component
	{
		c.GetLayoutElement().flexibleHeight = value;
		return c;
	}
}
