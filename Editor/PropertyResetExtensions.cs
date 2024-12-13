using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bipolar.Subcomponents.Editor
{
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
