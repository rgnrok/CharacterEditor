namespace CharacterEditor
{
    public static class AssetsConstants
    {
        public const string CharacterEditorRootPath = "Assets/Packages/Character_Editor";
        public static readonly string MeshPathTemplate = $"{CharacterEditorRootPath}/Prefabs/{{0}}/{{1}}";
        public static readonly string SkinPathTemplate = $"{CharacterEditorRootPath}/Textures/Character/{{0}}/Skin/{{1}}";
        public static readonly string ClothePathTemplate = $"{CharacterEditorRootPath}/Textures/Character/{{0}}/Clothes/{{1}}";

        public static readonly string CharacterStaticDataPath = $"{CharacterEditorRootPath}/Data";

        public const string GameRootPath = "Assets/Game";
        public static readonly string GameStaticDataPath = $"{GameRootPath}/Data";

        public static readonly string ArmorMergeMaterialPathKey = "ArmorMergeMaterialPathKey";
        public static readonly string ClothMergeMaterialPathKey = "ClothMergeMaterialPathKey";
        public static readonly string ModelMaterialPathKey = "ModelMaterialPathKey";
        public static readonly string PreviewMaterialPathKey = "PreviewMaterialPathKey";
    }
}