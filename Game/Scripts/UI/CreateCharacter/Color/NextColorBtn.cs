namespace CharacterEditor
{
    public class NextColorBtn : TextureTypeMaskSelector
    {
        protected override void OnClick()
        {
            TextureManager.Instance.OnNextColor(types);
        }
    }
}
