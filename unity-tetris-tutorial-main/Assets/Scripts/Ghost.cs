using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board mainBoard;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public List<Vector3Int> cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new List<Vector3Int>();
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        cells.Clear();

        for (int i = 0; i != trackingPiece.cells.Length; ++i)
        {
            cells.Add(trackingPiece.cells[i]);
        }

    }

    private void Drop()
    {
        Vector3Int position = trackingPiece.position;

        int current = position.y;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        mainBoard.Clear(trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (mainBoard.IsValidPosition(trackingPiece, position)) {
                this.position = position;
            } else {
                break;
            }
        }

        mainBoard.Set(trackingPiece);
    }

    private void Set()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }
    }

}
