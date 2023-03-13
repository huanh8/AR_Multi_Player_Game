using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Piece : MonoBehaviour
{
 public class Factory : PlaceholderFactory<Piece> { }
}
