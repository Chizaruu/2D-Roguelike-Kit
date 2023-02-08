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
  [field: SerializeField] public bool isPlayerTurn { get; set; } = true;

  [field: Header("Entities")]
  [field: SerializeField] public List<Entity> entities { get; private set; }
  [field: SerializeField] public List<Actor> actors { get; private set; }
  private Queue<Actor> actorQueue = new Queue<Actor>();

  [field: Header("Death")]
  [field: SerializeField] public Sprite deadSprite { get; private set; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    SceneState sceneState = SaveManager.instance.save.scenes.Find(x => x.floorNumber == SaveManager.instance.currentFloor);

    if (sceneState is not null) {
      LoadState(sceneState.gameState, true);
    } else {
      entities = new List<Entity>();
      actors = new List<Actor>();
    }

    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void StartTurn() {
    Actor actor = actorQueue.Peek();
    if(actor.GetComponent<Player>()) {
      isPlayerTurn = true;
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
      isPlayerTurn = false;
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
      entities.Add(entity);
    }
    else
    {
      entities.Insert(index, entity);
    }
  }

  public void RemoveEntity(Entity entity) {
    entity.gameObject.SetActive(false);
    entities.Remove(entity);
  }

  public void AddOrInsertActor(Actor actor, int index = -1)
  {
    if (index < 0)
    {
      actors.Add(actor);
    }
    else
    {
      actors.Insert(index, actor);
    }

    delayTime = SetTime();
    actorQueue.Enqueue(actor);
  }

  public void RemoveActor(Actor actor) {
    if(actor.GetComponent<Player>()) {
      return;
    }
    actors.Remove(actor);
    delayTime = SetTime();
    actorQueue = new Queue<Actor>(actorQueue.Where(x => x != actor));
  }

  public void RefreshPlayer() {
    actors[0].UpdateFieldOfView();
  }

  public Actor GetActorAtLocation(Vector3 location) {
    foreach (Actor actor in actors) {
      if (actor.BlocksMovement && actor.transform.position == location) {
        return actor;
      }
    }
    return null;
  }

  private float SetTime() => baseTime / actors.Count;

  public GameState SaveState() {
    foreach (Item item in actors[0].Inventory.Items) {
      AddOrInsertEntity(item);
    }

    GameState gameState = new GameState(entities: entities.ConvertAll(x => x.SaveState()));

    foreach (Item item in actors[0].Inventory.Items) {
      RemoveEntity(item);
    }

    return gameState;
  }

  public void LoadState(GameState state, bool canRemovePlayer) {
    isPlayerTurn = false; //Prevents player from moving during load

    Reset(canRemovePlayer);
    StartCoroutine(LoadEntityStates(state.entities, canRemovePlayer));
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
          actors[0].transform.position = entityStates[entityState].Position;
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
    isPlayerTurn = true; //Allows player to move after load
  }

  public void Reset(bool canRemovePlayer) {
    if (entities.Count > 0) {
      for (int i = 0; i < entities.Count; i++)
      {
        if (!canRemovePlayer && entities[i].GetComponent<Player>())
        {
          continue;
        }

        Destroy(entities[i].gameObject);
      }

      if (canRemovePlayer) {
        entities.Clear();
        actors.Clear();
        actorQueue.Clear();
      } else {
        entities.RemoveAll(x => x.GetComponent<Player>() == null);
        actors.RemoveAll(x => x.GetComponent<Player>() == null);
        actorQueue = new Queue<Actor>(actorQueue.Where(x => x.GetComponent<Player>()));
      }
    }
  }
}

[System.Serializable]
public class GameState {
  [field: SerializeField] public List<EntityState> entities { get; set; }

  public GameState(List<EntityState> entities)
  {
    this.entities = entities;
  }
}