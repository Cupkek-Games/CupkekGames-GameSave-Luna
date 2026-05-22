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

      if (enabled) OnEnable();
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