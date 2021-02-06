using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class AbilityHandler : MonoBehaviour
{
    [Header("--Buttons--")]
    [SerializeField] GameObject ability1Button;
    [SerializeField] Image ability1TimerOverlay;
    [SerializeField] GameObject ability2Button;
    [SerializeField] Image ability2TimerOverlay;
    [SerializeField] GameObject UltimateButton;
    [SerializeField] Image ultimateChargeBar;
    [SerializeField] TMP_Text ultimateChargePercentageText;

    [Header("Timers")]
    [SerializeField] Timer ability1Timer;
    [SerializeField] Timer ability2Timer;
    [SerializeField] Timer ultimateTimer;
    [SerializeField] Timer BetweenAbilitesTimer;

    [Header("Actions")]
    [SerializeField] UnityEvent Ability1OnClick;
    [SerializeField] UnityEvent Ability2OnClick;
    [SerializeField] UnityEvent UltimateOnClick;

    [Header("Cooldowns")]
    [SerializeField] float ablility1RechargeTime;
    [SerializeField] float ablility2RechargeTime;
    [SerializeField] float ultimateRechargeTime;

    bool pressedAblility1, pressedAblility2, pressedUltimate;
    bool canActivateAblility1 = true, canActivateAblility2 = true, canActivateUltimate = false;

    bool AbilitiesActive = false;

    float timeBetweenAbilites = 2f;
    bool abilityActivated = false;

    float fontSize;

    private void Update()
    {
        GetAllInputs();

        if (AbilitiesActive)
        {
            if (abilityActivated)
            {
                if (BetweenAbilitesTimer.IsTimerComplete())
                    BetweenAbilitesTimer.SetTimer(timeBetweenAbilites, () => { abilityActivated = false; });
            }

            if (canActivateAblility1 && pressedAblility1 && !abilityActivated)
            {
                abilityActivated = true;
                Ability1OnClick.Invoke();
                ability1Timer.SetTimer(ablility1RechargeTime,
                    () => { canActivateAblility1 = true; ability1TimerOverlay.gameObject.SetActive(false); },
                    () => { canActivateAblility1 = false; ability1TimerOverlay.gameObject.SetActive(true); });
            }

            if (canActivateAblility2 && pressedAblility2 && !abilityActivated)
            {
                abilityActivated = true;
                Ability2OnClick.Invoke();
                ability2Timer.SetTimer(ablility2RechargeTime,
                    () => { canActivateAblility2 = true; ability2TimerOverlay.gameObject.SetActive(false); },
                    () => { canActivateAblility2 = false; ability2TimerOverlay.gameObject.SetActive(true); });
            }

            if (canActivateUltimate && pressedUltimate && !abilityActivated)
            {
                abilityActivated = true;
                UltimateOnClick.Invoke();
                StartUlimateTimer();
            }

            //update visual timers
            if (!canActivateAblility1) { ability1TimerOverlay.fillAmount =  GetPercentage(ablility1RechargeTime, ability1Timer.GetTimerValue()) / 100; }
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
            () => { canActivateUltimate = true; },
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
        AbilitiesActive = true;
        StartUlimateTimer();
    }

}
