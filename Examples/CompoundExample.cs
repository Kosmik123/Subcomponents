using Bipolar.Subcomponents;
using UnityEngine;

[System.Serializable]
public class ExampleSubBehavior : SubBehavior
{
	[SerializeField]
	private string name;
}

[AddComponentMenu("Subcomponent Category/Compound Example")]
public class CompoundExample : CompoundBehavior<ExampleSubBehavior>
{
	[SerializeField]
	public int number;
}

[AddComponentMenu("Category/Subcomponent A", 10)]
public class ExampleSubcomponentA : ExampleSubBehavior
{
	public float power;
}

public class ExampleSubcomponentB : ExampleSubBehavior
{
	public int health;
	public Rect rect;
	public Transform parent;
	public Vector4 matrix;
	public Color color;
	public Gradient background;
	public Texture image;
	public AnimationCurve animation;
	public bool isActive;
	public Quaternion rotation;
	public Vector3 position;

	private void Reset()
	{
		color = Color.white;
		Debug.Log("Reset B");
	}
}

[AddComponentMenu("Category/SubCateee/Subcomponent C")]
public class ExampleSubcomponentC : ExampleSubBehavior
{
	public float power;
}

[AddComponentMenu("Category/Subcomponent D", 0)]
public class ExampleSubcomponentD : ExampleSubBehavior
{
	public int health;
}
