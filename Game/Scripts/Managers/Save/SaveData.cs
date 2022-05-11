using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CharacterEditor.CharacterInventory;

namespace CharacterEditor
{
    [Serializable]
    public class SaveData: ISerializable
    {
        public string saveName;
        public string levelKey;
        public string mainCharacterGuid;
        public string selectedCharacterGuid;
        public CharacterSaveData[] characters;
        public Dictionary<string, ContainerSaveData> containers;

        public SaveData()
        {

        }

        public SaveData(string name, CharacterSaveData mainCharacter)
        {
            saveName = name;
            mainCharacterGuid = mainCharacter.guid;
            selectedCharacterGuid = mainCharacter.guid;
            characters = new [] {mainCharacter};
            containers = new Dictionary<string, ContainerSaveData>();
        }

        public SaveData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                saveName = info.GetString("saveName");
                levelKey = info.GetString("levelKey");
                mainCharacterGuid = info.GetString("mainCharacterGuid");
                selectedCharacterGuid = info.GetString("selectedCharacterGuid");
                characters = (CharacterSaveData[]) info.GetValue("characters", typeof(CharacterSaveData[]));
                containers = (Dictionary<string, ContainerSaveData>) info.GetValue("containers", typeof(Dictionary<string, ContainerSaveData>));
            }
            catch
            {
                containers = new Dictionary<string, ContainerSaveData>();
            }
        }


        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("saveName", saveName);
            info.AddValue("levelKey", levelKey);
            info.AddValue("mainCharacterGuid", mainCharacterGuid);
            info.AddValue("selectedCharacterGuid", selectedCharacterGuid);
            info.AddValue("characters", characters);
            info.AddValue("containers", containers);
        }
    }
}