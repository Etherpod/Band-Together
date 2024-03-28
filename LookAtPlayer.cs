using System;
using UnityEngine;

namespace BandTogether;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] CharacterDialogueTree characterDialogueTree;
    [SerializeField] Transform idleTarget;
    [SerializeField] Transform neckBone;
    [SerializeField] bool isGhird;

    private bool _lookAtPlayer = false;

    private void FixedUpdate()
    {
        var npcTransform = transform;
        
        var relativePlayerPosition = Locator._playerBody.GetPosition() - npcTransform.position;
        var playerInRange = relativePlayerPosition.sqrMagnitude < 16;
        if (!playerInRange)
        {
            _lookAtPlayer = false;
            return;
        }

        _lookAtPlayer = Vector3.Angle(npcTransform.forward, Vector3.ProjectOnPlane(relativePlayerPosition, npcTransform.up)) < 90;
    }

    private void Update()
    {
        // ModMain.Instance.ModHelper.Console.WriteLine("In convo: " + _CharacterDialogueTree);
        
        var targetPosition = _lookAtPlayer
            ? Locator.GetActiveCamera().transform.position
            : idleTarget.position;
        var targetLookDir = (targetPosition - neckBone.position).normalized;
        Vector3 currentLookDir = isGhird ? -neckBone.up : neckBone.up;
        
        if ((targetLookDir - currentLookDir).sqrMagnitude < 0.001) return;
        
        var nextLookForward = Vector3.Lerp(currentLookDir, targetLookDir, Time.deltaTime);

        if (isGhird)
        {
            var nextLookUp = Vector3.Cross(nextLookForward, neckBone.parent.forward);
            var nextLookLeft = Vector3.Cross(nextLookUp, nextLookForward);
            neckBone.LookAt(neckBone.position + nextLookLeft, -nextLookForward);
        }
        else
        {
            var nextLookUp = Vector3.Cross(nextLookForward, -neckBone.parent.right);
            // because of the orientation of the bones, we actually need to look at
            // the up vector with up being the direction we actually look
            neckBone.LookAt(neckBone.position + nextLookUp, nextLookForward);
        }
    }
}