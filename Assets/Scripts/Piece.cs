using UnityEngine;

public class Piece : MonoBehaviour
{ 
    public Vector3Int Position { get; private set; }
    public TetrominoData TetrominoData { get; private set; }
    public Vector3Int[] Cells { get; private set; }
    
    [SerializeField] private Board board;
    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private float lockDelay = 0.5f;

    private float _stepTime;
    private float _lockTime;
    private int _rotationIndex;

    public void Initialize(Vector3Int position, TetrominoData data)
    {
        Position = position;
        TetrominoData = data;
        _rotationIndex = 0;
        _stepTime = Time.time + stepDelay;
        _lockTime = 0f;

        Cells ??= new Vector3Int[data.Cells.Length];

        for (int i = 0; i < data.Cells.Length; i++)
        {
            Cells[i] = (Vector3Int)data.Cells[i];
        }
    }

    private void Update()
    {
        board.Clear(this);

        _lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.A))
            Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D))
            Move(Vector2Int.right);
        
        if (Input.GetKeyDown(KeyCode.Q))
            Rotate(-1);
        else if (Input.GetKeyDown(KeyCode.E))
            Rotate(1);

        if (Input.GetKeyDown(KeyCode.Space))
            HardDrop();
        
        if(Time.time >= _stepTime)
           Step();

        board.Set(this);
    }

    private void Step()
    {
        _stepTime = Time.time + stepDelay;

        Move(Vector2Int.down);

        if (_lockTime >= lockDelay)
            Lock();
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
            continue;
        
        Lock();
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = Position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        if (valid)
        {
            Position = newPosition;
            _lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = _rotationIndex;
        _rotationIndex += Wrap(_rotationIndex + direction, 0, 4);

       ApplyRotationMatrix(direction);

        if (!TestWallKicks(direction))
        {
            _rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3 cell = Cells[i];

            int x, y;

            switch (TetrominoData.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) +
                                        (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) +
                                        (cell.y * Data.RotationMatrix[3] * direction));
                    break;
                    
                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) +
                                         (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) +
                                         (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            Cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationDirection);

        for (int i = 0; i < TetrominoData.WallKicks.GetLength(1); i++)
        {
            Vector2Int translation = TetrominoData.WallKicks[wallKickIndex, i];

            if (Move(translation))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int rotationDirection)
    {
        int wallKickIndex = _rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, TetrominoData.WallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
            return max - (min - input) % (max - min);
        else
            return min + (input - min) % (max - min);
    }
}
