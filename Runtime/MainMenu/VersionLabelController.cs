using CupkekGames.Luna;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.GameSave.Luna
{
  public class VersionLabelController : MonoBehaviour
  {
    private UIViewComponent _view;
    private Label _versionLabel;

    void Awake()
    {
      // Luna renders through a PanelRenderer that delivers its visual tree
      // ASYNCHRONOUSLY — there is no UIDocument and rootVisualElement isn't
      // available at Awake (the old UIDocument path NRE'd here once views
      // started spawning via NavHost). Resolve the sibling UIViewComponent
      // and defer the element lookup to its UILoaded milestone instead.
      _view = GetComponent<UIViewComponent>();
      if (_view == null)
      {
        Debug.LogError(
          "[VersionLabelController] Requires a UIViewComponent on the same GameObject.", this);
        return;
      }

      // Runs immediately if the tree is already loaded, else on the next load.
      _view.WhenUILoaded(ApplyVersion);
    }

    private void ApplyVersion()
    {
      _versionLabel = _view.ParentElement?.Q<Label>("VersionLabel");
      if (_versionLabel != null)
      {
        _versionLabel.text = $"Version: {Application.version}";
      }
    }
  }
}
