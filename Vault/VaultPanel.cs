using Core.Crafting;
using Core.Enums;
using Core.Inventory;
using Core.Level;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Data;
using Core.Gameflow;
using Sirenix.OdinInspector;

namespace Core.UI
{
    public class VaultPanel : MonoBehaviour
    {
        public Transform trapSlotHolder;
        public Transform materialSlotHolder;
        public Transform nutAmount;

        public int sellPercent = 10;

        public EnumManager.Materials selectedMaterialId;
        public CraftingMenu craftingMenu;
        private SlotSelectionHandler materialSelectionHandler;
        private TrapSlotListHandler trapListHandler;
        private SlotTrap selectedTrapSlot;
        public List<SlotMaterial> materialSlots;

        private VaultData vaultData = null;
        public static VaultPanel Instance { get; private set; }

        private void Awake()
        {
            vaultData = ReferenceManager.Instance.dataManager.playerData.vault;
            trapListHandler = new TrapSlotListHandler(trapSlotHolder.transform, new Vector3(1f, 1f, 1f));
            materialSelectionHandler = new SlotSelectionHandler();

            Instance = this;
            LoadTraps();
            LoadMaterials();

            //Events zuweisung
            vaultData.VaultTrapChanged      += ReceivedVaultTrapChange;
            vaultData.VaultMaterialChanged  += ReceivedVaultMaterialChange;
            vaultData.VaultNutsChanged      += ReceivedNutChange;

            trapListHandler.SelectionHandler.SlotSelectChanged  += ReceivedTrapSelectionChange;
            materialSelectionHandler.SlotSelectChanged          += ReceivedMaterialSelectionChange;
        }

        private void OnEnable()
        {
            UpdateNutAmount(vaultData.GetNutAmount());
        }

        public void OnDisable()
        {
            trapListHandler.SelectionHandler.Deselect();

            if (GameManager.Instance.currentScene == GameManager.Scenes.Overworld)
            {
                LevelInfoScreen.Instance.gameObject.SetActive(true);
            }
        }

        #region Events
        public void ReceivedVaultTrapChange(object sender, VaultTrapEventArgs vaultChange)
        {
            switch (vaultChange.Operation)
            {
                case VaultOp.Add: AddTrap(vaultChange.TrapData); break;
                case VaultOp.Delete: RemoveTrap(vaultChange.TrapData); break;
                case VaultOp.Change: break; // maybe update durability
            };
        }

        public void ReceivedVaultMaterialChange(object sender, VaultMaterialEventArgs vaultChange)
        {
            switch (vaultChange.Operation)
            {
                case VaultOp.Add: UpdateMaterialAmount(vaultChange.MaterialData.type, vaultChange.MaterialData.amount); break;
                case VaultOp.Delete: UpdateMaterialAmount(vaultChange.MaterialData.type, 0); break;
                case VaultOp.Change: UpdateMaterialAmount(vaultChange.MaterialData.type, vaultChange.MaterialData.amount); break;
            }
        }

        private void ReceivedNutChange(object sender, VaultNutsEventArgs vaultChange)
        {
            UpdateNutAmount(vaultChange.Nuts);
        }

        public void ReceivedTrapSelectionChange(object sender, SelectEventArgs selectionChange)
        {
            selectedTrapSlot = (SlotTrap)selectionChange.Slot;
        }

        public void ReceivedMaterialSelectionChange(object sender, SelectEventArgs selectionChange)
        {
            selectedMaterialId = ((SlotMaterial)selectionChange.Slot)?.materialType ?? EnumManager.Materials.None;
        }
        #endregion

        #region Traps
        private void LoadTraps()
        {
            trapListHandler.Load();
        }

        private void AddTrap(TrapData trapData)
        {
            trapListHandler.AddTrapToStack(trapData);
        }

        private void RemoveTrap(TrapData trapData)
        {
            trapListHandler.RemoveTrap(trapData);
        }

        public void SellTrap()
        {
            if (selectedTrapSlot != null) vaultData.SellTrap(selectedTrapSlot.trap);
        }

        public void DismantleTrap()
        {
            if (selectedTrapSlot != null) vaultData.DismantleTrap(selectedTrapSlot.trap);
        }
        #endregion

        #region Material
        private void LoadMaterials()
        {
            foreach (SO_Material mat in MaterialManager.Instance.materials)
            {
                EnumManager.Materials matType = mat.data.type;

                SlotMaterial newSlot = (SlotMaterial)SlotManager.Instance.SpawnSlot(SlotType.MaterialSlot, materialSlotHolder);
                newSlot.name = matType.ToString();
                newSlot.materialType = matType; // Enum material type
                newSlot.material = MaterialManager.Instance.GetMaterialData(matType); // template materialdata - not from vault, not unique
                newSlot.SelectionHandler = materialSelectionHandler;
                newSlot.gameObject.SetActive(true);
                materialSlots.Add(newSlot);
                newSlot.imageIcon.GetComponent<Image>().sprite = mat.data.sprite;

                if (vaultData.GetMaterialData(matType) != null)
                {
                    newSlot.amountText.GetComponent<TMP_Text>().text = vaultData.GetMaterialData(matType).amount.ToString();
                }
                else
                {
                    newSlot.amountText.GetComponent<TMP_Text>().text = "0";
                }
            }
        }

        public void ShowMaterials()
        {
            foreach (Slot slot in materialSlots) { slot.gameObject.SetActive(true); }
        }

        [Button]
        public void UpdateMaterialAmount(EnumManager.Materials mat, int newAmount)
        {
            foreach (SlotMaterial material in materialSlots)
            {
                if (material.materialType == mat)
                {
                    material.amountText.GetComponent<TMP_Text>().text = newAmount.ToString();
                }
            }
        }

        public void SellMaterial()
        {
            if (selectedMaterialId == EnumManager.Materials.None) return;
            int amount = 1;
            MaterialData matData = MaterialManager.Instance.ConfigureNewMaterial(selectedMaterialId);
            matData.amount = amount;
            if (matData != null) vaultData.SellMaterial(matData);
        }
        #endregion

        #region Nuts
        public void UpdateNutAmount(int amount)
        {
            nutAmount.GetComponent<TMP_Text>().text = amount.ToString();
        }
        #endregion
    }
}