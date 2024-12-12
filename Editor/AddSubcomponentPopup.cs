using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Bipolar.Subcomponents.Editor
{
	internal class AddSubcomponentPopup : AdvancedDropdown
	{
		public class Item : AdvancedDropdownItem
		{
			public Type Type { get; }

			public Item(Type type) : this(type, ObjectNames.NicifyVariableName(type.Name))
			{ }

			public Item(Type type, string name) : base(name)
			{
				Type = type;
			}
		}

		private static readonly Dictionary<Type, AddSubcomponentPopup> cachedPopups = new Dictionary<Type, AddSubcomponentPopup>();

		public event Action<Item> OnItemSelected;

		public Type SubcomponentType { get; private set; }

		private AdvancedDropdownItem root;

		public static AddSubcomponentPopup Get(Type subcomponentType)
		{
			if (cachedPopups.TryGetValue(subcomponentType, out var popup) == false)
			{
				popup = new AddSubcomponentPopup(subcomponentType);
				cachedPopups.Add(subcomponentType, popup);
			}
			popup.OnItemSelected = null;
			return popup;
		}

		private AddSubcomponentPopup(Type subcomponentType) : base(new AdvancedDropdownState())
		{
			SubcomponentType = subcomponentType;
			var types = TypeCache.GetTypesDerivedFrom(SubcomponentType);
			var builder = new AddSubcomponentPopupItemBuilder();	
			foreach (var type in types)
			{
				if (type.IsDefined(typeof(AddComponentMenu), true))
				{
					var attribute = (AddComponentMenu)type.GetCustomAttributes(typeof(AddComponentMenu), true)[0];
					var path = attribute.componentMenu;
					string[] pathItems = path.Contains('/')
						? path.Split('/')
						: path.Split('\\');

					var subcomponentName = pathItems[pathItems.Length - 1];
					if (string.IsNullOrWhiteSpace(subcomponentName) == false)
					{
						builder.AddType(type, pathItems, attribute.componentOrder);
						continue;
					}
				}
				builder.AddType(type);
			}

			root = builder.Build();
		}

		protected override AdvancedDropdownItem BuildRoot() => root;

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			base.ItemSelected(item);
			if (item is Item addSubcomponentItem)
			{
				OnItemSelected?.Invoke(addSubcomponentItem);
			}
		}
	}
}
