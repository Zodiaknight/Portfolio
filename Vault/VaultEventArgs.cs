using Core.Inventory;
using System;

namespace Core.Data
{
    public enum VaultOp
    {
        Add,
        Delete,
        Change
    }

    public abstract class VaultEventArgs : EventArgs
    {
        public VaultOp Operation { get; set; }
        public VaultEventArgs(VaultOp op)
        {
            Operation = op;
        }
    }
//-----------------------------------------------------------------------------------------
    public class VaultTrapEventArgs : VaultEventArgs
    {
        public TrapData TrapData { get; set; }

        public VaultTrapEventArgs(VaultOp op, TrapData td) : base(op)
        {
            TrapData = td;
        }
    }
//-----------------------------------------------------------------------------------------
    public class VaultMaterialEventArgs : VaultEventArgs
    {
        public MaterialData MaterialData { get; set; }

        public VaultMaterialEventArgs(VaultOp op, MaterialData md) : base(op)
        {
            MaterialData = md;
        }
    }
//-----------------------------------------------------------------------------------------
    public class VaultNutsEventArgs : VaultEventArgs
    {
        public int Nuts { get; set; }

        public VaultNutsEventArgs(VaultOp op, int nuts) : base(op)
        {
            Nuts = nuts;
        }
    }
}