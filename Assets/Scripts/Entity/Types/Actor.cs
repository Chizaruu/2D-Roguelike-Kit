using System.Collections.Generic;
using UnityEngine;

public class Actor : Entity {
  [field: SerializeField] public bool isAlive { get; set; } = true;
  [SerializeField] private int fieldOfViewRange = 8;
  [field: SerializeField] public List<Vector3Int> fieldOfView { get; private set; } = new List<Vector3Int>();
  [field: SerializeField] public Inventory inventory { get; private set; }
  [field: SerializeField] public Equipment equipment { get; private set; }
  [field: SerializeField] public AI aI { get; set; }
  [field: SerializeField] public Fighter fighter { get; private set; }
  [field: SerializeField] public Level level { get; private set; }
  AdamMilVisibility algorithm;

  private void OnValidate() {
    if (GetComponent<Inventory>()) {
      inventory = GetComponent<Inventory>();
    }

    if (GetComponent<AI>()) {
      aI = GetComponent<AI>();
    }

    if (GetComponent<Fighter>()) {
      fighter = GetComponent<Fighter>();
    }

    if (GetComponent<Level>()) {
      level = GetComponent<Level>();
    }

    if (GetComponent<Equipment>()) {
      equipment = GetComponent<Equipment>();
    }
  }

  private void Start() {
    AddToGameManager();

    if (isAlive) {
      algorithm = new AdamMilVisibility();
      UpdateFieldOfView();
    } else if (fighter != null) {
      fighter.Die();
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

    fieldOfView.Clear();
    algorithm.Compute(gridPosition, fieldOfViewRange, fieldOfView);

    if (GetComponent<Player>()) {
      MapManager.instance.UpdateFogMap(fieldOfView);
      MapManager.instance.SetEntitiesVisibilities();
    }
  }

  public override EntityState SaveState() => new ActorState(
    name: name,
    blocksMovement: blocksMovement,
    isAlive: isAlive,
    isVisible: MapManager.instance.visibleTiles.Contains(MapManager.instance.floorMap.WorldToCell(transform.position)),
    position: transform.position,
    currentAI: aI != null ? aI.SaveState() : null,
    fighterState: fighter != null ? fighter.SaveState() : null,
    levelState: level != null && GetComponent<Player>() ? level.SaveState() : null
  );

  public void LoadState(ActorState state) {
    transform.position = state.position;
    isAlive = state.isAlive;

    if (!isAlive) {
      GameManager.instance.RemoveActor(this);
    }

    if (!state.isVisible) {
      spriteRenderer.enabled = false;
    }

    if (state.currentAI != null) {
      if (state.currentAI.Type == "HostileEnemy") {
        aI = GetComponent<HostileEnemy>();
      } else if (state.currentAI.Type == "ConfusedEnemy") {
        aI = gameObject.AddComponent<ConfusedEnemy>();

        ConfusedState confusedState = state.currentAI as ConfusedState;
        ((ConfusedEnemy)aI).LoadState(confusedState);
      }
    }

    if (state.fighterState != null) {
      fighter.LoadState(state.fighterState);
    }

    if (state.levelState != null) {
      level.LoadState(state.levelState);
    }
  }
}

[System.Serializable]
public class ActorState : EntityState {
  [field: SerializeField] public bool isAlive { get; set; }
  [field: SerializeField] public AIState currentAI { get; set; }
  [field: SerializeField] public FighterState fighterState { get; set; }
  [field: SerializeField] public LevelState levelState { get; set; }

  public ActorState(EntityType type = EntityType.Actor, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
   bool isAlive = true, AIState currentAI = null, FighterState fighterState = null, LevelState levelState = null) : base(type, name, blocksMovement, isVisible, position) {
    this.isAlive = isAlive;
    this.currentAI = currentAI;
    this.fighterState = fighterState;
    this.levelState = levelState;
  }
}