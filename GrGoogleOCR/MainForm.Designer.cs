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
            Syncfusion.Windows.Forms.PdfViewer.MessageBoxSettings messageBoxSettings1 = new Syncfusion.Windows.Forms.PdfViewer.MessageBoxSettings();
            Syncfusion.Windows.PdfViewer.PdfViewerPrinterSettings pdfViewerPrinterSettings1 = new Syncfusion.Windows.PdfViewer.PdfViewerPrinterSettings();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Syncfusion.Windows.Forms.PdfViewer.TextSearchSettings textSearchSettings1 = new Syncfusion.Windows.Forms.PdfViewer.TextSearchSettings();
            BtnGo = new Button();
            PropGridSettings = new PropertyGrid();
            TbError = new TextBox();
            PdfViewer = new Syncfusion.Windows.Forms.PdfViewer.PdfDocumentView();
            SuspendLayout();
            // 
            // BtnGo
            // 
            BtnGo.Location = new Point(12, 12);
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
            TbError.Location = new Point(12, 324);
            TbError.Multiline = true;
            TbError.Name = "TbError";
            TbError.ReadOnly = true;
            TbError.Size = new Size(218, 461);
            TbError.TabIndex = 2;
            // 
            // PdfViewer
            // 
            PdfViewer.AutoScroll = true;
            PdfViewer.BackColor = Color.FromArgb(237, 237, 237);
            PdfViewer.BorderStyle = BorderStyle.FixedSingle;
            PdfViewer.CursorMode = Syncfusion.Windows.Forms.PdfViewer.PdfViewerCursorMode.SelectTool;
            PdfViewer.EnableContextMenu = true;
            PdfViewer.HorizontalScrollOffset = 0;
            PdfViewer.IsTextSearchEnabled = true;
            PdfViewer.IsTextSelectionEnabled = true;
            PdfViewer.Location = new Point(246, 12);
            messageBoxSettings1.EnableNotification = true;
            PdfViewer.MessageBoxSettings = messageBoxSettings1;
            PdfViewer.MinimumZoomPercentage = 50;
            PdfViewer.Name = "PdfViewer";
            PdfViewer.PageBorderThickness = 1;
            pdfViewerPrinterSettings1.Copies = 1;
            pdfViewerPrinterSettings1.PageOrientation = Syncfusion.Windows.PdfViewer.PdfViewerPrintOrientation.Auto;
            pdfViewerPrinterSettings1.PageSize = Syncfusion.Windows.PdfViewer.PdfViewerPrintSize.ActualSize;
            pdfViewerPrinterSettings1.PrintLocation = (PointF)resources.GetObject("pdfViewerPrinterSettings1.PrintLocation");
            pdfViewerPrinterSettings1.ShowPrintStatusDialog = true;
            PdfViewer.PrinterSettings = pdfViewerPrinterSettings1;
            PdfViewer.ReferencePath = null;
            PdfViewer.ScrollDisplacementValue = 0;
            PdfViewer.ShowHorizontalScrollBar = true;
            PdfViewer.ShowVerticalScrollBar = true;
            PdfViewer.Size = new Size(585, 774);
            PdfViewer.SpaceBetweenPages = 8;
            PdfViewer.TabIndex = 3;
            textSearchSettings1.CurrentInstanceColor = Color.FromArgb(127, 255, 171, 64);
            textSearchSettings1.HighlightAllInstance = true;
            textSearchSettings1.OtherInstanceColor = Color.FromArgb(127, 254, 255, 0);
            PdfViewer.TextSearchSettings = textSearchSettings1;
            PdfViewer.ThemeName = "Default";
            PdfViewer.VerticalScrollOffset = 0;
            PdfViewer.VisualStyle = Syncfusion.Windows.Forms.PdfViewer.VisualStyle.Default;
            PdfViewer.ZoomMode = Syncfusion.Windows.Forms.PdfViewer.ZoomMode.Default;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(843, 798);
            Controls.Add(PdfViewer);
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
        private Syncfusion.Windows.Forms.PdfViewer.PdfDocumentView PdfViewer;
    }
}
