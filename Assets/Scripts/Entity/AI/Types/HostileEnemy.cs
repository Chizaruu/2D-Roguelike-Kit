using UnityEngine;

[RequireComponent(typeof(Fighter))]
public class HostileEnemy : AI {
  [SerializeField] private Fighter fighter;
  [SerializeField] private bool isFighting;

  private void OnValidate() {
    fighter = GetComponent<Fighter>();
    AStar = GetComponent<AStar>();

    State.Type = "HostileEnemy";
  }

  public override void RunAI()
    {   
        if (!fighter.Target)
        {
            fighter.Target = GameManager.instance.Actors[0];
        }
        else if (fighter.Target && !fighter.Target.IsAlive)
        {
            fighter.Target = null;
        }

        if (fighter.Target)
        {
            Vector3Int TargetPosition = MapManager.instance.floorMap.WorldToCell(fighter.Target.transform.position);
            if (isFighting || GetComponent<Actor>().FieldOfView.Contains(TargetPosition))
            {
                if (!isFighting)
                {
                    isFighting = true;
                }

                float TargetDistance = Vector3.Distance(transform.position, fighter.Target.transform.position);

                if (TargetDistance <= 1.5f)
                {
                    Action.MeleeAction(GetComponent<Actor>(), fighter.Target);
                    return;
                }
                else
                { //If not in range, move towards target
                    MoveAlongPath(TargetPosition);
                    return;
                }
            }
        }

        Action.WaitAction();
    }
}