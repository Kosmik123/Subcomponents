using UnityEditor;
using UnityEngine;

namespace Bipolar.Subcomponents.Editor
{
	[CustomPropertyDrawer(typeof(SubcomponentList<>), useForChildren: true)]
	public class SubcomponentListDrawer : PropertyDrawer
	{
		private const string itemsListPropertyName = "items";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var itemsListProperty = property.FindPropertyRelative(itemsListPropertyName);
			var itemsCount = itemsListProperty.arraySize;

			float height = 52;
			for (int i = 0; i < itemsCount; i++)
			{
				var subcomponentProperty = itemsListProperty.GetArrayElementAtIndex(i);
				height += EditorGUI.GetPropertyHeight(subcomponentProperty);
			}
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			EditorUtility.DrawSplitter(position.y + 4);

			var owner = property.serializedObject;
			var listProperty = property.FindPropertyRelative(itemsListPropertyName);
			var itemsCount = listProperty.arraySize;
			for (int i = 0; i < itemsCount; i++)
			{
				var subcomponentProperty = listProperty.GetArrayElementAtIndex(i);


			}

			//SubcomponentListEditor.Draw(listProperty, owner);

			var buttonRect = position;
			buttonRect.y += 12;
			buttonRect.height = 20;
			var rect = position;
			rect.y += 10;
			rect.height = 10;
			EditorGUI.DrawRect(rect, Color.clear);
			if (EditorGUI.DropdownButton(buttonRect, EditorUtility.ButtonContent, FocusType.Keyboard, EditorStyles.miniButton)) 
			{
				var listType = fieldInfo.FieldType;
				while (listType.BaseType != typeof(object))
					listType = listType.BaseType;
				var subcomponentType = listType.GetGenericArguments()[0];

				var popupRect = buttonRect;
				popupRect.width = 250;
				popupRect.center = buttonRect.center;
				var popup = AddSubcomponentPopup.Get(subcomponentType);
				popup.Show(popupRect);
			}

			EditorUtility.DrawSplitter(buttonRect.yMax + 12);


			EditorGUI.EndProperty();
		}
	}
}
