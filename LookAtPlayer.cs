using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BandTogether;

[ExecuteInEditMode]
public class LookAtPlayer : MonoBehaviour
{
    [SerializeField]
    private CharacterDialogueTree characterDialogueTree;
    
    [SerializeField]
    private Transform idleTarget;
    
    [SerializeField]
    private Transform neckBone;

    private void Update()
    {
        // ModMain.Instance.ModHelper.Console.WriteLine("In convo: " + _CharacterDialogueTree);
        
        var targetPosition = characterDialogueTree.InConversation()
            ? Locator.GetActiveCamera().transform.position
            : idleTarget.position;
        var targetLookDir = (targetPosition - neckBone.position).normalized;
        var currentLookDir = neckBone.up;
        
        if ((targetLookDir - currentLookDir).sqrMagnitude < 0.001) return;
        
        var nextLookForward = Vector3.Lerp(currentLookDir, targetLookDir, Time.deltaTime);
        var nextLookUp = Vector3.Cross(nextLookForward, -neckBone.parent.right);
        
        // because of the orientation of the bones, we actually need to look at
        // the up vector with up being the direction we actually look
        neckBone.LookAt(neckBone.position + nextLookUp, nextLookForward);
    }
}