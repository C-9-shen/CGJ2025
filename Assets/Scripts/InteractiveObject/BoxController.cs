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
    public UnityEvent StepOn;
    public UnityEvent TouchSide;
    public UnityEvent OnLeaveTop;
    public BoxState currentState = BoxState.Movable; 
    public float riseHeight; 
    public float riseDuration; 
    public GameObject spikesPrefab; 
    public Vector3 spikesSpawnPoint;
    private GameObject spikesInstance;
    private Rigidbody2D rb; 
    public bool isRising = false;
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
                transform.position = new Vector3(transform.position.x, InitialPosition.y + riseHeight * fractionComplete, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, InitialPosition.y + riseHeight, transform.position.z);
                isRising = false;
            }
            Debug.Log(transform.position);
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
        BoxUp?.Invoke();
        HasBeenHold = true;
        ifCanBeHold = true;
        GetComponent<Rigidbody2D>().simulated = false; // ���ø����Է�ֹ��������
        transform.SetParent(Player.transform);
        transform.position = new Vector3(Player.transform.position.x+0.5f, Player.transform.position.y, Player.transform.position.z);

        Debug.Log("箱子�?拾起");
    }
    void PutDownBox() 
    {
        BoxDown?.Invoke();
        HasBeenHold = false;
        ifCanBeHold = false;
        transform.SetParent(null);
        GetComponent<Rigidbody2D>().simulated = true; // ������������ģ��
        transform.position = new Vector3(Player.transform.position.x + 0.5f, Player.transform.position.y, Player.transform.position.z);
        Debug.Log("箱子�?放下");
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
                    Debug.Log("玩�?�踩到�?�子");
                    StepOn?.Invoke();
                    collision.gameObject.GetComponent<MainCharacter>().isGrounded = true;
                    switch (currentState)
                    {
                        case BoxState.Movable:
                            break;
                        case BoxState.SpikesOnStep:
                            if (!hasSpikes)
                            {
                                spikesInstance = Instantiate(spikesPrefab, transform);
                                spikesInstance.transform.localPosition = new Vector3(0, 1, 0);
                                spikesInstance.transform.localScale = Vector3.one;
                                hasSpikes = true;
                            }
                            break;
                        case BoxState.RiseOnStep:
                            if (!isRising)
                            {
                                collision.transform.SetParent(transform);
                                riseStartTime = Time.time;
                                isRising = true;
                            }
                            break;
                    }
                    return;
                }
                else if (Mathf.Abs(normal.y) < sideThreshold)
                {
                    TouchSide?.Invoke();
                    Debug.Log("玩�?�接触�?�子侧面");
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
            if (currentState == BoxState.RiseOnStep&&isRising==false) {collision.transform.SetParent(null); }
            if (!HasBeenHold)
            {
                ifCanBeHold = false;
                Debug.Log("玩�?��?�开箱子，无法再拾取");
            }
            
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3 normal = contact.normal;
                if (normal.y < -topThreshold)
                {
                    Debug.Log("离开箱子顶部");
                    OnLeaveTop?.Invoke();
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