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
    Vector3Int gridPosition = MapManager.instance.floorMap.WorldToCell(transform.position);

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
    isVisible: MapManager.instance.visibleTiles.Contains(MapManager.instance.floorMap.WorldToCell(transform.position)),
    position: transform.position,
    currentAI: AI != null ? AI.SaveState() : null,
    fighterState: Fighter != null ? Fighter.SaveState() : null,
    levelState: Level != null && GetComponent<Player>() ? Level.SaveState() : null
  );

  public void LoadState(ActorState state) {
    transform.position = state.Position;
    IsAlive = state.IsAlive;

    if (!IsAlive) {
      GameManager.instance.RemoveActor(this);
    }

    if (!state.IsVisible) {
      SpriteRenderer.enabled = false;
    }

    if (state.CurrentAI != null) {
      if (state.CurrentAI.Type == "HostileEnemy") {
        AI = GetComponent<HostileEnemy>();
      } else if (state.CurrentAI.Type == "ConfusedEnemy") {
        AI = gameObject.AddComponent<ConfusedEnemy>();

        ConfusedState confusedState = state.CurrentAI as ConfusedState;
        ((ConfusedEnemy)AI).LoadState(confusedState);
      }
    }

    if (state.FighterState != null) {
      Fighter.LoadState(state.FighterState);
    }

    if (state.LevelState != null) {
      Level.LoadState(state.LevelState);
    }
  }
}

[System.Serializable]
public class ActorState : EntityState {
  [field: SerializeField] public bool IsAlive { get; set; }
  [field: SerializeField] public AIState CurrentAI { get; set; }
  [field: SerializeField] public FighterState FighterState { get; set; }
  [field: SerializeField] public LevelState LevelState { get; set; }

  public ActorState(EntityType type = EntityType.Actor, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
   bool isAlive = true, AIState currentAI = null, FighterState fighterState = null, LevelState levelState = null) : base(type, name, blocksMovement, isVisible, position) {
    this.IsAlive = isAlive;
    this.CurrentAI = currentAI;
    this.FighterState = fighterState;
    this.LevelState = levelState;
  }
}