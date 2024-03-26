using UnityEngine;

namespace BandTogether.TheDoor;

public class TheDoorController : MonoBehaviour
{
    private static readonly int Open = Animator.StringToHash("Open");

    public event OWEvent.OWCallback OnOpening;

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
        ModMain.Instance.ModHelper.Console.WriteLine("key fully inserted");
        _animator.SetTrigger(Open);
        OnOpening?.Invoke();
    }

    public void PlayKeyCompletionSfx()
    {
        theDoorKeySocket.PlayCompletionSfx();
    }
}