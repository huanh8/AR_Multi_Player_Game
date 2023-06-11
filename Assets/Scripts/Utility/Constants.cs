public static class Constants
{ 
    public const string CAMERA_NAME = "Camera";
    public const string BOARD_NAME = "Board";
    public const string PIECE_NAME = "Piece";
    public const int BOARD_SIZE = 6;
    public const string RED_NAME = "RedPiece";
    public const string BLUE_NAME = "BluePiece";
    public enum PieceTypeList { Red, Blue, None };
    public enum GameState
    {
        HasMoved,
        HasNotStarted
    }
}