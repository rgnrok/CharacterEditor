namespace CharacterEditor
{
    public class PrevTextureBtn : TextureTypeMaskSelector
    {
        protected override void OnClick()
        {
            TextureManager.Instance.OnPrevTexture(types, clearTypes);
        }
    }
}
