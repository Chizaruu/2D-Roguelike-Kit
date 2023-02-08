using UnityEngine;

[RequireComponent(typeof(Actor), typeof(AStar))]
public class AI : MonoBehaviour
{
  [field: SerializeField] public AIState State { get; set; }
  [field: SerializeField] public AStar AStar { get; set; }

  private void OnValidate() => AStar = GetComponent<AStar>();

  public virtual void RunAI() { }

  public void MoveAlongPath(Vector3Int targetPosition) =>
    Action.MovementAction(GetComponent<Actor>(), GetDirectionToTarget(targetPosition));

  public Vector2 GetDirectionToTarget(Vector3Int targetPosition)
  {
    Vector3Int gridPosition = MapManager.instance.floorMap.WorldToCell(transform.position);
    Vector2 direction = AStar.Compute((Vector2Int)gridPosition, (Vector2Int)targetPosition);
    return direction;
  }
}

[System.Serializable]
public class AIState
{
  [field: SerializeField] public string Type { get; set; }
  [field: SerializeField] public string PreviousAI { get; set; }
  [field: SerializeField] public int TurnsRemaining { get; set; }

  public AIState(string type = "", string previousAI = "", int turnsRemaining = 10)
  {
    this.Type = type;
    this.PreviousAI = previousAI;
    this.TurnsRemaining = turnsRemaining;
  }

  public void Load(AIState savedState)
  {
    Type = savedState.Type;
    PreviousAI = savedState.PreviousAI;
    TurnsRemaining = savedState.TurnsRemaining;
  }
}
