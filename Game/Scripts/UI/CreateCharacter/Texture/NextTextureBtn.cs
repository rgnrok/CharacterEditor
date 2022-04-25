namespace CharacterEditor
{
    public class NextTextureBtn : TextureTypeMaskSelector
    {
        protected override void OnClick()
        {
            TextureManager.Instance.OnNextTexture(types, clearTypes);
        }
    }
}
