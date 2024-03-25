using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BandTogether;

[ExecuteInEditMode]
public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] CharacterDialogueTree characterDialogueTree;
    [SerializeField] Transform idleTarget;
    [SerializeField] Transform neckBone;
    [SerializeField] bool isGhird;

    private void Update()
    {
        // ModMain.Instance.ModHelper.Console.WriteLine("In convo: " + _CharacterDialogueTree);
        
        var targetPosition = characterDialogueTree.InConversation()
            ? Locator.GetActiveCamera().transform.position
            : idleTarget.position;
        var targetLookDir = (targetPosition - neckBone.position).normalized;
        Vector3 currentLookDir = neckBone.up;
        //for ditzy: Invert the look direction if the isGhird bool is active
        
        if ((targetLookDir - currentLookDir).sqrMagnitude < 0.001) return;
        
        var nextLookForward = Vector3.Lerp(currentLookDir, targetLookDir, Time.deltaTime);
        var nextLookUp = Vector3.Cross(nextLookForward, -neckBone.parent.right);
        
        // because of the orientation of the bones, we actually need to look at
        // the up vector with up being the direction we actually look
        neckBone.LookAt(neckBone.position + nextLookUp, nextLookForward);
    }
}