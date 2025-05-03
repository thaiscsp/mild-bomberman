using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    bool isVillageLevel;
    // des = Destructible
    // indes = Indestructible
    int startingPositionCode = 0;
    int nearbyTileCode = 1;
    int staticIndesCode = 2;
    int randomIndesCode = 3;
    public int backgroundCode { get; private set; } = 4;
    int desCode = 5;
    public int totalRows { get; private set; } = 11;
    public int totalColumns { get; private set; } = 13;
    public int[,] map { get; private set; }
    public int totalDes { get; private set; } = 33;

    AnimatedTile desTop, desMiddle;

    public Tile Background { get; private set; }
    public Tile Indes { get; private set; }
    public Tile IndesShadow { get; private set; }
    public Tile DesShadow { get; private set; }

    [Header("Animated Tiles")]
    public AnimatedTile[] desTopTiles;
    public AnimatedTile[] desMiddleTiles;

    [Header("Regular Tiles")]
    public Tile[] backgroundTiles;
    public Tile[] indesTiles;
    public Tile[] indesShadowTiles;
    public Tile[] desShadowTiles;

    [Header("Tilemaps")]
    public Tilemap backgroundTilemap;
    public Tilemap destructiblesTilemap;
    public Tilemap indestructiblesTilemap;
    public Tilemap townBorderTilemap;
    public Tilemap villageBorderTilemap;
    public Tilemap castleBorderTilemap;

    Vector3 startPosition = new(0.5f, -12, 0);

    private void Awake()
    {
        if (DataManager.instance == null) DataManager.instance = FindFirstObjectByType<DataManager>(); // às vezes a referência não aparece?
        isVillageLevel = DataManager.instance.level >= 3 && DataManager.instance.level <= 5;

        ResetPosition();
        ClearInnerTilemaps();
        SetTileVariables();
        EnableBorder();
        SetAnimatedTileSpeeds(0);
        CreateMap(); // Must be called on Awake() so that the GameManager can retrieve the map variable on Start()
    }

    // To avoid accessing the arrays repeatedly
    private void SetTileVariables()
    {
        int index = DataManager.instance.level - 1;

        Background = backgroundTiles[index];

        Indes = indesTiles[index];
        IndesShadow = indesShadowTiles[index];
        
        desTop = desTopTiles[index];
        desMiddle = desMiddleTiles[index];
        DesShadow = desShadowTiles[index];
    }

    private void EnableBorder()
    {
        if (DataManager.instance.level < 3) townBorderTilemap.gameObject.SetActive(true);
        else if (DataManager.instance.level < 6) villageBorderTilemap.gameObject.SetActive(true);
        else castleBorderTilemap.gameObject.SetActive(true);
    }

    public void SetAnimatedTileSpeeds(int speed)
    {
        desMiddle.m_MinSpeed = speed;
        desMiddle.m_MaxSpeed = speed;

        desTop.m_MinSpeed = speed;
        desTop.m_MaxSpeed = speed;

        destructiblesTilemap.RefreshAllTiles();
    }

    // Ensures the amount of indestructibles and destructibles in rows and columns will stay limited to the maximum amount
    bool CheckTileLines(int tileType, int currentRow, int currentColumn)
    {
        bool canPlaceTile = false;
        int totalRow = 0;
        int totalColumn = 0;
        int maxRow = 0;
        int maxColumn = 0;

        if (tileType == randomIndesCode)
        {
            maxRow = 3;
            maxColumn = 4;
        } else if (tileType == desCode)
        {
            maxRow = 7;
            maxColumn = 7;
        }

        // Checks the current row
        for (int column = 0; column < totalColumns; column++)
        {
            if (column != currentColumn)
                totalRow += map[currentRow, column] == tileType ? 1 : 0;
        }

        // Checks the current column
        for (int row = 0; row < totalRows; row++)
        {
            if (row != currentRow)
                totalColumn += map[row, currentColumn] == tileType ? 1 : 0;
        }

        if (totalRow < maxRow && totalColumn < maxColumn)
        {
            canPlaceTile = true;
        }

        return canPlaceTile;
    }

    private void CreateMap()
    {
        map = new int[totalRows, totalColumns];

        CreateBaseMap();

        if (DataManager.instance.level < 8) // Map is static at level 8
        {
            PlaceIndestructiblesCodes();
            PlaceDestructiblesCodes();
        }

        CheckForInaccessibleAreas();
    }

    // Creates a 11x13 2D array representing the level map with the specified tile codes
    private void CreateBaseMap()
    {
        for (int row = 0; row < totalRows; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                if (row == 10 && column == 0)
                {
                    map[row, column] = startingPositionCode;
                }
                else if ((row == 9 && column == 0) || (row == 10 && column == 1))
                {
                    map[row, column] = nearbyTileCode;
                }
                else if (row % 2 == 0)
                {
                    map[row, column] = backgroundCode;
                }
                else
                {
                    if (column % 2 == 0)
                    {
                        map[row, column] = backgroundCode;
                    }
                    else
                    {
                        map[row, column] = staticIndesCode;
                    }
                }
            }
        }
    }

    private void PlaceIndestructiblesCodes()
    {
        int placedIndes = 0;
        int totalIndes = 8;

        while (placedIndes < totalIndes)
        {
            for (int i = 0; i < totalIndes; i++)
            {
                int row = Random.Range(0, totalRows);
                int column = Random.Range(0, totalColumns);
                bool canPlaceIndes = CheckTileLines(randomIndesCode, row, column);

                if (map[row, column] == backgroundCode && placedIndes < totalIndes && canPlaceIndes)
                {
                    map[row, column] = randomIndesCode;
                    placedIndes++;
                }
            }
        }
    }

    private void PlaceDestructiblesCodes()
    {
        int placedDes = 0;

        while (placedDes < totalDes)
        {
            for (int i = 0; i < totalDes; i++)
            {
                int row = Random.Range(0, totalRows);
                int column = Random.Range(0, totalColumns);
                bool canPlaceDes = CheckTileLines(desCode, row, column);

                if (map[row, column] == backgroundCode && placedDes < totalDes && canPlaceDes)
                {
                    map[row, column] = desCode;
                    placedDes++;
                }
            }
        }
    }

    private void CheckForInaccessibleAreas()
    {
        bool canPlaceTiles = false;

        if (DataManager.instance.level == 8) canPlaceTiles = true; // Because the map is static, there is no need to check for inaccessible areas
        else
        {
            // Total tiles = 11 * 13, static indestructibles = 30, random indestructibles = 8
            // 11 * 13 - 30 - 8 = 105
            int expectedToVisit = 105;
            List<Vector2Int> visitedPositions = new();
            Stack<Vector2Int> positionsToVisit = new();
            positionsToVisit.Push(Vector2Int.zero);

            while (positionsToVisit.Count > 0)
            {
                Vector2Int currentPos = positionsToVisit.Pop();

                CheckAndPushPosition(visitedPositions, positionsToVisit, new(currentPos.x + 1, currentPos.y)); // Up
                CheckAndPushPosition(visitedPositions, positionsToVisit, new(currentPos.x - 1, currentPos.y)); // Down
                CheckAndPushPosition(visitedPositions, positionsToVisit, new(currentPos.x, currentPos.y - 1)); // Left
                CheckAndPushPosition(visitedPositions, positionsToVisit, new(currentPos.x, currentPos.y + 1)); // Right
            }

            if (visitedPositions.Count < expectedToVisit)
            {
                print("Regenerating tilemaps. Inaccessible tiles: " + (expectedToVisit - visitedPositions.Count));
                CreateMap();
            }
            else canPlaceTiles = true;
        }
            
        if (canPlaceTiles) PlaceInnerTiles();
    }

    private void PlaceInnerTiles()
    {
        for (int row = 0; row < totalRows; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                Tile tile = Background;
                Vector3Int position = new(column, row, 0);
                int tileCode = map[row, column];

                if (tileCode == startingPositionCode)
                {
                    if (isVillageLevel) tile = Background;
                    else tile = IndesShadow;

                    backgroundTilemap.SetTile(position, tile);
                }
                
                else if (tileCode == nearbyTileCode)
                {
                    if (isVillageLevel || row == 9) tile = Background;
                    else if (row == 10) tile = IndesShadow;

                    backgroundTilemap.SetTile(position, tile);
                }

                
                else if (map[row, column] == staticIndesCode || map[row, column] == randomIndesCode) indestructiblesTilemap.SetTile(position, Indes);
                
                else if (map[row, column] == desCode) destructiblesTilemap.SetTile(position, row == 10 ? desTop : desMiddle);

                else if (map[row, column] == backgroundCode)
                {
                    if (row == 10)
                    {
                        if (isVillageLevel) tile = Background;
                        else tile = IndesShadow;
                    }
                    else
                    {
                        int upperPosition = map[row + 1, column];
                        tile = upperPosition == staticIndesCode || upperPosition == randomIndesCode ? IndesShadow : upperPosition == desCode ? DesShadow : Background;
                    }

                    backgroundTilemap.SetTile(position, tile);
                }
            }
        }
    }

    private void CheckAndPushPosition(List<Vector2Int> visitedPositions, Stack<Vector2Int> positionsToVisit, Vector2Int positionToVisit)
    {
        if (positionToVisit.x >= 0 && positionToVisit.x <= 10 && positionToVisit.y >= 0 && positionToVisit.y <= 12 && !visitedPositions.Contains(positionToVisit) && map[positionToVisit.x, positionToVisit.y] != staticIndesCode && map[positionToVisit.x, positionToVisit.y] != randomIndesCode)
        {
            positionsToVisit.Push(positionToVisit);
            visitedPositions.Add(positionToVisit);
        }
    }

    private void ResetPosition()
    {
        transform.position = startPosition;
    }

    private void ClearInnerTilemaps()
    {
        backgroundTilemap.ClearAllTiles();
        destructiblesTilemap.ClearAllTiles();
        indestructiblesTilemap.ClearAllTiles();
    }

}
