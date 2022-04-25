namespace CharacterEditor
{
    public class ClearColorBtn : TextureTypeMaskSelector
    {
        protected override void OnClick()
        { 
            TextureManager.Instance.OnResetColor(types);
        }
    }
}
