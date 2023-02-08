using UnityEngine;

public class Confusion : Consumable {
  [field: SerializeField] public int numberOfTurns { get; private set; } = 10;

  public override bool Activate(Actor consumer) {
    consumer.Inventory.SelectedConsumable = this;
    consumer.GetComponent<Player>().ToggleTargetMode();
    UIManager.instance.AddMessage($"Select a target to confuse.", "#63FFFF");
    return false;
  }

  public override bool Cast(Actor consumer, Actor target) {
    if (target.TryGetComponent(out ConfusedEnemy confusedEnemy)) {
      if (confusedEnemy.TurnsRemaining > 0) {
        UIManager.instance.AddMessage($"The {target.name} is already confused.", "#FF0000");
        consumer.Inventory.SelectedConsumable = null;
        return false;
      }
    } else {
      confusedEnemy = target.gameObject.AddComponent<ConfusedEnemy>();
    }
    confusedEnemy.PreviousAI = target.AI;
    confusedEnemy.TurnsRemaining = numberOfTurns;

    UIManager.instance.AddMessage($"The eyes of the {target.name} look vacant, as it starts to stumble around!", "#FF0000");
    target.AI = confusedEnemy;
    Consume(consumer);
    consumer.GetComponent<Player>().ToggleTargetMode();
    return true;
  }
}
