using UnityEngine;

namespace BandTogether;
public class LookAtPlayer : MonoBehaviour
{
    [SerializeField]
    private CharacterDialogueTree _CharacterDialogueTree;

    [SerializeField]
    private Animator _Animator;

    [SerializeField]
    private GameObject _idletarget;

    [SerializeField]
    private Transform _head;

    private Vector3 _currentlookpose = Vector3.zero;

    private Vector3 _targetlookpose = Vector3.zero;

    private Vector3 _newtarget = Vector3.zero;

    public void Awake()
    {
        //_head = GameObject.Find("Nomai_Rig_v01:Camera_01SHJnt").transform;
    }

    bool startedConvo = false;

    private void LateUpdate()
    {
        if (_CharacterDialogueTree.InConversation())
        {
            if (!startedConvo)
            {
                _currentlookpose = _head.position - transform.up;
                startedConvo = true;
            }

            _targetlookpose = Locator.GetActiveCamera().transform.position;
            _currentlookpose = Vector3.Lerp(_currentlookpose, _targetlookpose, Time.deltaTime);
            _head.LookAt(_currentlookpose, -transform.up);
        }
        if (!_CharacterDialogueTree.InConversation())
        {
            _newtarget = _idletarget.transform.position;
            _currentlookpose = Vector3.Lerp(_currentlookpose, _newtarget, Time.deltaTime);
            _head.LookAt(_currentlookpose, -transform.up);
            startedConvo = false;
        }
    }
}