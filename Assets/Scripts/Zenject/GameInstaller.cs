using UnityEngine;
using Zenject;
using static Constants;

public class GameInstaller : MonoInstaller
{
    [SerializeField] GameObject _boardGeneratorPrefab;
    [SerializeField] GameObject _piecePrefab;

    public override void InstallBindings()
    {
        Container.Bind<BoardGenerator>().FromComponentInNewPrefab(_boardGeneratorPrefab).AsSingle();
        Container.BindFactory<UnityEngine.Vector3, Quaternion, Constants.PieceTypeList, Piece, Piece.Factory>().FromComponentInNewPrefab(_piecePrefab).AsSingle();
    }
}