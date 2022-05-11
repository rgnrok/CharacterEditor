using System;
using System.Collections;
using System.Collections.Generic;

namespace CharacterEditor
{
    public interface IPlayerCharacterLoader
    {
        void LoadPlayerCharacters(Action<Dictionary<string, PlayableNpcConfig>> callback);

        void LoadPlayerCharacter(string guid, Action<PlayableNpcConfig> callback);

        void LoadPlayerCharacters(List<string> guids, Action<Dictionary<string, PlayableNpcConfig>> callback);
    }
}
