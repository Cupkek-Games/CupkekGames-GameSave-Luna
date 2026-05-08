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
    protected override void Awake()
    {
      base.Awake();

      _container = UIDocument.rootVisualElement.Q<VisualElement>("AutoSave");
      _radial = _container.Q<RadialLoading>();
    }

    private void OnEnable()
    {
      _radial.StopAnimation();

      GameSaveEvents.AutosaveStart += OnSavingStart;
      GameSaveEvents.AutosaveComplete += OnSavingComplete;
    }
    private void OnDisable()
    {
      _radial.StopAnimation();

      GameSaveEvents.AutosaveStart -= OnSavingStart;
      GameSaveEvents.AutosaveComplete -= OnSavingComplete;
    }

    public void OnSavingStart()
    {
      Show();
    }
    public void OnSavingComplete()
    {
      Hide();
    }
    public void Show()
    {
      _radial.ProgressColorOverride = _colorLoading;
      _radial.ArcSize = 0.2f;
      _radial.StartAnimation();
      _container.AddToClassList("show");

      CancelDelayedHide();
    }
    public void Hide()
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