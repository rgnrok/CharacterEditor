﻿using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface IMergeTextureService : IService
    {
        void MergeTextures(Material skinRenderShaderMaterial, RenderTexture renderSkinTexture, Dictionary<string, Texture2D> textures);
        Texture2D BuildTextureAtlas(int partTextureSize, List<Texture2D> textures);
    }
}