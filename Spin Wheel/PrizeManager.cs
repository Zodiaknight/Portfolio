using Core.Level;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Core.Enums;
using Core.Inventory;
using Core.Traps;
using Core.Data;
using System.Linq;
using Random = UnityEngine.Random;

namespace Core.UI
{
    public class PrizeManager : MonoBehaviour
    {
        public SpinWheel wheel;

        [SerializeField]
        private SO_PrizeSlot blankPrize;
        public SO_PrizeDatabase prizeDatabase;
        public SO_PrizeSlot[] nutPrizes;
        public List<SO_PrizeSlot> prizeSlot;
        public TextMeshProUGUI[] prizeNames;

        [SerializeField]
        int fakePlayerLevel = 1;

        [SerializeField]
        private Sprite nutSprite;

        public int currentNuts;

        private VaultData vaultData;

        private void Awake()
        {
            if (vaultData == null)
            {
                vaultData = ReferenceManager.Instance.dataManager.playerData.vault;
            }

            UpdateAvailableNuts();
        }

        private void OnEnable()
        {
            vaultData.VaultNutsChanged += ReceivedNutChange;
        }

        private void OnDisable()
        {
            vaultData.VaultNutsChanged -= ReceivedNutChange;
        }

        private void ReceivedNutChange(object sender, VaultNutsEventArgs vaultChange)
        {
            currentNuts = vaultChange.Nuts;
            wheel.nutText.text = currentNuts.ToString();
        }


        public void UpdateAvailableNuts()
        {
            currentNuts = vaultData?.GetNutAmount() ?? ReferenceManager.Instance.dataManager.playerData.vault.GetNutAmount();
        }

        public void PushRandomPrize()
        {
            var (filteredMats, filteredTraps, filteredNuts) = FilterPrize();

            prizeSlot.Clear();
            SO_PrizeSlot currentSlot = blankPrize;

            for (int i = 0; i < wheel.wheelItemPattern.Length; i++)
            {
                switch (wheel.wheelItemPattern[i])
                {
                    case EnumManager.Items.Trap:
                        currentSlot = filteredTraps[Random.Range(0, filteredTraps.Length)];
                        AssignTextAndSprite(i, currentSlot.item.itemName, currentSlot.item.itemImage);
                        break;

                    case EnumManager.Items.Material:
                        currentSlot = filteredMats[Random.Range(0, filteredMats.Length)];
                        AssignTextAndSprite(i, currentSlot.item.itemName, currentSlot.item.itemImage);
                        break;

                    case EnumManager.Items.Nut:
                        currentSlot = filteredNuts[Random.Range(0, filteredNuts.Length)];
                        AssignTextAndSprite(i, currentSlot.prizeNut.ToString(), nutSprite);
                        break;

                    case EnumManager.Items.Blank:
                        currentSlot = blankPrize;
                        prizeNames[i].text = " ";
                        prizeNames[i].gameObject.SetActive(false);
                        break;
                }
                prizeSlot.Add(currentSlot);
            }
        }

        private void AssignTextAndSprite(int wheelItemPosition, string text, Sprite image)
        {
            prizeNames[wheelItemPosition].text = text;
            Image currentImage = prizeNames[wheelItemPosition].GetComponentInChildren<Image>();
            currentImage.sprite = image;
        }

        public Tuple<SO_PrizeSlot[], SO_PrizeSlot[], SO_PrizeSlot[]> FilterPrize()
        {
            //filtert items aus der datenbank heraus welche für das mindest player level in frage kommen
            SO_PrizeSlot[] prizeBase = prizeDatabase.prizeBase;
            SO_PrizeSlot[] filteredPrizes = prizeBase.Where(x => x.minPlayerLevel <= fakePlayerLevel).ToArray();
            filteredPrizes = filteredPrizes.Where(x => x.item != null).ToArray();

            SO_PrizeSlot[] filteredMats = filteredPrizes.Where(x => x.item.itemType == EnumManager.Items.Material).ToArray();
            SO_PrizeSlot[] filteredTraps = filteredPrizes.Where(x => x.item.itemType == EnumManager.Items.Trap).ToArray();
            SO_PrizeSlot[] filteredNuts = nutPrizes.Where(x => x.minPlayerLevel <= fakePlayerLevel).ToArray();
            return Tuple.Create(filteredMats, filteredTraps, filteredNuts);
        }

        public void HandlePrizes(int itemNumber)                //findet raus was für ein preis typ es ist und geht dann dementsprechend um
        {
            if (prizeSlot[itemNumber].item != null) //nur wenn der preis ein item ist
            {
                wheel.winText.text = prizeSlot[itemNumber].amount + "x " + prizeSlot[itemNumber].item.itemName;
                //hier item reinpushen + amount
                PushPrize(prizeSlot[itemNumber].item, prizeSlot[itemNumber].amount);
            }

            if (prizeSlot[itemNumber].prizeNut <= 0) return; //wenn der preis nuts enthält

            vaultData.UpdateNutAmount(prizeSlot[itemNumber].prizeNut);
            SetWheelPrize(itemNumber);
        }

        private void SetWheelPrize(int itemNumber)
        {
            if (prizeSlot[itemNumber].item != null) //für den fall, wenn der preis nuts UND ein item ist
            {
                wheel.winText.text += " & " + prizeSlot[itemNumber].prizeNut.ToString() + " Nuts";
                PushPrize(prizeSlot[itemNumber].item, prizeSlot[itemNumber].amount);
            }
            else
            {
                wheel.winText.text = prizeSlot[itemNumber].prizeNut.ToString() + " Nuts";
            }
        }

        public void PushPrize(SO_Item item, int amount)         //pusht preis in den vault
        {
            switch (item.itemType)
            {
                case EnumManager.Items.Material:
                    PushMaterial(item, amount);
                    break;
                case EnumManager.Items.Trap:
                    PushTrap(item, amount);
                    break;
            };
        }

        public void PushMaterial(SO_Item item, int amount)
        {
            SO_Material prizemat = (SO_Material)item;
            vaultData.AddMaterial(prizemat.data.type, amount);
        }

        public void PushTrap(SO_Item item, int amount)
        {
            SO_Trap prizeTrap = (SO_Trap)item;
            if (prizeTrap.data != null)
            {
                //TrapData data = prizetrap.data.ShallowCopy();  //kopiert nur value types und keine reference types

                for (int i = 0; i < amount; i++)
                {
                    vaultData.AddNewTrap(prizeTrap.data.trapId);
                }
            }
            else
            { 
                Debug.Log("IDs der Traps stimmen nicht überein oder existieren nicht");
            }
        }
    }
}
