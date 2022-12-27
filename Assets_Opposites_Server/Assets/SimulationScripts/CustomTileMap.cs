using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTileMap : MonoBehaviour
{
    [SerializeField] Vector2Int xySize;
    [SerializeField] List<Vector2Int> towerTilePositions;
    [SerializeField] List<Vector2Int> minionTilePositions;
    private Vector2 moveOrigin;
    public GameObject[] startTiles;
    public GameObject[] endTiles;

    //0,0 bottom left, max,max top right
    private GameObject[,] tiles;
    private List<GameObject> minionTiles;
    private List<GameObject> towerTiles;
    public GameObject referenceTowerTile;
    public GameObject referenceMinionTile;
    public GameObject referenceFinishTile;
    public float distanceBetweenTiles;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        minionTiles = new List<GameObject>();
        generateMinionTiles();
        moveOrigin = new Vector2(-(((float)xySize.x / 2.0f) * distanceBetweenTiles), -(((float)xySize.y / 2.0f) * distanceBetweenTiles));
        generateGrid();
        findStartAndEndTiles();
        colourStartAndEndTiles();
    }
    private void findStartAndEndTiles(){
        startTiles = new GameObject[2];
        endTiles = new GameObject[2];
        startTiles[0] = minionTiles[0];
        startTiles[1] = minionTiles[1];
        endTiles[0] = minionTiles[minionTiles.Count - 1];
        endTiles[1] = minionTiles[minionTiles.Count - 2];

    }

    private void colourStartAndEndTiles()
    {
        startTiles[0].GetComponent<SpriteRenderer>().color = new Color(1, 0.5f, 0.5f);
        startTiles[1].GetComponent<SpriteRenderer>().color = new Color(1, 0.5f, 0.5f);

    }

    private void generateMinionTiles() 
    {
        minionTilePositions = new List<Vector2Int>();
        Vector2Int mapPos = new Vector2Int(7, 0);
        minionTilePositions.Add(mapPos);
        minionTilePositions.Add(mapPos + new Vector2Int(1,0));
        mapPos = moveUp(mapPos, 5);
        minionTilePositions.Add(mapPos + new Vector2Int(0, 1));
        mapPos = moveRight(mapPos, 6);
        minionTilePositions.Add(mapPos + new Vector2Int(1, 0));
        mapPos = moveUp(mapPos, 4);
        moveUp(mapPos, 1);
        mapPos = moveLeft(mapPos, 10);
        mapPos = moveUp(mapPos, 6);
    }
    private Vector2Int moveUp(Vector2Int pos, int amount)
    {
        for (int i = 1; i <= amount; i++) {

            addIfNotAlreadyAdded(pos + new Vector2Int(0, i));
            addIfNotAlreadyAdded(pos + new Vector2Int(1, i));
        }
        return (pos + new Vector2Int(0,amount));
    }
    private Vector2Int moveRight(Vector2Int pos, int amount)
    {
        for (int i = 1; i <= amount; i++)
        {
            addIfNotAlreadyAdded(pos + new Vector2Int(i, 0));
            addIfNotAlreadyAdded(pos + new Vector2Int(i, 1));
        }
        return (pos + new Vector2Int(amount, 0));
    }
    private Vector2Int moveLeft(Vector2Int pos, int amount)
    {
        for (int i = 1; i <= amount; i++)
        {
            addIfNotAlreadyAdded(pos + new Vector2Int(-i, 0));
            addIfNotAlreadyAdded(pos + new Vector2Int(-i, 1));

            
        }
        return (pos + new Vector2Int(-amount, 0));
    }

    private void addIfNotAlreadyAdded(Vector2Int pos)
    {
        if (tileDoesntExist(pos))
        {
            minionTilePositions.Add(pos);
        }
    }
    private bool tileDoesntExist(Vector2Int pos)
    {
        for (int i = 0; i < minionTilePositions.Count; i++)
        {
            if (minionTilePositions[i] == pos)
            {
                return false;
            }
        }
        return true;
    }

    private bool isMinionTile(Vector2Int pos)
    {
        return (!tileDoesntExist(pos));
    }
    private void generateGrid()
    {
        tiles = new GameObject[xySize.x, xySize.y];
        for (int y = 0; y < xySize.y; y++)
        {
            for (int x = 0; x < xySize.x ; x++)
            {
                if (isMinionTile(new Vector2Int(x,y)))
                {
                    if (minionTiles.Count >= minionTilePositions.Count - 2)
                    {
                        tiles[x, y] = Instantiate(referenceFinishTile);
                    }
                    else {
                        tiles[x, y] = Instantiate(referenceMinionTile);
                    }

                    minionTiles.Add(tiles[x, y]);
                }
                else
                {
                    tiles[x, y] = Instantiate(referenceTowerTile);
                }
                tiles[x, y].transform.position = new Vector3(x * distanceBetweenTiles + moveOrigin.x, y * distanceBetweenTiles + moveOrigin.y, 0) ;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
