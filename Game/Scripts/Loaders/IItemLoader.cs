using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;

namespace CharacterEditor
{
    public interface IItemLoader : IService
    {
        void LoadItems(Action<Dictionary<string, ItemData>> callback);

        void LoadItem(string guid, Action<ItemData> callback);

        void LoadItems(List<string> guids, Action<Dictionary<string, ItemData>> callback);
    }
}
