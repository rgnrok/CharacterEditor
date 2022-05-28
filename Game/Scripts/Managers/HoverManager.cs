using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverManager : MonoBehaviour
{
    public Color friendColor;
    public Color enemyColor;
    public Color neuralColor;

    private Dictionary<string, Outline> _cachedOutlines = new Dictionary<string, Outline>();
    private List<IHover> currentEntities = new List<IHover>();

    public void HoverFirends(params IHover[] hoverEntities)
    {
        Hover(friendColor, hoverEntities);
    }

    public void HoverEnemies(params IHover[] hoverEntities)
    {
        Hover(enemyColor, hoverEntities);
    }

    private void Hover(Color color, params IHover[] hoverEntities)
    {
        Outline outline;

        foreach (var hoverEntity in hoverEntities)
        {
            if (!_cachedOutlines.TryGetValue(hoverEntity.Guid, out outline))
            {
                outline = hoverEntity.EntityGameObject.GetComponent<Outline>();
                if (outline == null) outline = hoverEntity.EntityGameObject.AddComponent<Outline>();
                if (outline == null) continue;

                _cachedOutlines[hoverEntity.Guid] = outline;
            }
            outline.EnableOutilne(color);
        }

        foreach (var currentEntity in currentEntities)
        {
            if (Array.IndexOf(hoverEntities, currentEntity) == -1)
            {
                // If entity ready hover - than it must be into cache
                if (!_cachedOutlines.TryGetValue(currentEntity.Guid, out outline)) continue;

                outline.DisableOutilne();
            }
        }

        currentEntities.Clear();
        currentEntities.AddRange(hoverEntities);
    }

    public void UnHover()
    {
        Outline outline;
        foreach (var currentEntity in currentEntities)
        {
            // If entity ready hover - than it must be into cache
            if (!_cachedOutlines.TryGetValue(currentEntity.Guid, out outline)) continue;

            outline.DisableOutilne();
        }
        currentEntities.Clear();
    }
}
