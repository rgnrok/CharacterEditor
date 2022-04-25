using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipItemBtn : MonoBehaviour, IPointerClickHandler
{
    public ItemData data;
    private EquipItem _item;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item == null)
        {
            _item = new EquipItem(data, new EquipItemMesh((EquipItemData) data, AllServices.Container.Single<ITextureLoader>(), AllServices.Container.Single<IMeshLoader>()));
        }
        if (ItemManager.Instance.CanEquip(_item))
            ItemManager.Instance.EquipItem(_item);
    }
}
