using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public ItemData Data;

    private Item _item;
    private ITextureLoader _textureLoader;
    private IMeshLoader _meshLoader;

    public Item Item
    {
        get
        {
            if (_item == null)
            {
                var equipItemData = Data as EquipItemData;
                if (equipItemData == null) return null;

                var eiMesh = new EquipItemMesh(equipItemData, _textureLoader, _meshLoader);
                _item = new EquipItem(null, equipItemData, eiMesh, equipItemData.stats); //todo guid null
            }

            return _item;
        }

        set
        {
            _item = value;
            Data = _item.Data;
        }
    }

    public void Awake()
    {
        _textureLoader = AllServices.Container.Single<ITextureLoader>();
        _meshLoader = AllServices.Container.Single<IMeshLoader>();
    }
  
}
