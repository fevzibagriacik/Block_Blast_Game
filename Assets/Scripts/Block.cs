using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Types.ColorTypes color;
    public Types.IconTypes icon;

    public SpriteRenderer boardImage;
    public SpriteRenderer iconImage;

    public int x;
    public int y;

    public void Init(int _x, int _y, GameSettings settings)
    {
        x = _x;
        y = _y;

        int randomColorIndex = Random.Range(0, settings.colorNumber);
        color = (Types.ColorTypes)randomColorIndex;
        boardImage.sprite = settings.colorSprites[randomColorIndex];

        icon = Types.IconTypes.normal;
        if(iconImage != null)
        {
            iconImage.sprite = null;
        }

        gameObject.name = $"Block {x},{y}";
    }
}
