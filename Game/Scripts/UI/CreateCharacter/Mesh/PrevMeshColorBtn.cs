namespace CharacterEditor
{
    public class PrevMeshColorBtn : MeshAndTextureChangedBtn
    {
        protected override void ChangeTexture()
        {
            _textureManager.OnPrevColor(textureTypes);
        }

        protected override void ChangeMesh()
        {
            if (textureTypes.Length > 0)
                _meshManager.SetMeshColor(meshTypes, _textureManager.GetSelectedTextureColor(textureTypes[0]));
            else
                _meshManager.OnPrevColor(meshTypes);
        }
    }
}