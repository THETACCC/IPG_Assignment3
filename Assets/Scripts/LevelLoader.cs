using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKCell;
public class LevelLoader : SKMonoSingleton<LevelLoader>
{
    //references
    [SerializeField] GameManager gameManager;
    private GameObject Manager;

    //The array of the map
    public static MapData mapData;
    //This is the scale of the block that depends on the size of the map
    public static Vector3 BLOCK_SCALE_MAP;

    //The array that keeps records of how many blocks are inside the map
    GameObject[] blocks;
    //References to the Actual map
    [SerializeField] Transform mapLeft, mapRight;
    [SerializeField] Transform LeftSide, RightSide;
    [SerializeField] Transform blockContainerLeft, blockContainerRight;
    //This is how big the level is, it can be 3x3 or 4x4, or 5x5, depending on need;
    public Vector2Int levelDimensions = new Vector2Int(3, 3);

    public Bounds boundsLeft, boundsRight;


    // A Dictionary to check whether there are already blocks on a position or not.
    private Dictionary<Vector3, Block> blockPos = new Dictionary<Vector3, Block>();
    // This is the array for the best position of each block in both maps
    public Vector3[,] blockposLeft, blockposRight;
    // This is the center point between two maps
    public static Vector3 center;


    const float BLOCK_SCALE_RATIO = 3f;
    const float DRAGGABLE_BLOCK_SCALE = 0.7f;
    const float DRAGGABLE_BLOCK_GAP = 12.0f;

    //This is the matrix code created for left and right map
    public Block[,] blocksLeftMap, blocksRightMap;


    //Spawn Blocks
    public GameObject blockPrefabBlue;
    public GameObject blockPrefabRed;
    public float minX = -5f;
    public float maxX = 0f;
    public float minY = 0f;
    public float maxY = 1f;
    public float minZ = 0f;
    public float maxZ = 1f;

    //Winning Conditions
    public bool isWin = false;
    public bool isAligned = false;
    void Start()
    {
        Manager = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = Manager.GetComponent<GameManager>();
        boundsLeft = mapLeft.GetComponent<BoxCollider>().bounds;
        boundsRight = mapRight.GetComponent<BoxCollider>().bounds;
        center = (LeftSide.position + RightSide.position) / 2.0f;

        LoadMap();
    }

    private void Update()
    {

    }


    public void LoadMap()
    {
        //init map data
        mapData = new MapData();
        mapData.map = new int[levelDimensions.x, levelDimensions.y];

        // Initialize the matrices with the dimensions of the level
        blocksLeftMap = new Block[levelDimensions.x, levelDimensions.y];
        blocksRightMap = new Block[levelDimensions.x, levelDimensions.y];

        //Initialize The position array of blocks
        blockposLeft = new Vector3[levelDimensions.x, levelDimensions.y];
        blockposRight = new Vector3[levelDimensions.x, levelDimensions.y];

        //Leftside calculation
        float cellGapX = (boundsLeft.max.x - boundsLeft.min.x) / (2 * levelDimensions.x);
        float cellGapZ = (boundsLeft.max.z - boundsLeft.min.z) / (2 * levelDimensions.y);

        float startX = boundsLeft.min.x + cellGapX;
        float startZ = boundsLeft.min.z + cellGapZ;
        for (int i = 0; i < levelDimensions.y; i++)
        {
            for (int j = 0; j < levelDimensions.x; j++)
            {
                Vector3 pos = new Vector3(startX + 2 * cellGapX * j, 0, startZ + 2 * cellGapZ * i);
                blockposLeft[i, j] = pos;
            }
        }

        //right
        cellGapX = (boundsRight.max.x - boundsRight.min.x) / (2 * levelDimensions.x);
        cellGapZ = (boundsRight.max.z - boundsRight.min.z) / (2 * levelDimensions.y);

        startX = boundsRight.min.x + cellGapX;
        startZ = boundsRight.min.z + cellGapZ;
        for (int i = 0; i < levelDimensions.y; i++)
        {
            for (int j = 0; j < levelDimensions.x; j++)
            {
                Vector3 pos = new Vector3(startX + 2 * cellGapX * j, 0, startZ + 2 * cellGapZ * i);
                blockposRight[i, j] = pos;
            }
        }

        AlignBlocks();


    }

    public void AlignBlocks()
    {
        //Starting Aligning Blocks
        blocks = GameObject.FindGameObjectsWithTag("Block");
        BLOCK_SCALE_MAP = Vector3.one * BLOCK_SCALE_RATIO / levelDimensions.x;
        float alignmentThreshold = 0.1f;
        foreach (GameObject block in blocks)
        {
            Vector3 pos = block.transform.position;
            bool isAlreadyAligned = false;
            float min_delta = float.MaxValue;
            Vector3 best_pos = Vector3.zero;
            //Vector3[,] to_comp = pos.x < center.x ? blockposLeft : blockposRight;

            List<Vector3> combinedPositions = new List<Vector3>();
            foreach (Vector3 pos1 in blockposLeft) combinedPositions.Add(pos1);
            foreach (Vector3 pos1 in blockposRight) combinedPositions.Add(pos1);


            // Check if the block is already aligned
            foreach (Vector3 targetPos in combinedPositions)
            {
                if (Vector3.Distance(pos, targetPos) < alignmentThreshold)
                {
                    isAlreadyAligned = true;
                    break; // No need to align this block
                }
            }

            if (isAlreadyAligned)
            {
                continue; // Skip to the next block
            }


            foreach (Vector3 comp in combinedPositions)
            {
                if (blockPos.ContainsKey(comp)) // Skip positions already occupied
                    continue;

                float delta = Mathf.Abs(comp.x - pos.x) + Mathf.Abs(comp.z - pos.z);
                if (delta < min_delta)
                {
                    min_delta = delta;
                    best_pos = comp;
                }
            }


            // Only move the block if we found an unoccupied position
            if (min_delta != float.MaxValue)
            {
                block.transform.position = best_pos;
                // Inserting or updating the position in the dictionary
                SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, best_pos, block.GetComponent<Block>());

                block.transform.localScale = BLOCK_SCALE_MAP;
                block.GetComponent<Block>().SyncLocalScale(BLOCK_SCALE_MAP);
            }

        }
        Debug.Log(blockposLeft[0, 1]);
    }

    
    public void clearMap()
    {
        List<Vector3> combinedPositions = new List<Vector3>();
        foreach (Vector3 pos1 in blockposLeft) combinedPositions.Add(pos1);
        foreach (Vector3 pos1 in blockposRight) combinedPositions.Add(pos1);
        foreach (Vector3 comp in combinedPositions)
        {
            if (blockPos.ContainsKey(comp))
            {
                blockPos.Remove(comp);
            }
        }
    }


    public void OnMoveBlock(Block block, Vector3 from, Vector3 to)
    {
        if (blockPos.ContainsKey(from))
        {
            blockPos.Remove(from);
        }
        SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, to, block);
 
    }

    public static Vector3 WorldToCellPos(Vector3 wpos)
    {
        LevelLoader l = LevelLoader.instance;
        float min_delta = float.MaxValue;
        Vector3 best_pos = Vector3.zero;
        Vector3[,] to_comp = wpos.x < center.x ? l.blockposLeft : l.blockposRight;
        foreach (Vector3 comp in to_comp)
        {
            float delta = Mathf.Abs(comp.x - wpos.x) + Mathf.Abs(comp.z - wpos.z);
            if (delta < min_delta)
            {
                min_delta = delta;
                best_pos = comp;
            }
        }
        return best_pos;
    }

    public static Vector3 CellToWorldPos(bool isLeft, int x, int y)
    {
        LevelLoader l = LevelLoader.instance;
        return isLeft ? l.blockposLeft[x, y] : l.blockposRight[x, y];
    }

    public static bool HasBlockOnCellPos(Vector3 cpos)
    {
        LevelLoader l = LevelLoader.instance;
        return l.blockPos.ContainsKey(cpos);
    }

    public void CheckInPos()
    {

        blocks = GameObject.FindGameObjectsWithTag("Block");
        bool allBlocksInPosition = true;
        foreach (GameObject block in blocks)
        {
            Block _Block = block.GetComponent<Block>();
            if (!_Block.inPosition) 
            {
                allBlocksInPosition = false; 
                break; 
            }
        }

        if (allBlocksInPosition)
        {
            // All blocks are in position
            PlayerWins();
        }

    }

    void PlayerWins()
    {
        isWin = true;
        Debug.Log("Player Wins!");
    }

    public void SpawnBlocks()
    {
        StartCoroutine(StartSpawnBlocks());
        /*
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            Vector3 randomPosition2 = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            Instantiate(blockPrefabBlue, randomPosition, Quaternion.identity);
            Instantiate(blockPrefabRed, randomPosition2, Quaternion.identity);

        }
        AlignBlocks();
        */
    }

    private IEnumerator StartSpawnBlocks()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            Instantiate(blockPrefabBlue, randomPosition, Quaternion.identity);
            AlignBlocks();
            yield return new WaitForSeconds(0.05f); 

            Vector3 randomPosition2 = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            Instantiate(blockPrefabRed, randomPosition2, Quaternion.identity);
            AlignBlocks();
            yield return new WaitForSeconds(0.05f); 
        }
        isAligned = true;

    }
    public class MapData
    {
        public int[,] map;
    }

}
