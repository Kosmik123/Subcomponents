using System;
using System.Collections.Generic;
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
					DrawSubcomponent(itemProperty, i, subcomponent);
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

		public void DrawSubcomponent(SerializedProperty property, int index, ISubcomponent subcomponent)
		{
			var headerRect = GUILayoutUtility.GetRect(1f, 18f);

			var backgroundRect = headerRect;
			backgroundRect.xMin = 0;
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			headerRect.y -= 1;
			headerRect.x += 12;
			bool isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none);

			var documentationIcon = new GUIContent(EditorGUIUtility.TrIconContent("_Help").image);

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
				menu.AddItem(new GUIContent("Remove Subcomponent"), false, RemoveSubcomponent);
				menu.AddItem(new GUIContent("Move Up"), false, index <= 0 ? default(GenericMenu.MenuFunction) : MoveUp);
				menu.AddItem(new GUIContent("Move Down"), false, index >= componentsListProperty.arraySize - 1 ? default(GenericMenu.MenuFunction) : MoveDown);
				menu.ShowAsContext();

				ev.Use();
			}

			void ResetProperty()
			{
				foreach (var childProperty in GetChildProperties(property, excludeEnabled: true))
				{
					childProperty.ResetProperty();
				}

				CallReset(subcomponent);
				serializedObject.ApplyModifiedProperties();
			}

			if (isExpanded)
			{
				using (new EditorGUI.IndentLevelScope())
				{
					// TODO: Add possibility to use custom editor for subcomponents

					foreach (var childProperty in GetChildProperties(property, excludeEnabled: true))
					{
						EditorGUILayout.PropertyField(childProperty, true);
					}
				}
				EditorGUILayout.Space(4);
			}
			property.isExpanded = isExpanded;


			void RemoveSubcomponent()
			{
				componentsListProperty.DeleteArrayElementAtIndex(index);
				serializedObject.ApplyModifiedProperties();
			}

			void MoveUp() => Swap(index - 1);

			void MoveDown() => Swap(index + 1);

			void Swap(int newIndex)
			{
				bool wasExpanded = componentsListProperty.GetArrayElementAtIndex(index).isExpanded;
				bool otherWasExpanded = componentsListProperty.GetArrayElementAtIndex(newIndex).isExpanded;
				componentsListProperty.MoveArrayElement(index, newIndex);
				componentsListProperty.GetArrayElementAtIndex(index).isExpanded = otherWasExpanded;
				componentsListProperty.GetArrayElementAtIndex(newIndex).isExpanded = wasExpanded;
				serializedObject.ApplyModifiedProperties();
			}
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
		{ }
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
}
