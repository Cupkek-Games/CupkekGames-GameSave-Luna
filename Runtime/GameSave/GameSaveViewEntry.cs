using System;
using System.Collections.Generic;
using CupkekGames.GameSave;
using UnityEngine;
using UnityEngine.UIElements;
using CupkekGames.Luna;

namespace CupkekGames.GameSave.Luna
{
  public class GameSaveViewEntry<TSaveMetadata> where TSaveMetadata : GameSaveMetadata
  {
    private int _index;
    private VisualElement _container;
    private VisualElement _slotOne;
    private VisualElement _slotTwo;
    // Actions
    private int _saveSlot;
    public int SaveSlot => _saveSlot;
    private TSaveMetadata _metadata;
    public TSaveMetadata Metadata => _metadata;

    public void MakeItem(
      GameObject parent, 
      VisualElement container, 
      TooltipController tooltipController
    )
    {
      _container = container;
      _slotOne = container.Q<VisualElement>("SlotOne");
      _slotTwo = container.Q<VisualElement>("SlotTwo");
    }

    public void BindItem(int index, int saveSlot, TSaveMetadata metadata, bool isInGame, VisualElement slotOne, VisualElement slotTwo)
    {
      _slotOne.Add(slotOne);
      _slotTwo.Add(slotTwo);

      _index = index;
      _saveSlot = saveSlot;
      _metadata = metadata;
    }

    public void UnbindItem()
    {
      _slotOne.Clear();
      _slotTwo.Clear();

      _saveSlot = -1;
      _metadata = null;
    }
  }
}
