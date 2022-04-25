namespace CharacterEditor
{
    public enum LoaderType
    {
        Addresable,
        AssetBundle,
#if UNITY_EDITOR
        AssetDatabase
#endif
    }
}
