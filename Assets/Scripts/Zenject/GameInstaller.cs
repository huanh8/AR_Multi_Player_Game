using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] GameObject _boardGeneratorPrefab;
    [SerializeField] GameObject _redPiecePrefab;
    [SerializeField] GameObject _bluePiecePrefab;

    public override void InstallBindings()
    {
        Container.Bind<BoardGenerator>().FromComponentInNewPrefab(_boardGeneratorPrefab).AsSingle();
        Container.BindFactory<Piece, Piece.Factory>().FromComponentInNewPrefab(_redPiecePrefab);
        Container.BindFactory<Piece, Piece.Factory>().FromComponentInNewPrefab(_bluePiecePrefab);

    }
}