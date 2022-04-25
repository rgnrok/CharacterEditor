namespace CharacterEditor
{
    public class PrevMeshBtn : MeshTypeMaskSelector
    {
        protected override void OnClick()
        {
            MeshManager.Instance.OnPrevMesh(types);
        }
    }
}
