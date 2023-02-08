using UnityEngine;

namespace TheSleepyKoala.Entities
{
  public class Confusion : Consumable
  {
    [SerializeField] private int numberOfTurns = 10;

    public override bool Activate(Actor consumer)
    {
      consumer.Inventory.SelectedConsumable = this;
      consumer.GetComponent<Player>().ToggleTargetMode();
      UIManager.instance.AddMessage($"Select a target to confuse.", "#63FFFF");
      return false;
    }

    public override bool Cast(Actor consumer, Actor target)
    {
      if (target.TryGetComponent(out ConfusedEnemy confusedEnemy))
      {
        if (confusedEnemy.State.TurnsRemaining > 0)
        {
          UIManager.instance.AddMessage($"The {target.name} is already confused.", "#FF0000");
          consumer.Inventory.SelectedConsumable = null;
          return false;
        }
      }
      else
      {
        confusedEnemy = target.gameObject.AddComponent<ConfusedEnemy>();
      }
      confusedEnemy.PreviousAI = target.AI;
      confusedEnemy.State = new AIState("ConfusedEnemy", confusedEnemy.PreviousAI.State.Type, numberOfTurns);

      UIManager.instance.AddMessage($"The eyes of the {target.name} look vacant, as it starts to stumble around!", "#FF0000");
      target.AI = confusedEnemy;
      Consume(consumer);
      consumer.GetComponent<Player>().ToggleTargetMode();
      return true;
    }
  }
}