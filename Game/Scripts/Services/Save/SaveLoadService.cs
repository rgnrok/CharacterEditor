using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Mesh;
using CharacterEditor.Services;
using CharacterEditor.StaticData;
using EnemySystem;
using UnityEngine;
using Sprite = UnityEngine.Sprite;

namespace CharacterEditor
{
    public class SaveLoadService
    {
        protected const string CHARACTER_SKIN_TEXTURE_NAME = "Character_texture.png";
        protected const string CHARACTER_FACE_TEXTURE_NAME = "Character_face_texture.png";
        protected const string LOADED_SAVE_KEY = "loadedSaveKey";

        protected void KeepLastSaveName(string saveName) => 
            PlayerPrefs.SetString(LOADED_SAVE_KEY, saveName);

    }
}