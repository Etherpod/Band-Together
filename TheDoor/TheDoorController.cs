using UnityEngine;

namespace OWModJam.TheDoor;

public class TheDoorController : MonoBehaviour
{
  private static readonly int Open = Animator.StringToHash("Open");
  
  [SerializeField]
  private TheDoorKeySocket theDoorKeySocket;
  
  private Animator _animator;

  private void Awake()
  {
    _animator = base.gameObject.GetComponent<Animator>();

    theDoorKeySocket.OnKeyInserted += KeyInserted;
  }

  private void KeyInserted()
  {
    _animator.SetTrigger(Open);
  }
}