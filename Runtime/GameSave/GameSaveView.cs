using CupkekGames.Luna;
using CupkekGames.Luna.Navigation;
using UnityEngine.UIElements;

namespace CupkekGames.GameSave.Luna
{
  public class GameSaveView : UIViewComponent
  {
    // UI Elements
    private Button _returnButton;

    protected override void OnUILoaded(VisualElement root)
    {
      base.OnUILoaded(root);

      _returnButton = root.Q<Button>("ReturnButton");

      // ReturnClicked disables the button (double-click protection) and this
      // view survives its pop (persistent global destination), so re-arm it on
      // every open. -= before += guards the sync-reload re-run of OnUILoaded.
      UIView.Fade.OnFadeInStart -= OnFadeInStart;
      UIView.Fade.OnFadeInStart += OnFadeInStart;

      if (enabled) OnEnable();
    }

    private void OnFadeInStart()
    {
      if (_returnButton != null) _returnButton.SetEnabled(true);
    }

    private void OnEnable()
    {
      if (_returnButton == null) return; // panel hasn't reloaded yet
      _returnButton.clicked += ReturnClicked;
    }

    private void OnDisable()
    {
      if (_returnButton == null) return;
      _returnButton.clicked -= ReturnClicked;
    }

    public void ReturnClicked()
    {
      _returnButton.SetEnabled(false);
      LunaNavigation.PopBackStack();
    }
  }
}