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

    public bool moveExist = false;

    private void Start()
    {
        FillBoard();
        UpdateAllIcons();
        DeadlockControl();
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

                blockScript.gameObject.name = $"Block {x},{y}";
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

    public void FindNeighbor(int x, int y, Types.ColorTypes targetColor, List<Block> matchedBlocks)
    {
        if (x < 0 || x >= settings.width || y < 0 || y >= settings.height) return;

        Block currentBlock = grid[x, y];

        if (currentBlock == null) return;

        if (matchedBlocks.Contains(currentBlock)) return;

        if (targetColor != currentBlock.color) return;

        matchedBlocks.Add(currentBlock);

        FindNeighbor(x + 1, y, targetColor, matchedBlocks); 
        FindNeighbor(x - 1, y, targetColor, matchedBlocks); 
        FindNeighbor(x, y + 1, targetColor, matchedBlocks);
        FindNeighbor(x, y - 1, targetColor, matchedBlocks);
    }

    public void CheckMatches(int x, int y)
    {
        Block startBlock = grid[x, y];

        if (startBlock == null) return;

        Types.ColorTypes targetColor = startBlock.color;

        List<Block> matchedBlocks = new List<Block>();

        FindNeighbor(x, y, targetColor, matchedBlocks);

        if(matchedBlocks.Count >= 2){
            foreach(Block b in matchedBlocks)
            {
                grid[b.x, b.y] = null;
                b.gameObject.SetActive(false);
                blockPool.Add(b.gameObject);

                ApplyGravity();
                RefillBoard();
                UpdateAllIcons();
                DeadlockControl();
            }
        }
    }

    public void RaycastControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
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

                b.SetCoordinates(x, i);
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

                    newBlock.gameObject.name = $"Block {x},{y}";
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

                FindNeighbor(x, y, targetColor, matchedBlocks);

                foreach (Block b in matchedBlocks)
                {
                    b.updateIcon(matchedBlocks.Count);
                    visitedBlocks[b.x, b.y] = true;
                }
            }
        }
    }

    public bool HasMatches()
    {
        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                if(grid[x, y] != null)
                {
                    List<Block> matchedBlocks = new List<Block>();

                    FindNeighbor(x, y, grid[x, y].color, matchedBlocks);

                    if (matchedBlocks.Count >= 2)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void DeadlockControl()
    {
        if (HasMatches()) return;

        ShuffleBoard();
    }

    public void ShuffleBoard()
    {
        List<Block> tempList = new List<Block>();

        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                if(grid[x, y] != null)
                {
                    tempList.Add(grid[x, y]);
                }
            }
        }

        for(int i = 0; i < tempList.Count; i++)
        {
            Block temp = tempList[i];
            int rndIndex = Random.Range(i, tempList.Count);
            tempList[i] = tempList[rndIndex];
            tempList[rndIndex] = temp;
        }

        int listIndex = 0;
        for (int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                if(grid[x, y] != null)
                {
                    Block b = tempList[listIndex];
                    grid[x, y] = b;
                    b.SetCoordinates(x, y);
                    listIndex++;
                }
            }
        }

        if (!HasMatches())
        {
            ForceTeleport();
        }

        UpdateAllIcons();
    }

    public void ForceTeleport()
    {
        int rndX = Random.Range(1, settings.width-1);
        int rndY = Random.Range(1, settings.height-1);
        int rndDim = Random.Range(0, 4);

        Block targetBlock = grid[rndX, rndY];
        Block neighborBlock;

        switch (rndDim)
        {
            case 0:
                neighborBlock = grid[rndX, rndY + 1];
                break;
            case 1:
                neighborBlock = grid[rndX + 1, rndY];
                break;
            case 2:
                neighborBlock = grid[rndX, rndY - 1];
                break;
            case 3:
                neighborBlock = grid[rndX - 1, rndY];
                break;
            default:
                neighborBlock = grid[rndX, rndY + 1];
                break;
        }

        if (targetBlock == null || neighborBlock == null)
        {
            ForceTeleport(); 
            return;
        }

        Block distantBlock = null;

        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                Block currentBlock = grid[x, y];

                if (currentBlock == null) continue;
                if (currentBlock == targetBlock) continue;
                if (currentBlock == neighborBlock) continue;
                if (currentBlock.color != targetBlock.color) continue;

                distantBlock = currentBlock;
                break;
            }

            if (distantBlock != null) break;
        }

        if (distantBlock != null)
        {
            int neighborX = neighborBlock.x;
            int neighborY = neighborBlock.y;

            int distantX = distantBlock.x;
            int distantY = distantBlock.y;

            grid[neighborX, neighborY] = distantBlock;
            grid[distantX, distantY] = neighborBlock;

            distantBlock.SetCoordinates(neighborX, neighborY);
            neighborBlock.SetCoordinates(distantX, distantY);
        }
        else ForceTeleport();
    }
}
