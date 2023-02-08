using UnityEngine;

namespace TheSleepyKoala.Entities
{
  /// <summary>
  /// A confused enemy will stumble around aimlessly for a given number of turns, then revert back to its previous AI.
  /// If an actor occupies a tile it is randomly moving into, it will attack.
  /// </summary>
  [RequireComponent(typeof(Actor))]
  public class ConfusedEnemy : AI
  {
    [field: SerializeField] public AI PreviousAI { get; set; }

    public void SetPreviousAI()
    {
      if (State.PreviousAI == "HostileEnemy")
      {
        PreviousAI = GetComponent<HostileEnemy>();
      }
    }

    private void Start()
    {
      State = new AIState();
    }

    public override void RunAI()
    {
      // Revert the AI back to the original state if the effect has run its course.
      if (State.TurnsRemaining <= 0)
      {
        UIManager.instance.AddMessage($"The {gameObject.name} is no longer confused.", "#FF0000");

        Actor actor = GetComponent<Actor>();
        actor.AI = PreviousAI;
        actor.AI.State.Type = PreviousAI.GetType().ToString();
        actor.AI.RunAI();
        Destroy(this);
      }
      else
      {
        // Move randomly.
        Vector2Int direction = Random.Range(0, 8) switch
        {
          0 => new Vector2Int(0, 1), // North-West
          1 => new Vector2Int(0, -1), // North
          2 => new Vector2Int(1, 0), // North-East
          3 => new Vector2Int(-1, 0), // West
          4 => new Vector2Int(1, 1), // East
          5 => new Vector2Int(1, -1), // South-West
          6 => new Vector2Int(-1, 1), // South
          7 => new Vector2Int(-1, -1), // South-East
          _ => new Vector2Int(0, 0)
        };
        //The actor will either try to move or attack in the chosen random direction.
        //It's possible the actor will just bump into the wall, wasting a turn.
        Action.BumpAction(GetComponent<Actor>(), direction);
        State.TurnsRemaining--;
      }
    }
  }
}