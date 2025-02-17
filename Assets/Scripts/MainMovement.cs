using System;
using Unity.VisualScripting;
using UnityEngine;

/*NOTAS DEV
 * > En inside el salto siempre es a velocidad de running, por lo qu� me recomiendo implementar 
 * el estado running como defecto y el estado walk como el secundario.
 * > En inside el personaje corre segun la fuerza con la que mueves el analogo, en teclado camina 
 * con Shift
 * > Agregar la CAPA SUELO y quitar eso de que ignore el primer collider
 * */
public enum states
{
    idle,
    walk,
    run,
    air,
    grabbing
}
public class MainMovement : MonoBehaviour
{
    public states currentState;
    public states lastState;
    public bool grounded;
    public bool mirandoDer;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float runSpeed;
    [SerializeField] private float grabbingSpeed;
    [SerializeField] private float aceleration;
    [SerializeField] private LayerMask floorMask;
    private float horizontal;
    private float anguloPendienteAnterior = 0.0f;
    private Animator animator;
    private Rigidbody2D rb2d;
    private CapsuleCollider2D playerCollider;

    private bool btnJump;
    private bool btnRun;
    private bool btnGrab;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();

        currentState = states.idle;
        lastState = states.idle;
    }


    void Update()
    {
        //calcula el tamaño del colider:
        float colliderSizeY = (playerCollider.bounds.size.y);
        float colliderSizeX = (playerCollider.bounds.size.x);

        StateController(colliderSizeX, colliderSizeY);

        BtnController();

        AnimatorController();

        HMove();

        Jump();

        Grab(colliderSizeX, colliderSizeY);

        ComprobarPendiente();
    }

    void FixedUpdate()
    {
        //Movimiento horizontal del personaje en el suelo
        if (currentState == states.walk)
        {
            if (Mathf.Abs(rb2d.linearVelocityX) < walkSpeed)
            {
                rb2d.AddForce(new Vector2(horizontal * aceleration, 0));
            }
            // else if (Mathf.Abs(rb2d.linearVelocityX) > walkSpeed && horizontal != 0)
            // {
            //     rb2d.linearVelocity = new Vector2(horizontal * walkSpeed, rb2d.linearVelocity.y);
            // }
        }
        else if (currentState == states.grabbing)
        {
            if (Mathf.Abs(rb2d.linearVelocityX) < grabbingSpeed)
            {
                rb2d.AddForce(new Vector2(horizontal * aceleration, 0));
            }
        }
        else
        {
            if (Mathf.Abs(rb2d.linearVelocityX) < runSpeed)
            {
                rb2d.AddForce(new Vector2(horizontal * aceleration, 0));
            }
            // else if (Mathf.Abs(rb2d.linearVelocityX) > runSpeed && horizontal != 0)
            // {
            //     rb2d.linearVelocity = new Vector2(horizontal * runSpeed, rb2d.linearVelocity.y);
            // }

        }
    }

    private void HMove()
    {
        horizontal = Input.GetAxisRaw("Horizontal"); //Recibe Input horizontal
    }

    private void Jump()
    {
        if (btnJump && currentState != states.air)
        {
            rb2d.AddForce(Vector2.up * jumpForce);
            animator.SetBool("jump", true);
        }
        else
        {
            animator.SetBool("jump", false);
        }
    }

    private void Grab(float colliderSizeX, float colliderSizeY)
    {
        Vector3 sangriaR = new Vector3(0.5f, 0.0f, 0.0f);
        Vector3 sangriaL = new Vector3(-0.5f, 0.0f, 0.0f);
        float rcSize = 0.3f;

        //>>>>>>Todos estos RAYCAST son en direccion hacia la derecha<<<<<
        RaycastHit2D rcCenterR = Physics2D.Raycast(transform.position + sangriaR, Vector3.right, rcSize);
        Debug.DrawRay(transform.position + sangriaR, Vector3.right * rcSize, Color.blue);

        RaycastHit2D rcTopR = Physics2D.Raycast(transform.position + sangriaR + new Vector3(0.0f, colliderSizeY / 2, 0.0f), Vector3.right, rcSize);
        Debug.DrawRay(transform.position + sangriaR + new Vector3(0.0f, colliderSizeY / 2f, 0.0f), Vector3.right * rcSize, Color.blue);

        RaycastHit2D rcBotR = Physics2D.Raycast(transform.position + sangriaR + new Vector3(0.0f, -colliderSizeY / 2, 0.0f), Vector3.right, rcSize);
        Debug.DrawRay(transform.position + sangriaR + new Vector3(0.0f, -colliderSizeY / 2, 0.0f), Vector3.right * rcSize, Color.blue);

        //>>>>>Todos estos RAYCASR son en direccion hacia la izquierda<<<<<<
        RaycastHit2D rcCenterL = Physics2D.Raycast(transform.position + sangriaL, Vector3.left, rcSize);
        Debug.DrawRay(transform.position + sangriaL, Vector3.left * rcSize, Color.blue);

        RaycastHit2D rcTopL = Physics2D.Raycast(transform.position + sangriaL + new Vector3(0.0f, colliderSizeY / 2, 0.0f), Vector3.left, rcSize);
        Debug.DrawRay(transform.position + sangriaL + new Vector3(0.0f, colliderSizeY / 2f, 0.0f), Vector3.left * rcSize, Color.blue);

        RaycastHit2D rcBotL = Physics2D.Raycast(transform.position + sangriaL + new Vector3(0.0f, -colliderSizeY / 2, 0.0f), Vector3.left, rcSize);
        Debug.DrawRay(transform.position + sangriaL + new Vector3(0.0f, -colliderSizeY / 2, 0.0f), Vector3.left * rcSize, Color.blue);

        if (btnGrab)
        {
            Debug.Log("btnGrab presionado");
            if (rcBotR || rcCenterR || rcTopR)
            {
                currentState = states.grabbing;
                Debug.Log("objeto tomado");
            }
        }
        else
        {
            Debug.Log("objeto soltado");
        }


        // if ((rcBotR.collider != null || rcCenterR.collider != null || rcTopR.collider != null) && (rcBotR.collider.gameObject.tag != null || rcCenterR.collider.gameObject.tag != null || rcTopR.collider.gameObject.tag != null))
        // {
        //     string colTag = (rcBotR.collider.gameObject.tag) != null ? rcBotR.collider.gameObject.tag : (rcCenterR.collider.gameObject.tag) != null ? rcCenterR.collider.gameObject.tag : (rcBotR.collider.gameObject.tag) != null ? rcBotR.collider.gameObject.tag : "null";

        //     if (btnGrab)
        //     {
        //         currentState = states.grabbing;
        //         if (rcBotR)
        //         {
        //             if (colTag == "smallbox")
        //             {
        //                 Debug.Log("Caja chica");
        //                 rcBotR.collider.gameObject.transform.SetParent(this.transform);
        //                 rcBotR.collider.gameObject.transform.localPosition = new Vector3(1.0f, 0.0f, 0.0f);
        //             }
        //         }
        //         else if (rcCenterR)
        //         {
        //             if (colTag == "smallbox")
        //             {
        //                 Debug.Log("Caja chica");
        //                 rcCenterR.collider.gameObject.transform.SetParent(this.transform);
        //                 rcCenterR.collider.gameObject.transform.localPosition = new Vector3(1.0f, 0.0f, 0.0f);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log("else");
        //         if (currentState == states.grabbing)
        //         {
        //             Debug.Log("no grabbing");
        //             currentState = states.idle;
        //         }
        //         if (rcCenterR.collider != null && rcCenterR.collider.gameObject.transform.parent != null)
        //         {
        //             Debug.Log("deja de ser hijo");
        //             rcCenterR.collider.gameObject.transform.SetParent(null);
        //         }

        //}
        //}
    }

    private void StateController(float colliderSizeX, float colliderSizeY)
    {
        //Verifica si el personaje esta en el aire webon pe
        Vector3 btmHalfColSizeY = new Vector3(0.0f, -colliderSizeY / 2, 0.0f);
        float rcSize = 0.3f;

        //Todos estos RAYCAST son en direccion hacia abajo, para detectar el suelo
        RaycastHit2D rayCastCenter = Physics2D.Raycast(transform.position + btmHalfColSizeY, Vector3.down, rcSize, floorMask);
        RaycastHit2D rayCastLeft = Physics2D.Raycast(transform.position + btmHalfColSizeY + new Vector3(-colliderSizeX / 2.0f, 0.0f, 0.0f), Vector3.down, rcSize, floorMask);
        RaycastHit2D rayCastRight = Physics2D.Raycast(transform.position + btmHalfColSizeY + new Vector3(colliderSizeX / 2.0f, 0.0f, 0.0f), Vector3.down, rcSize, floorMask);

        //Debug.DrawRay(transform.position + new Vector3(0.0f, -colliderSizeY / 2, 0.0f), Vector3.down * rcSize, Color.red);
        //Debug.DrawRay(transform.position + new Vector3(0.0f, -colliderSizeY / 2, 0.0f) + new Vector3(-colliderSizeX / 2.0f, 0.0f, 0.0f), Vector3.down * rcSize, Color.red);
        //Debug.DrawRay(transform.position + new Vector3(0.0f, -colliderSizeY / 2, 0.0f) + new Vector3(colliderSizeX / 2.0f, 0.0f, 0.0f), Vector3.down * rcSize, Color.red);

        currentState = rayCastCenter || rayCastLeft || rayCastRight ? states.idle : states.air;
        grounded = rayCastCenter || rayCastLeft || rayCastRight ? true : false;

        //Verifica si el personaje esta en Idle, Walk o Run
        if (currentState != states.air && currentState != states.grabbing)
        {
            switch (horizontal)
            {
                case 0:
                    currentState = states.idle; break;
                default:
                    currentState = (btnRun && ((mirandoDer && rb2d.linearVelocityX > 3.4f) || (!mirandoDer && rb2d.linearVelocityX < -3.4f))) ? states.run : states.walk;
                    break;
            }
        }
    }

    public void ComprobarPendiente()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 4f, floorMask);
        //Debug.DrawRay(transform.position, Vector3.down * 4);
        float anguloPendienteABS = Vector2.Angle(hit.normal, Vector2.up);
        float signo = Mathf.Sign(Vector3.Cross(Vector3.up, hit.normal).z);
        float anguloPendiente = anguloPendienteABS * signo;

        //Debug.Log("Angulo pendiente: " + anguloPendiente);


        // Si el personaje está en el suelo, y no se ha pulsado el input de movimiento ni el de salto: 
        if (grounded && horizontal == 0 && !Input.GetKey(KeyCode.Space))
        {
            // Comprobamos si estamos en la pendiente 
            if (hit && Mathf.Abs(hit.normal.x) > 0.1f)
            {
                rb2d.gravityScale = 0;
                // Aplicar gravedad personalizada en base a la inclinación del suelo
                if (hit.collider != null)
                {
                    Vector2 gravityDirection = -hit.normal; // Dirección contraria a la normal
                    rb2d.AddForce(gravityDirection * 1f, ForceMode2D.Force); // Gravedad personalizada

                    //Debug.DrawRay(transform.position, gravityDirection * 10, Color.blue);
                }
            }
        }

        if ((grounded && horizontal > 0 && anguloPendiente > 0 && currentState != states.air) ||
            (grounded && horizontal < 0 && anguloPendiente < 0 && currentState != states.air))
        {
            //Cuando va en subida
            rb2d.gravityScale = 0;
        }
        else
        {
            //Cuando va en bajada
            rb2d.gravityScale = 1;
        }
        if ((anguloPendienteABS < anguloPendienteAnterior) && !Input.GetKey(KeyCode.Space))
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0);
        }
        anguloPendienteAnterior = anguloPendienteABS;
    }


    private void AnimatorController()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        switch (currentState)
        {
            case states.idle:
                animator.SetBool("idle", true);
                animator.SetBool("walk", false);
                animator.SetBool("run", false);
                animator.SetBool("fall", false); break;
            case states.walk:
                if (horizontal > 0.0f)
                {
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    mirandoDer = true;
                    animator.SetBool("walk", true);
                    animator.SetBool("run", false);
                    animator.SetBool("idle", false);
                    animator.SetBool("fall", false);
                }

                if (horizontal < 0.0f)
                {
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    mirandoDer = false;
                    animator.SetBool("walk", true);
                    animator.SetBool("run", false);
                    animator.SetBool("idle", false);
                    animator.SetBool("fall", false);
                }
                break;
            case states.run:
                if (horizontal > 0.0f)
                {
                    if (stateInfo.shortNameHash == Animator.StringToHash("walk"))
                    {
                        animator.speed += 0.01f;
                    }
                    else
                    {
                        animator.speed = 1;
                    }
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    mirandoDer = true;
                    animator.SetBool("walk", false);
                    animator.SetBool("run", true);
                    animator.SetBool("idle", false);
                    animator.SetBool("fall", false);
                }

                if (horizontal < 0.0f)
                {
                    if (stateInfo.shortNameHash == Animator.StringToHash("walk"))
                    {
                        animator.speed += 0.01f;
                    }
                    else
                    {
                        animator.speed = 1;
                    }
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    mirandoDer = false;
                    animator.SetBool("walk", false);
                    animator.SetBool("run", true);
                    animator.SetBool("idle", false);
                    animator.SetBool("fall", false);
                }
                break;
            case states.air:
                if (rb2d.linearVelocity.y < 0 && !grounded)
                {
                    animator.SetBool("jump", false);
                    animator.SetBool("fall", true);
                }
                animator.SetBool("walk", false);
                animator.SetBool("idle", false);
                animator.SetBool("run", false);
                break;
                // case states.grabbing:
                //     animator.SetBool("walk", false);
                //     animator.SetBool("idle", false);
                //     animator.SetBool("run", false); 
                //     animator.SetBool("jump", false);
                //     animator.SetBool("fall", false);
                //     animator.SetBool("grabbing", true);
                //     break;
        }
    }

    private void BtnController()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            btnJump = true;
        else
            btnJump = false;

        if (Input.GetKey(KeyCode.LeftShift))
            btnRun = true;
        else
            btnRun = false;

        if (Input.GetKey(KeyCode.LeftAlt))
            btnGrab = true;
        else
            btnGrab = false;
    }

}
