using Nanook.QueenBee.Parser;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nanook.QueenBee
{
    internal class ScriptEditor : QbItemEditorBase
    {
        private System.Windows.Forms.Button btnUpdate;
        private GenericQbEditItem eiItemQbKey;
        private GenericQbEditItem eiUnknown;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabString;
        private System.Windows.Forms.TabPage tabUncompressedScript;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.TextBox txtItem;
        private System.Windows.Forms.ErrorProvider err;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ListBox lstItems;
        private System.Windows.Forms.TextBox txtWarning;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.SaveFileDialog export;
        private System.Windows.Forms.OpenFileDialog import;
        private System.Windows.Forms.TextBox txtScript;
    
        public ScriptEditor() : base()
        {
            InitializeComponent();
        }

        private void ScriptEditor_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            try
            {
                _qbItem = QbItem as QbItemScript;
                if (_qbItem == null)
                    throw new Exception("Error");

                eiItemQbKey.SetData(new GenericQbItem("Item QB Key", _qbItem.ItemQbKey, typeof(string), false, false, QbItemType.SectionScript, "ItemQbKey"));
                eiUnknown.SetData(new GenericQbItem("Unknown", _qbItem.Unknown, typeof(byte[]), false, false, QbItemType.SectionScript, "Unknown"));

                int w = (eiItemQbKey.LabelWidth > eiUnknown.LabelWidth ? eiItemQbKey.LabelWidth : eiUnknown.LabelWidth) + 6;
                eiItemQbKey.TextBoxLeft = w;
                eiUnknown.TextBoxLeft = w;

                txtScript.Text = bytesToHexAsciiString(_qbItem.ScriptData);

                _preventUpdate = false;

                loadStringList();
            }
            catch (Exception ex)
            {
                ShowException("Script Load Item Error", ex);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                GenericQbEditItem ei;
                foreach (Control un in Controls)
                {
                    if ((ei = un as GenericQbEditItem) != null)
                    {
                        if (!ei.IsValid)
                        {
                            ShowError("Error", "QB cannot be updated while data is invalid.");
                            return;
                        }
                    }
                }

                _qbItem.ItemQbKey = eiItemQbKey.GenericQbItem.ToQbKey();
                _qbItem.Unknown = eiUnknown.GenericQbItem.ToUInt32();

                byte[] script = _qbItem.ScriptData;
                for (int i = 0; i < lstItems.Items.Count; i++)
                    _qbItem.Strings[i].Text = (string)lstItems.Items[i];

                _qbItem.UpdateStrings();

                //if QbKey, check to see if it's in the debug file, if not then add it to the user defined list
                AddQbKeyToUserDebugFile(QbItem.ItemQbKey);

                loadStringList();
                txtScript.Text = bytesToHexAsciiString(_qbItem.ScriptData);

                UpdateQbItem();
            }
            catch (Exception ex)
            {
                ShowException("Script Update Item Error", ex);
            }
        }

        private string bytesToHexAsciiString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            //char[] ch = new char[cl];

            for (int i = 0; i < bytes.Length; i++)
            {

                sb.Append(bytes[i].ToString("X").PadLeft(2, '0'));
                sb.Append(" ");

                if (i % _cl == _cl - 1)
                {
                    sb.Append(": ");
                    for (int j = i - (_cl - 1); j <= i; j++)
                        sb.Append(byteToPrintableChar(bytes[j]));
                    sb.Append(Environment.NewLine);
                }
            }


            if (bytes.Length % _cl != 0)
            {
                for (int i = 0; i < _cl - (bytes.Length % _cl); i++)
                    sb.Append("   ");

                sb.Append(": ");

                for (int j = bytes.Length - (bytes.Length % _cl); j < bytes.Length; j++)
                    sb.Append(byteToPrintableChar(bytes[j]));

                for (int i = 0; i < _cl - (bytes.Length % _cl); i++)
                    sb.Append(" ");

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        private char byteToPrintableChar(byte b)
        {
            char c = (char)b;
            //if (b != 0x09 && (Char.IsSymbol(c) || Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c) || Char.IsPunctuation(c)))
            if (b != 0x09 && !char.IsControl(c))
                return c;
            else
                return '.';
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                clearError();
                refreshEditValue(getSelectedItem());
            }
            catch (Exception ex)
            {
                ShowException("Script Select Item Error", ex);
            }
        }

        private void txtItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Return)
                btnSet_Click(this, new EventArgs());
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                string errMsg = GenericQbItem.ValidateText(typeof(string), typeof(string), txtItem.Text);
                if (errMsg.Length != 0)
                    err.SetError(txtItem, errMsg);
                else
                {
                    try
                    {
                        lstItems.BeginUpdate();
                        int idx = getSelectedItem();
                        _preventUpdate = true;
                        lstItems.Items[idx] = ""; //force item to update, if only case has changed it won't update
                        lstItems.Items[idx] = txtItem.Text;

                    }
                    finally
                    {
                        _preventUpdate = false;
                        lstItems.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Script Set Item Error", ex);
            }
        }

        private int getSelectedItem()
        {
            int idx = lstItems.SelectedIndex;
            if (idx == -1)
                idx = 0;

            return idx;
        }
        
        private void refreshEditValue(int index)
        {
            if (!_preventUpdate && index != -1)
            {
                txtItem.MaxLength = _qbItem.Strings[index].Length;
                txtItem.Text = (string)lstItems.Items[index];

            }
        }

        private void clearError()
        {
            err.SetError(txtItem, string.Empty);
        }

        private void loadStringList()
        {
            txtItem.Text = string.Empty;
            lstItems.Items.Clear();
            foreach (ScriptString ss in _qbItem.Strings)
                lstItems.Items.Add(ss.Text);

            bool hasStrings = lstItems.Items.Count != 0;
            if (hasStrings)
                lstItems.SelectedIndex = 0;

            lstItems.Enabled = hasStrings;
            txtItem.Enabled = hasStrings;
            btnSet.Enabled = hasStrings;


        }

        private string getBestFullFilename(string fullFilename)
        {
            if (fullFilename == null)
                return string.Empty;
            else if (fullFilename.Trim().Length == 0)
                return string.Empty;

            string pth = fullFilename;
            FileInfo fi = new FileInfo(pth);
            if (!fi.Exists)
            {
                DirectoryInfo di = fi.Directory;
                while (!di.Exists)
                {
                    di = di.Parent;
                    if (di == null)
                        break;
                }
                if (di != null)
                    pth = di.FullName;
                else
                    pth = string.Empty;

                pth = Path.Combine(pth, fi.Name);
            }
            return pth;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                string fname = string.Format("{0}_{1}.{2}", _qbItem.Root.Filename.Replace('\\', '#').Replace('/', '#').Replace('.', '#'), _qbItem.ItemQbKey.Crc.ToString("X").PadLeft(8, '0'), _fileExt);

                if (AppState.LastScriptPath.Length == 0)
                    fname = Path.Combine(AppState.LastScriptPath, fname);

                fname = getBestFullFilename(fname);

                export.Filter = string.Format("{0} (*.{0})|*.{0}|All files (*.*)|*.*", _fileExt);
                export.Title = string.Format("Export {0} file", _fileExt);
                export.OverwritePrompt = true;
                export.FileName = fname;

                if (export.ShowDialog(this) != DialogResult.Cancel)
                {
                    fname = export.FileName;
                    if (File.Exists(fname))
                        File.Delete(fname);

                    AppState.LastScriptPath = new FileInfo(fname).DirectoryName;

                    File.WriteAllBytes(fname, _qbItem.ScriptData);
                }
            }
            catch (Exception ex)
            {
                ShowException("Script Export Error", ex);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                string fname = string.Format("{0}_{1}.{2}", _qbItem.Root.Filename.Replace('\\', '#').Replace('/', '#').Replace('.', '#'), _qbItem.ItemQbKey.Crc.ToString("X").PadLeft(8, '0'), _fileExt);

                if (AppState.LastScriptPath.Length == 0)
                    fname = Path.Combine(AppState.LastScriptPath, fname);

                fname = getBestFullFilename(fname);


                import.Filter = string.Format("{0} (*.{0})|*.{0}|All files (*.*)|*.*", _fileExt);
                import.Title = string.Format("Import {0} file", _fileExt);
                import.CheckFileExists = true;
                import.CheckPathExists = true;
                import.FileName = fname;

                if (import.ShowDialog(this) != DialogResult.Cancel)
                {
                    fname = import.FileName;
                    _qbItem.ScriptData = File.ReadAllBytes(fname);
                    txtScript.Text = bytesToHexAsciiString(_qbItem.ScriptData);

                    AppState.LastScriptPath = new FileInfo(fname).DirectoryName;

                    loadStringList();
                }
            }
            catch (Exception ex)
            {
                ShowException("Script Import Error", ex);
            }
        }

        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AppState.ScriptActiveTab = tabs.SelectedIndex;
            }
            catch (Exception ex)
            {
                ShowException("Script Tab Select Error", ex);
            }
        }

        public int SelectedTabIndex
        {
            get { return tabs.SelectedIndex; }
            set { tabs.SelectedIndex = value; }
        }


        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditor));
            txtScript = new System.Windows.Forms.TextBox();
            btnUpdate = new System.Windows.Forms.Button();
            eiItemQbKey = new Nanook.QueenBee.GenericQbEditItem();
            eiUnknown = new Nanook.QueenBee.GenericQbEditItem();
            tabs = new System.Windows.Forms.TabControl();
            tabString = new System.Windows.Forms.TabPage();
            txtWarning = new System.Windows.Forms.TextBox();
            btnSet = new System.Windows.Forms.Button();
            txtItem = new System.Windows.Forms.TextBox();
            lstItems = new System.Windows.Forms.ListBox();
            tabUncompressedScript = new System.Windows.Forms.TabPage();
            err = new System.Windows.Forms.ErrorProvider(components);
            export = new System.Windows.Forms.SaveFileDialog();
            import = new System.Windows.Forms.OpenFileDialog();
            btnExport = new System.Windows.Forms.Button();
            btnImport = new System.Windows.Forms.Button();
            tabs.SuspendLayout();
            tabString.SuspendLayout();
            tabUncompressedScript.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)err).BeginInit();
            SuspendLayout();
            // 
            // txtScript
            // 
            txtScript.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                        | AnchorStyles.Left
                        | AnchorStyles.Right;
            txtScript.BackColor = System.Drawing.SystemColors.Window;
            txtScript.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtScript.HideSelection = false;
            txtScript.Location = new System.Drawing.Point(0, 1);
            txtScript.Multiline = true;
            txtScript.Name = "txtScript";
            txtScript.ReadOnly = true;
            txtScript.ScrollBars = ScrollBars.Both;
            txtScript.Size = new System.Drawing.Size(303, 336);
            txtScript.TabIndex = 0;
            txtScript.WordWrap = false;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new System.Drawing.Point(240, 431);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new System.Drawing.Size(75, 23);
            btnUpdate.TabIndex = 5;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += new System.EventHandler(btnUpdate_Click);
            // 
            // eiItemQbKey
            // 
            eiItemQbKey.Anchor = AnchorStyles.Top | AnchorStyles.Left
                        | AnchorStyles.Right;
            eiItemQbKey.Location = new System.Drawing.Point(-4, 9);
            eiItemQbKey.Name = "eiItemQbKey";
            eiItemQbKey.Size = new System.Drawing.Size(322, 24);
            eiItemQbKey.TabIndex = 0;
            eiItemQbKey.TextBoxLeft = 66;
            // 
            // eiUnknown
            // 
            eiUnknown.Anchor = AnchorStyles.Top | AnchorStyles.Left
                        | AnchorStyles.Right;
            eiUnknown.Location = new System.Drawing.Point(-4, 32);
            eiUnknown.Name = "eiUnknown";
            eiUnknown.Size = new System.Drawing.Size(322, 24);
            eiUnknown.TabIndex = 1;
            eiUnknown.TextBoxLeft = 66;
            // 
            // tabs
            // 
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                        | AnchorStyles.Left
                        | AnchorStyles.Right;
            tabs.Controls.Add(tabString);
            tabs.Controls.Add(tabUncompressedScript);
            tabs.Location = new System.Drawing.Point(3, 62);
            tabs.Name = "tabs";
            tabs.SelectedIndex = 0;
            tabs.Size = new System.Drawing.Size(312, 363);
            tabs.TabIndex = 2;
            tabs.SelectedIndexChanged += new System.EventHandler(tabs_SelectedIndexChanged);
            // 
            // tabString
            // 
            tabString.Controls.Add(txtWarning);
            tabString.Controls.Add(btnSet);
            tabString.Controls.Add(txtItem);
            tabString.Controls.Add(lstItems);
            tabString.Location = new System.Drawing.Point(4, 22);
            tabString.Name = "tabString";
            tabString.Padding = new System.Windows.Forms.Padding(3);
            tabString.Size = new System.Drawing.Size(304, 337);
            tabString.TabIndex = 0;
            tabString.Text = "Strings";
            tabString.UseVisualStyleBackColor = true;
            // 
            // txtWarning
            // 
            txtWarning.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                        | AnchorStyles.Right;
            txtWarning.BorderStyle = BorderStyle.None;
            txtWarning.Location = new System.Drawing.Point(2, 259);
            txtWarning.Multiline = true;
            txtWarning.Name = "txtWarning";
            txtWarning.ReadOnly = true;
            txtWarning.Size = new System.Drawing.Size(300, 78);
            txtWarning.TabIndex = 3;
            txtWarning.Text = resources.GetString("txtWarning.Text");
            // 
            // btnSet
            // 
            btnSet.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSet.Location = new System.Drawing.Point(249, 229);
            btnSet.Name = "btnSet";
            btnSet.Size = new System.Drawing.Size(35, 21);
            btnSet.TabIndex = 2;
            btnSet.Text = "Set";
            btnSet.UseVisualStyleBackColor = true;
            btnSet.Click += new System.EventHandler(btnSet_Click);
            // 
            // txtItem
            // 
            txtItem.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                        | AnchorStyles.Right;
            err.SetIconPadding(txtItem, 37);
            txtItem.Location = new System.Drawing.Point(0, 229);
            txtItem.Name = "txtItem";
            txtItem.Size = new System.Drawing.Size(249, 20);
            txtItem.TabIndex = 1;
            txtItem.KeyDown += new System.Windows.Forms.KeyEventHandler(txtItem_KeyDown);
            // 
            // lstItems
            // 
            lstItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                        | AnchorStyles.Left
                        | AnchorStyles.Right;
            lstItems.FormattingEnabled = true;
            lstItems.IntegralHeight = false;
            lstItems.Location = new System.Drawing.Point(0, 0);
            lstItems.Name = "lstItems";
            lstItems.Size = new System.Drawing.Size(304, 223);
            lstItems.TabIndex = 0;
            lstItems.SelectedIndexChanged += new System.EventHandler(lstItems_SelectedIndexChanged);
            // 
            // tabUncompressedScript
            // 
            tabUncompressedScript.Controls.Add(txtScript);
            tabUncompressedScript.Location = new System.Drawing.Point(4, 22);
            tabUncompressedScript.Name = "tabUncompressedScript";
            tabUncompressedScript.Padding = new System.Windows.Forms.Padding(3);
            tabUncompressedScript.Size = new System.Drawing.Size(304, 337);
            tabUncompressedScript.TabIndex = 1;
            tabUncompressedScript.Text = "Uncompressed Script";
            tabUncompressedScript.UseVisualStyleBackColor = true;
            // 
            // err
            // 
            err.ContainerControl = this;
            // 
            // export
            // 
            export.AddExtension = false;
            // 
            // import
            // 
            import.Title = "Open QB to Replace in PAK";
            // 
            // btnExport
            // 
            btnExport.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExport.Location = new System.Drawing.Point(3, 431);
            btnExport.Name = "btnExport";
            btnExport.Size = new System.Drawing.Size(75, 23);
            btnExport.TabIndex = 3;
            btnExport.Text = "Export...";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += new System.EventHandler(btnExport_Click);
            // 
            // btnImport
            // 
            btnImport.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnImport.Location = new System.Drawing.Point(84, 431);
            btnImport.Name = "btnImport";
            btnImport.Size = new System.Drawing.Size(75, 23);
            btnImport.TabIndex = 4;
            btnImport.Text = "Import...";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += new System.EventHandler(btnImport_Click);
            // 
            // ScriptEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            Controls.Add(btnImport);
            Controls.Add(btnExport);
            Controls.Add(eiItemQbKey);
            Controls.Add(eiUnknown);
            Controls.Add(tabs);
            Controls.Add(btnUpdate);
            Name = "ScriptEditor";
            Size = new System.Drawing.Size(318, 461);
            Load += new System.EventHandler(ScriptEditor_Load);
            tabs.ResumeLayout(false);
            tabString.ResumeLayout(false);
            tabString.PerformLayout();
            tabUncompressedScript.ResumeLayout(false);
            tabUncompressedScript.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)err).EndInit();
            ResumeLayout(false);

        }

        private bool _preventUpdate;
        private const int _cl = 16; //chars per line
        private const string _fileExt = "qbScript";
        private QbItemScript _qbItem;



    }
}
