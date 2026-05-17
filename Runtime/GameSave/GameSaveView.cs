using CupkekGames.Luna;
using CupkekGames.Luna.Navigation;
using UnityEngine.UIElements;

namespace CupkekGames.GameSave.Luna
{
  public class GameSaveView : UIViewComponent
  {
    // UI Elements
    private Button _returnButton;

    // Start is called before the first frame update
    protected override void Awake()
    {
      base.Awake();

      _returnButton = UIDocument.rootVisualElement.Q<Button>("ReturnButton");
    }

    private void OnEnable()
    {
      _returnButton.clicked += ReturnClicked;
    }

    private void OnDisable()
    {
      _returnButton.clicked -= ReturnClicked;
    }

    public void ReturnClicked()
    {
      _returnButton.SetEnabled(false);
      LunaNavigation.PopBackStack();
    }
  }
}