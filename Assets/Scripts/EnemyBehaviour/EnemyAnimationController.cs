using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private string idleTriggerName = "Idle";
    [SerializeField] private string walkTriggerName = "Walk";
    [SerializeField] private string runTriggerName = "Run";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string rammingTriggerName = "Ramming";
    [SerializeField] private string stunTriggerName = "Stun";
    [SerializeField] private string retreatTriggerName = "Retreat";
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private bool controller = false;

    private Animator animator;
    private TroopAI troopAI;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        troopAI = GetComponent<TroopAI>();
    }

    private void OnEnable()
    {
        troopAI.IdleAnimation += IdleTrigger;
        troopAI.PatrolAnimation += WalkTrigger;
        troopAI.ChasingAnimation += RunTrigger;
        troopAI.AttackAnimation += AttackTrigger;
        troopAI.RammingAnimation += RammingTrigger;
        troopAI.StunnedAnimation += StunTrigger;
        troopAI.RetreatAnimation += RetreatTrigger;
        troopAI.DeadAnimation += DeathTrigger;
    }

    private void OnDisable()
    {
        troopAI.IdleAnimation -= IdleTrigger;
        troopAI.PatrolAnimation -= WalkTrigger;
        troopAI.ChasingAnimation -= RunTrigger;
        troopAI.AttackAnimation -= AttackTrigger;
        troopAI.RammingAnimation -= RammingTrigger;
        troopAI.StunnedAnimation -= StunTrigger;
        troopAI.RetreatAnimation -= RetreatTrigger;
        troopAI.DeadAnimation -= DeathTrigger;
    }

    private void IdleTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(idleTriggerName);
    }

    private void WalkTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(walkTriggerName);
    }

    private void RunTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(runTriggerName);
    }

    private void AttackTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(attackTriggerName);
    }

    private void RammingTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(rammingTriggerName);
    }

    private void StunTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(stunTriggerName);
    }

    private void RetreatTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(retreatTriggerName);
    }

    private void DeathTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(deathTriggerName);
    }

    private void ResetAllTriggers()
    {
        if (HasTrigger(idleTriggerName)) animator.ResetTrigger(idleTriggerName);
        if (HasTrigger(walkTriggerName)) animator.ResetTrigger(walkTriggerName);
        if (HasTrigger(runTriggerName)) animator.ResetTrigger(runTriggerName);
        if (HasTrigger(attackTriggerName)) animator.ResetTrigger(attackTriggerName);
        if (HasTrigger(stunTriggerName)) animator.ResetTrigger(stunTriggerName);
        if (HasTrigger(retreatTriggerName)) animator.ResetTrigger(retreatTriggerName);
        if (HasTrigger(deathTriggerName)) animator.ResetTrigger(deathTriggerName);
    }

    private bool HasTrigger(string triggerName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
            {
                return true;
            }
        }

        return false;
    }

    public void OnPatrolStep()
    {
        AudioController.Instance.PlayPatrolEnemy(PatrolEnemySFX.Footstep);
    }

    public void OnMeleeStep()
    {
        AudioController.Instance.PlayMeleeEnemy(MeleeEnemySFX.Footstep);
    }

    public void OnFlyingFlap()
    {
        AudioController.Instance.PlayFlyingEnemy(FlyingEnemySFX.Flapping);
    }
}
