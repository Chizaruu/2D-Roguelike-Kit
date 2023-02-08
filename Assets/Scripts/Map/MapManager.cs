using System.Collections.Generic;
using TheSleepyKoala.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace TheSleepyKoala.Map
{
  public class MapManager : MonoBehaviour
  {
    public static MapManager instance;

    [field: SerializeField] public MapState State { get; set; }

    [field: Header("Map Settings")]
    [field: SerializeField] public int Width { get; private set; } = 80;
    [field: SerializeField] public int Height { get; private set; } = 45;
    [SerializeField] private int roomMaxSize = 10;
    [SerializeField] private int roomMinSize = 6;
    [SerializeField] private int maxRooms = 30;

    [field: Header("Tiles")]
    [field: SerializeField] public TileBase FloorTile { get; private set; }
    [field: SerializeField] public TileBase WallTile { get; private set; }
    [field: SerializeField] public TileBase FogTile { get; private set; }
    [field: SerializeField] public TileBase UpStairsTile { get; private set; }
    [field: SerializeField] public TileBase DownStairsTile { get; private set; }

    [field: Header("Tilemaps")]
    [field: SerializeField] public Tilemap FloorMap { get; private set; }
    [field: SerializeField] public Tilemap ObstacleMap { get; private set; }
    [field: SerializeField] public Tilemap FogMap { get; private set; }

    [field: Header("Features")]
    [field: SerializeField] public List<Vector3Int> VisibleTiles { get; private set; }
    public Dictionary<Vector2Int, Node> Nodes { get; set; } = new Dictionary<Vector2Int, Node>();

    private void Awake()
    {
      if (instance == null)
      {
        instance = this;
      }
      else
      {
        Destroy(gameObject);
      }

      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.FloorNumber == SaveManager.instance.CurrentFloor);

      if (sceneState is not null)
      {
        LoadState(sceneState.MapState);
      }
      else
      {
        GenerateDungeon(true);
      }

      SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
      Camera.main.transform.position = new Vector3(40, 20.25f, -10);
      Camera.main.orthographicSize = 27;
    }

    public void GenerateDungeon(bool isNewGame = false)
    {
      if (!isNewGame)
      {
        Reset();
      }
      else
      {
        State = new MapState();
        VisibleTiles = new List<Vector3Int>();
      }

      ProcGen procGen = new ProcGen();
      procGen.GenerateDungeon(Width, Height, roomMaxSize, roomMinSize, maxRooms, State.rooms, isNewGame);

      AddTileMapToDictionary(FloorMap);
      AddTileMapToDictionary(ObstacleMap);
      SetupFogMap();

      if (!isNewGame)
      {
        GameManager.instance.RefreshPlayer();
      }
    }

    ///<summary>Return True if x and y are inside of the bounds of this map. </summary>
    public bool InBounds(int x, int y) => 0 <= x && x < Width && 0 <= y && y < Height;

    public GameObject CreateEntity(string entity, Vector2 position)
    {
      GameObject entityObject = Instantiate(Resources.Load<GameObject>($"{entity}"), new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
      entityObject.name = entity;
      return entityObject;
    }

    public void UpdateFogMap(List<Vector3Int> playerFOV)
    {
      foreach (Vector3Int pos in VisibleTiles)
      {
        if (!State.tiles[(Vector3)pos].isExplored)
        {
          State.tiles[(Vector3)pos].isExplored = true;
        }

        State.tiles[(Vector3)pos].isVisible = false;
        FogMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.5f));
      }

      VisibleTiles.Clear();

      foreach (Vector3Int pos in playerFOV)
      {
        State.tiles[(Vector3)pos].isVisible = true;
        FogMap.SetColor(pos, Color.clear);
        VisibleTiles.Add(pos);
      }
    }

    public void SetEntitiesVisibilities()
    {
      foreach (Entity entity in GameManager.instance.Entities)
      {
        if (entity.GetComponent<Player>())
        {
          continue;
        }

        Vector3Int entityPosition = FloorMap.WorldToCell(entity.transform.position);

        if (VisibleTiles.Contains(entityPosition))
        {
          entity.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
          entity.GetComponent<SpriteRenderer>().enabled = false;
        }
      }
    }

    public bool IsValidPosition(Vector3 futurePosition)
    {
      Vector3Int gridPosition = FloorMap.WorldToCell(futurePosition);
      if (!InBounds(gridPosition.x, gridPosition.y) || ObstacleMap.HasTile(gridPosition))
      {
        return false;
      }
      return true;
    }

    private void AddTileMapToDictionary(Tilemap tilemap)
    {
      foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
      {
        if (!tilemap.HasTile(pos))
        {
          continue;
        }

        TileData tile = new TileData(
          name: tilemap.GetTile(pos).name,
          isExplored: false,
          isVisible: false
        );


        State.tiles.Add((Vector3)pos, tile);
      }
    }

    private void SetupFogMap()
    {
      foreach (Vector3 pos in State.tiles.Keys)
      {
        Vector3Int posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        FogMap.SetTile(posInt, FogTile);
        FogMap.SetTileFlags(posInt, TileFlags.None);

        if (State.tiles[pos].isExplored)
        {
          FogMap.SetColor(posInt, new Color(1.0f, 1.0f, 1.0f, 0.5f));
        }
        else
        {
          FogMap.SetColor(posInt, Color.white);
        }
      }
    }

    private void Reset()
    {
      State = new MapState();
      VisibleTiles.Clear();
      Nodes.Clear();

      FloorMap.ClearAllTiles();
      ObstacleMap.ClearAllTiles();
      FogMap.ClearAllTiles();
    }

    public void LoadState(MapState savedState)
    {
      if (FloorMap.cellBounds.size.x > 0)
      {
        Reset();
      }

      State.Load(savedState);

      if (VisibleTiles.Count > 0)
      {
        VisibleTiles.Clear();
      }

      foreach (Vector3 pos in State.tiles.Keys)
      {
        Vector3Int posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
        if (State.tiles[pos].name == FloorTile.name)
        {
          FloorMap.SetTile(posInt, FloorTile);
        }
        else if (State.tiles[pos].name == WallTile.name)
        {
          ObstacleMap.SetTile(posInt, WallTile);
        }
        else if (State.tiles[pos].name == UpStairsTile.name)
        {
          FloorMap.SetTile(posInt, UpStairsTile);
        }
        else if (State.tiles[pos].name == DownStairsTile.name)
        {
          FloorMap.SetTile(posInt, DownStairsTile);
        }
      }
      SetupFogMap();
    }
  }

  [System.Serializable]
  public class MapState
  {
    [field: SerializeField] public Dictionary<Vector3, TileData> tiles { get; set; }
    [field: SerializeField] public List<RectangularRoom> rooms { get; set; }

    public MapState()
    {
      tiles = new Dictionary<Vector3, TileData>();
      rooms = new List<RectangularRoom>();
    }

    public void Load(MapState savedState)
    {
      tiles = savedState.tiles;
      rooms = savedState.rooms;
    }
  }
}