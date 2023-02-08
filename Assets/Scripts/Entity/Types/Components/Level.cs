using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Level : MonoBehaviour
{
  [field: SerializeField] public LevelState State { get; set; }
  [field: SerializeField] public int XpGiven { get; set; }

  public void IncreaseMaxHp(int amount = 10)
  {
    GetComponent<Fighter>().MaxHp += amount;
    GetComponent<Fighter>().Hp += amount;

    UIManager.instance.AddMessage($"Your health improves!", "#00FF00"); //Green
    State.LevelPoints--;
  }

  public void IncreasePower(int amount = 1)
  {
    GetComponent<Fighter>().State.BasePower += amount;

    UIManager.instance.AddMessage($"You feel stronger!", "#00FF00"); //Green
    State.LevelPoints--;
  }

  public void IncreaseDefense(int amount = 1)
  {
    GetComponent<Fighter>().State.BaseDefense += amount;

    UIManager.instance.AddMessage($"Your movements are getting swifter!", "#00FF00"); //Green
    State.LevelPoints--;
  }
}

[System.Serializable]
public class LevelState
{
  [SerializeField] private int levelUpBase = 200, levelUpFactor = 150;
  [field: SerializeField] public int CurrentLevel {get; set;} = 1;
  [field: SerializeField] public int CurrentXp {get; set;}
  [field: SerializeField] public int XpToNextLevel {get; private set;}
  [field: SerializeField] public int LevelPoints {get; set;} = 0;

  public LevelState() => XpToNextLevel = ExperienceToNextLevel();

  public LevelState(int currentLevel, int currentXp, int xpToNextLevel, int levelPoints)
  {
    this.CurrentLevel = currentLevel;
    this.CurrentXp = currentXp;
    this.XpToNextLevel = xpToNextLevel;
    this.LevelPoints = levelPoints;
  }

  public int ExperienceToNextLevel() => levelUpBase + CurrentLevel * levelUpFactor;
  private bool RequiresLevelUp() => CurrentXp >= XpToNextLevel;

  private void IncreaseLevel()
  {
    CurrentXp -= XpToNextLevel;
    CurrentLevel++;
    XpToNextLevel = ExperienceToNextLevel();
    LevelPoints++;
  }

  public void AddExperience(int xp)
  {
    if (xp == 0 || levelUpBase == 0) return;

    CurrentXp += xp;

    UIManager.instance.AddMessage($"You gain {xp} experience points.", "#FFFFFF");

    if (RequiresLevelUp())
    {
      IncreaseLevel();
      UIManager.instance.AddMessage($"You advance to level {CurrentLevel}!", "#00FF00"); //Green
    }
  }

  public void Load(LevelState savedState)
  {
    CurrentLevel = savedState.CurrentLevel;
    CurrentXp = savedState.CurrentXp;
    XpToNextLevel = savedState.XpToNextLevel;
    LevelPoints = savedState.LevelPoints;
  }
}