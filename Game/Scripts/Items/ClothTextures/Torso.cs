namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Torso : ItemTexture
            {
                public Torso(ITextureLoader loader, string path) : base(loader, path, TextureType.Torso)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_TorsoTex";
                }
            }
        }
    }
}