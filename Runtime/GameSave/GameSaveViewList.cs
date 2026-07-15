using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.GameSave;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;
using CupkekGames.Luna.Navigation;
using System.Linq;
using System.Collections;


#if UNITY_INPUT
using UnityEngine.InputSystem;
#endif

namespace CupkekGames.GameSave.Luna
{
  [RequireComponent(typeof(PanelRenderer))]
  [RequireComponent(typeof(GameSaveView))]
  public abstract class GameSaveViewList<TSaveData, TSaveMetadata> : MonoBehaviour where TSaveData : IGameSaveData, IData, new()
    where TSaveMetadata : GameSaveMetadata
  {
    // References
    protected GameSaveManager<TSaveData, TSaveMetadata> _gameSaveManager;
    public GameSaveManager<TSaveData, TSaveMetadata> GameSaveManager => _gameSaveManager;
    protected GameSaveView _gameSaveView;

    public GameSaveView GameSaveView => _gameSaveView;

    // UI
    protected VisualElement _root;
    [SerializeField] protected VisualTreeAsset _listEntryTemplate;
    protected ListView _listView;
    protected ListViewWrapper _listViewWrapper;
    List<GameSaveMetadataWithSlot<TSaveMetadata>> _metadataCache;
    List<GameSaveMetadataWithSlot<TSaveMetadata>> _metadataFiltered;
    protected bool _isInGame;
    protected Button _newSaveButton;
    protected Toggle _showAutoToggle;
    protected Toggle _showManualToggle;
    protected InputPrompt _loadSaveButton;
    protected InputPrompt _overwriteSaveButton;
    protected InputPrompt _deleteSaveButton;

    [Tooltip("Global ChoicePopup nav destination pushed (with ChoicePopupArgs) for the " +
             "overwrite/delete confirms. Required — a shared node-bound ChoicePopupController, " +
             "e.g. the 'confirm' node.")]
    [CatalogKeyConstraint(NavConstants.NavDestinationCatalogId)]
    [SerializeField] protected CatalogKey _confirmDest;

#if UNITY_INPUT
    protected InputAction _loadSaveAction;
    protected InputAction _overwriteSaveAction;
    protected InputAction _deleteSaveAction;
#endif

    protected virtual void Awake()
    {
      // Component refs resolve at Awake; element lookups do NOT — this view
      // renders through a PanelRenderer that delivers its tree asynchronously
      // (no UIDocument / synchronous rootVisualElement). Defer everything that
      // touches the tree to the UILoaded milestone (see OnUILoaded).
      _gameSaveManager = GetSaveManager();
      _gameSaveView = GetComponent<GameSaveView>();

      ConfirmDestIsSet();

      _gameSaveView.WhenUILoaded(OnUILoaded);
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
      if (Application.isPlaying) return;
      if (_confirmDest.IsEmpty)
        Debug.LogWarning("[GameSaveViewList] _confirmDest is not set — assign the global ChoicePopup nav destination (e.g. 'confirm'); overwrite/delete confirms route through it.", this);
    }
#endif

    // _confirmDest is required: overwrite/delete confirms route through the
    // global confirm destination.
    private bool ConfirmDestIsSet()
    {
      if (!_confirmDest.IsEmpty) return true;
      Debug.LogError("[GameSaveViewList] _confirmDest is not set — assign the global ChoicePopup nav destination (e.g. 'confirm'); overwrite/delete confirms cannot run without it.", this);
      return false;
    }


    private void InitializeListView()
    {
      _listView.makeItem = () =>
      {
        VisualElement item = _listEntryTemplate.Instantiate();

        GameSaveViewEntry<TSaveMetadata> entry = new GameSaveViewEntry<TSaveMetadata>();
        entry.MakeItem(gameObject, item, GetTooltipController());

        item.userData = entry;

        return item;
      };

      _listView.bindItem = (item, index) =>
      {
        GameSaveViewEntry<TSaveMetadata> entry = (GameSaveViewEntry<TSaveMetadata>)item.userData;

        GameSaveMetadataWithSlot<TSaveMetadata> metadata = _metadataFiltered[index];

        entry.BindItem(
          index,
          metadata.SaveSlot,
          metadata.Metadata,
          _isInGame,
          SlotOne(index, metadata),
          SlotTwo(index, metadata)
        );
      };

      _listView.unbindItem = (item, index) =>
      {
        GameSaveViewEntry<TSaveMetadata> entry = (GameSaveViewEntry<TSaveMetadata>)item.userData;
        entry.UnbindItem();
      };

      _listView.selectionChanged += OnSelectionChanged;
    }

    protected virtual void RegisterCallbacks()
    {
      // Handle boundary navigation - which element to navigate to at edges
      _listViewWrapper.OnGetBoundaryNavigationTarget += (direction) =>
      {
        // Called when at boundary (first or last item) and navigating Up/Down/Next/Previous
        if (direction == NavigationMoveEvent.Direction.Up ||
            direction == NavigationMoveEvent.Direction.Down ||
            direction == NavigationMoveEvent.Direction.Next ||
            direction == NavigationMoveEvent.Direction.Previous)
        {
          // Return the element to navigate to
          if (_newSaveButton.enabledSelf)
            return _newSaveButton;
          else
            return _showAutoToggle;
        }

        return null;
      };

      // Handle horizontal navigation (Left/Right)
      _listViewWrapper.OnNavigateHorizontal += (direction) => { _showAutoToggle.Focus(); };

      // Enable wrapper (registers FocusInEvent and NavigationMoveEvent on ListView)
      _listViewWrapper.Enable();

      // Register bidirectional navigation from adjacent element back to ListView
      _listViewWrapper.RegisterAdjacentElementNavigation(_newSaveButton, _showAutoToggle);
    }

    public void ResetListSelection()
    {
      _listViewWrapper.ResetSelection();
    }

    // Element lookups + all element-dependent wiring, run once when the
    // PanelRenderer tree is live (formerly split across Awake + Start).
    protected virtual void OnUILoaded()
    {
      _root = _gameSaveView.ParentElement;

      _newSaveButton = _root.Q<Button>("NewSave");
      _showAutoToggle = _root.Q<Toggle>("ShowAuto");
      _showManualToggle = _root.Q<Toggle>("ShowManual");

      _loadSaveButton = _root.Q<InputPrompt>("Load");
      _overwriteSaveButton = _root.Q<InputPrompt>("Overwrite");
      _deleteSaveButton = _root.Q<InputPrompt>("Delete");

      _listView = _root.Q<ListView>("LoadList");

      _listViewWrapper = new ListViewWrapper(_listView);

      UpdateMetadataCache();

      InitializeListView();
      RegisterCallbacks();
      _listView.itemsSource = _metadataFiltered;
      ResetListSelection();

      _listView.Focus();

      _isInGame = IsInGame();

      if (_isInGame)
      {
        _newSaveButton.clicked += OnNewSaveButtonClicked;
        _overwriteSaveButton.clicked += OnOverwriteButtonClicked;
      }
      else
      {
        _newSaveButton.SetEnabled(false);
        _overwriteSaveButton.style.display = DisplayStyle.None;
      }

      _showAutoToggle.RegisterValueChangedCallback(OnShowAutoToggleChanged);
      _showManualToggle.RegisterValueChangedCallback(OnShowManualToggleChanged);

      _loadSaveButton.clicked += OnLoadButtonClicked;
      _deleteSaveButton.clicked += OnDeleteButtonClicked;

#if UNITY_INPUT
      // InputPrompt resolves its backing InputAction on its own AttachToPanel,
      // which can run after this view's OnUILoaded. Defer a frame so the
      // actions exist; WireInputActions null-guards for a missing input device.
      _root.schedule.Execute(WireInputActions);
#endif
    }

#if UNITY_INPUT
    private void WireInputActions()
    {
      _overwriteSaveAction = _overwriteSaveButton.Action;
      _loadSaveAction = _loadSaveButton.Action;
      _deleteSaveAction = _deleteSaveButton.Action;

      if (_loadSaveAction != null) _loadSaveAction.performed += OnLoadInputPerformed;
      if (_deleteSaveAction != null) _deleteSaveAction.performed += OnDeleteInputPerformed;
      if (_isInGame && _overwriteSaveAction != null) _overwriteSaveAction.performed += OnOverwriteInputPerformed;
    }
#endif

    protected virtual void OnDestroy()
    {
      // Teardown only applies if the tree loaded (OnUILoaded ran). Guard so a
      // view destroyed before its panel loaded doesn't NRE here.
      if (_newSaveButton == null) return;

      _newSaveButton.clicked -= OnNewSaveButtonClicked;

      _showAutoToggle.UnregisterValueChangedCallback(OnShowAutoToggleChanged);
      _showManualToggle.UnregisterValueChangedCallback(OnShowManualToggleChanged);

      _loadSaveButton.clicked -= OnLoadButtonClicked;
      _overwriteSaveButton.clicked -= OnOverwriteButtonClicked;
      _deleteSaveButton.clicked -= OnDeleteButtonClicked;

      _listViewWrapper?.Disable();

#if UNITY_INPUT
      if (_loadSaveAction != null) _loadSaveAction.performed -= OnLoadInputPerformed;
      if (_overwriteSaveAction != null) _overwriteSaveAction.performed -= OnOverwriteInputPerformed;
      if (_deleteSaveAction != null) _deleteSaveAction.performed -= OnDeleteInputPerformed;
#endif
    }

    private void OnShowAutoToggleChanged(ChangeEvent<bool> evt)
    {
      UpdateMetadataFiltered();
      UpdateListView();
    }

    private void OnShowManualToggleChanged(ChangeEvent<bool> evt)
    {
      UpdateMetadataFiltered();
      UpdateListView();
    }

    private void UpdateMetadataCache()
    {
      _metadataCache = _gameSaveManager.GetAllMetadata(true);
      UpdateMetadataFiltered();
    }

    private void UpdateMetadataFiltered()
    {
      _metadataFiltered = _metadataCache.Where(m =>
        (m.Metadata.IsAutosave && _showAutoToggle.value) ||
        (!m.Metadata.IsAutosave && _showManualToggle.value)
      ).ToList();
    }

    protected virtual void OnSelectionChanged(IEnumerable<object> selection)
    {
      OnSelectionChanged();
    }

    protected virtual void OnSelectionChanged()
    {
      GameSaveMetadataWithSlot<TSaveMetadata>? entry = GetSelectedMetadata();
      string saveSlotText = entry.HasValue ? entry.Value.SaveSlot.ToString() : "";
      _loadSaveButton.LabelText = "Load Save " + saveSlotText;
      _overwriteSaveButton.LabelText = "Overwrite Save " + saveSlotText;
      _deleteSaveButton.LabelText = "Delete Save" + saveSlotText;
    }

    private void UpdateListView()
    {
      _listView.itemsSource = _metadataFiltered;
      _listView.RefreshItems();
      _listViewWrapper.ValidateSelection();

      if (_listView.selectedIndex > -1 && _listView.selectedIndex < _metadataFiltered.Count)
      {
        OnSelectionChanged();
      }
    }

    private void OnNewSaveButtonClicked()
    {
      int availableSlot = _gameSaveManager.GetFirstAvailableSlot();
      _gameSaveManager.SaveToFile(availableSlot, _gameSaveManager.CurrentSave.Data);

      UpdateMetadataCache();
      UpdateListView();
    }

    private void OnOverwriteButtonClicked()
    {
      GameSaveMetadataWithSlot<TSaveMetadata>? entry = GetSelectedMetadata();
      if (!entry.HasValue)
      {
        return;
      }

      if (!ConfirmDestIsSet()) return;

      int saveSlot = entry.Value.SaveSlot;
      string saveSlotText = saveSlot.ToString();

      ConfirmThen("Overwrite Save " + saveSlotText, "Are you sure you want to overwrite this save?", () =>
      {
        _gameSaveManager.SaveToFile(saveSlot, _gameSaveManager.CurrentSave.Data);
        ResetListSelection();
        UpdateMetadataCache();
        UpdateListView();
      });
    }

    private void OnDeleteButtonClicked()
    {
      GameSaveMetadataWithSlot<TSaveMetadata>? entry = GetSelectedMetadata();
      if (!entry.HasValue)
      {
        return;
      }

      if (!ConfirmDestIsSet()) return;

      int saveSlot = entry.Value.SaveSlot;
      string saveSlotText = saveSlot.ToString();

      ConfirmThen("Delete Save " + saveSlotText, "Are you sure you want to delete this save?", () =>
      {
        _gameSaveManager.DeleteFile(saveSlot);
        UpdateMetadataCache();
        UpdateListView();
      });
    }

    // Pushes the confirm destination with per-call args and runs onConfirm when
    // the affirmative choice (index 0) is picked; Esc/dismiss is a no-op. The
    // acted-on slot is captured before the push so a selection change while the
    // popup is open cannot retarget the action.
    private async void ConfirmThen(string header, string body, System.Action onConfirm)
    {
      var result = await LunaNavigation.PushAsync<int>(_confirmDest, new ChoicePopupArgs
      {
        Header = header,
        Body = body,
      });

      if (this == null) return; // view destroyed while the confirm was open

      if (!result.IsDismissed && result.Value == 0)
      {
        onConfirm();
      }
    }

    public GameSaveMetadataWithSlot<TSaveMetadata>? GetSelectedMetadata()
    {
      if (_listView.selectedIndex < 0 || _listView.selectedIndex >= _metadataFiltered.Count)
      {
        return null;
      }

      return _metadataFiltered[_listView.selectedIndex];
    }

    protected abstract void OnLoadButtonClicked();
    protected abstract GameSaveManager<TSaveData, TSaveMetadata> GetSaveManager();
    protected abstract bool IsInGame();
    protected abstract VisualElement SlotOne(int index, GameSaveMetadataWithSlot<TSaveMetadata> metadata);
    protected abstract VisualElement SlotTwo(int index, GameSaveMetadataWithSlot<TSaveMetadata> metadata);
    protected abstract TooltipController GetTooltipController();

#if UNITY_INPUT
    private void OnLoadInputPerformed(InputAction.CallbackContext context)
    {
      OnLoadButtonClicked();
    }
    
    private void OnOverwriteInputPerformed(InputAction.CallbackContext context)
    {
      OnOverwriteButtonClicked();
    }

    private void OnDeleteInputPerformed(InputAction.CallbackContext context)
    {
      OnDeleteButtonClicked();
    }
#endif
  }
}
