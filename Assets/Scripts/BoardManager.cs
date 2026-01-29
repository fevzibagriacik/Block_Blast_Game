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

    private Camera cam;
    public float cameraOffset = 2f; 
    public float aspectRatio = 0.625f;

    public bool moveExist = false;

    private void Start()
    {
        cam = Camera.main; 
        grid = new Block[settings.width, settings.height];

        PrepareCamera();

        RefillBoard();
        UpdateAllIcons();
        DeadlockControl();
    }

    private void Update()
    {
        RaycastControl();
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

        return Instantiate(blockPrefab, boardHolder);
    }

    public void FindNeighbor(int x, int y, Types.ColorTypes targetColor, List<Block> matchedBlocks)
    {
        if (x < 0 || x >= settings.width || y < 0 || y >= settings.height) return;

        Block currentBlock = grid[x, y];

        if (currentBlock == null || matchedBlocks.Contains(currentBlock) || targetColor != currentBlock.color) return;

        matchedBlocks.Add(currentBlock);

        FindNeighbor(x + 1, y, targetColor, matchedBlocks); 
        FindNeighbor(x - 1, y, targetColor, matchedBlocks); 
        FindNeighbor(x, y + 1, targetColor, matchedBlocks);
        FindNeighbor(x, y - 1, targetColor, matchedBlocks);
    }

    public void CheckMatches(int x, int y)
    {
        if (grid[x, y] == null) return;

        List<Block> matchedBlocks = new List<Block>();

        FindNeighbor(x, y, grid[x,y].color, matchedBlocks);

        if(matchedBlocks.Count >= 2){
            foreach(Block b in matchedBlocks)
            {
                grid[b.x, b.y] = null;
                b.gameObject.SetActive(false);
                blockPool.Add(b.gameObject);
            }

            ApplyGravity();
            RefillBoard();
            UpdateAllIcons();
            DeadlockControl();
        }
    }

    public void RaycastControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Transform the screen mouse position to world point
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            //Create a ray at the clicked point
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                //Access to script of clickedBlock
                Block clickedBlock = hit.collider.GetComponent<Block>();

                if (clickedBlock != null)
                    CheckMatches(clickedBlock.x, clickedBlock.y);
            }
        }
    }

    public void ApplyGravity()  
    {
        for(int x = 0; x < settings.width; x++)
        {
            int writeIndex = 0;

            for (int y = 0; y < settings.height; y++)
            {
                if (grid[x, y] != null)
                {
                    if (y != writeIndex)
                    {
                        grid[x, writeIndex] = grid[x, y];
                        grid[x, y] = null;
                        grid[x, writeIndex].SetCoordinates(x, writeIndex);
                    }

                    writeIndex++;
                }
            }
        }
    }

    public void RefillBoard()
    {
        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                if (grid[x, y] == null)
                {
                    GameObject go = GetBlock();
                    Block newBlock = go.GetComponent<Block>();

                    newBlock.Init(x, y, settings);

                    grid[x, y] = newBlock;
                }
            }
        }
    }

    public void UpdateAllIcons()
    {
        bool[,] visited = new bool[settings.width, settings.height];

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                if (grid[x, y] != null && !visited[x, y])
                {
                    List<Block> matchedBlocks = new List<Block>();

                    FindNeighbor(x, y, grid[x, y].color, matchedBlocks);

                    foreach (Block b in matchedBlocks)
                    {
                        b.UpdateIcon(matchedBlocks.Count);
                        visited[b.x, b.y] = true;
                    }
                }
            }
        }
    }

    public bool HasMatches()
    {
        bool[,] visited = new bool[settings.width, settings.height];

        for (int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                if(grid[x, y] != null && !visited[x, y])
                {
                    List<Block> matchedBlocks = new List<Block>();
                    FindNeighbor(x, y, grid[x, y].color, matchedBlocks);

                    if (matchedBlocks.Count >= 2) return true;

                    foreach (var b in matchedBlocks) visited[b.x, b.y] = true;
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
            for(int y = 0; y < settings.height; y++)
                if(grid[x, y] != null)
                    tempList.Add(grid[x, y]);

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
                    Block b = tempList[listIndex++];
                    grid[x, y] = b;
                    b.SetCoordinates(x, y);
                }
            }
        }

        if(settings.width >= 3 && settings.height >= 3)
            if (!HasMatches())
                ForceTeleport();

        UpdateAllIcons();
    }

    public void ForceTeleport(int retryCount = 0)
    {
        if (settings.width < 3 || settings.height < 3) return;

        if (retryCount >= 50)
        {
            ShuffleBoard();
            return;
        }

        int rndX = Random.Range(1, settings.width-1);
        int rndY = Random.Range(1, settings.height-1);
        int rndDim = Random.Range(0, 4);

        Block targetBlock = grid[rndX, rndY];

        int neighborX = rndX;
        int neighborY = rndY;

        switch (rndDim)
        {
            case 0: neighborY++; break;
            case 1: neighborX++; break;
            case 2: neighborY--; break;
            case 3: neighborX--; break;
            default: neighborY++; break;
        }

        if (neighborX < 0 || neighborX >= settings.width || neighborY < 0 || neighborY >= settings.height)
        {
            ForceTeleport(retryCount + 1);
            return;
        }

        Block neighborBlock = grid[neighborX, neighborY];

        if (targetBlock == null || neighborBlock == null)
        {
            ForceTeleport(retryCount + 1);
            return;
        }

        Block distantBlock = null;

        for(int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                Block currentBlock = grid[x, y];

                if (currentBlock != null && currentBlock != targetBlock && currentBlock != neighborBlock && currentBlock.color == targetBlock.color)
                {
                    distantBlock = currentBlock;
                    break;
                }
            }

            if (distantBlock != null) break;
        }

        if (distantBlock != null)
        {
            int distantX = distantBlock.x;
            int distantY = distantBlock.y;

            grid[neighborX, neighborY] = distantBlock;
            grid[distantX, distantY] = neighborBlock;

            distantBlock.SetCoordinates(neighborX, neighborY);
            neighborBlock.SetCoordinates(distantX, distantY);
        }
        else ForceTeleport(retryCount + 1);
    }

    public void PrepareCamera()
    {
        cam.transform.position = new Vector3(0, 0, -10f);

        float boardHeight = settings.height / 2f;
        float boardWidth = settings.width / 2f;

        boardHeight += cameraOffset;
        boardWidth += cameraOffset;

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = boardWidth / boardHeight;

        if (screenRatio >= targetRatio) cam.orthographicSize = boardHeight;
        else cam.orthographicSize = boardWidth / screenRatio;
    }
}
