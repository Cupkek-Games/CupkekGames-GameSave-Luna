using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.GameSave.Luna
{
  public class VersionLabelController : MonoBehaviour
  {
    // UI
    private UIDocument _uiDocument;
    private Label _versionLabel;

    // Start is called before the first frame update
    void Awake()
    {
      _uiDocument = GetComponent<UIDocument>();
      _versionLabel = _uiDocument.rootVisualElement.Q<Label>("VersionLabel");
      _versionLabel.text = $"Version: {Application.version}";
    }
  }
}