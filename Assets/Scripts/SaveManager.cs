using UnityEngine;
using OdinSerializer;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TheSleepyKoala.Map;

namespace TheSleepyKoala
{
  public class SaveManager : MonoBehaviour
  {
    public static SaveManager instance;

    [field: SerializeField] public int CurrentFloor { get; set; } = 0;
    [SerializeField] private string saveFileName = "saveThe.koala";
    [field: SerializeField] public SaveData Save { get; set; }

    private void Awake()
    {
      if (SaveManager.instance == null)
      {
        SaveManager.instance = this;
        DontDestroyOnLoad(gameObject);
      }
      else
      {
        Destroy(gameObject);
      }
    }

    public bool HasSaveAvailable()
    {
      string path = Path.Combine(Application.persistentDataPath, saveFileName);

      if (!File.Exists(path))
      {
        return false;
      }
      return true;
    }

    public void SaveGame(bool tempSave = true)
    {
      Save.SavedFloor = CurrentFloor;

      bool hasScene = Save.Scenes.Find(x => x.FloorNumber == CurrentFloor) is not null;
      if (hasScene)
      {
        UpdateScene(SaveState());
      }
      else
      {
        AddScene(SaveState());
      }

      if (tempSave) return;

      string path = Path.Combine(Application.persistentDataPath, saveFileName);
      byte[] saveJson = SerializationUtility.SerializeValue(Save, DataFormat.JSON); //Serialize the state to JSON
      File.WriteAllBytes(path, saveJson); //Save the state to a file
    }

    public void LoadGame()
    {
      string path = Path.Combine(Application.persistentDataPath, saveFileName);
      byte[] saveJson = File.ReadAllBytes(path); //Load the state from the file
      Save = SerializationUtility.DeserializeValue<SaveData>(saveJson, DataFormat.JSON); //Deserialize the state from JSON

      CurrentFloor = Save.SavedFloor;

      if (SceneManager.GetActiveScene().name is not "Dungeon")
      {
        SceneManager.LoadScene("Dungeon");
      }
      else
      {
        LoadScene();
      }
    }

    public void DeleteSave()
    {
      string path = Path.Combine(Application.persistentDataPath, saveFileName);
      File.Delete(path);
    }

    public void AddScene(SceneState sceneState) => Save.Scenes.Add(sceneState);

    public void UpdateScene(SceneState sceneState) => Save.Scenes[CurrentFloor - 1] = sceneState;

    public void LoadScene(bool canRemovePlayer = true)
    {
      SceneState sceneState = Save.Scenes.Find(x => x.FloorNumber == CurrentFloor);
      if (sceneState is not null)
      {
        LoadState(sceneState, canRemovePlayer);
      }
      else
      {
        Debug.LogError("No save data for this floor");
      }
    }

    public SceneState SaveState() => new SceneState(
      CurrentFloor,
      GameManager.instance.SaveState(),
      MapManager.instance.State
    );

    public void LoadState(SceneState sceneState, bool canRemovePlayer)
    {
      MapManager.instance.LoadState(sceneState.MapState);
      GameManager.instance.LoadState(sceneState.GameState, canRemovePlayer);
    }
  }

  [System.Serializable]
  public class SaveData
  {
    [field: SerializeField] public int SavedFloor { get; set; }
    [field: SerializeField] public List<SceneState> Scenes { get; set; }

    public SaveData()
    {
      SavedFloor = 0;
      Scenes = new List<SceneState>();
    }
  }

  [System.Serializable]
  public class SceneState
  {
    [field: SerializeField] public int FloorNumber { get; set; }
    [field: SerializeField] public GameState GameState { get; set; }
    [field: SerializeField] public MapState MapState { get; set; }

    public SceneState(int floorNumber, GameState gameState, MapState mapState)
    {
      this.FloorNumber = floorNumber;
      this.GameState = gameState;
      this.MapState = mapState;
    }
  }
}