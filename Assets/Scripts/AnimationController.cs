using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimationController : MonoBehaviour
{
    private Animator _animator;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayFlipPieceAnimation()
    {
        _animator.SetTrigger("isFlipping");
        Debug.Log("Flip Piece Animation");
    }

    
}
