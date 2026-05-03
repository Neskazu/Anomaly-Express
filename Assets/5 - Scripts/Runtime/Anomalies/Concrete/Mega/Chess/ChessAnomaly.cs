using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using ChessDotNet;
using Unity.Collections;

namespace Anomalies
{
    public enum ChessPieceType { Pawn, Knight, Bishop, Rook, Queen, King }

    [System.Serializable]
    public struct PieceModelMapping
    {
        public ChessPieceType type;
        public ChessDotNet.Player color;
        public GameObject prefab;
    }

    public class ChessAnomaly : AnomalyBase
    {
        [Header("Settings")]
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private PieceModelMapping[] _mappings;

        private NetworkVariable<FixedString128Bytes> _currentBoardFen = new NetworkVariable<FixedString128Bytes>();
        private NetworkVariable<ulong> _whitePlayerId = new NetworkVariable<ulong>(999);

        private ChessGame _gameLogic;
        private ChessPiece[,] _activePieces = new ChessPiece[8, 8];
        private Vector2Int? _selectedCoord = null;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentBoardFen.OnValueChanged += OnBoardChanged;

            if (IsServer && _gameLogic == null)
            {
                _gameLogic = new ChessGame();
                _currentBoardFen.Value = _gameLogic.GetFen();
            }
        }

        protected override void OnActivate()
        {
            if (!IsServer) return;

            _gameLogic = new ChessGame();
            _currentBoardFen.Value = _gameLogic.GetFen();

            var clients = NetworkManager.Singleton.ConnectedClientsIds;
            if (clients.Count > 0) _whitePlayerId.Value = clients[0];

            LockPlayerCameraClientRpc(_whitePlayerId.Value);
        }

        protected override void OnDeactivate()
        {
            if (IsServer) _whitePlayerId.Value = 999;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (_activePieces[x, y] != null) Destroy(_activePieces[x, y].gameObject);
                }
            }
        }

        [ClientRpc]
        private void LockPlayerCameraClientRpc(ulong targetId)
        {
            if (NetworkManager.Singleton.LocalClientId != targetId) return;
        }

        private void Update()
        {
            if (NetworkManager.Singleton.LocalClientId != _whitePlayerId.Value) return;
            if (Input.GetMouseButtonDown(0)) HandleInput();
        }

        private void HandleInput()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 localPos = _boardRoot.InverseTransformPoint(hit.point);
                int x = Mathf.RoundToInt(localPos.x / _cellSize);
                int y = Mathf.RoundToInt(localPos.z / _cellSize);

                if (x < 0 || x > 7 || y < 0 || y > 7)
                {
                    return;
                }

                if (_selectedCoord == null)
                {
                    if (_activePieces[x, y] != null)
                    {
                        char pieceChar = _activePieces[x, y].name[0];

                        if (!char.IsUpper(pieceChar))
                        {
                            return;
                        }
                        _selectedCoord = new Vector2Int(x, y);
                        _activePieces[x, y].SetSelected(true);
                    }
                }
                else
                {
                    _activePieces[_selectedCoord.Value.x, _selectedCoord.Value.y].SetSelected(false);

                    string moveFrom = CoordToAlgebraic(_selectedCoord.Value);
                    string moveTo = CoordToAlgebraic(new Vector2Int(x, y));
                    MakeMoveServerRpc(moveFrom, moveTo);
                    _selectedCoord = null;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void MakeMoveServerRpc(string moveFrom, string moveTo, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (senderId != _whitePlayerId.Value)
            {
                return;
            }

            Move move = new Move(moveFrom, moveTo, _gameLogic.WhoseTurn);
            bool isValid = _gameLogic.IsValidMove(move);

            if (isValid)
            {
                _gameLogic.MakeMove(move, true);

                if (_gameLogic.IsCheckmated(ChessDotNet.Player.Black) || _gameLogic.IsCheckmated(ChessDotNet.Player.White))
                {
                    Deactivate();
                }
                else
                {
                    ExecuteAIMove();
                    _currentBoardFen.Value = _gameLogic.GetFen();
                }
            }
        }

        private void ExecuteAIMove()
        {
            var moves = _gameLogic.GetValidMoves(ChessDotNet.Player.Black);
            if (moves.Count > 0)
            {
                _gameLogic.MakeMove(moves[Random.Range(0, moves.Count)], true);
            }
        }

        private void OnBoardChanged(FixedString128Bytes oldFen, FixedString128Bytes newFen)
        {
            UpdateBoardVisuals(newFen.ToString());
        }

        private void UpdateBoardVisuals(string fen)
        {
            char[,] newBoardState = ParseFen(fen);
            ChessPiece[,] nextActivePieces = new ChessPiece[8, 8];

            Vector2Int moveFrom = new Vector2Int(-1, -1);
            Vector2Int moveTo = new Vector2Int(-1, -1);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    char newChar = newBoardState[x, y];
                    ChessPiece oldPiece = _activePieces[x, y];
                    char oldChar = oldPiece != null ? oldPiece.name[0] : ' ';

                    if (newChar == ' ' && oldChar != ' ') moveFrom = new Vector2Int(x, y);
                    if (newChar != ' ' && newChar != oldChar) moveTo = new Vector2Int(x, y);
                }
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    char newChar = newBoardState[x, y];
                    if (x == moveTo.x && y == moveTo.y && moveFrom.x != -1)
                    {
                        if (_activePieces[x, y] != null) _activePieces[x, y].PlayCaptureAndDestroy();
                        ChessPiece movingPiece = _activePieces[moveFrom.x, moveFrom.y];
                        if (movingPiece != null)
                        {
                            movingPiece.MoveTo(new Vector3(x * _cellSize, 0, y * _cellSize));
                            movingPiece.name = $"{newChar}_{x}_{y}";
                            nextActivePieces[x, y] = movingPiece;
                        }
                    }
                    else if (newChar != ' ' && _activePieces[x, y] != null && _activePieces[x, y].name[0] == newChar)
                    {
                        nextActivePieces[x, y] = _activePieces[x, y];
                    }
                    else if (newChar != ' ')
                    {
                        nextActivePieces[x, y] = SpawnPiece(x, y, newChar);
                    }
                }
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (_activePieces[x, y] != null)
                    {
                        bool stillExists = false;
                        foreach (var p in nextActivePieces) if (p == _activePieces[x, y]) stillExists = true;
                        if (!stillExists) _activePieces[x, y].PlayCaptureAndDestroy();
                    }
                }
            }
            _activePieces = nextActivePieces;
        }

        private char[,] ParseFen(string fen)
        {
            char[,] board = new char[8, 8];
            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++) board[i, j] = ' ';
            string boardPart = fen.Split(' ')[0];
            string[] rows = boardPart.Split('/');
            for (int r = 0; r < 8; r++)
            {
                int col = 0;
                foreach (char c in rows[r])
                {
                    if (char.IsDigit(c)) col += (int)char.GetNumericValue(c);
                    else { board[col, 7 - r] = c; col++; }
                }
            }
            return board;
        }

        private ChessPiece SpawnPiece(int x, int y, char fenChar)
        {
            ChessDotNet.Player color = char.IsUpper(fenChar) ? ChessDotNet.Player.White : ChessDotNet.Player.Black;
            ChessPieceType type = GetTypeFromChar(fenChar);
            var mapping = _mappings.FirstOrDefault(m => m.type == type && m.color == color);
            if (mapping.prefab == null) return null;

            GameObject obj = Instantiate(mapping.prefab, _boardRoot);
            obj.transform.localPosition = new Vector3(x * _cellSize, 0, y * _cellSize);
            obj.name = $"{fenChar}_{x}_{y}";
            return obj.GetComponent<ChessPiece>();
        }

        private string CoordToAlgebraic(Vector2Int coord) => $"{(char)('a' + coord.x)}{coord.y + 1}";

        private ChessPieceType GetTypeFromChar(char c)
        {
            return char.ToLower(c) switch
            {
                'p' => ChessPieceType.Pawn,
                'n' => ChessPieceType.Knight,
                'b' => ChessPieceType.Bishop,
                'r' => ChessPieceType.Rook,
                'q' => ChessPieceType.Queen,
                'k' => ChessPieceType.King,
                _ => ChessPieceType.Pawn
            };
        }
    }
}