namespace CharacterEditor
{
    public class PrevSkinMeshBtn : ChangeSkinMeshBtn
    {
        protected override void ChangeSkinTexture()
        {
            TextureManager.Instance.OnPrevTexture(types, clearTypes);
        }
    }
}
