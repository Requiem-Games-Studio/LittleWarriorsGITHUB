using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsManager : MonoBehaviour
{
    public static SkinsManager instance;

    public SkinMaker PJ_AMARILLO;
    public SkinMaker PJ_AZUL;
    public SkinMaker PJ_CYAN;
    public SkinMaker PJ_MAGENTA;
    public SkinMaker PJ_MORADO;
    public SkinMaker PJ_NARANJA;
    public SkinMaker PJ_PRINCIPAL;
    public SkinMaker PJ_ROJO;
    public SkinMaker PJ_VERDE;

    public SkinMaker skinSelected;

    public Dictionary<int, SkinMaker> skinsInt = new Dictionary<int, SkinMaker>();

    private void Awake()
    {
        instance = this;

        skinsInt.Clear();
        skinsInt.Add(0, PJ_AMARILLO);
        skinsInt.Add(1, PJ_AZUL);
        skinsInt.Add(2, PJ_CYAN);
        skinsInt.Add(3, PJ_MAGENTA);
        skinsInt.Add(4, PJ_MORADO);
        skinsInt.Add(5, PJ_NARANJA);
        skinsInt.Add(6, PJ_PRINCIPAL);
        skinsInt.Add(7, PJ_ROJO);
        skinsInt.Add(8, PJ_VERDE);
    }

    public SkinMaker GetSkinValue(int skinNumber)
    {
        skinsInt.TryGetValue(skinNumber, out skinSelected);

        return skinSelected;
    }

    public int GetSkinID(SkinMaker skinObject)
    {
        foreach (var pair in skinsInt)
        {
            if (pair.Value == skinObject)
            {
                return pair.Key;
            }
        }

        Debug.LogWarning("Skin no encontrada en el diccionario.");
        return -1; // Retorna -1 si no se encuentra
    }
}
