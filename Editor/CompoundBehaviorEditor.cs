#define VOLUME

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bipolar.Subcomponents.Editor
{
	[CustomEditor(typeof(CompoundBehavior<>), editorForChildClasses: true)]
	public class CompoundBehaviorEditor : UnityEditor.Editor
	{
		private SerializedProperty componentsListProperty;
		private ICompoundBehavior compoundBehavior;

		private static readonly GUIContent buttonContent = new GUIContent($"Add Subcomponent");

		private void OnEnable()
		{
			componentsListProperty = serializedObject.FindProperty("subcomponents");
			compoundBehavior = target as ICompoundBehavior;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();
			if (componentsListProperty != null)
			{
				GUILayout.Space(10);
				bool changed = false;

				int count = compoundBehavior.SubcomponentsCount;

				EditorGUI.BeginChangeCheck();
				for (int i = 0; i < count; i++)
				{
					EditorUtility.DrawSplitter();
					var subcomponent = compoundBehavior.Subcomponents[i];
					var itemProperty = componentsListProperty.GetArrayElementAtIndex(i);
					DrawSubcomponent(itemProperty, subcomponent);
				}
				changed |= EditorGUI.EndChangeCheck();

				EditorUtility.DrawSplitter();
				GUILayout.Space(6);

				var buttonRect = EditorGUILayout.GetControlRect();
				if (count >= 0 && EditorGUI.DropdownButton(buttonRect, buttonContent, FocusType.Keyboard, EditorStyles.miniButton))
				{
					var popupRect = buttonRect;
					popupRect.width = 250;
					popupRect.center = buttonRect.center;
					var popup = AddSubcomponentPopup.Get(compoundBehavior.SubcomponentsType);
					popup.Show(popupRect);
					popup.OnItemSelected += AddSubcomponentFromButton;
					changed = true;
				}
				GUILayout.Space(6);

				if (changed)
				{
					serializedObject.ApplyModifiedProperties();
				}
			}
		}

		private void AddSubcomponentFromButton(AddSubcomponentPopup.Item item)
		{
			var subcomponentType = item.Type;
			int count = componentsListProperty.arraySize;

			componentsListProperty.InsertArrayElementAtIndex(count);
			var newSubcomponentProperty = componentsListProperty.GetArrayElementAtIndex(count);

			var newSubcomponent = Activator.CreateInstance(subcomponentType);
			CallReset(newSubcomponent);
			if (newSubcomponent is SubBehavior behavior)
				behavior.IsEnabled = true;
			
			newSubcomponentProperty.managedReferenceValue = newSubcomponent;
			serializedObject.ApplyModifiedProperties();
		}

		public static void DrawSubcomponent(SerializedProperty property, ISubcomponent subcomponent)
		{
#if VOLUME9
			const float height = 17f;
			var backgroundRect = GUILayoutUtility.GetRect(1f, height);

			var labelRect = backgroundRect;
			labelRect.xMin += 32f;
			labelRect.xMax -= 20f + 16 + 5;

			var foldoutRect = backgroundRect;
			foldoutRect.y += 1f;
			foldoutRect.width = 13f;
			foldoutRect.height = 13f;

			var toggleRect = backgroundRect;
			toggleRect.x += 16f;
			toggleRect.y += 2f;
			toggleRect.width = 13f;
			toggleRect.height = 13f;

			// Background rect should be full-width
			backgroundRect.xMin = 0f;
			backgroundRect.width += 4f;

			// Background
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			using (new EditorGUI.DisabledScope(!activeField.boolValue))
				EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

			// Foldout
			group.serializedObject.Update();
			group.isExpanded = GUI.Toggle(foldoutRect, group.isExpanded, GUIContent.none, EditorStyles.foldout);
			group.serializedObject.ApplyModifiedProperties();

			// Active checkbox
			activeField.serializedObject.Update();
			activeField.boolValue = GUI.Toggle(toggleRect, activeField.boolValue, GUIContent.none, CoreEditorStyles.smallTickbox);
			activeField.serializedObject.ApplyModifiedProperties();


			// Context menu
			Texture menuIcon = null;
			var menuRect = new Rect(labelRect.xMax + 3f + 16 + 5, labelRect.y + 1f, menuIcon.width, menuIcon.height);

			if (contextAction != null)
				GUI.DrawTexture(menuRect, menuIcon);

			// Documentation button
			if (!String.IsNullOrEmpty(documentationURL))
			{
				var documentationRect = menuRect;
				documentationRect.x -= 16 + 5;
				documentationRect.y -= 1;

				var documentationTooltip = $"Open Reference for {title.text}.";
				var documentationIcon = new GUIContent(EditorGUIUtility.TrIconContent("_Help").image, documentationTooltip);
				var documentationStyle = new GUIStyle("IconButton");

				if (GUI.Button(documentationRect, documentationIcon, documentationStyle))
					System.Diagnostics.Process.Start(documentationURL);
			}

			// Handle events
			var e = Event.current;

			if (e.type == EventType.MouseDown)
			{
				if (contextAction != null && menuRect.Contains(e.mousePosition))
				{
					contextAction(new Vector2(menuRect.x, menuRect.yMax));
					e.Use();
				}
				else if (labelRect.Contains(e.mousePosition))
				{
					if (e.button == 0)
						group.isExpanded = !group.isExpanded;
					else if (contextAction != null)
						contextAction(e.mousePosition);

					e.Use();
				}
			}

#else
			var headerRect = GUILayoutUtility.GetRect(1f, 18f); //EditorGUILayout.GetControlRect();

			var backgroundRect = headerRect;
			backgroundRect.xMin = 0;
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			headerRect.y -= 1;
			headerRect.x += 12;
			bool isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none);


			var toggleRect = headerRect;
			toggleRect.x += 4;
			toggleRect.y += 3;
			toggleRect.width = 20;

			if (subcomponent is SubBehavior behavior)
			{
				behavior.IsEnabled = EditorGUI.Toggle(toggleRect, behavior.IsEnabled, new GUIStyle("ShurikenToggle"));
			}

			var labelRect = headerRect;
			labelRect.xMin += 20f;
			labelRect.xMax -= 20f + 16 + 5;
			string typeName = subcomponent.GetType().Name;

			EditorGUI.LabelField(labelRect, ObjectNames.NicifyVariableName(typeName), EditorStyles.boldLabel);

			var ev = Event.current;
			if (ev.button == 0 && labelRect.Contains(ev.mousePosition) && ev.type == EventType.MouseUp)
			{
				isExpanded = !isExpanded;
				ev.Use();
			}
			else if (ev.button == 1 && headerRect.Contains(ev.mousePosition) && ev.type == EventType.MouseDown)
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Reset"), false, ResetProperty);
				menu.AddSeparator(string.Empty);
				menu.AddItem(new GUIContent("bar"), false, null, "Bar");
				menu.ShowAsContext();

				ev.Use();
			}

			void ResetProperty()
			{
				foreach (var childProperty in GetChildProperties(property, excludeEnabled: true))
				{
					childProperty.ResetProperty();
				}
				property.serializedObject.ApplyModifiedProperties();

				CallReset(subcomponent);
			}

			//var propertyRect = EditorGUILayout.GetControlRect();
			//EditorGUI.PropertyField(propertyRect, property);

			if (isExpanded)
			{
				using (new EditorGUI.IndentLevelScope())
				{

					//		// Check if a custom property drawer exists for this type.
					//		PropertyDrawer customDrawer = GetCustomPropertyDrawer(property);
					//		if (customDrawer != null)
					//		{
					//			// Draw the property with custom property drawer.
					//			Rect indentedRect = position;
					//			float foldoutDifference = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					//			indentedRect.height = customDrawer.GetPropertyHeight(property, label);
					//			indentedRect.y += foldoutDifference;
					//			customDrawer.OnGUI(indentedRect, property, label);
					//		}
					//		else
					//		{
					//			// Draw the properties of the child elements.
					//			// NOTE: In the following code, since the foldout layout isn't working properly, I'll iterate through the properties of the child elements myself.
					//			// EditorGUI.PropertyField(position, property, GUIContent.none, true);

					//Rect childPosition = position;
					//childPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					foreach (var childProperty in GetChildProperties(property, excludeEnabled: true))
					{
						EditorGUILayout.PropertyField(childProperty, true);
					}
					//		}


				}
				EditorGUILayout.Space(4);
			}
			property.isExpanded = isExpanded;
#endif
		}

		private static void CallReset(object subcomponent)
		{
			var resetMethod = subcomponent.GetType().GetResetMethod();
			resetMethod?.Invoke(subcomponent, Array.Empty<object>());
		}

		private static string GetPropertyTypeName(SerializedProperty property)
		{
			var typeName = property.type;
			if (property.propertyType != SerializedPropertyType.ManagedReference)
				return typeName;

			int typeNameStart = typeName.IndexOf('<') + 1;
			int typeNameEnd = typeName.LastIndexOf('>');
			int typeNameLength = typeNameEnd - typeNameStart;
			if (typeNameLength < 0)
				return string.Empty;

			return typeName.Substring(typeNameStart, typeNameLength);
		}

		public static IEnumerable<SerializedProperty> GetChildProperties(SerializedProperty parent, bool excludeEnabled)
		{
			int depthOfParent = parent.depth;
			var iterator = parent.Copy();
			bool searchChildren = true;
			while (iterator.Next(searchChildren))
			{
				searchChildren = false;
				if (iterator == null || iterator.depth <= depthOfParent)
					break;

				if (excludeEnabled && iterator.name == "<IsEnabled>k__BackingField")
					continue;

				yield return iterator.Copy();
			}
		}
		private void OnDisable()
		{
		}
	}

	public static class EditorUtility
	{
		public static void DrawSplitter(bool isBoxed = false)
		{
			var rect = GUILayoutUtility.GetRect(1f, 1f);
			float xMin = rect.xMin;
			rect.x -= 1;

			// Splitter rect should be full-width
			rect.xMin = 0f;
			rect.width += 4f;

			if (isBoxed)
			{
				rect.xMin = xMin == 7.0 ? 4.0f : EditorGUIUtility.singleLineHeight;
				rect.width -= 1;
			}

			if (Event.current.type != EventType.Repaint)
				return;

			EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
				? new Color(0.12f, 0.12f, 0.12f, 1.333f)
				: new Color(0.6f, 0.6f, 0.6f, 1.333f));
		}
	}

	public static class PropertyResetExtensions
	{
		public static void ResetProperty(this SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer:
					property.intValue = default;
					break;
				case SerializedPropertyType.Boolean:
					property.boolValue = default;
					break;
				case SerializedPropertyType.Float:
					property.floatValue = default;
					break;
				case SerializedPropertyType.String:
					property.stringValue = default;
					break;
				case SerializedPropertyType.Color:
					property.colorValue = default;
					break;
				case SerializedPropertyType.ObjectReference:
					property.objectReferenceValue = default;
					break;
				case SerializedPropertyType.LayerMask:
					property.intValue = default;
					break;
				case SerializedPropertyType.Enum:
					property.enumValueIndex = default;
					break;
				case SerializedPropertyType.Vector2:
					property.vector2Value = default;
					break;
				case SerializedPropertyType.Vector3:
					property.vector3Value = default;
					break;
				case SerializedPropertyType.Vector4:
					property.vector4Value = default;
					break;
				case SerializedPropertyType.Rect:
					property.rectValue = default;
					break;
				case SerializedPropertyType.ArraySize:
					property.arraySize = default;
					break;
				case SerializedPropertyType.Character:
					// what even is this?!
					break;
				case SerializedPropertyType.AnimationCurve:
					property.animationCurveValue = new AnimationCurve();
					break;
				case SerializedPropertyType.Bounds:
					property.boundsValue = default;
					break;
				case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
					property.gradientValue = defalt;
#endif
					break;
				case SerializedPropertyType.Quaternion:
					property.quaternionValue = default;
					break;
				case SerializedPropertyType.ExposedReference:
					property.exposedReferenceValue = default;
					break;
				case SerializedPropertyType.FixedBufferSize:
					// cannot modify 
					break;
				case SerializedPropertyType.Vector2Int:
					property.vector2IntValue = default;
					break;
				case SerializedPropertyType.Vector3Int:
					property.vector3IntValue = default;
					break;
				case SerializedPropertyType.RectInt:
					property.rectIntValue = default;
					break;
				case SerializedPropertyType.BoundsInt:
					property.boundsIntValue = default;
					break;
				case SerializedPropertyType.ManagedReference:
					property.managedReferenceValue = default;
					break;

#if UNITY_2021_1_OR_NEWER
				case SerializedPropertyType.Hash128:
					property.hash128Value = default;
					break;
#endif
			}
		}


		private static readonly Dictionary<Type, MethodInfo> resetMethodsCache = new Dictionary<Type, MethodInfo>();
		public static MethodInfo GetResetMethod(this Type type)
		{
			const BindingFlags resetBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			if (resetMethodsCache.TryGetValue(type, out var resetMethod) == false)
			{
				resetMethod = type.GetMethod("Reset", resetBindings);
				resetMethodsCache.Add(type, resetMethod);
			}
			return resetMethod;
		}
	}
}
