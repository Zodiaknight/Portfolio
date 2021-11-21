using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public enum AI_STATE
{
    IDLE,
    CHASING,
    ATTACKING,
}

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    [Header("Settings")]
    public AI_STATE state = AI_STATE.IDLE;
    public float AggroRange = 11;
    public float AttackRange = 5;
    public float Damage = 5;
    public AnimationCurve JumpCurve;

    public PlayerController Target;
    private NavMeshAgent _agent;

    public AudioSource AudioSources;
    public AudioClip[] Sounds;

    private float _timer = 5;
    private float _walkTime = 5;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        AudioSources = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, Target.transform.position) <= AggroRange && !Target.isDead)
        {
            _agent.SetDestination(Target.transform.position);

            if (_agent.hasPath)
            {
                if (_agent.remainingDistance <= AttackRange)
                {
                    state = AI_STATE.ATTACKING;
                }

                else
                {
                    state = AI_STATE.CHASING;
                }
            }
        }

        else
        {
            state = AI_STATE.IDLE;
        }

        switch (state)
        {
            case AI_STATE.ATTACKING:
                Target.GetDamage(Damage * Time.deltaTime);
                PlayAttackSound();
                break;
        }

        if (_agent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(.5f));
            _agent.CompleteOffMeshLink();
        }
    }

    private void PlayAttackSound()
    {
        AudioSources.clip = Sounds[2];
        AudioSources.Play();
    }

    private IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 destination = data.endPos;

        float time = 0;
        while (time < 1)
        {
            float y = JumpCurve.Evaluate(time);
            _agent.transform.position = Vector3.Lerp(start, destination, time) + y * Vector3.up;
            time += Time.deltaTime / duration;
            yield return null;
        }
    }
}
