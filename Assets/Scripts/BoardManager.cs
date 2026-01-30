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

    private Stack<int> searchStack;
    private bool[] visited;

    private readonly List<Block> tempGroup = new List<Block>(16);
    private readonly List<Block> tempCheck = new List<Block>(8);
    private readonly List<Block> clickMatch = new List<Block>(16);
    private readonly List<Block> shuffleList = new List<Block>(100);

    private RaycastHit2D[] rayHits = new RaycastHit2D[1];

    public LayerMask blockLayer;
    private void Start()
    {
        cam = Camera.main; 
        grid = new Block[settings.width, settings.height];

        int size = settings.width * settings.height;

        visited = new bool[size];
        searchStack = new Stack<int>(size);

        PrewarmPool(100);

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

    public void FindNeighbor(int startX, int startY, Types.ColorTypes targetColor, List<Block> result)
    {
        int w = settings.width;
        int h = settings.height;

        int startIndex = startX + startY * w;

        Block startBlock = grid[startX, startY];
        if (startBlock == null || startBlock.color != targetColor)
            return;

        searchStack.Clear();

        searchStack.Push(startIndex);
        visited[startIndex] = true;

        while (searchStack.Count > 0)
        {
            int index = searchStack.Pop();

            int x = index % w;
            int y = index / w;

            Block b = grid[x, y];
            result.Add(b);

            TryPush(x + 1, y, targetColor, w, h);
            TryPush(x - 1, y, targetColor, w, h);
            TryPush(x, y + 1, targetColor, w, h);
            TryPush(x, y - 1, targetColor, w, h);
        }
    }


    private void TryPush(int x, int y, Types.ColorTypes targetColor, int w, int h)
    {
        if (x < 0 || x >= w || y < 0 || y >= h)
            return;

        int index = x + y * w;

        if (visited[index])
            return;

        Block b = grid[x, y];
        if (b == null || b.color != targetColor)
            return;

        visited[index] = true;
        searchStack.Push(index);
    }


    public void CheckMatches(int x, int y)
    {
        if (grid[x, y] == null) return;

        clickMatch.Clear();
        System.Array.Clear(visited, 0, visited.Length); 
        FindNeighbor(x, y, grid[x, y].color, clickMatch);

        if (clickMatch.Count < 2) return;

        foreach (Block b in clickMatch)
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

    private void RaycastControl()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        int hitCount = Physics2D.RaycastNonAlloc(worldPos, Vector2.zero, rayHits, 0f, blockLayer);

        if (hitCount == 0)
            return;

        if (rayHits[0].collider.TryGetComponent(out Block b))
        {
            CheckMatches(b.x, b.y);
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
        System.Array.Clear(visited, 0, visited.Length);

        int w = settings.width;

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                int index = x + y * w;

                if (grid[x, y] == null || visited[index])
                    continue;

                tempGroup.Clear();
                FindNeighbor(x, y, grid[x, y].color, tempGroup);

                int count = tempGroup.Count;

                foreach (Block b in tempGroup)
                {
                    b.UpdateIcon(count);
                }
            }
        }
    }



    public bool HasMatches()
    {
        System.Array.Clear(visited, 0, visited.Length);
        int w = settings.width;

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                int index = x + y * w;

                if (grid[x, y] == null || visited[index])
                    continue;

                tempCheck.Clear();
                FindNeighbor(x, y, grid[x, y].color, tempCheck);

                if (tempCheck.Count >= 2)
                    return true;
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
        shuffleList.Clear();

        for (int x = 0; x < settings.width; x++)
            for(int y = 0; y < settings.height; y++)
                if(grid[x, y] != null)
                    shuffleList.Add(grid[x, y]);

        for(int i = 0; i < shuffleList.Count; i++)
        {
            Block temp = shuffleList[i];
            int rndIndex = Random.Range(i, shuffleList.Count);
            shuffleList[i] = shuffleList[rndIndex];
            shuffleList[rndIndex] = temp;
        }

        int listIndex = 0;
        for (int x = 0; x < settings.width; x++)
        {
            for(int y = 0; y < settings.height; y++)
            {
                if(grid[x, y] != null)
                {
                    Block b = shuffleList[listIndex++];
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

    private void PrewarmPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject b = Instantiate(blockPrefab, boardHolder);
            b.SetActive(false);
            blockPool.Add(b);
        }
    }
}
