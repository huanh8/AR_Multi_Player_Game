using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] GameObject _boardGeneratorPrefab;
    [SerializeField] GameObject _piecePrefab;

    public override void InstallBindings()
    {
        Container.Bind<BoardGenerator>().FromComponentInNewPrefab(_boardGeneratorPrefab).AsSingle();
        Container.BindFactory<Vector3, Quaternion, Transform, Material, Piece, Piece.Factory>().FromComponentInNewPrefab(_piecePrefab).AsSingle();
    }
}