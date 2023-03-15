using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject]
    private BoardGenerator _boardGenerator;

    private void Init (
        BoardGenerator boardGenerator
    ) 
    {
        _boardGenerator = boardGenerator;
    }

    private  void Start()
    {
        _boardGenerator.ManualStart();
    }
}
