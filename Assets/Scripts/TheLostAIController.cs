using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class TheLostAIController : MonoBehaviour
{
    public enum TheLostType
    {
        Default,
        Hallucination,
        ReverseHallucination
    }

    public float MaxRoamDistance;
    public LayerMask areaMask;
    public float minIdleTime = 4f;
    public float maxIdleTime = 6f;
    public float enragedSpeed;

    public float enrageTime = 3f;
    public float saveTime = 3f;
    public GameObject savingEffect;

    float idleTimer = 0f;

    NavMeshAgent navAgent;
    Animator animator;
    TheLostType type;

    bool enragedThisFrame = false;
    bool isEnraged = false;
    float enrageMeter;
    float normalSpeed;
    GameObject playerObject;
    PlayerController playerController;

    bool saveThisFrame = false;
    float saveMeter = 0f;

    int state = 0; // 0 = Idle, 1 = walk, 2 = enraged, 3 = saving, 4 = saved
    int State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            if (navAgent == null) return;

            navAgent.speed = normalSpeed;
            switch (state)
            {
                case 0:
                    animator.SetTrigger("Idle");
                    savingEffect.SetActive(false);
                    saveMeter = 0;
                    break;
                case 1:
                    animator.SetTrigger("Walk");
                    break;
                case 2:
                    animator.SetTrigger("Enraged");
                    navAgent.speed = enragedSpeed;
                    break;
                case 3:
                    savingEffect.SetActive(true);
                    animator.SetTrigger("Saving");
                    navAgent.isStopped = true;
                    break;
                case 4:
                    Destroy(this.gameObject);
                    break;
            }
        }
    }



    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        normalSpeed = navAgent.speed;

        if (gameObject.layer == LayerMask.NameToLayer("Hallucination"))
        {
            type = TheLostType.Hallucination;
        }
        else if (gameObject.layer == LayerMask.NameToLayer("ReverseHallucination"))
        {
            type = TheLostType.ReverseHallucination;
        }
        else
        {
            type = TheLostType.Default;
        }
        idleTimer = GetIdleTime();

        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObject.GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (enragedThisFrame)
        {
            enragedThisFrame = false;
        }
        else if (enrageMeter > 0)
        {
            enrageMeter -= Time.deltaTime;
            if (enrageMeter < 0) enrageMeter = 0;
        }



        switch (State)
        {
            case 0:
                Idle();
                break;

            case 1:
                if (!navAgent.pathPending)
                {
                    if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                    {
                        if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                        {
                            State = 0;
                        }
                    }
                }
                break;
            case 2:
                if (Vector3.Distance(playerObject.transform.position, navAgent.pathEndPosition) > 1f)
                {
                    navAgent.destination = playerObject.transform.position;
                }
                if (!playerController.IsPhoneOut)
                {
                    State = 0;
                }
                break;
            case 3:
                if (saveThisFrame)
                {
                    saveThisFrame = false;
                }
                else
                {
                    State = 0;
                }
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (State == 2)
        {
            playerController.OnReset();
        }
    }

    float GetIdleTime()
    {
        return Random.Range(minIdleTime, maxIdleTime);
    }

    bool WalkToRandomPosition()
    {
        Vector3 samplePos = transform.position + Random.insideUnitSphere * MaxRoamDistance;
        if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, MaxRoamDistance, areaMask))
        {

            navAgent.destination = hit.position;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Enrage(float amount)
    {
        if (type == TheLostType.ReverseHallucination)
        {
            enrageMeter += amount;
            enragedThisFrame = true;
            if (enrageMeter > enrageTime)
            {
                enrageMeter = enrageTime;
                isEnraged = true;
                State = 2;
            }
        }
    }

    public void Save(float amount)
    {
        if (type == TheLostType.ReverseHallucination) return;

        saveMeter += amount;
        if (saveMeter > saveTime)
        {
            if (type == TheLostType.Hallucination)
                State = 4;
            else
            {
                playerController.OnReset();
            }
            return;
        }

        if (State != 3)
        {
            State = 3;
        }
        saveThisFrame = true;
    }

    void Idle()
    {
        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
        }
        else if (WalkToRandomPosition())
        {
            idleTimer = GetIdleTime();

            State = 1;
        }
    }
}
