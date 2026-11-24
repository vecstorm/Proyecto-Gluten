using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private string idleTriggerName = "Idle";
    [SerializeField] private string walkTriggerName = "Walk";
    [SerializeField] private string runTriggerName = "Run";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string rammingTriggerName = "Ramming";
    [SerializeField] private string stunTriggerName = "Stun";
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

    private void DeathTrigger()
    {
        ResetAllTriggers();
        animator.SetTrigger(deathTriggerName);
    }

    private void ResetAllTriggers()
    {
        animator.ResetTrigger(idleTriggerName);
        animator.ResetTrigger(walkTriggerName);
        animator.ResetTrigger(runTriggerName);
        animator.ResetTrigger(attackTriggerName);
        animator.ResetTrigger(stunTriggerName);
        animator.ResetTrigger(deathTriggerName);
    }
}
