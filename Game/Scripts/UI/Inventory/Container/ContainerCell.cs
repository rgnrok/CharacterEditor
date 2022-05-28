public class ContainerCell : ItemCell
{
    protected override void OnClickHandler()
    {
        GameManager.Instance.ContainerPopup.AddToInventory(this);
    }
}
