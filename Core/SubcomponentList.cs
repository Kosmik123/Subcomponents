using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Subcomponents
{
	internal interface ISubcomponentList : ISubcomponentOwner
	{
		int Count { get; }
		void Update();
	}

	[System.Serializable]
	public class SubcomponentList<TComponent> : ISubcomponentList, IList<TComponent>, IReadOnlyList<TComponent>
		where TComponent : ISubcomponent
	{
		[SerializeReference]
		[HideInInspector]
		protected List<TComponent> items = new List<TComponent>();
		protected readonly List<IUpdatable> updatableItems = new List<IUpdatable>();
		
		public TComponent this[int index]
		{
			get => items[index];
			set => items[index] = value;
		}

		public int Count => items.Count;
		public bool IsReadOnly => ((IList<TComponent>)items).IsReadOnly;

		public Type SubcomponentsType => typeof(TComponent);

		public void Add(TComponent item)
		{
			items.Add(item);
			if (item is IUpdatable updatable)
				updatableItems.Add(updatable);
		}

		public void Clear()
		{
			items.Clear();
			updatableItems.Clear();
		}

		public bool Contains(TComponent item) => items.Contains(item);
		public void CopyTo(TComponent[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
		public IEnumerator<TComponent> GetEnumerator() => items.GetEnumerator();
		public int IndexOf(TComponent item) => items.IndexOf(item);
		public void Insert(int index, TComponent item) => items.Insert(index, item);
		public bool Remove(TComponent item)
		{
			if (items.Remove(item) == false)
				return false;

			if (item is IUpdatable updatable)
				updatableItems.Remove(updatable);

			return true;
		}

		public void RemoveAt(int index)
		{
			var item = items[index];
			items.RemoveAt(index);
			if (item is IUpdatable updatable)
				updatableItems.Remove(updatable);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Enable()
		{
			SubcomponentListUpdater.Instance.AddList(this);
			foreach (var subcomponent in items.OfType<IEnableCallbackReceiver>())
				subcomponent.OnEnable();

			updatableItems.Clear();
			foreach (var subcomponent in items.OfType<IUpdatable>())
				updatableItems.Add(subcomponent);
		}

		public void Disable()
		{
			SubcomponentListUpdater.Instance.RemoveList(this);
			foreach (var subcomponent in items.OfType<IDisableCallbackReceiver>())
				subcomponent.OnDisable();

			updatableItems.Clear();
		}

	    void ISubcomponentList.Update()
		{
			for (int i = 0; i < updatableItems.Count; i++)
			{
				updatableItems[i].Update();
			}
		}
	}

	internal class SubcomponentListUpdater : MonoBehaviour
	{
		private static SubcomponentListUpdater instance;
		public static SubcomponentListUpdater Instance
		{
			get
			{
				if (instance = null)
					instance = new GameObject(string.Empty).AddComponent<SubcomponentListUpdater>();
				return instance;
			}
		}

		private List<ISubcomponentList> lists = new List<ISubcomponentList>();

		private void Awake()
		{
			if (instance != this)
				Destroy(gameObject);
		}

		public void AddList(ISubcomponentList list) => lists.Add(list);

		public void RemoveList(ISubcomponentList list) => lists.Remove(list);

		private void Update()
		{
			for (int i = 0; i < lists.Count; i++)
			{
				lists[i].Update();
			}
		}

		private void OnDestroy()
		{
			if (instance == this)
				instance = null;
		}
	}
}
