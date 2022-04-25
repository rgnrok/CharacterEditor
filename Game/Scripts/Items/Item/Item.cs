namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Item
        {
            public ItemData Data { get; private set; }

            public virtual string Guid
            {
                get { return Data.guid; }
            }

            public Item(ItemData itemData)
            {
                Data = itemData;
            }
        }
    }
}