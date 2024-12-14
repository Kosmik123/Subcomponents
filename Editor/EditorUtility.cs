using System;
using UnityEditor;
using UnityEngine;

namespace Bipolar.Subcomponents.Editor
{
	public static class EditorUtility
	{
		public static readonly GUIContent ButtonContent = new GUIContent($"Add Subcomponent");

		public static void DrawSplitterLayout(bool isBoxed = false)
		{
			var rect = GUILayoutUtility.GetRect(1, 1);
			DrawSplitter(rect.y, isBoxed);
		}

		public static void DrawSplitter(float position, bool isBoxed = false)
		{
			float width = EditorGUIUtility.currentViewWidth;
			var rect = new Rect(0, position, 0, 1);
			rect.width += width;
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


		public static void AddSubcomponent(Type subcomponentType, SerializedProperty listProperty)
		{
			int count = listProperty.arraySize;

			listProperty.InsertArrayElementAtIndex(count);
			var newSubcomponentProperty = listProperty.GetArrayElementAtIndex(count);

			var newSubcomponent = Activator.CreateInstance(subcomponentType);
			CallReset(newSubcomponent);
			if (newSubcomponent is SubBehavior behavior)
				behavior.IsEnabled = true;

			newSubcomponentProperty.managedReferenceValue = newSubcomponent;
		}

		public static void CallReset(object subcomponent)
		{
			var resetMethod = subcomponent.GetType().GetResetMethod();
			resetMethod?.Invoke(subcomponent, Array.Empty<object>());
		}
	}
}
