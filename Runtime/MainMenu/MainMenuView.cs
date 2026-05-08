using UnityEngine.UIElements;
using CupkekGames.Data;
using CupkekGames.Luna;
using CupkekGames.GameSave;

namespace CupkekGames.GameSave.Luna
{
  public abstract class MainMenuView<TSaveData, TSaveMetadata> : UIViewComponent where TSaveData : IGameSaveData, IData, new() where TSaveMetadata : GameSaveMetadata
  {
    private GameSaveManager<TSaveData, TSaveMetadata> _gameSaveManager;
    public GameSaveManager<TSaveData, TSaveMetadata> GameSaveManager => _gameSaveManager;
    private TSaveMetadata _lastSaveMetadata;
    public TSaveMetadata LastSaveMetadata => _lastSaveMetadata;
    private int _lastSaveSlot;
    public int LastSaveSlot => _lastSaveSlot;
    // UI
    protected Button _buttonContinue;
    protected Button _buttonLoad;
    protected Button _buttonNewGame;
    protected Button _buttonCredits;
    protected Button _buttonSettings;
    protected Button _buttonQuit;

    // Start is called before the first frame update
    protected override void Awake()
    {
      _gameSaveManager = GetSaveManager();

      GameSaveMetadataWithSlot<TSaveMetadata> lastSaveInfo = _gameSaveManager.GetLastMetadata();
      _lastSaveMetadata = lastSaveInfo.Metadata;
      _lastSaveSlot = lastSaveInfo.SaveSlot;

      if (_lastSaveMetadata != null)
      {
        _focusName = "ButtonContinue";
      }
      else
      {
        _focusName = "ButtonNewGame";
      }

      base.Awake();

      _buttonContinue = UIDocument.rootVisualElement.Q<Button>("ButtonContinue");
      _buttonLoad = UIDocument.rootVisualElement.Q<Button>("ButtonLoad");
      _buttonNewGame = UIDocument.rootVisualElement.Q<Button>("ButtonNewGame");
      _buttonCredits = UIDocument.rootVisualElement.Q<Button>("ButtonCredits");
      _buttonSettings = UIDocument.rootVisualElement.Q<Button>("ButtonSettings");
      _buttonQuit = UIDocument.rootVisualElement.Q<Button>("ButtonQuit");
    }

    protected abstract GameSaveManager<TSaveData, TSaveMetadata> GetSaveManager();

    protected virtual void OnEnable()
    {
      _buttonContinue.clicked += OnButtonContinueClicked;
      _buttonLoad.clicked += OnButtonLoadClicked;
      _buttonNewGame.clicked += OnButtonNewGameClicked;
      _buttonCredits.clicked += OnButtonCreditsClicked;
      _buttonSettings.clicked += OnButtonSettingsClicked;
      _buttonQuit.clicked += OnButtonQuitClicked;

      if (_lastSaveMetadata != null)
      {
        _buttonContinue.SetEnabled(true);
        _buttonLoad.SetEnabled(true);
        _buttonContinue.Focus();
      }
      else
      {
        _buttonContinue.SetEnabled(false);
        _buttonLoad.SetEnabled(false);
        _buttonNewGame.Focus();
      }
    }

    protected virtual void OnDisable()
    {
      _buttonContinue.clicked -= OnButtonContinueClicked;
      _buttonLoad.clicked -= OnButtonLoadClicked;
      _buttonNewGame.clicked -= OnButtonNewGameClicked;
      _buttonCredits.clicked -= OnButtonCreditsClicked;
      _buttonSettings.clicked -= OnButtonSettingsClicked;
      _buttonQuit.clicked -= OnButtonQuitClicked;
    }

    protected abstract void OnButtonContinueClicked();

    protected abstract void OnButtonLoadClicked();

    protected abstract void OnButtonNewGameClicked();

    protected abstract void OnButtonCreditsClicked();

    protected abstract void OnButtonSettingsClicked();

    protected abstract void OnButtonQuitClicked();

  }
}