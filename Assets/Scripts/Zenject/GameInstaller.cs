using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] GameObject _boardGeneratorPrefab;
    public override void InstallBindings()
    {
        Container.Bind<BoardGenerator>().FromComponentInNewPrefab(_boardGeneratorPrefab).AsSingle();
    }
}