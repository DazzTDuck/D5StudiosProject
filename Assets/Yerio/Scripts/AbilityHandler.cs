using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilityHandler : MonoBehaviour
{
    [Header("--Abilities--")]
    [SerializeField] GameObject ability1Button;
    [SerializeField] Image ability1TimerOverlay;
    [SerializeField] GameObject ability2Button;
    [SerializeField] Image ability2TimerOverlay;
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
                timeBetweenAbilites = ability1UseTime;
                abilityActivated = true;
                ability1OnClick.Invoke();
                ability1Timer.SetTimer(ablility1RechargeTime,
                    () => { canActivateAblility1 = true; ability1TimerOverlay.gameObject.SetActive(false); },
                    () => { canActivateAblility1 = false; ability1TimerOverlay.gameObject.SetActive(true); });
            }

            if (canActivateAblility2 && pressedAblility2 && !abilityActivated && abilityTwoActivatable)
            {
                timeBetweenAbilites = ability2UseTime;
                abilityActivated = true;
                ability2OnClick.Invoke();
                ability2Timer.SetTimer(ablility2RechargeTime,
                    () => { canActivateAblility2 = true; ability2TimerOverlay.gameObject.SetActive(false); },
                    () => { canActivateAblility2 = false; ability2TimerOverlay.gameObject.SetActive(true); });
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
