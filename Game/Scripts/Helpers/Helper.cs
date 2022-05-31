using System.Collections;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEngine;

public static partial class Helper
{
    public static Transform FindTransform(this Transform parent, string childName)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == childName)
                return child;

            if (child.childCount == 0) continue;

            var obj =  FindTransform(child, childName);
            if (obj != null)
                return obj;
        }
        return null;
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public static EquipItemSlot[] GetAvailableSlotsByItemType(EquipItemType type)
    {
        switch (type)
        {
            case EquipItemType.Armor:
                return new[] {EquipItemSlot.Armor};
            case EquipItemType.Pants:
                return new[] { EquipItemSlot.Pants };
            case EquipItemType.Belt:
                return new[] { EquipItemSlot.Belt };
            case EquipItemType.Helm:
                return new[] { EquipItemSlot.Helm };
            case EquipItemType.Boots:
                return new[] { EquipItemSlot.Boots };
            case EquipItemType.Gloves:
                return new[] { EquipItemSlot.Gloves };
            case EquipItemType.Cloak:
                return new[] { EquipItemSlot.Cloak };
            case EquipItemType.Shield:
                return new[] { EquipItemSlot.HandLeft };
            case EquipItemType.Weapon:
                return new[] { EquipItemSlot.HandRight, EquipItemSlot.HandLeft };
            default:
                return new[] { EquipItemSlot.Undefined };
        }
    }

    public static string GetShaderTextureName(TextureType type)
    {
        switch (type)
        {
            case TextureType.Skin:
                return "_SkinTex";
            case TextureType.Eye:
                return "_EyeTex";
            case TextureType.Eyebrow:
                return "_EyebrowTex";
            case TextureType.FaceFeature:
                return "_FaceFeatureTex";
            case TextureType.Beard:
                return "_BeardTex";
            case TextureType.Scar:
                return "_ScarTex";
            case TextureType.Glove:
                return "_GloveTex";
            case TextureType.Shoe:
                return "_ShoeTex";
            case TextureType.Belt:
                return "_BeltTex";
            case TextureType.Torso:
                return "_TorsoTex";
            case TextureType.Pants:
                return "_PantsTex";
            case TextureType.RobeLong:
                return "_RobeLongTex";
            case TextureType.RobeShort:
                return "_RobeShortTex";
            case TextureType.Hair:
                return "_HairTex";
            case TextureType.Head:
                return "_HeadTex";
            default:
                return null;
        }
    }

    public static string GetShaderTextureName(MeshType type)
    {
        switch (type)
        {
            case MeshType.Beard:
                return "_BeardTex";
            case MeshType.Hair:
                return "_HairTex";
            case MeshType.FaceFeature:
                return "_FaceFeatureTex";

            case MeshType.Helm:
                return "_HelmTex";
            case MeshType.Torso:
                return "_TorsoTex";
            case MeshType.TorsoAdd:
                return "_TorsoAddTex";
            case MeshType.Belt:
                return "_BeltTex";
            case MeshType.BeltAdd:
                return "_BeltAddTex";
            case MeshType.ShoulderRight:
                return "_ShoulderRightTex";
            case MeshType.ShoulderLeft:
                return "_ShoulderLeftTex";
            case MeshType.ArmRight:
                return "_ArmRightTex";
            case MeshType.ArmLeft:
                return "_ArmLeftTex";
            case MeshType.LegRight:
                return "_LegRightTex";
            case MeshType.LegLeft:
                return "_LegLeftTex";
            case MeshType.HandRight:
                return "_HandRightTex";
            case MeshType.HandLeft:
                return "_HandLeftTex";

            default:
                return null;
        }
    }

    public static string GetCursorTextureNameByType(CursorType type)
    {
        switch (type)
        {
            case CursorType.Default:
                return "Cursor_Arrow_1";
            case CursorType.Hand:
                return "Cursor_ItemMove_1";
            case CursorType.PickUp:
                return "Cursor_ItemMove_2";
            case CursorType.Attack:
                return "Cursor_Melee_1";
        }

        return null;
    }

    public static bool IsAdditionalSlot(this EquipItemSlot equipSlot) => 
        equipSlot == EquipItemSlot.HandLeft;

    public static MeshType GetHandMeshTypeBySlot(EquipItemSlot type)
    {
        switch (type)
        {
            case EquipItemSlot.HandLeft:
                return MeshType.HandLeft;
            case EquipItemSlot.HandRight:
                return MeshType.HandRight;
            default:
                return MeshType.Undefined;
        }
    }

    public static bool IsZero(float value)
    {
        return Mathf.Abs(value) <= Mathf.Epsilon;
    }

    public static float DistanceToObject(Vector3 from, Vector3 to)
    {
        from.y = to.y = 0;
        return Vector3.Distance(from, to);
    }

    public static float AngleBetweenObjects(Quaternion from, Quaternion to)
    {
        return Mathf.Abs(from.eulerAngles.y - to.eulerAngles.y);
    }

    public static bool IsNear(Vector3 from, Vector3 to, float distance = 1f)
    {
        return DistanceToObject(from, to) < distance;
    }

    public static bool IsNear(Vector3 from, Collider to, float distance = 1f)
    {
        return DistanceToObject(from, to.transform.position) < distance + Mathf.Min(to.bounds.size.x, to.bounds.size.y);
    }

    public static Vector3 GetDirection(Vector3 from, Vector3 to)
    {
        var heading = to - from;
        var distance = heading.magnitude;
        return heading / distance;
    }

    // old shit code, remove
    public static Coroutine StartCoroutine(IEnumerator enumerator)
    {
        return GameManager.Instance.StartCoroutine(enumerator);
    }

    public static int GetActualIndex(int index, int arraySize, int defaultIndex = 0)
    {
        if (index >= arraySize) return defaultIndex;
        if (index < defaultIndex) return arraySize - 1;
        return index;
    }

    public static string SanitizeName(this string name) =>
        name.Replace("(Clone)", "").Trim();

    public static Texture2D CreateGameMergeTexture(int size)
    {
        return new Texture2D(size, size, TextureFormat.RGB24, false);
    }
}
