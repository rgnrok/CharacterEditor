namespace CharacterEditor
{
    public class NextMeshBtn : MeshTypeMaskSelector
    {
        protected override void OnClick()
        { 
            MeshManager.Instance.OnNextMesh(types);
        }
    }
}
