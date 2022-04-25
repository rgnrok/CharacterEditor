namespace CharacterEditor
{
    public class PrevColorBtn : TextureTypeMaskSelector
    {
        protected override void OnClick()
        {
            TextureManager.Instance.OnPrevColor(types);
        }
    }
}
