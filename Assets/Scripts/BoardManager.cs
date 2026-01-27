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

    public bool isClicked = false;

    private void Start()
    {
        FillBoard();

        UpdateAllIcons();
    }

    private void Update()
    {
        RaycastControl();
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

    public void FindNeighboor(int x, int y, Types.ColorTypes targetColor, List<Block> matchedBlocks)
    {
        if (x < 0 || x >= settings.width || y < 0 || y >= settings.height) return;

        Block currentBlock = grid[x, y];

        if (currentBlock == null) return;

        if (matchedBlocks.Contains(currentBlock)) return;

        if (targetColor != currentBlock.color) return;

        matchedBlocks.Add(currentBlock);

        FindNeighboor(x + 1, y, targetColor, matchedBlocks); 
        FindNeighboor(x - 1, y, targetColor, matchedBlocks); 
        FindNeighboor(x, y + 1, targetColor, matchedBlocks);
        FindNeighboor(x, y - 1, targetColor, matchedBlocks);
    }

    public void CheckMatches(int x, int y)
    {
        Block startBlock = grid[x, y];

        if (startBlock == null) return;

        Types.ColorTypes targetColor = startBlock.color;

        List<Block> matchedBlocks = new List<Block>();

        FindNeighboor(x, y, targetColor, matchedBlocks);

        if(matchedBlocks.Count >= 2){
            foreach(Block b in matchedBlocks)
            {
                grid[b.x, b.y] = null;
                b.gameObject.SetActive(false);
                blockPool.Add(b.gameObject);

                ApplyGravity();
                RefillBoard();
                UpdateAllIcons();
            }
        }
    }

    public void RaycastControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isClicked = true;

            //Transform the screen mouse position to world point
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Create a ray at the clicked point
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                //Access to script of clickedBlock
                Block clickedBlock = hit.collider.GetComponent<Block>();

                if (clickedBlock != null)
                {
                    CheckMatches(clickedBlock.x, clickedBlock.y);
                }
            }
        }
    }

    public void ApplyGravity()  
    {
        for(int x = 0; x < settings.width; x++)
        {
            List<Block> tempList = new List<Block>();

            for (int y = 0; y < settings.height; y++)
            {
                Block currentBlock = grid[x, y];

                if(currentBlock != null)
                {
                    tempList.Add(currentBlock);
                }
            }

            for (int y = 0; y < settings.height; y++)
            {
                grid[x, y] = null;
            }

            for(int i = 0; i < tempList.Count; i++)
            {
                Block b = tempList[i];

                grid[x, i] = b;

                b.x = x;
                b.y = i;

                float xPos = x - (settings.width / 2f) + 0.5f;
                float yPos = i - (settings.height / 2f) + 0.5f;

                b.transform.position = new Vector3(xPos, yPos, 0f);
            }
        }
    }

    public void RefillBoard()
    {
        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                Block currentBlock = grid[x, y];

                if (currentBlock != null)
                {
                    continue;
                }
                else
                {
                    GameObject go = GetBlock();

                    float xPos = x - (settings.width / 2f) + 0.5f;
                    float yPos = y - (settings.height / 2f) + 0.5f;
                    go.transform.position = new Vector3(xPos, yPos, 0f);

                    Block newBlock = go.GetComponent<Block>();
                    newBlock.Init(x, y, settings);

                    grid[x, y] = newBlock;
                }

            }
        }
    }

    public void UpdateAllIcons()
    {
        bool[,] visitedBlocks = new bool[settings.width, settings.height];

        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                Block currentBlock = grid[x, y];

                if (currentBlock == null) continue;

                if (visitedBlocks[x, y] == true) continue;

                List<Block> matchedBlocks = new List<Block>();

                Types.ColorTypes targetColor = currentBlock.color;

                FindNeighboor(x, y, targetColor, matchedBlocks);

                foreach (Block b in matchedBlocks)
                {
                    b.updateIcon(matchedBlocks.Count);
                    visitedBlocks[b.x, b.y] = true;
                }
            }
        }
    }
}
