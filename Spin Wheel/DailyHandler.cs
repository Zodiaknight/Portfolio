using Core.Level;
using UnityEngine;
using System;

namespace Core.UI
{
    public class DailyHandler : MonoBehaviour
    {
        public PrizeManager manager;

        [SerializeField]
        private double nextRewardDelay;

        public int attempts;
        private int savedSpins;

        private void Awake()
        {
            attempts = ReferenceManager.Instance.dataManager.playerData.freeSpins;
            savedSpins = ReferenceManager.Instance.dataManager.playerData.freeSpins;
            
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("Reward_Claim_Datetime")))
            {
                PlayerPrefs.SetString("Reward_Claim_Datetime", DateTime.Now.ToString());
            }
        }

        public void CheckForDailySpin()                         //überprüft ob das wheel neue preise bekommen kann
        {
            DateTime currentDatetime = DateTime.Now;
            DateTime rewardClaimDatetime = DateTime.Parse(PlayerPrefs.GetString("Reward_Claim_Datetime", currentDatetime.ToString()));

            double elapsedSeconds = (currentDatetime - rewardClaimDatetime).TotalSeconds;   //TODO: Ändern zu TotalHours
            Debug.Log("so viele sekunden " + elapsedSeconds);

            if (elapsedSeconds >= nextRewardDelay)
            {
                Debug.Log("neue rewards verfügbar");
                attempts = savedSpins;

                manager.PushRandomPrize();
                Debug.Log("hab jetzt neue attemps" + attempts + "und das sind die gesicherten" + savedSpins);

                PlayerPrefs.SetString("Reward_Claim_Datetime", currentDatetime.ToString());
            }

            else
            {
                Debug.Log("keine neuen verfügbar");
            }
        }
    }
}
