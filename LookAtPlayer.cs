using UnityEngine;

namespace BandTogether;
public class LookAtPlayer : MonoBehaviour
{
    [SerializeField]
    private CharacterDialogueTree _CharacterDialogueTree;
    [SerializeField]
    private GameObject _idletarget;
    [SerializeField]
    private Transform head;

    bool startedConvo = false;

    private void Update()
    {
        //ModMain.Instance.ModHelper.Console.WriteLine("Up: " + head.up);
        //ModMain.Instance.ModHelper.Console.WriteLine("Forward: " + head.forward);
        Vector3 currentLookDir = Vector3.zero;
        Vector3 targetLookDir;
        if (_CharacterDialogueTree.InConversation())
        {
            ModMain.Instance.ModHelper.Console.WriteLine("In convo: " + _CharacterDialogueTree);
            if (!startedConvo)
            {
                currentLookDir = -head.transform.parent.forward;
                startedConvo = true;
            }

            targetLookDir = Locator.GetActiveCamera().transform.position;
            currentLookDir = Vector3.Lerp(currentLookDir, targetLookDir, Time.deltaTime);
        }
        else
        {
            targetLookDir = -head.transform.parent.forward;
            currentLookDir = Vector3.Lerp(currentLookDir, targetLookDir, Time.deltaTime);
            startedConvo = false;
        }

        head.forward = Vector3.up;
    }
}