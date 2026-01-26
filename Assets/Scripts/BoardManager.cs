using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameSettings settings;

    public GameObject blockPrefab;

    public Block[,] grid;

    public List<GameObject> blockPool;

    public Transform boardHolder;

    private void Start()
    {
        FillBoard();
    }

    public void FillBoard()
    {
        grid = new Block[settings.width, settings.height];

        for (int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                GameObject go = GetBlock();

                float xPos = x - (settings.width / 2f) + 0.5f;
                float yPos = y - (settings.height / 2f) + 0.5f;
                go.transform.position = new Vector3(xPos, yPos, 0f);

                Block blockScript = go.GetComponent<Block>();
                blockScript.Init(x, y, settings);
                grid[x, y] = blockScript;
            }
        }
    }

    public GameObject GetBlock()
    {
        if(blockPool.Count > 0)
        {
            GameObject block = blockPool[blockPool.Count - 1];
            blockPool.RemoveAt(blockPool.Count - 1);
            block.SetActive(true);
            return block;
        }

        GameObject newBlock = Instantiate(blockPrefab, boardHolder.transform);
        return newBlock;
    }
}
