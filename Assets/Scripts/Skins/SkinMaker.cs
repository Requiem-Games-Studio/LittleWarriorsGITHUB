using UnityEngine;
// Use the CreateAssetMenu attribute to allow creating instances of this ScriptableObject from the Unity Editor.
[CreateAssetMenu(fileName = "Skin", menuName = "Skin/NewSkin", order = 1)]
public class SkinMaker : ScriptableObject
{
    public Sprite brazoDerecho;
    public Sprite brazoIzquierdo;
    public Sprite cuerpo;
    public Sprite ojoDerecho;
    public Sprite ojoIzquierdo;
    public Sprite pieDerecho;
    public Sprite pieIzquierdo;
}