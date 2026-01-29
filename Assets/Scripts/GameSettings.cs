using UnityEngine;

[CreateAssetMenu(fileName = "NewGameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Range(3, 20)] public int width = 8;
    [Range(3, 20)] public int height = 8;

    public int conditionA = 4;
    public int conditionB = 6;
    public int conditionC = 8;

    public BlockSet[] blockAssetSets; 

    public int colorNumber => blockAssetSets.Length;
}

[System.Serializable]
public class BlockSet
{
    public string colorName;
    public Sprite Default;
    public Sprite TypeA;
    public Sprite TypeB;
    public Sprite TypeC;

    public Sprite GetSprite(Types.IconTypes type)
    {
        switch (type)
        {
            case Types.IconTypes.normal: return Default;
            case Types.IconTypes.A: return TypeA;
            case Types.IconTypes.B: return TypeB;
            case Types.IconTypes.C: return TypeC;
            default: return Default;
        }
    }
}