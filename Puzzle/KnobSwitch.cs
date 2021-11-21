using UnityEngine;

public class KnobSwitch : PuzzleInteractable
{
    //eines der puzzle objekte. wenn diese funktion aufgerufen wird, dann ist der bool true. 
    //aufgerufen wird diese, wenn im RaycastObjects.cs ein raycast mit dem schalter kollidiert und eine taste gedrückt wird.

    public Animator anim;
    public void KnobPressed()
    {
        IsSolved = true;
        NotifyDoor();
        anim.Play("SchalterOpen");
    }
}
