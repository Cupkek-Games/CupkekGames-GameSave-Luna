using System.Collections;
using CupkekGames.GameSave;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.GameSave.Luna
{
  public class AutoSaveView : UIViewComponent
  {
    [SerializeField] private float _hideDelay = 1;
    private VisualElement _container;
    private RadialLoading _radial;
    private Coroutine _hideCoroutine;
    [SerializeField] private Color _colorLoading = Color.magenta;
    [SerializeField] private Color _colorComplete = Color.green;
    protected override void OnUILoaded(VisualElement root)
    {
      base.OnUILoaded(root);

      _container = root.Q<VisualElement>("AutoSave");
      _radial = _container.Q<RadialLoading>();

      if (enabled) OnEnable();
    }

    private void OnEnable()
    {
      if (_radial == null) return; // panel hasn't reloaded yet

      _radial.StopAnimation();

      GameSaveEvents.AutosaveStart += OnSavingStart;
      GameSaveEvents.AutosaveComplete += OnSavingComplete;
    }
    private void OnDisable()
    {
      if (_radial == null) return;

      _radial.StopAnimation();

      GameSaveEvents.AutosaveStart -= OnSavingStart;
      GameSaveEvents.AutosaveComplete -= OnSavingComplete;
    }

    public void OnSavingStart()
    {
      BeginIndicator();
    }
    public void OnSavingComplete()
    {
      EndIndicator();
    }

    // Renamed from Show/Hide to avoid colliding with the base
    // UIViewComponent.Show/Hide (which drive the fade-in/out
    // pipeline). The autosave indicator is a self-contained
    // radial-loading animation in this view's USS, not a nav
    // visibility change.
    private void BeginIndicator()
    {
      _radial.ProgressColorOverride = _colorLoading;
      _radial.ArcSize = 0.2f;
      _radial.StartAnimation();
      _container.AddToClassList("show");

      CancelDelayedHide();
    }

    private void EndIndicator()
    {
      CancelDelayedHide();

      _hideCoroutine = StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
      yield return new WaitForSeconds(_hideDelay);

      _radial.ProgressColorOverride = _colorComplete;
      _radial.ArcSize = 1f;

      yield return new WaitForSeconds(1f);

      _container.RemoveFromClassList("show");
      _radial.StopAnimation();
      _hideCoroutine = null;
    }
    private void CancelDelayedHide()
    {
      if (_hideCoroutine != null)
      {
        StopCoroutine(_hideCoroutine);
      }
    }
  }
}