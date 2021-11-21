using System.Collections.Generic;
using UnityEngine;

public class DoorRoomManager : MonoBehaviour
{
    private Animator _doorAnimator;
    private bool _doorBool = false;

    public List<GameObject> ListObjects; //liste mit objekten, welche das puzzleinteractable.cs besitzen.

    private void Awake()
    {
        _doorAnimator = gameObject.GetComponent<Animator>();

        foreach (var obj in ListObjects)
        {
            var puzzle = obj.GetComponent<PuzzleInteractable>();
            puzzle.Door = this;
        }
    }

    public void CheckAllSolved()
    {
        bool allSolved = true;

        foreach (var go in ListObjects)
        {
            PuzzleInteractable puzzle = go.GetComponent<PuzzleInteractable>();      //geht die liste durch und wenn isSolved
                                                                                    //nicht true ist, dann ist der allsolved bool ebenfalls false
            if (!puzzle.IsSolved)
            {
                allSolved = false;
                break;
            }
        }

        if (allSolved)  //wenn also alle bools in der liste true sind, so ist allsolved nicht false und führt somit die türöffner funktion aus.
        {
            DoorOpen();
        }

        else
        {
            DoorClose();
        }
    }

    public void DoorToggle()
    {
        _doorBool = !_doorBool;
        _doorAnimator.SetBool("boolDoor", _doorBool);
    }

    public void DoorOpen()
    {
        _doorBool = true;
        _doorAnimator.SetBool("boolDoor", _doorBool);
    }

    public void DoorClose()
    {
        _doorBool = false;
        _doorAnimator.SetBool("boolDoor", _doorBool);
    }
}
