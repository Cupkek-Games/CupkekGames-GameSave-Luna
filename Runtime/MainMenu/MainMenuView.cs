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

      // _focusName must be set before base.Awake() so UIViewComponent
      // picks it up when it registers the panel-reload callback.
      if (_lastSaveMetadata != null)
      {
        _focusName = "ButtonContinue";
      }
      else
      {
        _focusName = "ButtonNewGame";
      }

      base.Awake();
    }

    protected override void OnUILoaded(VisualElement root)
    {
      base.OnUILoaded(root);

      _buttonContinue = root.Q<Button>("ButtonContinue");
      _buttonLoad = root.Q<Button>("ButtonLoad");
      _buttonNewGame = root.Q<Button>("ButtonNewGame");
      _buttonCredits = root.Q<Button>("ButtonCredits");
      _buttonSettings = root.Q<Button>("ButtonSettings");
      _buttonQuit = root.Q<Button>("ButtonQuit");

      // We were enabled before the panel delivered its tree — apply
      // the enable-side wire/focus now. Subsequent enable cycles route
      // through OnEnable directly.
      if (enabled) OnEnable();
    }

    protected abstract GameSaveManager<TSaveData, TSaveMetadata> GetSaveManager();

    protected virtual void OnEnable()
    {
      if (_buttonContinue == null) return; // panel hasn't reloaded yet

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
      if (_buttonContinue == null) return;

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