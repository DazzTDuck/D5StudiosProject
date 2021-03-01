using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilityHandler : MonoBehaviour
{
    [Header("--Abilities--")]
    [SerializeField] GameObject ability1Button;
    [SerializeField] Image ability1TimerOverlay;
    [SerializeField] Animator ability1Animator;
    [SerializeField] bool ability1IsSwitch;
    [SerializeField] bool ability1IsHold;

    [Space, SerializeField] GameObject ability2Button;
    [SerializeField] Image ability2TimerOverlay;
    [SerializeField] Animator ability2Animator;
    [SerializeField] bool ability2IsSwitch;
    [SerializeField] bool ability2IsHold;

    [Header("--Ultimate--")]
    [SerializeField] Animator ultimateAnimator;
    [SerializeField] Image ultimateChargeBar;
    [SerializeField] GameObject ultimateIcon;
    [SerializeField] TMP_Text ultimateChargePercentageText;

    [Header("--Input Names--")]
    [SerializeField] string Ability1Input = "AbilityOne";
    [SerializeField] string Ability2Input = "AbilityTwo";
    [SerializeField] string UltimateInput = "Ultimate";

    [Header("Timers")]
    [SerializeField] Timer ability1Timer;
    [SerializeField] Timer ability2Timer;
    [SerializeField] Timer ultimateTimer;
    [SerializeField] Timer betweenAbilitesTimer;

    [Header("--Activatable--")]
    [SerializeField] bool abilityOneActivatable;
    [SerializeField] bool abilityTwoActivatable;
    [SerializeField] bool ultimateActivatable;

    [Header("--Actions--")]
    [SerializeField] UnityEvent ability1OnClick;
    [SerializeField] UnityEvent ability2OnClick;
    [SerializeField] UnityEvent ultimateOnClick;

    [Header("--Cooldowns--")]
    [SerializeField] float ablility1RechargeTime;
    [SerializeField] float ablility2RechargeTime;
    [SerializeField] float ultimateRechargeTime;
    [SerializeField] float ability1UseTime;
    [SerializeField] float ability2UseTime;
    [SerializeField] float ultimateUseTime;

    bool pressedAblility1, pressedAblility2, pressedUltimate;
    bool canActivateAblility1 = true, canActivateAblility2 = true, canActivateUltimate = false;

    bool AbilitiesActive = false;

    float timeBetweenAbilites = 0;
    bool abilityActivated = false;
    bool ultimateActivated = false;

    bool switchAbility1 = false;
    bool switchAbility2 = false;

    bool isStunned = false;

    private void Update()
    {
        GetAllInputs();

        if (AbilitiesActive && !isStunned)
        {
            if (abilityActivated)
            {
                if (betweenAbilitesTimer.IsTimerComplete())
                    betweenAbilitesTimer.SetTimer(timeBetweenAbilites, () => { abilityActivated = false; });
            }

            if (canActivateAblility1 && pressedAblility1 && !abilityActivated && abilityOneActivatable)
            {
                if (!ability1IsHold)
                {
                    abilityActivated = false;
                    timeBetweenAbilites = ability1UseTime;
                }
                
                if (ability1IsSwitch)
                {
                    switchAbility1 = !switchAbility1;
                }
                else
                {
                    ability1Timer.SetTimer(ablility1RechargeTime,
                    () => { canActivateAblility1 = true; ability1TimerOverlay.gameObject.SetActive(false); },
                    () => { canActivateAblility1 = false; ability1TimerOverlay.gameObject.SetActive(true); });
                }

                if (switchAbility1 && ability1IsSwitch)
                {
                    ability1Animator.ResetTrigger("Switch");
                    ability1Animator.SetTrigger("Switch");
                }
                else if (!switchAbility1 && ability1IsSwitch)
                {
                    ability1Animator.ResetTrigger("SwitchBack");
                    ability1Animator.SetTrigger("SwitchBack");
                }
                else
                {
                    ability1Animator.ResetTrigger("Press");
                    ability1Animator.SetTrigger("Press");
                }

                ability1OnClick.Invoke();
            }

            if (canActivateAblility2 && pressedAblility2 && !abilityActivated && abilityTwoActivatable)
            { 
                if (!ability2IsHold)
                {
                    abilityActivated = false;
                    timeBetweenAbilites = ability2UseTime;
                }

                if (ability2IsSwitch)
                {
                    switchAbility2 = !switchAbility2;
                }
                else
                {                   
                    ability2Timer.SetTimer(ablility2RechargeTime,
                        () => { canActivateAblility2 = true; ability2TimerOverlay.gameObject.SetActive(false); },
                        () => { canActivateAblility2 = false; ability2TimerOverlay.gameObject.SetActive(true); });
                }

                if (switchAbility2 && ability2IsSwitch)
                {
                    ability2Animator.ResetTrigger("Switch");
                    ability2Animator.SetTrigger("Switch");
                }
                else if(!switchAbility2 && ability2IsSwitch)
                {
                    ability2Animator.ResetTrigger("SwitchBack");
                    ability2Animator.SetTrigger("SwitchBack");
                }
                else
                {
                    ability2Animator.ResetTrigger("Press");
                    ability2Animator.SetTrigger("Press");
                }

                ability2OnClick.Invoke();
            }

            if (canActivateUltimate && pressedUltimate && !abilityActivated && ultimateActivatable)
            {
                timeBetweenAbilites = ultimateUseTime;
                abilityActivated = true;
                ultimateOnClick.Invoke();
                StartUlimateTimer();
                ultimateIcon.SetActive(false);
                ultimateAnimator.ResetTrigger("Charged");
                ultimateAnimator.SetTrigger("Back");
            }

            //update visual timers
            if (!canActivateAblility1) { ability1TimerOverlay.fillAmount = GetPercentage(ablility1RechargeTime, ability1Timer.GetTimerValue()) / 100; }
            if (!canActivateAblility2) { ability2TimerOverlay.fillAmount = GetPercentage(ablility2RechargeTime, ability2Timer.GetTimerValue()) / 100; }
            if (!canActivateUltimate)
            {
                float percent = 100 - GetPercentage(ultimateRechargeTime, ultimateTimer.GetTimerValue());
                ultimateChargeBar.fillAmount = percent / 100;
                ultimateChargePercentageText.text = $"{percent:0}";
            }
        }
    }

    public void StartUlimateTimer()
    {
        ultimateTimer.SetTimer(ultimateRechargeTime,
        () => { canActivateUltimate = true; ultimateIcon.SetActive(true); ultimateAnimator.ResetTrigger("Back"); ultimateAnimator.SetTrigger("Charged"); },
        () => { canActivateUltimate = false; });
    }

    public void PlayerStunned(float time)
    {
        if (!isStunned)
        {        
            betweenAbilitesTimer.SetTimer(time, () => { isStunned = false; });
            isStunned = true;
        }
    }

    public void GetAllInputs()
    {
        if (ability1IsHold)
        {
            if(Input.GetButtonDown(Ability1Input)) { pressedAblility1 = true; } else if(!Input.GetButtonUp(Ability1Input)) { pressedAblility1 = false; }
            if(Input.GetButtonUp(Ability1Input)) { pressedAblility1 = true; } else if (!Input.GetButtonDown(Ability1Input)) { pressedAblility1 = false; }
        }
        else { pressedAblility1 = Input.GetButtonDown(Ability1Input); }

        if (ability2IsHold)
        {
            if (Input.GetButtonDown(Ability2Input)) { pressedAblility2 = true; } else if (!Input.GetButtonUp(Ability2Input)) { pressedAblility2 = false; }
            if (Input.GetButtonUp(Ability2Input)) { pressedAblility2 = true; } else if (!Input.GetButtonDown(Ability2Input)) { pressedAblility2= false; }
        }
        else { pressedAblility2 = Input.GetButtonDown(Ability2Input); }

        pressedUltimate = Input.GetButtonDown(UltimateInput);
    }

    public float GetPercentage(float maxValue, float currentValue)
    {
        return 100f / maxValue * currentValue;
    }

    public void ActivateAbilities()
    {
        ability1Timer.gameObject.SetActive(true);
        ability2Timer.gameObject.SetActive(true);
        ultimateTimer.gameObject.SetActive(true);
        betweenAbilitesTimer.gameObject.SetActive(true);
        AbilitiesActive = true;
        if (!ultimateActivated)
        {
            StartUlimateTimer();
            ultimateActivated = true;
        }
        if (!abilityOneActivatable) { ability1TimerOverlay.gameObject.SetActive(true); }
        if (!abilityTwoActivatable) { ability2TimerOverlay.gameObject.SetActive(true); }
    }

    public void ResetTimers()
    {
        ability1Timer.ResetTimer();
        ability2Timer.ResetTimer();
    }

    public void ResetAbility2Timer()
    {
        ability2Timer.ResetTimer();
    }
}
