using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ForceTurnButton : MonoBehaviour
{
    [SerializeField] private Button forceButton;
    [SerializeField] private float activationDelay = 1f;

    private TurnManager currentTM;
    private Coroutine activationCoroutine;

    public void Initialize(TurnManager tm)
    {
        // Cleanup previous subscription
        if (currentTM != null)
        {
            currentTM.OnTurnSwitched -= HandleTurnSwitched;
        }

        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }

        currentTM = tm;

        if (forceButton != null)
        {
            forceButton.onClick.RemoveAllListeners();
            forceButton.onClick.AddListener(OnButtonClick);
            forceButton.interactable = false;
        }

        if (currentTM != null)
        {
            currentTM.OnTurnSwitched += HandleTurnSwitched;
            // Trigger initial delay if a turn is already active
            HandleTurnSwitched();
        }
    }

    private void HandleTurnSwitched()
    {
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
        }

        if (forceButton != null)
        {
            forceButton.interactable = false;
            activationCoroutine = StartCoroutine(ActivateAfterDelay());
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        if (forceButton != null)
        {
            forceButton.interactable = true;
        }
        activationCoroutine = null;
    }

    private void OnButtonClick()
    {
        if (currentTM != null)
        {
            if (forceButton != null) forceButton.interactable = false;
            currentTM.ForceNextTurn();
        }
    }

    private void OnDestroy()
    {
        if (currentTM != null)
        {
            currentTM.OnTurnSwitched -= HandleTurnSwitched;
        }
    }
}
