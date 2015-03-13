namespace FreenetTray
{
    partial class PreferencesWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferencesWindow));
            this.StartupCheckboxList = new System.Windows.Forms.CheckedListBox();
            this.StartupLabel = new System.Windows.Forms.Label();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.ClosePreferencesButton = new System.Windows.Forms.Button();
            this.BrowserChoice = new System.Windows.Forms.ComboBox();
            this.BrowserLabel = new System.Windows.Forms.Label();
            this.BehaviorLabel = new System.Windows.Forms.Label();
            this.SlowStartOption = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LogLevelChoice = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CustomLocationDisplay = new System.Windows.Forms.TextBox();
            this.customLocationClear = new System.Windows.Forms.Button();
            this.CustomLocationBrowse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StartupCheckboxList
            // 
            this.StartupCheckboxList.BackColor = System.Drawing.SystemColors.Window;
            this.StartupCheckboxList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.StartupCheckboxList.CheckOnClick = true;
            resources.ApplyResources(this.StartupCheckboxList, "StartupCheckboxList");
            this.StartupCheckboxList.FormattingEnabled = true;
            this.StartupCheckboxList.Items.AddRange(new object[] {
            resources.GetString("StartupCheckboxList.Items"),
            resources.GetString("StartupCheckboxList.Items1")});
            this.StartupCheckboxList.Name = "StartupCheckboxList";
            // 
            // StartupLabel
            // 
            resources.ApplyResources(this.StartupLabel, "StartupLabel");
            this.StartupLabel.Name = "StartupLabel";
            // 
            // ApplyButton
            // 
            resources.ApplyResources(this.ApplyButton, "ApplyButton");
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.Apply_Click);
            // 
            // ClosePreferencesButton
            // 
            this.ClosePreferencesButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.ClosePreferencesButton, "ClosePreferencesButton");
            this.ClosePreferencesButton.Name = "ClosePreferencesButton";
            this.ClosePreferencesButton.UseVisualStyleBackColor = true;
            this.ClosePreferencesButton.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // BrowserChoice
            // 
            resources.ApplyResources(this.BrowserChoice, "BrowserChoice");
            this.BrowserChoice.FormattingEnabled = true;
            this.BrowserChoice.Name = "BrowserChoice";
            // 
            // BrowserLabel
            // 
            resources.ApplyResources(this.BrowserLabel, "BrowserLabel");
            this.BrowserLabel.Name = "BrowserLabel";
            // 
            // BehaviorLabel
            // 
            resources.ApplyResources(this.BehaviorLabel, "BehaviorLabel");
            this.BehaviorLabel.Name = "BehaviorLabel";
            // 
            // SlowStartOption
            // 
            resources.ApplyResources(this.SlowStartOption, "SlowStartOption");
            this.SlowStartOption.Name = "SlowStartOption";
            this.SlowStartOption.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // LogLevelChoice
            // 
            resources.ApplyResources(this.LogLevelChoice, "LogLevelChoice");
            this.LogLevelChoice.FormattingEnabled = true;
            this.LogLevelChoice.Items.AddRange(new object[] {
            resources.GetString("LogLevelChoice.Items"),
            resources.GetString("LogLevelChoice.Items1"),
            resources.GetString("LogLevelChoice.Items2"),
            resources.GetString("LogLevelChoice.Items3"),
            resources.GetString("LogLevelChoice.Items4")});
            this.LogLevelChoice.Name = "LogLevelChoice";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // CustomLocationDisplay
            // 
            resources.ApplyResources(this.CustomLocationDisplay, "CustomLocationDisplay");
            this.CustomLocationDisplay.Name = "CustomLocationDisplay";
            this.CustomLocationDisplay.ReadOnly = true;
            // 
            // customLocationClear
            // 
            this.customLocationClear.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.customLocationClear, "customLocationClear");
            this.customLocationClear.Name = "customLocationClear";
            this.customLocationClear.UseVisualStyleBackColor = false;
            this.customLocationClear.Click += new System.EventHandler(this.customLocationClear_Click);
            // 
            // CustomLocationBrowse
            // 
            resources.ApplyResources(this.CustomLocationBrowse, "CustomLocationBrowse");
            this.CustomLocationBrowse.Name = "CustomLocationBrowse";
            this.CustomLocationBrowse.UseVisualStyleBackColor = true;
            this.CustomLocationBrowse.Click += new System.EventHandler(this.CustomLocationBrowse_Click);
            // 
            // PreferencesWindow
            // 
            this.AcceptButton = this.ApplyButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ClosePreferencesButton;
            this.Controls.Add(this.CustomLocationBrowse);
            this.Controls.Add(this.customLocationClear);
            this.Controls.Add(this.CustomLocationDisplay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LogLevelChoice);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SlowStartOption);
            this.Controls.Add(this.BehaviorLabel);
            this.Controls.Add(this.BrowserLabel);
            this.Controls.Add(this.BrowserChoice);
            this.Controls.Add(this.ClosePreferencesButton);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.StartupLabel);
            this.Controls.Add(this.StartupCheckboxList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreferencesWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox StartupCheckboxList;
        private System.Windows.Forms.Label StartupLabel;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button ClosePreferencesButton;
        private System.Windows.Forms.ComboBox BrowserChoice;
        private System.Windows.Forms.Label BrowserLabel;
        private System.Windows.Forms.Label BehaviorLabel;
        private System.Windows.Forms.CheckBox SlowStartOption;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox LogLevelChoice;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CustomLocationDisplay;
        private System.Windows.Forms.Button customLocationClear;
        private System.Windows.Forms.Button CustomLocationBrowse;
    }
}