namespace CharacterEditor
{
    public class NextSkinMeshBtn : ChangeSkinMeshBtn
    {
        protected override void ChangeSkinTexture()
        {
            TextureManager.Instance.OnNextTexture(types, clearTypes);
        }
    }
}
