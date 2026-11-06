using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
// This is the slime, also a NetworkBehaviour. The Player prefab carries
// NetworkObject, NetworkTransform (syncs position), NetworkMecanimAnimator (syncs animations),
// Rigidbody2D, BoxCollider2D and Animator.
{
    public static readonly List<PlayerController> All = new List<PlayerController>();
    public static PlayerController Local { get; private set; }

    static readonly int SpeedParameter = Animator.StringToHash("Speed");
    static readonly int GroundedParameter = Animator.StringToHash("Grounded");

    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float jumpSpeed = 16.5f;
    [SerializeField] Transform body;
    [SerializeField] RuntimeAnimatorController[] skins;

    [Networked] public NetworkBool FacingLeft { get; set; }
    [Networked] public int ExitReached { get; set; }

    Rigidbody2D rb;
    BoxCollider2D box;
    Animator animator;
    NetworkTransform networkTransform;
    bool jumpQueued;
    int spawnedForLoad;

    public int Slot
    // Decides whether you’re player 1 (0) or player 2 (1). It sorts the connected players by ID and
    // finds your position in that list. Both computers get the same answer without sending anything. 
    // Slot picks your spawn point, your allowed colors and your skin.
    {
        get
        {
            List<PlayerRef> players = Runner.ActivePlayers.OrderBy(player => player.PlayerId).ToList();
            return Mathf.Clamp(players.IndexOf(Object.StateAuthority), 0, 1);
        }
    }

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        networkTransform = GetComponent<NetworkTransform>();
        All.Add(this);

        if (HasStateAuthority)
        {
            Local = this;
            rb.simulated = false;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.None;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        All.Remove(this);
        if (Local == this) Local = null;
    }

    void Update()
    {
        if (Local != this) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            jumpQueued = true;

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame) RequestColor(GameColor.Red);
        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame) RequestColor(GameColor.Blue);
        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame) RequestColor(GameColor.Yellow);
    }

    void RequestColor(GameColor color)
    {
        GameManager game = GameManager.Instance;
        if (game == null || game.Phase != GamePhase.Playing) return;
        if (!game.CurrentLevel.ColorsFor(Slot).Contains(color)) return;
        game.RpcRequestColor(color);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        GameManager game = GameManager.Instance;
        bool levelReady = game != null && game.LoadId > 0 && LevelManager.Instance.LoadedId == game.LoadId;
        if (levelReady && spawnedForLoad != game.LoadId)
        {
            spawnedForLoad = game.LoadId;
            Respawn();
        }

        bool playing = levelReady && game.Phase == GamePhase.Playing;
        rb.simulated = playing;
        if (!playing)
        {
            jumpQueued = false;
            animator.SetFloat(SpeedParameter, 0f);
            animator.SetBool(GroundedParameter, true);
            return;
        }

        Move();
        CheckSurroundings(game);
    }

    void Move()
    {
        Keyboard keyboard = Keyboard.current;
        float input = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input += 1f;
        }

        bool grounded = IsGrounded();
        Vector2 velocity = rb.linearVelocity;
        velocity.x = input * moveSpeed;
        if (jumpQueued && grounded) velocity.y = jumpSpeed;
        jumpQueued = false;
        rb.linearVelocity = velocity;

        if (input != 0f) FacingLeft = input < 0f;
        animator.SetFloat(SpeedParameter, Mathf.Abs(velocity.x));
        animator.SetBool(GroundedParameter, grounded);
    }

    bool IsGrounded()
    {
        Vector2 feet = rb.position + box.offset + Vector2.down * (box.size.y / 2f + box.edgeRadius);
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(feet, new Vector2(box.size.x * 0.9f, 0.1f), 0f))
        {
            if (!hit.isTrigger && hit.attachedRigidbody != rb) return true;
        }
        return false;
    }

    void CheckSurroundings(GameManager game)
    {
        Vector2 center = rb.position + box.offset;
        bool atExit = false;
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, box.size, 0f))
        {
            if (hit.GetComponent<Hazard>() != null)
            {
                Die(game);
                return;
            }
            if (hit.GetComponent<ExitZone>() != null) atExit = true;
        }

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, box.size * 0.6f, 0f))
        {
            if (!hit.isTrigger && hit.GetComponent<PlayerController>() == null)
            {
                Die(game);
                return;
            }
        }

        ExitReached = atExit ? game.LoadId : 0;
    }

    void Die(GameManager game)
    {
        Respawn();
        game.RpcPlayerDied();
    }

    void Respawn()
    {
        Vector2 spawn = LevelManager.Instance.CurrentLevel.SpawnPoints[Slot].position;
        rb.linearVelocity = Vector2.zero;
        rb.position = spawn;
        networkTransform.Teleport((Vector3)spawn);
        ExitReached = 0;
    }

    public override void Render()
    {
        body.localScale = new Vector3(FacingLeft ? 1f : -1f, 1f, 1f);
        RuntimeAnimatorController skin = skins[Slot];
        if (animator.runtimeAnimatorController != skin) animator.runtimeAnimatorController = skin;
    }
}
