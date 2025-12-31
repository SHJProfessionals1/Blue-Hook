using UnityEngine;

[CreateAssetMenu(menuName = "Store/Category")]
public class StoreCategorySO : ScriptableObject
{
	public StoreCategoryId id;
	public string displayName;

	public Sprite iconSelected;
	public Sprite iconNotSelected;
}
