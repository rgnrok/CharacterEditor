namespace CharacterEditor
{
    public class NextMeshColorBtn : MeshAndTextureChangedBtn
    {
        protected override void ChangeTexture()
        {
            _textureManager.OnNextColor(textureTypes);
        }

        protected override void ChangeMesh()
        {
            if (textureTypes.Length > 0)
                _meshManager.SetMeshColor(meshTypes, _textureManager.GetSelectedTextureColor(textureTypes[0]));
            else
                _meshManager.OnNextColor(meshTypes);
        }
    }
}