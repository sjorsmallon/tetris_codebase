using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I, J, L, O, S, T, Z
}

// - Golden (money) 
// - glass (more points but can break (or will break?))
// - moss (proliferate / grow piece over time?)
// - Stone (do not dissolve after row solve)
// - Steroids: pieces are twice as big.
// - Moon stone: cannot rotate at all, but gives point bonus?
// - cruise control: piece continually moves left to right, wrapping side-to-side, or bouncing.
// - Corrosive: (dissolve pieces next to it over time)
// - Viral: also populates your hold spot.
// - Explosive: if row is solved with one of these bricks, explode in a radius.
// - Osmium: all individual blocks of the shape are affected by "gravity".
// - Black hole / magnetic: pulls nearby individual blocks on row level to it.
// - Shadow / Gemini / Twin: drop the identical piece immediately in the same position.
// - joined at the hip: drop two pieces together, side by side.
// - acid / laser / diamond drill bit: drill all the way to the lowest rank.
// - Lazy Susan: piece continually rotates. can use harddrop to "select" orientation. 
// - free format: able to move piece in 2d place, place wherever you want.
// - the thing: one tile block, transform in the piece you hit
// - asteroid: able to move the piece across the border from left to right and from right to left, effectively being able to split the piece.

public enum TetrominoType
{
    Default, Gold, MoonStone, Shadow,
}

[System.Serializable]
public struct TetrominoData
{
    public Tile tile;
    public Tetromino tetromino;

    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }

    public void Initialize()
    {
        cells = Data.Cells[tetromino];
        wallKicks = Data.WallKicks[tetromino];
    }

}
