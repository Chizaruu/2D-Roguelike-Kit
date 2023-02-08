using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
  public static MapManager instance;

  [field: SerializeField] public MapState state { get; set; }

  [field: Header("Map Settings")]
  [field: SerializeField] public int width { get; private set;} = 80;
  [field: SerializeField] public int height { get; private set;} = 45;
  [SerializeField] private int roomMaxSize = 10;
  [SerializeField] private int roomMinSize = 6;
  [SerializeField] private int maxRooms = 30;

  [field: Header("Tiles")]
  [field: SerializeField] public TileBase floorTile { get; private set;}
  [field: SerializeField] public TileBase wallTile { get; private set;}
  [field: SerializeField] public TileBase fogTile { get; private set;}
  [field: SerializeField] public TileBase upStairsTile { get; private set;}
  [field: SerializeField] public TileBase downStairsTile { get; private set;}

  [field: Header("Tilemaps")]
  [field: SerializeField] public Tilemap floorMap { get; private set; }
  [field: SerializeField] public Tilemap obstacleMap { get; private set; }
  [field: SerializeField] public Tilemap fogMap { get; private set; }

  [field: Header("Features")]
  [field: SerializeField] public List<Vector3Int> visibleTiles { get; private set; }
  public Dictionary<Vector2Int, Node> nodes { get; set; } = new Dictionary<Vector2Int, Node>();

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }

    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.FloorNumber == SaveManager.instance.CurrentFloor);

    if (sceneState is not null) {
      LoadState(sceneState.MapState);
    } else {
      GenerateDungeon(true);
    }

    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void Start() {
    Camera.main.transform.position = new Vector3(40, 20.25f, -10);
    Camera.main.orthographicSize = 27;
  }

  public void GenerateDungeon(bool isNewGame = false) {
    if (!isNewGame)
    {
      Reset();
    }
    else
    {
      state = new MapState();
      visibleTiles = new List<Vector3Int>();
    }

    ProcGen procGen = new ProcGen();
    procGen.GenerateDungeon(width, height, roomMaxSize, roomMinSize, maxRooms, state.rooms, isNewGame);

    AddTileMapToDictionary(floorMap);
    AddTileMapToDictionary(obstacleMap);
    SetupFogMap();

    if (!isNewGame) {
      GameManager.instance.RefreshPlayer();
    }
  }

  ///<summary>Return True if x and y are inside of the bounds of this map. </summary>
  public bool InBounds(int x, int y) => 0 <= x && x < width && 0 <= y && y < height;

  public GameObject CreateEntity(string entity, Vector2 position) {
    GameObject entityObject = Instantiate(Resources.Load<GameObject>($"{entity}"), new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
    entityObject.name = entity;
    return entityObject;
  }

  public void UpdateFogMap(List<Vector3Int> playerFOV) {
    foreach (Vector3Int pos in visibleTiles) {
      if (!state.tiles[(Vector3)pos].isExplored) {
        state.tiles[(Vector3)pos].isExplored = true;
      }

      state.tiles[(Vector3)pos].isVisible = false;
      fogMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.5f));
    }

    visibleTiles.Clear();

    foreach (Vector3Int pos in playerFOV) {
      state.tiles[(Vector3)pos].isVisible = true;
      fogMap.SetColor(pos, Color.clear);
      visibleTiles.Add(pos);
    }
  }

  public void SetEntitiesVisibilities() {
    foreach (Entity entity in GameManager.instance.Entities) {
      if (entity.GetComponent<Player>()) {
        continue;
      }

      Vector3Int entityPosition = floorMap.WorldToCell(entity.transform.position);

      if (visibleTiles.Contains(entityPosition)) {
        entity.GetComponent<SpriteRenderer>().enabled = true;
      } else {
        entity.GetComponent<SpriteRenderer>().enabled = false;
      }
    }
  }

  public bool IsValidPosition(Vector3 futurePosition) {
    Vector3Int gridPosition = floorMap.WorldToCell(futurePosition);
    if (!InBounds(gridPosition.x, gridPosition.y) || obstacleMap.HasTile(gridPosition)) {
      return false;
    }
    return true;
  }

  private void AddTileMapToDictionary(Tilemap tilemap) {
    foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
      if (!tilemap.HasTile(pos)) {
        continue;
      }

      TileData tile = new TileData(
        name: tilemap.GetTile(pos).name,
        isExplored: false,
        isVisible: false
      );

      state.tiles.Add((Vector3)pos, tile);
    }
  }

  private void SetupFogMap() {
    foreach (Vector3 pos in state.tiles.Keys)
    {
      Vector3Int posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
      fogMap.SetTile(posInt, fogTile);
      fogMap.SetTileFlags(posInt, TileFlags.None);

      if (state.tiles[pos].isExplored) {
        fogMap.SetColor(posInt, new Color(1.0f, 1.0f, 1.0f, 0.5f));
      } else {
        fogMap.SetColor(posInt, Color.white);
      }
    }
  }

  private void Reset() {
    state = new MapState();
    visibleTiles.Clear();
    nodes.Clear();

    floorMap.ClearAllTiles();
    obstacleMap.ClearAllTiles();
    fogMap.ClearAllTiles();
  }

  public void LoadState(MapState savedState) {
    if (floorMap.cellBounds.size.x > 0) {
      Reset();
    }

    state.Load(savedState);

    if (visibleTiles.Count > 0) {
      visibleTiles.Clear();
    }

    foreach (Vector3 pos in state.tiles.Keys) {
      Vector3Int posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
      if (state.tiles[pos].name == floorTile.name) {
        floorMap.SetTile(posInt, floorTile);
      } else if (state.tiles[pos].name == wallTile.name) {
        obstacleMap.SetTile(posInt, wallTile);
      } else if (state.tiles[pos].name == upStairsTile.name) {
        floorMap.SetTile(posInt, upStairsTile);
      } else if (state.tiles[pos].name == downStairsTile.name) {
        floorMap.SetTile(posInt, downStairsTile);
      }
    }
    SetupFogMap();
  }
}

[System.Serializable]
public class MapState {
  [field: SerializeField] public Dictionary<Vector3, TileData> tiles { get; set; }
  [field: SerializeField] public List<RectangularRoom> rooms { get; set; }

  public MapState() {
    tiles = new Dictionary<Vector3, TileData>();
    rooms = new List<RectangularRoom>();
  }

  public void Load(MapState savedState)
  {
    tiles = savedState.tiles;
    rooms = savedState.rooms;
  }
}