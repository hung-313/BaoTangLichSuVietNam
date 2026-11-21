using UnityEngine;

/// <summary>
/// Dữ liệu của một hiện vật. Tạo mới: chuột phải trong Project > Create > Museum > Exhibit Data.
/// </summary>
[CreateAssetMenu(fileName = "NewExhibit", menuName = "Museum/Exhibit Data")]
public class ExhibitData : ScriptableObject
{
    public string title;
    public string era;
    [TextArea(4, 12)] public string description;
    public Sprite image; // không bắt buộc
}
