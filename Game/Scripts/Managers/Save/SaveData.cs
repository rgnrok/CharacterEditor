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
        public string mainCharacterGuid;
        public string selectedCharacterGuid;
        public CharacterSaveData[] characters;
        public Dictionary<string, ContainerSaveData> containers;
        public string[] enemies = { "833a388b-6760-4567-989d-b5fa8f5c9d4d" };

        public SaveData()
        {

        }

        public SaveData(string file, CharacterSaveData mainCharacter)
        {
            saveName = file;
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
                mainCharacterGuid = info.GetString("mainCharacterGuid");
                selectedCharacterGuid = info.GetString("selectedCharacterGuid");
                characters = (CharacterSaveData[]) info.GetValue("characters", typeof(CharacterSaveData[]));
                containers = (Dictionary<string, ContainerSaveData>) info.GetValue("containers", typeof(Dictionary<string, ContainerSaveData>));
            }
            catch (SerializationException e)
            {
                containers = new Dictionary<string, ContainerSaveData>();
            }
        }


        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("saveName", saveName);
            info.AddValue("mainCharacterGuid", mainCharacterGuid);
            info.AddValue("selectedCharacterGuid", selectedCharacterGuid);
            info.AddValue("characters", characters);
            info.AddValue("containers", containers);
        }
    }
}