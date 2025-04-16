using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapController : MonoBehaviour
{
    GameManager gameManager;

    int startingPositionCode = 0;
    int nearbyTileCode = 1;
    int staticBuildingCode = 2;
    int randomBuildingCode = 3;
    public int backgroundCode { get; private set; } = 4;
    int redLightCode = 5;
    public int totalRows { get; private set; } = 11;
    public int totalColumns { get; private set; } = 13;
    public int[,] map { get; private set; }
    public int totalRedLights { get; private set; } = 33;

    [Header("Animated Tiles")]
    public AnimatedTile[] redLightMiddleTiles;
    public AnimatedTile[] redLightTopTiles;

    [Header("Regular Tiles")]
    public Tile[] backgroundTiles;
    public Tile[] topBuildingTiles, bottomBuildingTiles,
        leftBuildingTiles, leftRearBuildingTiles, rightBuildingTiles, rightRearBuildingTiles,
        topLeftCornerBuildingTiles, topLeftCornerRearBuildingTiles, topRightCornerBuildingTiles, topRightCornerRearBuildingTiles,
        bottomLeftCornerBuildingTiles, bottomLeftCornerRearBuildingTiles, bottomRightCornerBuildingTiles, bottomRightCornerRearBuildingTiles;
    public Tile[] buildingTiles;
    public Tile[] roundShadowTiles;
    public Tile[] straightShadowTiles;

    [Header("Tilemaps")]
    public Tilemap backgroundTilemap;
    public Tilemap buildingsTilemap;
    public Tilemap redLightsTilemap;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        SetAnimatedTileSpeeds(0);
        PlaceBorderTiles();
        CreateMap(); // Must be called on Awake() so that the GameManager can retrieve the map variable on Start()
    }

    public void SetAnimatedTileSpeeds(int speed)
    {
        redLightMiddleTiles[gameManager.currentLevel].m_MinSpeed = speed;
        redLightMiddleTiles[gameManager.currentLevel].m_MaxSpeed = speed;

        redLightTopTiles[gameManager.currentLevel].m_MinSpeed = speed;
        redLightTopTiles[gameManager.currentLevel].m_MaxSpeed = speed;

        redLightsTilemap.RefreshAllTiles();
    }

    private void PlaceBorderTiles()
    {
        for (int column = 0; column < totalColumns; column++)
        {
            backgroundTilemap.SetTile(new Vector3Int(column, 11), topBuildingTiles[gameManager.currentLevel]);
            backgroundTilemap.SetTile(new Vector3Int(column, -1), bottomBuildingTiles[gameManager.currentLevel]);
        }

        for (int row = 0; row < totalRows; row++)
        {
            backgroundTilemap.SetTile(new Vector3Int(-1, row), leftBuildingTiles[gameManager.currentLevel]);
            backgroundTilemap.SetTile(new Vector3Int(-2, row), leftRearBuildingTiles[gameManager.currentLevel]);
            backgroundTilemap.SetTile(new Vector3Int(-1, row), rightBuildingTiles[gameManager.currentLevel]);
            backgroundTilemap.SetTile(new Vector3Int(-2, row), rightRearBuildingTiles[gameManager.currentLevel]);
        }

        backgroundTilemap.SetTile(new Vector3Int(11, -1), topLeftCornerBuildingTiles[gameManager.currentLevel]);
        backgroundTilemap.SetTile(new Vector3Int(11, -2), topLeftCornerRearBuildingTiles[gameManager.currentLevel]);
        backgroundTilemap.SetTile(new Vector3Int(11, 13), topRightCornerBuildingTiles[gameManager.currentLevel]);
        backgroundTilemap.SetTile(new Vector3Int(11, 14), topRightCornerRearBuildingTiles[gameManager.currentLevel]);

        backgroundTilemap.SetTile(new Vector3Int(-1, -1), bottomLeftCornerBuildingTiles[gameManager.currentLevel]);
        backgroundTilemap.SetTile(new Vector3Int(-1, -2), bottomLeftCornerRearBuildingTiles[gameManager.currentLevel]);
        backgroundTilemap.SetTile(new Vector3Int(-1, 13), bottomRightCornerBuildingTiles[gameManager.currentLevel]);
        backgroundTilemap.SetTile(new Vector3Int(-1, 14), bottomRightCornerRearBuildingTiles[gameManager.currentLevel]);
    }

    // Ensures the amount of Buildings and Red Lights in rows and columns will stay limited to the maximum amount
    bool CheckTileLines(int tileType, int currentRow, int currentColumn)
    {
        bool canPlaceTile = false;
        int totalRow = 0;
        int totalColumn = 0;
        int maxRow = 0;
        int maxColumn = 0;

        if (tileType == randomBuildingCode)
        {
            maxRow = 3;
            maxColumn = 4;
        } else if (tileType == redLightCode)
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
        int placedBuildings = 0;
        int placedRedLights = 0;
        int totalBuildings = 8;
        map = new int[totalRows, totalColumns];

        System.Random random = new();
        Vector3Int position = new(0, 0, 0); // 0x0 in the Tilemap corresponds to the lower left green Background tile (Background_1)

        // Creates a 11x13 2D array representing the level map with the specified tile codes
        for (int row = 0; row < totalRows; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                if (row == 10 && column == 0)
                {
                    map[row, column] = startingPositionCode;
                } else if ((row == 9 && column == 0) || (row == 10 && column == 1))
                {
                    map[row, column] = nearbyTileCode;
                } else if (row % 2 == 0)
                {
                    map[row, column] = backgroundCode;
                } else
                {
                    if (column % 2 == 0)
                    {
                        map[row, column] = backgroundCode;
                    } else
                    {
                        map[row, column] = staticBuildingCode;
                    }
                }
            }
        }

        // Places the random Buildings
        while (placedBuildings < totalBuildings)
        {
            for (int i = 0; i < totalBuildings; i++)
            {
                int row = random.Next(0, totalRows);
                int column = random.Next(0, totalColumns);
                bool canPlaceBuilding = CheckTileLines(randomBuildingCode, row, column)/* && CheckDiagonalTiles(row, column)*/;

                if (map[row, column] == backgroundCode && placedBuildings < totalBuildings && canPlaceBuilding)
                {
                    //position = new(column, row, 0);
                    //buildingsTilemap.SetTile(position, buildingTile);

                    //// If the Building was positioned above a regular Background tile, replaces it with the corresponding shadowed Background tile
                    //if (row > 0 && map[row - 1, column] == backgroundCode)
                    //{
                    //    position.y = row - 1;
                    //    backgroundTilemap.SetTile(position, straightShadowTile);
                    //}

                    map[row, column] = randomBuildingCode;
                    placedBuildings++;
                }
            }
        }

        // Places the Red Lights
        while (placedRedLights < totalRedLights)
        {
            for (int i = 0; i < totalRedLights; i++)
            {
                int row = random.Next(0, totalRows);
                int column = random.Next(0, totalColumns);
                bool canPlaceRedLight = CheckTileLines(redLightCode, row, column);

                if (map[row, column] == backgroundCode && placedRedLights < totalRedLights && canPlaceRedLight)
                {
                    //position = new(column, row, 0);

                    //AnimatedTile tile = row == 10 ? redLightTopTile : redLightMiddleTile;
                    //redLightsTilemap.SetTile(position, tile);

                    //// If the RedLight was positioned above a regular Background tile, replaces it with the corresponding shadowed Background tile
                    //if (row > 0 && map[row - 1, column] == backgroundCode)
                    //{
                    //    position.y = row - 1;
                    //    backgroundTilemap.SetTile(position, roundShadowTile);
                    //}

                    map[row, column] = redLightCode;
                    placedRedLights++;
                }
            }
        }

        CheckForInaccessibleAreas();
    }

    private void CheckForInaccessibleAreas()
    {
        // Total tiles = 11 * 13, static buildings = 30, random buildings = 8
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
            // ClearTilemaps();
            CreateMap();
        }
        else PlaceInnerTiles();
    }

    private void PlaceInnerTiles()
    {
        for (int row = 0; row < totalRows; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                Vector3Int position = new(column, row, 0);

                if (map[row, column] == staticBuildingCode || map[row, column] == randomBuildingCode) buildingsTilemap.SetTile(position, buildingTiles[gameManager.currentLevel]);
                else if (map[row, column] == redLightCode)
                {
                    AnimatedTile tile = row == 10 ? redLightTopTiles[gameManager.currentLevel] : redLightMiddleTiles[gameManager.currentLevel];
                    redLightsTilemap.SetTile(position, tile);
                }
                else if (map[row, column] == backgroundCode)
                {
                    if (row == 10) backgroundTilemap.SetTile(position, straightShadowTiles[gameManager.currentLevel]);
                    else if (row > 0)
                    {
                        int lowerPosition = map[row + 1, column];
                        Tile tile = lowerPosition == staticBuildingCode || lowerPosition == randomBuildingCode ? straightShadowTiles[gameManager.currentLevel] : lowerPosition == redLightCode ? roundShadowTiles[gameManager.currentLevel] : backgroundTiles[gameManager.currentLevel];
                        backgroundTilemap.SetTile(position, tile);
                    }
                }
            }
        }
    }

    private void CheckAndPushPosition(List<Vector2Int> visitedPositions, Stack<Vector2Int> positionsToVisit, Vector2Int positionToVisit)
    {
        if (positionToVisit.x >= 0 && positionToVisit.x <= 10 && positionToVisit.y >= 0 && positionToVisit.y <= 12 && !visitedPositions.Contains(positionToVisit) && map[positionToVisit.x, positionToVisit.y] != staticBuildingCode && map[positionToVisit.x, positionToVisit.y] != randomBuildingCode)
        {
            positionsToVisit.Push(positionToVisit);
            visitedPositions.Add(positionToVisit);
        }
    }

    //private void ClearTilemaps()
    //{
    //    Vector3Int position;

    //    backgroundTilemap.ClearAllTiles();
    //    redLightsTilemap.ClearAllTiles();

    //    for (int row = 0; row < totalRows; row++)
    //    {
    //        for (int column = 0; column < totalColumns; column++)
    //        {
    //            position = new(column, row, 0);

    //            if (map[row, column] == randomBuildingCode)
    //            {
    //                buildingsTilemap.SetTile(position, null);
    //                backgroundTilemap.SetTile(position, backgroundTile);
    //            }
    //            else if (map[row, column] != staticBuildingCode)
    //            {
    //                Tile tile = row == 10 ? straightShadowTile : backgroundTile;
    //                backgroundTilemap.SetTile(position, tile);
    //            }
    //        }
    //    }
    //}

}
