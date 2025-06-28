using UnityEngine;
using System.Collections;
using UnityEngine.Events;
public enum BoxState
{
    Movable, 
    SpikesOnStep, 
    RiseOnStep 
}

public class BoxController : MonoBehaviour
{
    public UnityEvent BoxUp;
    public UnityEvent BoxDown;
    public BoxState currentState = BoxState.Movable; 
    public float riseHeight; 
    public float riseDuration; 
    public GameObject spikesPrefab; 
    public Vector3 spikesSpawnPoint;
    private GameObject spikesInstance;
    private Rigidbody2D rb; 
    private bool isRising = false;
    //private bool isFalling = false;
    private float riseStartTime;
    //private float fallStartTime;
    private bool hasSpikes = false;
    public float topThreshold = 0.7f;
    public float sideThreshold = 0.5f;
    public bool ifCanBeHold=false;
    public bool HasBeenHold=false;
    public float delayBeforeReset = 2f;
    public Vector3 InitialPosition;
    public GameObject Player;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        InitialPosition = transform.position;
        spikesSpawnPoint = new Vector3(InitialPosition.x,InitialPosition.y+0.5f,InitialPosition.z);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == BoxState.Movable && ifCanBeHold)
            {
                if (!HasBeenHold) { PickUpBox(); }
                else { PutDownBox(); }
            }
            
        }
        if (currentState == BoxState.RiseOnStep && isRising)
        {
            float elapsedTime = Time.time - riseStartTime;
            if (elapsedTime < riseDuration)
            {
                float fractionComplete = elapsedTime / riseDuration;
                transform.position = new Vector3(transform.position.x, InitialPosition.y + riseHeight*fractionComplete, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, InitialPosition.y + riseHeight, transform.position.z);
                isRising = false;
            }
        }
        
        /*if (currentState == BoxState.RiseOnStep && isFalling)
        {
            float elapsedTime = Time.time - fallStartTime;
            if (elapsedTime < riseDuration)
            {
                float fractionComplete = elapsedTime / riseDuration;
                transform.position = new Vector3(transform.position.x, InitialPosition.y - riseHeight * (1-fractionComplete), transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, InitialPosition.y, transform.position.z);
                isFalling = false;
            }
        }*/
    }
    void PickUpBox() 
    {
        BoxUp.Invoke();
        HasBeenHold = true;
        transform.SetParent(Player.transform);
        transform.position = new Vector3(Player.transform.position.x+0.5f, Player.transform.position.y, Player.transform.position.z);
    }
    void PutDownBox() 
    {
        BoxDown.Invoke();
        ifCanBeHold = false;
        HasBeenHold = false;
        transform.SetParent(null);
        transform.position = new Vector3(Player.transform.position.x + 0.5f, Player.transform.position.y, Player.transform.position.z);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3 normal = contact.normal;
                if (normal.y < -topThreshold)
                {
                    Debug.Log("踩在箱子顶部");
                    collision.gameObject.GetComponent<MainCharacter>().isGrounded = true;
                    switch (currentState)
                    {
                        case BoxState.Movable:
                            break;
                        case BoxState.SpikesOnStep:
                            if (!hasSpikes)
                            {
                                spikesSpawnPoint = new Vector3(transform.position.x,transform.position.y+0.5f,transform.position.z);
                                spikesInstance = Instantiate(spikesPrefab, spikesSpawnPoint,transform.rotation);
                                hasSpikes = true;
                            }
                            break;
                        case BoxState.RiseOnStep:
                            if (!isRising)
                            {
                                riseStartTime = Time.time;
                                isRising = true;
                            }
                            break;
                    }
                    return;
                }
                else if (Mathf.Abs(normal.y) < sideThreshold)
                {
                    Debug.Log("撞到箱子侧面");
                    ifCanBeHold=true;
                    return;
                }
            }
            
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3 normal = contact.normal;
                if (normal.y < -topThreshold)
                {
                    Debug.Log("离开箱子顶部");
                    collision.gameObject.GetComponent<MainCharacter>().isGrounded = false;
                    switch (currentState)
                    {
                        case BoxState.Movable:
                            break;
                        case BoxState.SpikesOnStep:
                            if (hasSpikes)
                            {
                                //StartCoroutine(ResetBoxStateAfterDelay2());
                            }
                            break;
                        case BoxState.RiseOnStep:
                            if (!isRising)
                            {
                                //StartCoroutine(ResetBoxStateAfterDelay3());
                            }
                            break;
                    }
                    return;
                }
            }
        }
    }
    public void SetState(BoxState state)
    {
        if(currentState == BoxState.SpikesOnStep) 
        {
            if (hasSpikes) 
            {
                Destroy(spikesInstance);
                hasSpikes = false;
            }
        }
        currentState = state;
    }
    /*private IEnumerator ResetBoxStateAfterDelay2()
    {
        yield return new WaitForSeconds(delayBeforeReset);
        ResetBoxState2();
    }
    private IEnumerator ResetBoxStateAfterDelay3()
    {
        yield return new WaitForSeconds(delayBeforeReset);
        ResetBoxState3();
    }
    private void ResetBoxState2()
    {
        if (spikesInstance != null)
        {
            Destroy(spikesInstance);
            hasSpikes = false;
        }
    }
    private void ResetBoxState3()
    {
        isFalling = true;
    }*/
}