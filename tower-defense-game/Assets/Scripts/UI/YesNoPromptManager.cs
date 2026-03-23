using UnityEngine;
using TMPro;

public class YesNoPromptManager : MonoBehaviour
{
    [SerializeField] private GameObject yesNoUI;
    [SerializeField] private TextMeshProUGUI promptText;
    private YesNoInteractor interactor;

    /*
    Enables the entire Yes/No UI
    */
    public void EnableUI() {
        yesNoUI.SetActive(true);
    }

    /*
    Disables the entire Yes/No UI
    */
    public void DisableUI() {
        yesNoUI.SetActive(false);
    }

    /*
    Sets the object that wants to listen to the prompt input
    */
    public void SetInteractor(YesNoInteractor interactor){
        this.interactor = interactor;
    }

    /*
    Gives the object responsible for the prompt text
    */
    public TextMeshProUGUI GetPromptText() {
        return promptText;
    }

    /*
    Used by the buttons to notify the interactor on which
    prompt has been pressed
    isYes is true for the yes prompt being pressed, and flase for 
    the no prompt being pressed
    */
    public void NotifyInteractor(bool isYes) {
        if(interactor != null) {
            if(isYes) {
                interactor.YesResponse();
            } else {
                interactor.NoResponse();
            }
        }
    }
}
