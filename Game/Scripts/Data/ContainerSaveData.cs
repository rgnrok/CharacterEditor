using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CharacterEditor;
using CharacterEditor.CharacterInventory;

[Serializable]
public class ContainerSaveData : EntitySaveData
{
    // Ceil index and item
    public Dictionary<int, CharacterItemData> items = new Dictionary<int, CharacterItemData>();

    public ContainerSaveData()
    {

    }

    public ContainerSaveData(Container container)
    {
        guid = container.Guid;
        foreach (var item in container.GetItems())
        {
            items[item.Key] = new CharacterItemData(item.Value);
        }
    }

    public ContainerSaveData(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        items = (Dictionary<int, CharacterItemData>)info.GetValue("items", typeof(Dictionary<int, CharacterItemData>));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("items", items);
    }

}
