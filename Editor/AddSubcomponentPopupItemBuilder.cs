#define VOLUME

using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Bipolar.Subcomponents.Editor
{
	public class AddSubcomponentPopupItemBuilder
	{
		internal class Node
		{
			public string Name { get; set; } = "Subcomponent";

			private readonly List<Node> subfolders = new List<Node>();
			private readonly List<(AddSubcomponentPopup.Item item, int order)> items = new List<(AddSubcomponentPopup.Item item, int order)>();

			public void AddItem(Type type, string name, int order)
			{
				items.Add((new AddSubcomponentPopup.Item(type, name), order));
				items.Sort((lhs, rhs) => lhs.order.CompareTo(rhs.order));
			}

			public void AddItem(Type type) => items.Add((new AddSubcomponentPopup.Item(type), 20));

			public Node GetOrAddSubfolder(string name)
			{
				int index = subfolders.FindIndex(sub => sub.Name == name);
				if (index >= 0)
					return subfolders[index];

				var newSubfolder = new Node
				{
					Name = name
				};
				subfolders.Add(newSubfolder);
				subfolders.Sort((lhs, rhs) => lhs.Name.CompareTo(rhs.Name));
				return newSubfolder;
			}

			public void Clear()
			{
				Name = null;
				subfolders.Clear();
				items.Clear();
			}

			public AdvancedDropdownItem Build()
			{
				var builtItem = new AdvancedDropdownItem(Name);
				foreach (var subfolder in subfolders)
					builtItem.AddChild(subfolder.Build());

				foreach (var it in items)
					builtItem.AddChild(it.item);

				return builtItem;
			}
		}

		private readonly Node root = new Node();

		public void AddType(Type type, string[] path, int order)
		{
			var node = root;
			for (int i = 0; i < path.Length - 1; i++)
				node = node.GetOrAddSubfolder(path[i]);

			node.AddItem(type, path[path.Length - 1], order);
		}

		public void AddType(Type type) => root.AddItem(type);

		public void Clear() => root.Clear();

		public AdvancedDropdownItem Build() => root.Build();
	}
}
