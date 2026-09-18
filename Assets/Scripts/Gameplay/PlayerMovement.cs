using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Units per second the goalkeeper moves toward the target lane position.")]
    [SerializeField] private float speed = 12f;

    [Tooltip("World-space distance between adjacent lanes. Must match the ball's lane spacing.")]
    [SerializeField] private float laneDistance = 5f;

    [Header("References")]
    [SerializeField] private BallController ballController;
    [SerializeField] private CrowdController crowdController;
    [SerializeField] private BackGroundTeamManager backGroundTeamManager;

    private static readonly int leftDiveHash = Animator.StringToHash("LeftDive");
    private static readonly int rightDiveDash = Animator.StringToHash("RightDive");
    private static readonly int headerHash = Animator.StringToHash("Header");
    private static readonly int leftMoveHash = Animator.StringToHash("LeftMove");
    private static readonly int rightMoveHash = Animator.StringToHash("RightMove");
    private static readonly int playAfterLeftDiveHash = Animator.StringToHash("AfterLeftDive");
    private static readonly int playAfterRightDiveHash = Animator.StringToHash("AfterRightDive");

    private const int TotalLanes = 5;
    private const int CenterLane = 2;

    public int _currentLane;

    private float _targetX;
    private Rigidbody _rb;
    private Animator _animator;

    private bool _scoredThisBall;
    private bool _isDiving;
    private bool _isReturning;

    // True when the last dive moved from center toward the left.
    private bool _lastDiveWasLeft;

    private Vector3 pos;
    private float _initialY;
    private Quaternion _initialYRotation;

    public bool canHeader = false;
    public Action onBallStop;

    // Notifies the exercise input bridge when the goalkeeper
    // has completely returned to the starting/center position.
    public Action OnReturnedToCenter;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb == null)
        {
            enabled = false;
            return;
        }

        _rb.freezeRotation = true;
        _rb.isKinematic = true;

        _currentLane = CenterLane;
        _targetX = 0f;

        _initialY = transform.position.y;
        _initialYRotation = transform.rotation;

        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            enabled = false;
            return;
        }

        pos = _rb.position;
        pos.x = _targetX;
        pos.y = _initialY;

        _rb.position = pos;

        OnDiveFinished();
    }

    void Update()
    {
        // Do not accept another movement/dive while the player
        // is diving or returning to the center.
        if (_isDiving || _isReturning)
            return;

        // Keyboard fallback / testing.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.LeftArrow))
            {
                TriggerLeftDive();
                return;
            }

            if (Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.RightArrow))
            {
                TriggerRightDive();
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _animator.applyRootMotion = false;
            _currentLane++;

            _animator.SetTrigger(leftMoveHash);
        }
        else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow))
        {
            _animator.applyRootMotion = false;
            _currentLane--;

            _animator.SetTrigger(rightMoveHash);
        }

        _currentLane =
            Mathf.Clamp(
                _currentLane,
                0,
                TotalLanes - 1);

        _targetX =
            (_currentLane - CenterLane) *
            laneDistance;
    }

    void FixedUpdate()
    {
        if (!_isDiving && !_isReturning)
        {
            Vector3 targetPosition =
                new Vector3(
                    _targetX,
                    _rb.position.y,
                    _rb.position.z);

            _rb.MovePosition(
                Vector3.MoveTowards(
                    _rb.position,
                    targetPosition,
                    speed * Time.fixedDeltaTime));
        }

        // Return physically to the exact center position.
        // LeftMove/RightMove are used for the animation;
        // Rigidbody movement guarantees the full return distance.
        if (_isReturning)
        {
            Vector3 targetPosition =
                new Vector3(
                    0f,
                    _rb.position.y,
                    _rb.position.z);

            Vector3 newPosition =
                Vector3.MoveTowards(
                    _rb.position,
                    targetPosition,
                    speed * Time.fixedDeltaTime);

            _rb.MovePosition(newPosition);

            if (Mathf.Abs(newPosition.x) <= 0.01f)
            {
                FinishReturnToCenter();
            }
        }
    }

    public void CanCatch()
    {
        if (_currentLane == CenterLane &&
            ballController.laneX == 0f)
        {
            canHeader = true;
        }
        else
        {
            canHeader = false;
        }
    }

    public void TriggerLeftDive()
    {
        if (_isDiving || _isReturning)
            return;

        if (_currentLane == TotalLanes - 1)
            return;

        _lastDiveWasLeft = true;

        _currentLane += 2;

        _targetX =
            (_currentLane - CenterLane) *
            laneDistance;

        _animator.applyRootMotion = true;
        _isDiving = true;

        AudioManager.instance.PlayPlayerDiveSound();

        _animator.SetTrigger(leftDiveHash);

        CancelInvoke(nameof(OnDiveFinished));
        Invoke(nameof(OnDiveFinished), 2.5f);
    }

    public void TriggerRightDive()
    {
        if (_isDiving || _isReturning)
            return;

        if (_currentLane == 0)
            return;

        _lastDiveWasLeft = false;

        _currentLane -= 2;

        _targetX =
            (_currentLane - CenterLane) *
            laneDistance;

        _animator.applyRootMotion = true;
        _isDiving = true;

        AudioManager.instance.PlayPlayerDiveSound();

        _animator.SetTrigger(rightDiveDash);

        CancelInvoke(nameof(OnDiveFinished));
        Invoke(nameof(OnDiveFinished), 2.5f);
    }

    public void OnDiveFinished()
    {
        _isDiving = false;
        _animator.applyRootMotion = false;

        if (_currentLane == CenterLane)
        {
            _animator.SetBool(
                playAfterLeftDiveHash,
                false);

            _animator.SetBool(
                playAfterRightDiveHash,
                false);

            return;
        }

        _animator.SetBool(
            playAfterRightDiveHash,
            true);

        _animator.SetBool(
            playAfterLeftDiveHash,
            true);

        // ---------------------------------------------------------
        // RETURN TO CENTER
        // ---------------------------------------------------------
        //
        // Left dive:
        //     Center lane 2 -> lane 4
        //     Return animation = RightMove
        //
        // Right dive:
        //     Center lane 2 -> lane 0
        //     Return animation = LeftMove
        //
        // Physical Rigidbody movement handles the full return.
        // ---------------------------------------------------------

        _isReturning = true;
        _animator.applyRootMotion = false;

        _currentLane = CenterLane;
        _targetX = 0f;

        if (_lastDiveWasLeft)
        {
            _animator.SetTrigger(rightMoveHash);
        }
        else
        {
            _animator.SetTrigger(leftMoveHash);
        }
    }

    private void FinishReturnToCenter()
    {
        _isReturning = false;
        _animator.applyRootMotion = false;

        _currentLane = CenterLane;
        _targetX = 0f;

        Vector3 currentPos = _rb.position;
        currentPos.x = 0f;
        currentPos.y = _initialY;

        _rb.position = currentPos;
        _rb.rotation = _initialYRotation;

        _animator.SetBool(
            playAfterLeftDiveHash,
            false);

        _animator.SetBool(
            playAfterRightDiveHash,
            false);

        // Allow the input bridge to accept the same direction
        // again on the next completed/started repetition.
        OnReturnedToCenter?.Invoke();
    }

    public void OnReturnFinished()
    {
        // Kept public in case an existing Animation Event references it.
        // The automatic Rigidbody return normally finishes the return.
        FinishReturnToCenter();
    }

    public void ResetSaveGuard()
    {
        _scoredThisBall = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball") &&
            !_scoredThisBall)
        {
            if (canHeader)
            {
                _animator.SetTrigger(headerHash);
            }

            onBallStop?.Invoke();

            backGroundTeamManager.PlayWin();
            crowdController.PlayCheer();

            _scoredThisBall = true;

            AudioManager.instance.PlaySave();

            ballController?.RegisterSave();
        }
    }
}
