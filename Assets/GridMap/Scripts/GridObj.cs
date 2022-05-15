using System.Collections;
using System.Collections.Generic;
using Extensions;
using GridTools;
using Unity.Mathematics;
using UnityEngine;

public class GridObj : MonoBehaviour
{
    public Grid Grid { get; private set; }
    public Vector3 PlayerPosition { get; set; }
    public LayerMask WallsLayerMask;

    public void OnEnable()
    {
        var startPosition = new float2(0.64f * (-35), 0.64f * (-80));
        const int width = 170;
        const int height = 130;
        const float cellSize = 1.28f;
        WallsLayerMask = LayerMask.GetMask("Walls");

        Grid = new Grid(startPosition, width, height, cellSize);

        var sizeVector = new Vector2(Grid.CellSize, Grid.CellSize);
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)   
        {

            var position = Grid.GridToWorldPosition(new float2(x, y)).ToVector2() + sizeVector / 2;
            var checkBox = Physics2D.OverlapArea(position - sizeVector / 5, position + sizeVector / 5, WallsLayerMask);

            if (checkBox != null)
            {
                Grid.CreateWall(new int2(x, y));
            }
        }

        Grid.DrawGrid();
    }

    private void FixedUpdate()
    {
        if (Grid.pathsToDraw.Count > 0)
            Grid.DrawPaths();

        //if (Input.GetMouseButtonDown(1))
        //    CreateWall(Tools.GetMouseWordPosition());
    }

    public float2 GridToWorldPosition(float2 gridPosition)
        => Grid.GridToWorldPosition(gridPosition);

    public int2 WorldToGridPosition(Vector3 worldPosition)
        => Grid.WorldToGridPosition(worldPosition);

    public void CreateWall(Vector3 worldPosition)
        => Grid.CreateWall(worldPosition);

    public void AddPathsToDraw(List<int2> pathToDraw)
    {
        Grid.AddPathsToDraw(pathToDraw);
    }

    public void FillCell(int2 gridPosition)
        => Grid.FillCell(gridPosition);

    public void UnFillCell(int2 gridPosition)
        => Grid.UnFillCell(gridPosition);

    public void CreateWall(Vector3 startWorldWall, Vector3 endWorldWall)
        => Grid.CreateWall(startWorldWall, endWorldWall);

    
}
