namespace ImageOptiz
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.listView = new System.Windows.Forms.ListView();
            this.statusColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.fileColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.originalSizeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.optimizedSizeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.differenceSizeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.percentageOptimizedColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.helpLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.statusColumnHeader,
            this.fileColumnHeader,
            this.originalSizeColumnHeader,
            this.optimizedSizeColumnHeader,
            this.differenceSizeColumnHeader,
            this.percentageOptimizedColumnHeader});
            this.listView.FullRowSelect = true;
            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, -3);
            this.listView.Name = "listView";
            this.listView.ShowItemToolTips = true;
            this.listView.Size = new System.Drawing.Size(564, 143);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // statusColumnHeader
            // 
            this.statusColumnHeader.Text = "";
            this.statusColumnHeader.Width = 20;
            // 
            // fileColumnHeader
            // 
            this.fileColumnHeader.Text = "File";
            this.fileColumnHeader.Width = 200;
            // 
            // originalSizeColumnHeader
            // 
            this.originalSizeColumnHeader.Text = "Original size";
            this.originalSizeColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.originalSizeColumnHeader.Width = 80;
            // 
            // optimizedSizeColumnHeader
            // 
            this.optimizedSizeColumnHeader.Text = "Optimized size";
            this.optimizedSizeColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.optimizedSizeColumnHeader.Width = 80;
            // 
            // differenceSizeColumnHeader
            // 
            this.differenceSizeColumnHeader.Text = "Difference";
            this.differenceSizeColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.differenceSizeColumnHeader.Width = 80;
            // 
            // percentageOptimizedColumnHeader
            // 
            this.percentageOptimizedColumnHeader.Text = "% Optimized";
            this.percentageOptimizedColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.percentageOptimizedColumnHeader.Width = 80;
            // 
            // helpLabel
            // 
            this.helpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.helpLabel.AutoSize = true;
            this.helpLabel.Location = new System.Drawing.Point(12, 159);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(262, 13);
            this.helpLabel.TabIndex = 2;
            this.helpLabel.Text = "To optimize images, drag&&drop them into this window";
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 189);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.listView);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(520, 200);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "ImageOptiz";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader statusColumnHeader;
        private System.Windows.Forms.ColumnHeader fileColumnHeader;
        private System.Windows.Forms.ColumnHeader originalSizeColumnHeader;
        private System.Windows.Forms.ColumnHeader optimizedSizeColumnHeader;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.ColumnHeader differenceSizeColumnHeader;
        private System.Windows.Forms.ColumnHeader percentageOptimizedColumnHeader;
    }
}

