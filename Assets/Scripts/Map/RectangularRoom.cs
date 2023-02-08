using System.Collections.Generic;
using UnityEngine;

namespace TheSleepyKoala.Map
{
  [System.Serializable]
  public class RectangularRoom
  {
    [field: SerializeField] public int x { get; set; }
    [field: SerializeField] public int y { get; set; }
    [field: SerializeField] public int width { get; set; }
    [field: SerializeField] public int height { get; set; }

    public RectangularRoom(int x, int y, int width, int height)
    {
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
    }

    /// <summary>
    ///  Return the center of the room
    /// </summary>
    public Vector2Int Center() => new Vector2Int(x + width / 2, y + height / 2);

    /// <summary>
    /// Return a random inner position inside the room
    /// </summary>
    public Vector2Int RandomPoint() => new Vector2Int(Random.Range(x + 1, x + width - 1), Random.Range(y + 1, y + height - 1));

    /// <summary>
    ///  Return the area of this room as a Bounds.
    /// </summary>
    public Bounds GetBounds() => new Bounds(new Vector3(x, y, 0), new Vector3(width, height, 0));

    /// <summary>
    /// Return the area of this room as BoundsInt
    /// </summary>
    public BoundsInt GetBoundsInt() => new BoundsInt(new Vector3Int(x, y, 0), new Vector3Int(width, height, 0));

    /// <summary>
    /// Return True if this room overlaps with another RectangularRoom.
    /// </summary>
    public bool Overlaps(List<RectangularRoom> otherRooms)
    {
      foreach (RectangularRoom otherRoom in otherRooms)
      {
        if (GetBounds().Intersects(otherRoom.GetBounds()))
        {
          return true;
        }
      }
      return false;
    }
  }
}