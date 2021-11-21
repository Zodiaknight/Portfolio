using UnityEngine;
using System.Collections.Generic;
using Core.Enums;
using TMPro;
using Random = UnityEngine.Random;


namespace Core.UI
{
    public class SpinWheel : MonoBehaviour
    {
        public EnumManager.Items[] wheelItemPattern = new EnumManager.Items[] {
            EnumManager.Items.Trap, EnumManager.Items.Blank, EnumManager.Items.Nut, EnumManager.Items.Blank, EnumManager.Items.Material, EnumManager.Items.Blank,
            EnumManager.Items.Trap, EnumManager.Items.Blank, EnumManager.Items.Nut, EnumManager.Items.Blank, EnumManager.Items.Material, EnumManager.Items.Blank};

        public List<AnimationCurve> animationCurves;

        public Transform wheel;
        public DailyHandler dailyHandler;
        public PrizeManager prizeManager;

        //Player Level (Fake)
        [SerializeField]
        int fakePlayerLevel = 1;
 
        //Randoms
        [SerializeField]
        private int minRandom;
        [SerializeField]
        private int maxRandom;

        //Text
        public TextMeshProUGUI nutText;
        public TextMeshProUGUI winText;

        //Bools
        private bool isSpinning;
        public bool hasAttempts;
        private bool hasSpinned = false;

        //Free Spin
        public int winCount;
        //private int attempts;

        //Prepare Stuff
        private float anglePerItem;
        private int randomTime;

        private int itemNumber;

        //SpinningWheel stuff
        private float elapsedSpinTime;
        private float spinTime;
        private float startAngle;
        private float maxAngle;
        private int animationCurveNumber;

        private void Awake()
        {
            isSpinning = false;

            dailyHandler.CheckForDailySpin();

            if (dailyHandler.attempts >= 0)
            { 
                hasAttempts = true;
            }
        }

        private void OnEnable()
        {
            if (prizeManager != null)
            {
                prizeManager.UpdateAvailableNuts();
                nutText.text = prizeManager.currentNuts.ToString();
            }
        }

        void Start()
        {
            anglePerItem = 360 / wheelItemPattern.Length; // 12 prizes  
        }


        private void Update()
        {
            if (isSpinning)
            {
                SpinTheWheel();
            }
        }                                //ruft SpinTheWheel auf

        public void ShowUpWheel()                               //10% wahrscheinlichkeit oder nach 3 versuchen zu 100% garantierter wheel aufruf
        {
            if (dailyHandler.attempts <= 0) return;
            dailyHandler.CheckForDailySpin();

            winCount++;

            if (CheckWheel())
            {
                CanvasManager.Instance.AddCanvas(CanvasType.Wheel);
            }
        }

        public bool CheckWheel()
        {
            int i = Random.Range(minRandom, maxRandom);
            Debug.Log("Randomzahl ist " + i);

            if (i == 0) return true;

            if (winCount == 3)
            {
                winCount = 0;
                return true;
            }
            return false;
        }

        public void FreeSpin()                                  //button zum ausführen des spin-prozess
        {
            if (isSpinning) return;

            if (hasSpinned == true) return;

            if (dailyHandler.attempts <= 0) return;

            dailyHandler.attempts--;

            PrepareSpin();
        }

        public void PrepareSpin()                             //vorbereitung des preises und funktionsaufruf zum drehen des rads
        {
            winText.text = " ";
            randomTime = Random.Range(2, 4)*2;
            itemNumber = Random.Range(0, wheelItemPattern.Length);  //hier steht der preis fest
            maxAngle = 360 * randomTime + (itemNumber * anglePerItem);

            isSpinning = true;
            hasSpinned = true;

            elapsedSpinTime = 0.0f;
            spinTime = randomTime;

            startAngle = wheel.transform.eulerAngles.z;

            maxAngle = maxAngle - startAngle;
            maxAngle += anglePerItem / 2;

            animationCurveNumber = Random.Range(0, animationCurves.Count);
        }

        public void SpinTheWheel()                              //wie sich das rad dreht
        {
            if (elapsedSpinTime < spinTime)
            {
                float angle = maxAngle * animationCurves[animationCurveNumber].Evaluate(elapsedSpinTime / spinTime);
                wheel.transform.eulerAngles = new Vector3(0.0f, 0.0f, angle + startAngle);
                elapsedSpinTime += Time.deltaTime;
            }
            else
            {
                FinishedSpin();
            }
        }

        public void FinishedSpin()
        {
            wheel.transform.eulerAngles = new Vector3(0.0f, 0.0f, maxAngle + startAngle);
            isSpinning = false;

            prizeManager.HandlePrizes(itemNumber);
        }                           
        
        public void DisableWheel()                              
        {
            if (isSpinning) return;
            winText.text = " ";
            hasSpinned = false;
            this.gameObject.SetActive(false);
        }
    }
}