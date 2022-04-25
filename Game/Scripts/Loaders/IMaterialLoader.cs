using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMaterialLoader {

    void LoadMaterials(Action<Dictionary<string, Material>> callback);
}
