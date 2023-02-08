using System.Collections.Generic;
using UnityEngine;

public class Actor : Entity {
  [field: SerializeField] public bool IsAlive { get; set; } = true;
  [SerializeField] private int fieldOfViewRange = 8;
  [field: SerializeField] public List<Vector3Int> FieldOfView { get; private set; } = new List<Vector3Int>();
  [field: SerializeField] public Inventory Inventory { get; private set; }
  [field: SerializeField] public Equipment Equipment { get; private set; }
  [field: SerializeField] public AI AI { get; set; }
  [field: SerializeField] public Fighter Fighter { get; private set; }
  [field: SerializeField] public Level Level { get; private set; }
  AdamMilVisibility algorithm;

  private void OnValidate() {
    if (GetComponent<Inventory>()) {
      Inventory = GetComponent<Inventory>();
    }

    if (GetComponent<AI>()) {
      AI = GetComponent<AI>();
    }

    if (GetComponent<Fighter>()) {
      Fighter = GetComponent<Fighter>();
    }

    if (GetComponent<Level>()) {
      Level = GetComponent<Level>();
    }

    if (GetComponent<Equipment>()) {
      Equipment = GetComponent<Equipment>();
    }
  }

  private void Start() {
    AddToGameManager();

    if (IsAlive) {
      algorithm = new AdamMilVisibility();
      UpdateFieldOfView();
    } else if (Fighter != null) {
      Fighter.Die();
    }
  }

  public override void AddToGameManager() {
    base.AddToGameManager();

    if (GetComponent<Player>()) {
      GameManager.instance.AddOrInsertActor(this, 0);
    } else {
      GameManager.instance.AddOrInsertActor(this);
    }
  }

  public void UpdateFieldOfView() {
    Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);

    FieldOfView.Clear();
    algorithm.Compute(gridPosition, fieldOfViewRange, FieldOfView);

    if (GetComponent<Player>()) {
      MapManager.instance.UpdateFogMap(FieldOfView);
      MapManager.instance.SetEntitiesVisibilities();
    }
  }

  public override EntityState SaveState() => new ActorState(
    name: name,
    blocksMovement: BlocksMovement,
    isAlive: IsAlive,
    isVisible: MapManager.instance.VisibleTiles.Contains(MapManager.instance.FloorMap.WorldToCell(transform.position)),
    position: transform.position,
    aiState: AI != null ? AI.State : null,
    fighterState: Fighter != null ? Fighter.State : null,
    levelState: Level != null && GetComponent<Player>() ? Level.State : null
  );

  public void LoadState(ActorState savedState) {
    transform.position = savedState.Position;
    IsAlive = savedState.IsAlive;

    if (!IsAlive) {
      GameManager.instance.RemoveActor(this);
    }

    if (!savedState.IsVisible) {
      SpriteRenderer.enabled = false;
    }

    if (savedState.AIState != null)
    {
      AI.State = savedState.AIState;

      switch (savedState.AIState.Type)
      {
        case "HostileEnemy":
          AI = GetComponent<HostileEnemy>();
          break;
        case "ConfusedEnemy":
          AI = gameObject.AddComponent<ConfusedEnemy>();
          (AI as ConfusedEnemy).SetPreviousAI();
          break;
        default:
          break;
      }
    }
    if (savedState.FighterState != null) {
      Fighter.State.Load(savedState.FighterState);
    }

    if (savedState.LevelState != null) {
      Level.State.Load(savedState.LevelState);
    }
  }
}

[System.Serializable]
public class ActorState : EntityState {
  [field: SerializeField] public bool IsAlive { get; set; }
  [field: SerializeField] public AIState AIState { get; set; }
  [field: SerializeField] public FighterState FighterState { get; set; }
  [field: SerializeField] public LevelState LevelState { get; set; }

  public ActorState(EntityType type = EntityType.Actor, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
   bool isAlive = true, AIState aiState = null, FighterState fighterState = null, LevelState levelState = null) : base(type, name, blocksMovement, isVisible, position) {
    this.IsAlive = isAlive;
    this.AIState = aiState;
    this.FighterState = fighterState;
    this.LevelState = levelState;
  }
}