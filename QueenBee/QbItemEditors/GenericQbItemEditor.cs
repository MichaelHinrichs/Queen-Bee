using Nanook.QueenBee.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nanook.QueenBee
{
    internal class GenericQbItemEditor : QbItemEditorBase
    {
        public GenericQbItemEditor() : base()
        {
            InitializeComponent();
        }

        private void GenericQbItemEditor_Load(object sender, EventArgs e)
        {
            bool tooManyItems = false;
            int spacing = 22;
            int top = 20;
            int tabIndex = TabIndex;
            int lblWidth = 0;

            List<GenericQbItem> gis = QbFile.GetGenericItems(QbItem);

            if (gis.Count > 500)
            {
                tooManyItems = true;
                AutoScrollMinSize = new Size(0, (top * 2) + spacing * (1 + 1) + 10); //+ 1 for button
            }
            else
                AutoScrollMinSize = new Size(0, (top * 2) + spacing * (gis.Count + 1) + 10); //+ 1 for button

            Tag = QbItem; //store item for update

            bool hasEditable = false;

            try
            {
                int qbItemFound = 0;
                if (gis.Count > 0 && gis[gis.Count - 1].SourceProperty == "ItemQbKey") //nasty hack
                {
                    addEditItem(spacing, ref top, ref lblWidth, ref hasEditable, gis[gis.Count - 1]);
                    qbItemFound = 1;
                }

                if (!tooManyItems)
                {
                    for (int i = 0; i < gis.Count - qbItemFound; i++)
                        addEditItem(spacing, ref top, ref lblWidth, ref hasEditable, gis[i]);
                }
            }
            catch (Exception ex)
            {
                ShowException("Edit Item List Error", ex);
            }

            foreach (GenericQbEditItem et in Controls)
                et.TextBoxLeft = lblWidth + 6;

            try
            {
                if (Controls.Count != 0)
                {
                    Button btnUpdateItems = new Button
                    {
                        Text = "&Update",
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };
                    btnUpdateItems.Left = ClientSize.Width - btnUpdateItems.Width - 15;
                    btnUpdateItems.Top = top + 10;
                    btnUpdateItems.Height = 22;
                    btnUpdateItems.Enabled = hasEditable;
                    btnUpdateItems.Click += new EventHandler(btnUpdateItems_Click);
                    Controls.Add(btnUpdateItems);
                }

                if (tooManyItems)
                    ShowError("Too Many Items", string.Format("This item contains {0} items. It is likely that it should be edited by a dedicated application.", gis.Count.ToString()));
            }
            catch (Exception ex)
            {
                ShowException("Update Button Error", ex);
            }
        }

        private void addEditItem(int spacing, ref int top, ref int lblWidth, ref bool hasEditable, GenericQbItem gi)
        {
            GenericQbEditItem ei;
            ei = new GenericQbEditItem();
            ei.SetData(gi);
            ei.Left = 0;
            ei.Width = ClientSize.Width;
            ei.Top = top;
            top += spacing;
            ei.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            if (!hasEditable && !gi.ReadOnly)
                hasEditable = true;

            if (gi.UseQbItemType)
                ei.ConvertTo(QbItemDataType);
            //else
            //    ei.ConvertTo(base.EditType);

            Controls.Add(ei);

            if (ei.LabelWidth > lblWidth)
                lblWidth = ei.LabelWidth;
        }



        private void btnUpdateItems_Click(object sender, EventArgs e)
        {
            List<GenericQbItem> gis = new List<GenericQbItem>();
            GenericQbEditItem ei;
            GenericQbItem gi;

            //check that all the items are valid before saving

            try
            {
                //Check if QbKey is in the debug file, if not then add it to the user defined list
                AddQbKeyToUserDebugFile(QbItem.ItemQbKey);

                foreach (Control un in Controls)
                {
                    if ((ei = un as GenericQbEditItem) != null)
                    {
                        if (!ei.IsValid)
                        {
                            ShowError("Error", "QB cannot be updated while data is invalid.");
                            return;
                        }
                        gi = ei.GenericQbItem;

                        //if QbKey, check to see if it's in the debug file, if not then add it to the user defined list
                        if (gi.Type == typeof(QbKey))
                            AddQbKeyToUserDebugFile(gi.ToQbKey());

                        gis.Add(gi);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Failed to Get Item Values", ex);
                return;
            }

            try
            {
                QbFile.SetGenericItems(QbItem, gis);
            }
            catch (Exception ex)
            {
                ShowException("Edit Values Update Error", ex);
                return;
            }

            UpdateQbItem();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // GenericQbItemEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            Name = "GenericQbItemEditor";
            Load += new System.EventHandler(GenericQbItemEditor_Load);
            ResumeLayout(false);

        }

    }
}
