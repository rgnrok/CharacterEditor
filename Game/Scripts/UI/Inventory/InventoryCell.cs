using CharacterEditor.CharacterInventory;

public class InventoryCell : ItemCell
{
    protected override void OnClickHandler()
    {
        if (Item is EquipItem equipItem && GameManager.Instance.Inventory.CanEquip(equipItem))
        {
            GameManager.Instance.Inventory.EquipItem(equipItem, this);
        }
    }
}
