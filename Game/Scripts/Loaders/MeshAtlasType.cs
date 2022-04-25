namespace CharacterEditor
{
    /*
     *  Static - Static mesh atlas with 2048x2048px size, аll textures have their own fixed position. Use Static Mesh UV
     *  
     *  Dynamic - The atlas is collected only from the selected meshes
     *  (if 1n mesh is selected, the size of the atlas is 512x512, if 2-4, then 1024x1024. Use original Mesh UV
     */
    public enum MeshAtlasType
    {
        Static,
        Dynamic
    }
}
