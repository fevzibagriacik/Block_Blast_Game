using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Types.ColorTypes color;
    public Types.IconTypes icon;

    public SpriteRenderer boardImage;

    public int x;
    public int y;

    private GameSettings settings;
    public void Init(int _x, int _y, GameSettings _settings)
    {
        x = _x;
        y = _y;
        settings = _settings;

        int randomColorIndex = Random.Range(0, _settings.colorNumber);
        color = (Types.ColorTypes)randomColorIndex;

        icon = Types.IconTypes.normal;  

        boardImage.sprite = _settings.blockAssetSets[randomColorIndex].GetSprite(Types.IconTypes.normal);

        gameObject.name = $"Block {x},{y}";
    }

    public void updateIcon(int groupNumber)
    {
        Types.IconTypes oldIcon = icon;
        Types.IconTypes newType;

        if (groupNumber < settings.conditionA)
        {
            newType = Types.IconTypes.normal;
        }
        else if (groupNumber >= settings.conditionA && groupNumber < settings.conditionB)
        {
            newType = Types.IconTypes.A;
        }
        else if (groupNumber >= settings.conditionB && groupNumber < settings.conditionC)
        {
            newType = Types.IconTypes.B;
        }
        else
        {
            newType = Types.IconTypes.C;
        }

        if (oldIcon != newType)
        {
            icon = newType;

            int colorIndex = (int)color;
            boardImage.sprite = settings.blockAssetSets[colorIndex].GetSprite(newType);
        }
    }

    public void SetCoordinates(int newX, int newY)
    {
        x = newX;
        y = newY;

        gameObject.name = $"Block {x},{y}";

        if (settings != null)
        {
            float screenX = x - (settings.width / 2f) + 0.5f;
            float screenY = y - (settings.height / 2f) + 0.5f;

            transform.position = new Vector3(screenX, screenY, 0f);
        }
    }
}
