namespace GrGoogleOCR
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnGo = new Button();
            PropGridSettings = new PropertyGrid();
            TbError = new TextBox();
            SuspendLayout();
            // 
            // BtnGo
            // 
            BtnGo.Location = new Point(12, 11);
            BtnGo.Name = "BtnGo";
            BtnGo.Size = new Size(218, 36);
            BtnGo.TabIndex = 0;
            BtnGo.Text = "Go";
            BtnGo.UseVisualStyleBackColor = true;
            BtnGo.Click += BtnGo_Click;
            // 
            // PropGridSettings
            // 
            PropGridSettings.HelpVisible = false;
            PropGridSettings.Location = new Point(12, 54);
            PropGridSettings.Name = "PropGridSettings";
            PropGridSettings.PropertySort = PropertySort.Alphabetical;
            PropGridSettings.Size = new Size(218, 253);
            PropGridSettings.TabIndex = 1;
            PropGridSettings.ToolbarVisible = false;
            // 
            // TbError
            // 
            TbError.BorderStyle = BorderStyle.FixedSingle;
            TbError.Location = new Point(246, 12);
            TbError.Multiline = true;
            TbError.Name = "TbError";
            TbError.ReadOnly = true;
            TbError.Size = new Size(218, 295);
            TbError.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(504, 332);
            Controls.Add(TbError);
            Controls.Add(PropGridSettings);
            Controls.Add(BtnGo);
            Name = "MainForm";
            Text = "GrGoogleOCR";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button BtnGo;
        private PropertyGrid PropGridSettings;
        private TextBox TbError;
    }
}
