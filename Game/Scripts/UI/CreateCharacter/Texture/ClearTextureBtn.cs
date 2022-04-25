namespace CharacterEditor
{
    public class ClearTextureBtn : TextureTypeMaskSelector
    {
        protected override void OnClick()
        {
            TextureManager.Instance.OnResetTexure(types);
        }
    }
}
