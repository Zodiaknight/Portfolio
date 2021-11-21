using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerSwitch : MonoBehaviour
{

    //herzstück zum wechseln zwischen den beiden spielbaren charakteren

    public Cutscenes    Cutscene;

    public GameObject   HumanPlayer;
    public GameObject   HumanCamera;
    public NavMeshAgent HumanAgent;
    public Animator     HumanAnimator;

    public GameObject   BearPlayer;
    public GameObject   BearCamera;
    public NavMeshAgent BearAgent;
    public Animator     BearAnimator;

    public bool Human   = true;
    public bool Bear    = false;
    public bool Follow  = false;

    public Image Humanpic;
    public Image Bearpic;
    public Image Humanborder;
    public Image Bearborder;

    void Start()
    {
        HumanPlayer.GetComponent<PlayerController>().enabled = true;
        HumanPlayer.GetComponent<Rigidbody>().isKinematic = false;
        HumanAgent.GetComponent<NavMeshAgent>().enabled = false;
        HumanAnimator.GetComponent<Animator>();
        HumanCamera.SetActive(true);

        BearPlayer.GetComponent<BearController>().enabled = false;
        BearPlayer.GetComponent<Rigidbody>().isKinematic = true;
        BearAgent.GetComponent<NavMeshAgent>().enabled = true;
        BearAnimator.GetComponent<Animator>();
        BearCamera.SetActive(false);

        Bearpic.gameObject.SetActive(false);
        Bearborder.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Cutscene.cutsceneIsPlaying == false)
        {
            if (Input.GetKeyDown(KeyCode.C) == true) SwitchPlayers();
            if (Input.GetKeyDown(KeyCode.F) == true) FollowMe();
            if (Human == true && Bear == false && Follow == true) BearFollowHuman();
            if (Human == false && Bear == true && Follow == true) HumanFollowBear();
        }
    }

    void SwitchPlayers()    //enabeld und disabeld skripte welche benötigt werden, damit der spielbare charakter sich bewegen kann und der nicht gespielte charakter einen navmeshagent erhält.
    {
        HumanCamera.SetActive(Bear);
        HumanPlayer.GetComponent<PlayerController>().enabled = Bear;
        HumanPlayer.GetComponent<Rigidbody>().isKinematic = Human;
        HumanAgent.GetComponent<NavMeshAgent>().enabled = Human;
        
        BearCamera.SetActive(Human);
        BearPlayer.GetComponent<BearController>().enabled = Human;
        BearPlayer.GetComponent<Rigidbody>().isKinematic = Bear;
        BearAgent.GetComponent<NavMeshAgent>().enabled = Bear;
        
        Human = !Human;
        Bear = !Bear;

        if (Human == true)      //ändert die bilder in der ui zu dem charakter, welcher gerade gespielt wird
        {
            Humanpic.color = Color.white;
            Bearpic.color = Color.grey;
        }

        if (Bear == true)
        {
            Humanpic.color = Color.grey;
            Bearpic.color = Color.white;
        }
    }
    void FollowMe()     //de- aktiviert bool zum folgen + de- aktiviert den grafischen folgemodus im ui (protraits leuchten gelb)
    {
        Follow = !Follow;

        if (Follow == true)
        {
            Humanborder.color = Color.yellow;
            Bearborder.color = Color.yellow;
        }
        if (Follow == false)
        {
            Humanborder.color = Color.black;
            Bearborder.color = Color.black;
        }
    }
    void HumanFollowBear()  //Mensch wird zum navmeshagent und läuft dem bären hinterher.
    {
        if (BearPlayer)
        {
            HumanAnimator.SetFloat("Mode", HumanAgent.velocity.magnitude);
            HumanAgent.SetDestination(BearPlayer.transform.position);
        }
    }
    void BearFollowHuman()      // das gleiche nur umgekehrt.
    {
        if (HumanPlayer)
        {
            BearAnimator.SetFloat("Mode", BearAgent.velocity.magnitude);
            BearAgent.SetDestination(HumanPlayer.transform.position);
        }
    }
}
