using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
  public static GameManager instance;

  [Header("Time")]
  [SerializeField] private float baseTime = 0.075f;
  [SerializeField] private float delayTime; //Read-only
  [field: SerializeField] public bool IsPlayerTurn { get; set; } = true;

  [field: Header("Entities & Actors")]
  [field: SerializeField] public List<Entity> Entities { get; private set; }
  [field: SerializeField] public List<Actor> Actors { get; private set; }
  private Queue<Actor> actorQueue = new Queue<Actor>();

  [field: Header("Death")]
  [field: SerializeField] public Sprite DeadSprite { get; private set; }

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
      LoadState(sceneState.GameState, true);
    } else {
      Entities = new List<Entity>();
      Actors = new List<Actor>();
    }

    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void StartTurn() {
    Actor actor = actorQueue.Peek();
    if(actor.GetComponent<Player>()) {
      IsPlayerTurn = true;
    } else {
      if (actor.AI != null) {
        actor.AI.RunAI();
      } else {
        Action.WaitAction();
      }
    }
  }

  public void EndTurn() {
    Actor actor = actorQueue.Dequeue();

    if (actor.GetComponent<Player>()) {
      IsPlayerTurn = false;
    }

    actorQueue.Enqueue(actor);

    StartCoroutine(TurnDelay());
  }

  public IEnumerator TurnDelay() {
    yield return new WaitForSeconds(delayTime);
    StartTurn();
  }

  public void AddOrInsertEntity(Entity entity, int index = -1)
  {
    if (!entity.gameObject.activeSelf)
    {
      entity.gameObject.SetActive(true);
    }

    if (index < 0)
    {
      Entities.Add(entity);
    }
    else
    {
      Entities.Insert(index, entity);
    }
  }

  public void RemoveEntity(Entity entity) {
    entity.gameObject.SetActive(false);
    Entities.Remove(entity);
  }

  public void AddOrInsertActor(Actor actor, int index = -1)
  {
    if (index < 0)
    {
      Actors.Add(actor);
    }
    else
    {
      Actors.Insert(index, actor);
    }

    delayTime = SetTime();
    actorQueue.Enqueue(actor);
  }

  public void RemoveActor(Actor actor) {
    if(actor.GetComponent<Player>()) {
      return;
    }
    Actors.Remove(actor);
    delayTime = SetTime();
    actorQueue = new Queue<Actor>(actorQueue.Where(x => x != actor));
  }

  public void RefreshPlayer() {
    Actors[0].UpdateFieldOfView();
  }

  public Actor GetActorAtLocation(Vector3 location) {
    foreach (Actor actor in Actors) {
      if (actor.BlocksMovement && actor.transform.position == location) {
        return actor;
      }
    }
    return null;
  }

  private float SetTime() => baseTime / Actors.Count;

  public GameState SaveState() {
    foreach (Item item in Actors[0].Inventory.Items) {
      AddOrInsertEntity(item);
    }

    GameState gameState = new GameState(entities: Entities.ConvertAll(x => x.SaveState()));

    foreach (Item item in Actors[0].Inventory.Items) {
      RemoveEntity(item);
    }

    return gameState;
  }

  public void LoadState(GameState state, bool canRemovePlayer) {
    IsPlayerTurn = false; //Prevents player from moving during load

    Reset(canRemovePlayer);
    StartCoroutine(LoadEntityStates(state.entityStates, canRemovePlayer));
  }

  private IEnumerator LoadEntityStates(List<EntityState> entityStates, bool canPlacePlayer) {
    int entityState = 0;
    while (entityState < entityStates.Count) {
      yield return new WaitForEndOfFrame();

      if (entityStates[entityState].Type == EntityState.EntityType.Actor) {
        ActorState actorState = entityStates[entityState] as ActorState;

        string entityName = entityStates[entityState].Name.Contains("Remains of") ?
          entityStates[entityState].Name.Substring(entityStates[entityState].Name.LastIndexOf(' ') + 1) : entityStates[entityState].Name;

        if (entityName == "Player" && !canPlacePlayer) {
          Actors[0].transform.position = entityStates[entityState].Position;
          RefreshPlayer();
          entityState++;
          continue;
        }

        Actor actor = MapManager.instance.CreateEntity(entityName, actorState.Position).GetComponent<Actor>();

        actor.LoadState(actorState);
      } else if (entityStates[entityState].Type == EntityState.EntityType.Item) {
        ItemState itemState = entityStates[entityState] as ItemState;

        string entityName = entityStates[entityState].Name.Contains("(E)") ?
          entityStates[entityState].Name.Replace(" (E)", "") : entityStates[entityState].Name;

        if (itemState.Parent == "Player" && !canPlacePlayer) {
          entityState++;
          continue;
        }

        Item item = MapManager.instance.CreateEntity(entityName, itemState.Position).GetComponent<Item>();

        item.LoadState(itemState);
      }

      entityState++;
    }
    IsPlayerTurn = true; //Allows player to move after load
  }

  public void Reset(bool canRemovePlayer) {
    if (Entities.Count > 0) {
      for (int i = 0; i < Entities.Count; i++)
      {
        if (!canRemovePlayer && Entities[i].GetComponent<Player>())
        {
          continue;
        }

        Destroy(Entities[i].gameObject);
      }

      if (canRemovePlayer) {
        Entities.Clear();
        Actors.Clear();
        actorQueue.Clear();
      } else {
        Entities.RemoveAll(x => x.GetComponent<Player>() == null);
        Actors.RemoveAll(x => x.GetComponent<Player>() == null);
        actorQueue = new Queue<Actor>(actorQueue.Where(x => x.GetComponent<Player>()));
      }
    }
  }
}

[System.Serializable]
public class GameState {
  [field: SerializeField] public List<EntityState> entityStates { get; set; }

  public GameState(List<EntityState> entities)
  {
    this.entityStates = entities;
  }
}