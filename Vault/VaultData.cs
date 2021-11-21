using System.Collections.Generic;
using System;
using Core.Traps;
using Core.Inventory;
using Core.Enums;
using Core.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public class VaultData
    {
        public List<TrapData> trapInfos = new List<TrapData>();
        public List<MaterialData> materialInfos = new List<MaterialData>();

        #region Events
        public event EventHandler<VaultTrapEventArgs>       VaultTrapChanged;
        public event EventHandler<VaultMaterialEventArgs>   VaultMaterialChanged;
        public event EventHandler<VaultNutsEventArgs>       VaultNutsChanged;

        private void OnVaultTrapChanged(VaultTrapEventArgs vaultChange)
        {
            Debug.Log("Event Trap Change");
            VaultTrapChanged?.Invoke(this, vaultChange);
        }

        private void OnVaultMaterialChanged(VaultMaterialEventArgs vaultChange)
        {
            Debug.Log("Event Material Change");
            VaultMaterialChanged?.Invoke(this, vaultChange);
        }

        private void OnVaultNutsChanged(VaultNutsEventArgs vaultChange)
        {
            Debug.Log("Event Nuts Change");
            VaultNutsChanged?.Invoke(this, vaultChange);
        }
        #endregion

        #region Traps
        public void AddTrap(TrapData newTrap)
        {
            trapInfos.Add(newTrap);

            VaultTrapEventArgs vaultChange = new VaultTrapEventArgs(VaultOp.Add, newTrap); // change-event
            OnVaultTrapChanged(vaultChange);
        }

        public TrapData AddNewTrap(EnumManager.Traps trapType) // adds "brand new trap" = full durability.
        {
            TrapData trapToAdd = ReferenceManager.Instance.trapManager.GetTrapDataById(trapType);
            if (trapToAdd == null) return null;

            AddTrap(trapToAdd);
            return trapToAdd; // return the new trap to allow modification
        }

        public void UpdateTrap(TrapData trap)
        {
            DeleteTrap(trap); // delete an send delete event
            AddTrap(trap); // re add and send add event -> results in "update"
            // "change" event type is never used for traps...
        }

        public bool DeleteTrap(TrapData trapDataDelete)
        {
            int i = trapInfos.FindIndex(d => d == trapDataDelete);
            if (i == -1) return false;

            trapInfos.RemoveAt(i);

            VaultTrapEventArgs vaultChange = new VaultTrapEventArgs(VaultOp.Delete, trapDataDelete); // change-event
            OnVaultTrapChanged(vaultChange);
            return true;
        }

        public void SellTrap(TrapData trapDataSell)
        {
            if (!DeleteTrap(trapDataSell)) return;

            float percent = trapDataSell.percentDurability;
            UpdateNutAmount(CalculateTrapLossOfValue(percent, trapDataSell.nutPrice));
        }

        public void DismantleTrap(TrapData trapData)
        {
            float percent = trapData.percentDurability;

            foreach (var requiredMaterial in trapData.requiredMaterials)
            {
                AddMaterial(requiredMaterial.material, CalculateTrapLossOfValue(percent, requiredMaterial.count));
            }
            DeleteTrap(trapData);
        }

        private int CalculateTrapLossOfValue(float percent, int originalAmount, float maxValuePercent = 0.8f, int minReturnValue = 1)
        {
            if (percent > maxValuePercent) percent = maxValuePercent;
            int amount = Mathf.FloorToInt(originalAmount * percent);
            if (amount < minReturnValue) amount = minReturnValue; // always return at least one
            return amount;
        }

        #endregion

        #region Material
        public MaterialData GetMaterialData(EnumManager.Materials matType)
        {
            for (int i = 0; i < materialInfos.Count; i++)
            {
                if (matType == materialInfos[i].type)
                {
                    return materialInfos[i];
                }
            }
            return null;
        }

        public void AddMaterial(MaterialData newMaterial)
        {
            UpdateMaterial(newMaterial);
        }

        public void AddMaterial(EnumManager.Materials matType, int amount = 1)
        {
            MaterialData materialToAdd = ReferenceManager.Instance.materialManager.ConfigureNewMaterial(matType);
            materialToAdd.amount = amount;
            UpdateMaterial(materialToAdd);
        }

        public void UpdateMaterial(MaterialData newMaterial)
        {
            bool addedToExisting = false;
            for (int i = 0; i < materialInfos.Count; i++)
            {
                if (materialInfos[i].type == newMaterial.type)
                {
                    ChangeMaterialAmount(newMaterial, materialInfos[i]);
                    addedToExisting = true;
                    break;
                }
            }
            if (!addedToExisting)
            {
                materialInfos.Add(newMaterial);
                VaultMaterialEventArgs vaultChange = new VaultMaterialEventArgs(VaultOp.Add, newMaterial); // change-event
                OnVaultMaterialChanged(vaultChange);
            }
        }

        public void UseMaterial(RequiredMaterialData requiredMaterial)
        {
            MaterialData materialToUse = ReferenceManager.Instance.materialManager.ConfigureNewMaterial(requiredMaterial.material);
            materialToUse.amount = -requiredMaterial.count;
            UpdateMaterial(materialToUse);
        }

        private void ChangeMaterialAmount(MaterialData newMaterial, MaterialData existingMaterial)
        {
            existingMaterial.amount += newMaterial.amount;

            if (existingMaterial.amount <= 0)
            {
                existingMaterial.amount = 0;
                DeleteMaterial(existingMaterial);
                return;
            }

            VaultMaterialEventArgs vaultChange = new VaultMaterialEventArgs(VaultOp.Change, existingMaterial); // change-event
            OnVaultMaterialChanged(vaultChange);
        }

        private void DeleteMaterial(MaterialData material)
        {
            materialInfos.Remove(material);
            VaultMaterialEventArgs vaultChange = new VaultMaterialEventArgs(VaultOp.Delete, material); // change-event
            OnVaultMaterialChanged(vaultChange);
        }

        public void SellMaterial(MaterialData materialSellData, int amount)
        {
            MaterialData material = GetMaterialData(materialSellData.type);

            materialSellData = materialSellData.ShallowCopy();

            if ((material == null) || (material.amount <= 0)) return;
            if (material.amount < amount) amount = material.amount;

            materialSellData.amount = -amount;

            UpdateMaterial(materialSellData);

            int sumNuts = materialSellData.nutPrice * amount;
            UpdateNutAmount(sumNuts);
        }

        public void SellMaterial(MaterialData materialSellData) // materialData positive amount of amount to sell
        {
            MaterialData material = GetMaterialData(materialSellData.type);

            if ((material == null) || (material.amount <= 0)) return;
            if (material.amount < materialSellData.amount) materialSellData.amount = material.amount;

            int sumNuts = materialSellData.nutPrice * materialSellData.amount;
            UpdateNutAmount(sumNuts);

            materialSellData.amount = -materialSellData.amount;
            UpdateMaterial(materialSellData);
        }

        public int GetSumOfAllMaterialAmounts()
        {
            int amount = 0;
            foreach (MaterialData material in materialInfos)
            {
                amount += material.amount;
            }
            return amount;
        }

        public bool CheckIfEnoughMaterials(EnumManager.Materials matType, int amountNeeded)
        {
            for (int i = 0; i < materialInfos.Count; i++)
            {
                if (materialInfos[i].type == matType && materialInfos[i].amount >= amountNeeded) return true;
            }
            return false;
        }
        #endregion

        #region Nuts
        public int GetNutAmount() // get current nuts
        {
            return ReferenceManager.Instance.dataManager.playerData.nutAmount;
        }

        public void UpdateNutAmount(int nuts) // handles relative +/- nuts
        {
            ReferenceManager.Instance.dataManager.playerData.nutAmount += nuts;

            VaultNutsEventArgs vaultChange = new VaultNutsEventArgs(VaultOp.Change, ReferenceManager.Instance.dataManager.playerData.nutAmount); // change-event
            OnVaultNutsChanged(vaultChange);
        }

        public void SetNutAmount(int nuts)
        {
            ReferenceManager.Instance.dataManager.playerData.nutAmount = nuts;

            VaultNutsEventArgs vaultChange = new VaultNutsEventArgs(VaultOp.Change, ReferenceManager.Instance.dataManager.playerData.nutAmount); // change-event
            OnVaultNutsChanged(vaultChange);
        }
        #endregion

        #region Editor Buttons
        [EnumToggleButtons]
        public EnumManager.Traps enumTraps;

        [Button]
        public void AddOneTrapToVault()
        {
            AddNewTrap(enumTraps);
        }

        [Button]
        public void AddAllTraps()
        {
            foreach (EnumManager.Traps item in Enum.GetValues(typeof(EnumManager.Traps)))
            {
                AddNewTrap(item);
            }
        }

        [Button]
        public void ClearAllTraps()
        {
            foreach (var item in trapInfos)
            {
                VaultTrapEventArgs vaultChange = new VaultTrapEventArgs(VaultOp.Delete, item); // change-event
                OnVaultTrapChanged(vaultChange);
            }
            trapInfos.Clear();
        }
        #endregion
    }
}