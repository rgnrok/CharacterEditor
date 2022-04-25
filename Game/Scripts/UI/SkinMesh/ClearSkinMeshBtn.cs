namespace CharacterEditor
{
    public class ClearSkinMeshBtn : ChangeSkinMeshBtn
    {
        protected override void ChangeSkinTexture()
        {
            TextureManager.Instance.OnClear(types);
        }
    }
}
