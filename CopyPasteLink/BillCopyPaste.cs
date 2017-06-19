﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ImprovedWorkbenches
{
    public class BillCopyPaste
    {
        public void Clear()
        {
            _copiedBills.Clear();
        }

        public void DoCopy(Bill_Production billProduction)
        {
            _copiedBills.Clear();
            _copiedBills.Add(billProduction);
        }

        public void DoCopy(Building_WorkTable workTable)
        {
            if (workTable.BillStack == null || workTable.BillStack.Count == 0)
                return;

            _copiedBills.Clear();
            foreach (var bill in workTable.BillStack.Bills)
            {
                var billProduction = bill as Bill_Production;
                if (billProduction == null)
                    continue;

                _copiedBills.Add(billProduction);
            }
        }

        public bool CanPasteInto(Building_WorkTable workTable)
        {
            if (_copiedBills.Count == 0)
                return false;

            if (workTable.BillStack == null || workTable.BillStack.Count >= 15)
                return false;

            _copiedBills.RemoveAll(bill => bill.DeletedOrDereferenced);

            foreach (var bill in _copiedBills)
            {
                if (CanWorkTableDoRecipeNow(workTable, bill.recipe))
                    return true;
            }

            return false;
        }

        public bool IsMultipleBillsCopied()
        {
            return _copiedBills.Count > 1;
        }

        public void DoPasteInto(Building_WorkTable workTable, bool link)
        {
            foreach (var sourceBill in _copiedBills)
            {
                if (sourceBill.DeletedOrDereferenced)
                    continue;

                if (!CanWorkTableDoRecipeNow(workTable, sourceBill.recipe))
                    continue;

                var newBill = (Bill_Production)sourceBill.recipe.MakeNewBill();
                workTable.BillStack.AddBill(newBill);

                Main.Instance.GetExtendedBillDataStorage().MirrorBills(sourceBill, newBill);

                if (link)
                {
                    Main.Instance.GetExtendedBillDataStorage().LinkBills(sourceBill, newBill);
                }
            }
        }

        private static bool CanWorkTableDoRecipeNow(Building_WorkTable workTable, RecipeDef recipe)
        {
            return workTable.BillStack.Count < 15 &&
                recipe.AvailableNow &&
                workTable.def.AllRecipes != null &&
                workTable.def.AllRecipes.Contains(recipe);
        }

        private readonly List<Bill_Production> _copiedBills = new List<Bill_Production>();
    }
}