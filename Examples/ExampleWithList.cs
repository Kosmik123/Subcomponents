using UnityEngine;
using Bipolar.Subcomponents;

public class ExampleWithList : MonoBehaviour
{
	public Vector3 oneField;

	[SerializeField]
	private SubcomponentList<ExampleSubBehavior> subBehaviors;

	public SubcomponentList<ExampleSubBehavior> subBehaviors2;

	[SerializeField]
	private Sprite secondField;
}
