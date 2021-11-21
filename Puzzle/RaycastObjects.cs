using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaycastObjects : MonoBehaviour
{
    //layermask für interaktion
    [SerializeField] private int rayLength = 5;
    [SerializeField] private LayerMask layerMaskInteract;
    [SerializeField] private string excludeLayerName = null;

    //mit welchen skripten interagiert werden soll.
    private DoorRoomManager _raycastDoorObj;
    private KnobSwitch      _raycastSchalterObj;
    private PickupObject    _raycastKeyObj;
    private PickupBox       _raycastBoxObj;
    private PickupStone     _raycastStoneObj;

    private bool _keyBool = false;

    //welche taste gedrückt werden muss. im inspektor einstellbar.
    [SerializeField] private KeyCode _openDoorKey   = KeyCode.E;
    [SerializeField] private KeyCode _pickUpKey     = KeyCode.E;
    [SerializeField] private KeyCode _schalterKey   = KeyCode.E;
    [SerializeField] private KeyCode _boxKey        = KeyCode.E;
    [SerializeField] private KeyCode _stoneKey      = KeyCode.E;

    [SerializeField] private Image _crosshair = null;
    private bool _isCrosshairActive;
    private bool _doOnce;

    //objekt muss das layer UND einen tag besitzen
    private const string _interactableTagDoor       = "Door";
    private const string _interactableTagKey        = "Key";
    private const string _interactableTagSchalter   = "Schalter";
    private const string _interactableTagBox        = "Box";
    private const string _interactableTagStone      = "Rock";

    public TextMeshProUGUI UIText;
    public Transform RightHand;
    public bool IsDragged;
    public bool IsBoxDragged;
    public bool IsStoneDragged;

    private void Start()
    {
        _keyBool = false;
        IsDragged = false;
        IsBoxDragged = false;
        IsStoneDragged = false;
    }

    public void ObjectComponent(GameObject obj, bool objBool)
    {
        IsDragged = false;
        objBool = false;

        obj.transform.parent = null;
        obj.GetComponent<BoxCollider>().enabled = true;
        obj.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void Update()
    {
        if (IsDragged == true) //guckt ob bool true ist und deaktivert den rest des skriptes
        {
            if(IsBoxDragged == true)    //wen zudem noch der bool für eine box auf true ist, dann führe nur diesen code aus
            {
                if (Input.GetKeyDown(_boxKey))
                {
                    ObjectComponent(_raycastBoxObj, IsBoxDragged);
                }
            }

            if (IsStoneDragged == true)
            {
                if (Input.GetKeyDown(_stoneKey))     //wenn bool zum stein gehört, dann diesen code hier.
                {
                    ObjectComponent(_raycastStoneObj, IsStoneDragged);
                }
            }
        }

        else
        {
            //wenn isdragged nicht true ist, dann führe hier den eigentlichen raycast code aus.
            RaycastHit hit;
            Vector3 fwd = transform.TransformDirection(Vector3.forward);

            int mask = 1 << LayerMask.NameToLayer(excludeLayerName) | layerMaskInteract.value;

            if (Physics.Raycast(transform.position, fwd, out hit, rayLength, mask))
            {
                if (hit.collider.CompareTag(_interactableTagDoor))   //if abfrage für interaktion mit einer türe.
                {
                    if (!_doOnce)
                    {
                        _raycastDoorObj = hit.collider.gameObject.GetComponent<DoorRoomManager>();
                        CrosshaitChange(true);
                    }

                    _isCrosshairActive = true;
                    _doOnce = true;

                    if (_keyBool == false)
                    {
                        UIText.text = "missing key";
                    }

                    else
                    {
                        UIText.text = "press [" + _openDoorKey + "]";
                    }

                    if (Input.GetKeyDown(_openDoorKey) && _keyBool == true)
                    {
                        _raycastDoorObj.DoorToggle();
                    }
                }

                if (hit.collider.CompareTag(_interactableTagKey))        //if abfrage für interaktion mit einem schlüssel
                {
                    if (!_doOnce)
                    {
                        _raycastKeyObj = hit.collider.gameObject.GetComponent<PickupObject>();
                        CrosshaitChange(true);
                    }
                    _isCrosshairActive = true;
                    _doOnce = true;

                    UIText.text = "press [" + _pickUpKey + "] to pick up";

                    if (Input.GetKeyDown(_pickUpKey))
                    {
                        _raycastKeyObj.pickupKey();
                        _keyBool = true;
                    }
                }

                if (hit.collider.CompareTag(_interactableTagSchalter))       //if abfrage für interaktion mit einem schalter
                {
                    if (!_doOnce)
                    {
                        _raycastSchalterObj = hit.collider.gameObject.GetComponent<KnobSwitch>();
                        CrosshaitChange(true);
                    }
                    _isCrosshairActive = true;
                    _doOnce = true;

                    UIText.text = "press [" + _schalterKey + "] to activate";

                    if (Input.GetKeyDown(_pickUpKey))
                    {
                        _raycastSchalterObj.KnobPressed();
                    }
                }

                if (hit.collider.CompareTag(_interactableTagBox))    //if abfrage für interaktion mit einer box
                {
                    if (!_doOnce)
                    {
                        _raycastBoxObj = hit.collider.gameObject.GetComponent<PickupBox>();
                        CrosshaitChange(true);
                    }

                    _isCrosshairActive = true;
                    _doOnce = true;

                    UIText.text = "press [" + _boxKey + "] to pick up";

                    if (Input.GetKeyDown(_boxKey) && IsDragged == false && IsBoxDragged == false)
                    {
                        IsDragged = true;
                        IsBoxDragged = true;
                        _raycastBoxObj.GetComponent<BoxCollider>().enabled = false;
                        _raycastBoxObj.GetComponent<Rigidbody>().isKinematic = true;
                        _raycastBoxObj.transform.position = RightHand.position;
                        _raycastBoxObj.transform.parent = RightHand;
                        UIText.text = "press [" + _boxKey + "] to drop it";
                    }
                }

                if (hit.collider.CompareTag(_interactableTagStone))  //if abfrage für interaktion mit einem stein
                {
                    if (!_doOnce)
                    {
                        _raycastStoneObj = hit.collider.gameObject.GetComponent<PickupStone>();
                        CrosshaitChange(true);
                    }

                    _isCrosshairActive = true;
                    _doOnce = true;

                    if (transform.parent.gameObject.CompareTag("Player"))
                    {
                        UIText.text = "too heavy!!";        //wenn der parent des objektes der mensch ist, dann kann er dieses objekt nicht heben.
                    }

                    if (transform.parent.gameObject.CompareTag("PlayerBear"))
                    {
                        UIText.text = "press [" + _stoneKey + "] to pick up";

                        if (Input.GetKeyDown(_stoneKey) && IsDragged == false && IsStoneDragged == false)
                        {
                            IsDragged = true;
                            IsStoneDragged = true;
                            _raycastStoneObj.GetComponent<MeshCollider>().enabled = false;
                            _raycastStoneObj.GetComponent<Rigidbody>().isKinematic = true;
                            _raycastStoneObj.transform.position = RightHand.position;
                            _raycastStoneObj.transform.parent = RightHand;
                            UIText.text = "press [" + _stoneKey + "] to drop it";
                        }
                    }
                }
            }

            else
            {
                UIText.text = " ";      //standard text, welcher sich immer ändert sobald der raycast mit etwas interagiert.

                if (_isCrosshairActive)
                {
                    CrosshaitChange(false);
                    _doOnce = false;
                }
            }
        }
    }
    void CrosshaitChange(bool on)       //je nachdem ist das fadenkreuz weiß oder rot.
    {
        if (on && !_doOnce)
        {
            _crosshair.color = Color.red;
        }

        else
        {
            _crosshair.color = Color.white;
            _isCrosshairActive = false;
        }
    }
}
