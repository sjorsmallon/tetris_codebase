using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq; // Required for using Any()
using System;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{


    //@FIXME(SJORS): hacky workaround to dynamically alter the sprite at a position
    // (by replacing it by this tile which is filled in via the editor)
    public Tile crackedTilePhase2;

    public bool debug = true;

    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    // which tetrominos are currently in the player bag?
    public List<Tetromino> playerBag;

    // list to evict an item from every time it is played. populate from player bag when it is empty.
    public List<int> piecesToPlay;



    public static List<int> Shuffle(List<int> list)  
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1); // Pick a random index from 0 to n
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }


    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }

        // add initial pieces to the bag.
        foreach (Tetromino tetromino in Tetromino.GetValues(typeof(Tetromino)))
        {
            playerBag.Add(tetromino);
        }

        //@RESET
        piecesToPlay = new List<int>(playerBag.Count);
        for (int i = 0; i < playerBag.Count; i++)
        {
            piecesToPlay.Add(i);
        }
        Debug.Log("pieces to play size: " + piecesToPlay.Count);
        piecesToPlay = Shuffle(piecesToPlay);


    }

    private void Start()
    {
        SpawnPiece();
    }

    // public void OnGUI()
    // {
    //     {
    //         // GUI.Label(new Rect(10, 10, 100, 20), "resetting already played.");
    //     }
    // }

    public void SpawnPiece()
    {
        if (debug)
        {
            if (piecesToPlay.Count == 0)
            {
               //@RESET
                piecesToPlay = new List<int>(playerBag.Count);
                for (int i = 0; i < playerBag.Count; i++)
                {
                    piecesToPlay.Add(i);
                }
                piecesToPlay = Shuffle(piecesToPlay);
            }
        }
    
        // pick the first piece_idx in the shuffled array
        int piece_idx = piecesToPlay[0];
        piecesToPlay.RemoveAt(0);
        
        TetrominoData data = tetrominoes[piece_idx];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition)) {
            Set(activePiece);
        } else {
            GameOver();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();

        // Do anything else you want on game over here..
        //@RESET
        piecesToPlay = new List<int>(playerBag.Count);
        for (int i = 0; i < playerBag.Count; i++)
        {
            piecesToPlay.Add(i);
        }
        piecesToPlay = Shuffle(piecesToPlay);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        //TODO(SJORS): add check for 4 rows (or more?) to invoke "TETRIS!"

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row)) {
                LineClear(row);
            } else {
                row++;
            }
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
            


        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // if there is stone in this row, it does not get erased.
        List<int> columns_to_skip_dropping = new List<int>();
        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            TileBase tileBase = tilemap.GetTile(position);

            // Check if the tile is a Tile
            if (tileBase is Tile tile)
            {
                if (tile.sprite.name == "CrackedStone")
                {
                    columns_to_skip_dropping.Add(col);
                    tilemap.SetTile(position, crackedTilePhase2);
                    continue;    
                }
            }

            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                
                // if there is stone underneath this column entry, persist.
                if (columns_to_skip_dropping.Contains(col))
                {
                   continue;
                }

                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }

        //TODO(SJORS): show "line clear!"
    }

}
