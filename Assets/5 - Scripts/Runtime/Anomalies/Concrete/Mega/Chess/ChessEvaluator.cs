using ChessDotNet;
using ChessDotNet.Pieces;

public static class ChessEvaluator
{
    public static int Evaluate(ChessGame game)
    {
        int score = 0;
        for (int rank = 1; rank <= 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                Piece piece = game.GetPieceAt(new Position((File)file, rank));

                if (piece != null)
                {
                    int val = GetPieceValue(piece);
                    score += (piece.Owner == ChessDotNet.Player.White) ? val : -val;
                }
            }
        }
        return score;
    }

    private static int GetPieceValue(Piece piece)
    {
        if (piece is Pawn) return 100;
        if (piece is Knight) return 300;
        if (piece is Bishop) return 310;
        if (piece is Rook) return 500;
        if (piece is Queen) return 900;
        if (piece is King) return 10000;
        return 0;
    }
}