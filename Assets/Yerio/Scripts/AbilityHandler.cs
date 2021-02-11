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

    [Space, SerializeField] GameObject ability2Button;
    [SerializeField] Image ability2TimerOverlay;
    [SerializeField] Animator ability2Animator;
    [SerializeField] bool ability2IsSwitch;

    [Header("--Ultimate--")]
    [SerializeField] Animator ultimateAnimator;
    [SerializeField] Image ultimateChargeBar;
    [SerializeField] GameObject ultimateIcon;
    [SerializeField] TMP_Text ultimateChargePercentageText;

    [Header("Timers")]
    [SerializeField] Timer ability1Timer;
    [SerializeField] Timer ability2Timer;
    [SerializeField] Timer ultimateTimer;
    [SerializeField] Timer betweenAbilitesTimer;

    [Header("Activatable")]
    [SerializeField] bool abilityOneActivatable;
    [SerializeField] bool abilityTwoActivatable;
    [SerializeField] bool ultimateActivatable;

    [Header("Actions")]
    [SerializeField] UnityEvent ability1OnClick;
    [SerializeField] UnityEvent ability2OnClick;
    [SerializeField] UnityEvent ultimateOnClick;

    [Header("Cooldowns")]
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

    private void Update()
    {
        GetAllInputs();

        if (AbilitiesActive)
        {
            if (abilityActivated)
            {
                if (betweenAbilitesTimer.IsTimerComplete())
                    betweenAbilitesTimer.SetTimer(timeBetweenAbilites, () => { abilityActivated = false; });
            }

            if (canActivateAblility1 && pressedAblility1 && !abilityActivated && abilityOneActivatable)
            {
                ability1OnClick.Invoke();
                abilityActivated = true;
                timeBetweenAbilites = ability1UseTime;

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

            }

            if (canActivateAblility2 && pressedAblility2 && !abilityActivated && abilityTwoActivatable)
            {
                ability2OnClick.Invoke();
                abilityActivated = true;
                timeBetweenAbilites = ability2UseTime;

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

    public void GetAllInputs()
    {
        pressedAblility1 = Input.GetButtonDown("AbilityOne");
        pressedAblility2 = Input.GetButtonDown("AbilityTwo");
        pressedUltimate = Input.GetButtonDown("Ultimate");
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
}
