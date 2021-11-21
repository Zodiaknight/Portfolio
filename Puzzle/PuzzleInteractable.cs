using UnityEngine;

public class PuzzleInteractable : MonoBehaviour
{
    //hauptklasse der rätsel. übergibt einen bool und prüft in der tür klasse, ob alle verlangen boole true sind und wenn ja, dann öffnet sich die türe.

    public bool IsSolved = false;
    public DoorRoomManager Door;

    public void NotifyDoor()
    {
        Door.CheckAllSolved();
    }
}
