using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject phoneObject;
    [SerializeField] LayerMask enrageMask;
    [SerializeField] float enrageCastRadius = 1f;

    [SerializeField] LayerMask saveMask;

    bool isSavedPressed = false;

    public bool IsPhoneOut
    {
        get
        {
            return phoneObject.activeInHierarchy;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if(IsPhoneOut && Physics.SphereCast(phoneObject.transform.position, enrageCastRadius, phoneObject.transform.forward, out RaycastHit hitInfo, 25f, enrageMask))
        {
            TheLostAIController lostController = hitInfo.rigidbody?.GetComponent<TheLostAIController>();
            if(lostController != null)
            {
                lostController.Enrage(Time.fixedDeltaTime);
            }
        }

        if (isSavedPressed && Physics.SphereCast(transform.position, enrageCastRadius, transform.forward, out RaycastHit hitInfo2, 25f, saveMask))
        {
            TheLostAIController lostController = hitInfo2.rigidbody?.GetComponent<TheLostAIController>();
            if (lostController != null)
            {
                lostController.Save(Time.fixedDeltaTime);
            }
        }
    }

    void OnToggleCamera()
    {
        phoneObject.SetActive(!phoneObject.activeInHierarchy);
    }

    public void OnReset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnSave(InputValue value)
    {
        isSavedPressed = value.Get<float>() > 0;
    }

    void PlayerDied()
    {
        OnReset();
    }
}
