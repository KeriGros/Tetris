using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z
}

[System.Serializable]
public struct TetrominoData
{
    public Tetromino Tetromino;
    public Tile Tile;
    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKiks { get; private set; }

    public void Intilialize()
    { 
        cells = Data.Cells[Tetromino];
        wallKiks = Data.WallKicks[this.Tetromino];
    }
}