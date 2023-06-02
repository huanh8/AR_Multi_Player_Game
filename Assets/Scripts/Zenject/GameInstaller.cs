using UnityEngine;
using Zenject;
using static Constants;

public class GameInstaller : MonoInstaller
{
    // [SerializeField] GameObject _boardGeneratorPrefab;
    // [SerializeField] GameObject _piecePrefab;

    // public override void InstallBindings()
    // {
    //     Container.Bind<BoardGenerator>().FromComponentInNewPrefab(_boardGeneratorPrefab).AsSingle();
    //     Container.BindFactory<Vector3, Quaternion, PieceTypeList, Piece, Piece.Factory>().FromComponentInNewPrefab(_piecePrefab).AsSingle();
    //     Container.Bind<InputController>().FromNewComponentOnNewGameObject().AsSingle();
    // }
}