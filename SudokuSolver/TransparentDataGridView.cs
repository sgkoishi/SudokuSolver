using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    /// <summary>
    /// https://stackoverflow.com/questions/1330220/set-datagrid-view-background-to-transparent
    /// </summary>
    public class TransparentDataGridView : DataGridView
    {
        public Image Screenshot;
        protected override void PaintBackground(Graphics graphics, Rectangle clipBounds, Rectangle gridBounds)
        {
            base.PaintBackground(graphics, clipBounds, gridBounds);
            if (this.Screenshot != null)
            {
                var rectSource = new Rectangle(this.Location.X, this.Location.Y, this.Width, this.Height);
                var rectDest = new Rectangle(0, 0, rectSource.Width, rectSource.Height);
                var b = new Bitmap(this.Parent.ClientRectangle.Width, this.Parent.ClientRectangle.Height);
                Graphics.FromImage(b).DrawImage(this.Screenshot, this.Parent.ClientRectangle);
                graphics.DrawImage(b, rectDest, rectSource, GraphicsUnit.Pixel);

                this.EnableHeadersVisualStyles = false;
                this.ColumnHeadersDefaultCellStyle.BackColor = Color.Transparent;
                this.RowHeadersDefaultCellStyle.BackColor = Color.Transparent;
                foreach (DataGridViewColumn col in this.Columns)
                {
                    col.DefaultCellStyle.BackColor = Color.Transparent;
                    col.DefaultCellStyle.SelectionBackColor = Color.Transparent;
                }
            }
        }
    }
}
