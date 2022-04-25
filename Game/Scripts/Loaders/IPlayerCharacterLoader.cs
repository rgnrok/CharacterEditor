using System;
using System.Collections;
using System.Collections.Generic;

namespace CharacterEditor
{
    public interface IPlayerCharacterLoader
    {
        void LoadPlayerCharacters(Action<Dictionary<string, PlayerCharacterConfig>> callback);

        void LoadPlayerCharacter(string guid, Action<PlayerCharacterConfig> callback);

        void LoadPlayerCharacters(List<string> guids, Action<Dictionary<string, PlayerCharacterConfig>> callback);
    }
}
