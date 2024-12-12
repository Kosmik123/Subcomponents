using Bipolar.Subcomponents;
using UnityEngine;

[System.Serializable]
public class ExampleSubcomponentBase : SubBehavior
{
	[SerializeField]
	private string name;
}

[AddComponentMenu("Subcomponent Category/Compound Example")]
public class CompoundExample : CompoundBehavior<ExampleSubcomponentBase>
{
	[SerializeField]
	public int number;
}

[AddComponentMenu("Category/Subcomponent A", 10)]
public class ExampleSubcomponentA : ExampleSubcomponentBase
{
	public float power;
}

public class ExampleSubcomponentB : ExampleSubcomponentBase
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
	}
}

[AddComponentMenu("Category/SubCateee/Subcomponent C")]
public class ExampleSubcomponentC : ExampleSubcomponentBase
{
	public float power;
}

[AddComponentMenu("Category/Subcomponent D", 0)]
public class ExampleSubcomponentD : ExampleSubcomponentBase
{
	public int health;
}
