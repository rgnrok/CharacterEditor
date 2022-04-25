
public sealed class Constants
{
#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
    public const int MESH_TEXTURE_SIZE = 256;
#else
    public const int MESH_TEXTURE_SIZE = 512; // PC and others
#endif

#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
    public const int SKIN_MESHES_ATLAS_SIZE = 512;
#else
    public const int SKIN_MESHES_ATLAS_SIZE = 1024; // PC and others
#endif


#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
    public const int ARMOR_MESHES_ATLAS_SIZE = 1024;
#else
    public const int ARMOR_MESHES_ATLAS_SIZE = 2048; // PC and others
#endif

    public const int SKIN_TEXTURE_ATLAS_SIZE = 1024;

    public const int LAYER_CHARACTER_PREVIEW = 8;
    public const int LAYER_GROUND = 9;
    public const int LAYER_WALL = 10;
    public const int LAYER_CHARACTER = 11;
    public const int LAYER_NPC = 12;
    public const int LAYER_ENEMY = 13;
    public const int LAYER_CONTAINER = 14;
    public const int LAYER_PICKUP = 15;


    #region Animations

    public const string CHARACTER_DIE_TRIGGER = "Die";
    public const string CHARACTER_START_BATTLE_TRIGGER = "StartBattle";
    public const string CHARACTER_END_BATTLE_TRIGGER = "EndBattle";

    public const string CHARACTER_MELEE_ATTACK_1_TRIGGER = "Attack1";
    public const string CHARACTER_MELEE_ATTACK_2_TRIGGER = "Attack2";


    #endregion
}

