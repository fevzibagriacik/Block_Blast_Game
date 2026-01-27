using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public int width = 8;
    public int height = 8;

    public int conditionA = 4; 
    public int conditionB = 6; 
    public int conditionC = 8; 

    [System.Serializable]
    public struct BlockSet
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

    public BlockSet[] blockAssetSets;

    public int colorNumber => blockAssetSets.Length; 
}
