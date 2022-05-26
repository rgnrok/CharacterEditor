using System.Threading.Tasks;
using CharacterEditor;

public class CreateGameRotateComponent : PointerYRotateComponent
{
    private IConfigManager _configManager;

    private void Awake()
    {
        _configManager = AllServices.Container.Single<IConfigManager>();
        if (_configManager != null)
            _configManager.OnChangeConfig += OnChangeConfigHandler;
    }

    private Task OnChangeConfigHandler(CharacterGameObjectData objData)
    {
        SetTarget(objData.CharacterObject.transform);
        return Task.CompletedTask;
    }
}
