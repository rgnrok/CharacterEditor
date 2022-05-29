namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Item
        {
            public ItemData Data { get; }

            public virtual string Guid => Data.guid;

            public Item(ItemData itemData)
            {
                Data = itemData;
            }
        }
    }
}