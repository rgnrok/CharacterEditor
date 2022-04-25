namespace CharacterEditor
{
    public class ClearMeshBtn : MeshTypeMaskSelector
    {
        protected override void OnClick()
        { 
            MeshManager.Instance.OnClearMesh(types);
        }
    }
}
