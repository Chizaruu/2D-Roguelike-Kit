using UnityEngine;

namespace TheSleepyKoala.Entities
{
  [RequireComponent(typeof(Actor))]
  public class Fighter : MonoBehaviour
  {
    [field: SerializeField] public FighterState State { get; set; }
    [field: SerializeField] public Actor Target { get; set; }

    public int Hp
    {
      get => State.Hp; set
      {
        State.Hp = Mathf.Max(0, Mathf.Min(value, State.MaxHp));

        if (GetComponent<Player>())
        {
          UIManager.instance.SetHealth(State.Hp, State.MaxHp);
        }

        if (State.Hp == 0)
          Die();
      }
    }

    public int MaxHp
    {
      get => State.MaxHp; set
      {
        State.MaxHp = value;
        if (GetComponent<Player>())
        {
          UIManager.instance.SetHealthMax(State.MaxHp);
        }
      }
    }

    public int Power() => State.BasePower + PowerBonus();

    public int Defense() => State.BaseDefense + DefenseBonus();

    public int DefenseBonus()
    {
      if (GetComponent<Equipment>() is not null)
      {
        return GetComponent<Equipment>().DefenseBonus();
      }

      return 0;
    }

    public int PowerBonus()
    {
      if (GetComponent<Equipment>() is not null)
      {
        return GetComponent<Equipment>().PowerBonus();
      }

      return 0;
    }

    private void Start()
    {
      if (GetComponent<Player>())
      {
        UIManager.instance.SetHealthMax(State.MaxHp);
        UIManager.instance.SetHealth(State.Hp, State.MaxHp);
      }
    }


    public void Die()
    {
      if (GetComponent<Actor>().IsAlive)
      {
        if (GetComponent<Player>())
        {
          UIManager.instance.AddMessage("You died!", "#ff0000"); //Red
        }
        else
        {
          GameManager.instance.Actors[0].Level.State.AddExperience(GetComponent<Level>().XpGiven); //Give XP to player
          UIManager.instance.AddMessage($"{name} is dead!", "#ffa500"); //Light Orange
        }
        GetComponent<Actor>().IsAlive = false;
      }

      SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
      spriteRenderer.sprite = GameManager.instance.DeadSprite;
      spriteRenderer.color = new Color(191, 0, 0, 1);
      spriteRenderer.sortingOrder = 0;

      name = $"Remains of {name}";
      GetComponent<Actor>().BlocksMovement = false;
      if (!GetComponent<Player>())
      {
        GameManager.instance.RemoveActor(this.GetComponent<Actor>());
      }
    }

    public int Heal(int amount)
    {
      if (State.Hp == State.MaxHp)
      {
        return 0;
      }

      int newHPValue = State.Hp + amount;

      if (newHPValue > State.MaxHp)
      {
        newHPValue = State.MaxHp;
      }

      int amountRecovered = newHPValue - State.Hp;
      Hp = newHPValue;
      return amountRecovered;
    }
  }

  [System.Serializable]
  public class FighterState
  {
    [field: SerializeField] public int MaxHp { get; set; }
    [field: SerializeField] public int Hp { get; set; }
    [field: SerializeField] public int BaseDefense { get; set; }
    [field: SerializeField] public int BasePower { get; set; }

    public FighterState(int maxHp, int hp, int defense, int power)
    {
      this.MaxHp = maxHp;
      this.Hp = hp;
      this.BaseDefense = defense;
      this.BasePower = power;
    }

    public void Load(FighterState savedState)
    {
      MaxHp = savedState.MaxHp;
      Hp = savedState.Hp;
      BaseDefense = savedState.BaseDefense;
      BasePower = savedState.BasePower;
    }
  }
}