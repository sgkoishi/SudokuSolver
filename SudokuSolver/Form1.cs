using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Chireiden.SudokuSolver
{
    public partial class Form1 : Form
    {
        private readonly Solver solver = new Solver();
        public Rule currentRule = new Rule();
        public Image Screenshot;
        public Rectangle[,] Background;
        public Dictionary<RuleInfo, int>[,] RulesCovered = new Dictionary<RuleInfo, int>[9, 9];

        public Form1()
        {
            this.InitializeComponent();
            this.dataGridView.KeyUp += this.DataGridView_KeyUp;
            this.dataGridView.CellEnter += this.DataGridView_CellEnter;
            this.pictureBox1.LoadCompleted += this.PictureBox1_LoadCompleted;
            this.dataGridView.CellPainting += this.DataGridView_CellPainting;
            this.comboBoxRule.DrawItem += this.ComboBoxRule_DrawItem;
            this.pictureBox1.LoadAsync("https://www.sgkoishi.app/img/koishi.png");
            this.Text += " v" + Assembly.GetExecutingAssembly().GetName().Version;
            this.buttonStart.Focus();
            for (var i = 0; i < 9; i++)
            {
                this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn
                {
                    MaxInputLength = 1,
                    Width = 45
                });
                this.dataGridView.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dataGridView.Rows.Add(new DataGridViewRow
                {
                    Height = 45
                });
            }
            // mark the 9 square areas consisting of 9 cells
            this.dataGridView.Columns[2].DividerWidth = 2;
            this.dataGridView.Columns[5].DividerWidth = 2;
            this.dataGridView.Rows[2].DividerHeight = 2;
            this.dataGridView.Rows[5].DividerHeight = 2;
            this.dataGridView.RowHeadersVisible = this.dataGridView.ColumnHeadersVisible = false;

            this.RulesCovered = new Dictionary<RuleInfo, int>[this.solver.Height, this.solver.Width];
            for (var i = 0; i < this.solver.Height; i++)
            {
                for (var j = 0; j < this.solver.Width; j++)
                {
                    if (this.RulesCovered[i, j] == null)
                    {
                        this.RulesCovered[i, j] = new Dictionary<RuleInfo, int>();
                    }
                }
            }
            this.comboBoxRule.Items.Clear();
            this.comboBoxRule.Items.AddRange(RuleInfoHelper.GetAll());
            this.comboBoxRule.SelectedIndex = 0;
            this.dataGridView.DefaultCellStyle.SelectionForeColor = Color.Black;
            var rects = new Rectangle[9, 9];
            for (var i = 0; i < this.solver.Height; i++)
            {
                for (var j = 0; j < this.solver.Width; j++)
                {
                    rects[j, i] = new Rectangle(i * 45, j * 45, 45, 45);
                }
            }
            this.Background = rects;
        }

        private void ComboBoxRule_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            var text = ((ComboBox) sender).Items[e.Index].ToString();
            var item = RuleInfoHelper.Get(text);
            e.Graphics.DrawString(text, ((Control) sender).Font, new SolidBrush(Color.FromArgb(item.ColorR, item.ColorG, item.ColorB)), e.Bounds.X, e.Bounds.Y);
        }

        private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var selected = this.currentRule.Target.Any(p => p.X == e.RowIndex && p.Y == e.ColumnIndex);
            if (this.Screenshot != null)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.White), this.Background[e.RowIndex, e.ColumnIndex]);
                e.Graphics.DrawImage(this.Screenshot, e.CellBounds, this.Background[e.RowIndex, e.ColumnIndex], GraphicsUnit.Pixel);
            }
            else
            {
                e.PaintBackground(e.CellBounds, selected);
            }
            e.PaintContent(e.CellBounds);
            if (this.Screenshot != null && selected)
            {
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(127, 0, 0, 255)), this.Background[e.RowIndex, e.ColumnIndex]);
            }
            foreach (var item in this.RulesCovered[e.RowIndex, e.ColumnIndex])
            {
                var rule = RuleInfoHelper.Get(item.Key);
                var brush = new SolidBrush(Color.FromArgb(rule.ColorA, rule.ColorB, rule.ColorG, rule.ColorB));
                for (var i = 0; i < item.Value; i++)
                {
                    e.Graphics.FillRectangle(brush, this.Background[e.RowIndex, e.ColumnIndex]);
                }
            }
            if (!e.Handled)
            {
                e.Handled = true;
            }
        }

        private void PictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.label1.Location = new Point(92, 62);
            this.label1.Text = e.Error != null || e.Cancelled ? "There should be a icon in the picture box ->" : "";
        }

        private void DataGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.currentRule.Target = this.dataGridView.SelectedCells.Cast<DataGridViewCell>().Select(i => new Point(i.RowIndex, i.ColumnIndex)).Reverse().ToList();
            this.UpdateListBox();
        }

        private void DataGridView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyValue == (int) Keys.V)
                {
                    var input = Clipboard.GetText().Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("\t", "");
                    if (input.Length == this.solver.Width * this.solver.Height)
                    {
                        this.dataGridView.SuspendLayout();
                        for (var i = 0; i < input.Length; i++)
                        {
                            int.TryParse(input[i].ToString(), out var value);
                            this.SetElement(i / this.solver.Width, i % this.solver.Width, value);
                        }
                        this.dataGridView.ResumeLayout();
                    }
                    return;
                }
                else if (e.KeyValue == (int) Keys.C)
                {
                    var result = "";
                    for (var i = 0; i < this.solver.Width * this.solver.Height; i++)
                    {
                        result += this.solver.resultArray[i / this.solver.Width, i % this.solver.Width];
                    }
                    Clipboard.SetText(result.Replace("0", "."));
                    return;
                }
            }
            var number = 0;
            var change = false;
            if (e.KeyValue >= (int) Keys.D0 && e.KeyValue <= (int) Keys.D9)
            {
                number = e.KeyValue - 48;
                change = true;
            }
            else if (e.KeyValue >= (int) Keys.NumPad0 && e.KeyValue <= (int) Keys.NumPad9)
            {
                number = e.KeyValue - 96;
                change = true;
            }
            if (e.KeyValue == (int) Keys.Back || e.KeyValue == (int) Keys.Delete)
            {
                change = true;
            }
            if (change)
            {
                if (number == 0)
                {
                    this.dataGridView.SuspendLayout();
                    foreach (DataGridViewTextBoxCell item in this.dataGridView.SelectedCells)
                    {
                        this.SetElement(item.RowIndex, item.ColumnIndex, 0);
                    }
                    this.dataGridView.ResumeLayout();
                }
                else
                {
                    this.SetElement(this.dataGridView.CurrentCell.RowIndex, this.dataGridView.CurrentCell.ColumnIndex, number);
                }
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (this.currentRule.Valid())
            {
                foreach (var item in this.solver.extraRules)
                {
                    if (item.Type == this.currentRule.Type && RuleInfoHelper.Get(item.Type).Unique)
                    {
                        this.currentRule = new Rule();
                        this.UpdateListBox();
                        return;
                    }
                }
                this.solver.extraRules.Add(this.currentRule);
            }
            this.currentRule = new Rule();
            this.UpdateListBox();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.currentRule = new Rule();
            this.UpdateListBox();
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            this.solver.extraRules.RemoveAll(i => i.ToString() == (string) this.listBoxRules.SelectedItem);
            this.UpdateListBox();
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            this.solver.extraRules.Clear();
            this.UpdateListBox();
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            var startTime = DateTime.Now;
            new Thread(() =>
            {
                var result = this.solver.Solve();
                if (result == null)
                {
                    this.labelLog.Invoke(new MethodInvoker(() => this.labelLog.Text = "SudokuNotSolved!"));
                    this.buttonStart.Invoke(new MethodInvoker(() => this.buttonStart.Enabled = true));
                    return;
                }
                this.dataGridView.SuspendLayout();
                for (var i = 0; i < this.solver.Height; i++)
                {
                    for (var j = 0; j < this.solver.Width; j++)
                    {
                        this.SetElement(i, j, int.Parse(result[i, j]));
                    }
                }
                this.dataGridView.ResumeLayout();
                this.buttonStart.Invoke(new MethodInvoker(() => this.buttonStart.Enabled = true));
                var endTime = DateTime.Now;
                var take = endTime - startTime;
                this.labelLog.Invoke(new MethodInvoker(() => this.labelLog.Text = $"[{endTime:hh:mm:ss}] Done in {take:mm':'ss':'fff}."));
            }).Start();
            this.buttonStart.Enabled = false;
            this.labelLog.Text = $"[{startTime:hh:mm:ss}] Start solve.";
        }

        private void ComboBoxRule_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateListBox();
        }

        private void TextBoxExtra_Validating(object sender, CancelEventArgs e)
        {
            var s = this.textBoxExtra.Text;
            if (!int.TryParse(s, out var value) && !string.IsNullOrWhiteSpace(s))
            {
                e.Cancel = true;
                return;
            }
            this.currentRule.Extra = value;
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                this.Screenshot = Clipboard.GetImage();
            }
            if (this.Screenshot != null)
            {
                var destRect = new Rectangle(0, 0, 405, 405);
                var destImage = new Bitmap(405, 405);

                destImage.SetResolution(this.Screenshot.HorizontalResolution, this.Screenshot.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(this.Screenshot, destRect, 0, 0, this.Screenshot.Width, this.Screenshot.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
                this.dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
                for (var i = 0; i < this.solver.Width; i++)
                {
                    this.dataGridView.Columns[i].DividerWidth = 0;
                    this.dataGridView.Rows[i].DividerHeight = 0;
                }
                this.Screenshot = destImage;
                this.Refresh();
            }
        }

        public void SetElement(int x, int y, int number)
        {
            this.solver.resultArray[x, y] = number;
            this.dataGridView.Rows[x].Cells[y].Value = number == 0 ? "" : number.ToString();
        }

        private void UpdateListBox()
        {
            this.currentRule.Type = RuleInfoHelper.Get(this.comboBoxRule.SelectedItem.ToString()).Value;
            this.currentRule.Extra = int.Parse(this.textBoxExtra.Text);
            var ti = this.listBoxRules.TopIndex;
            var selected = this.listBoxRules.SelectedIndex;
            // No idea why databinding not working so manually update listbox
            this.listBoxRules.Items.Clear();
            this.listBoxRules.Items.AddRange(this.solver.extraRules.Select(i => i.ToString()).Cast<object>().ToArray());
            this.listBoxRules.TopIndex = ti;
            if (this.listBoxRules.Items.Count > 0)
            {
                this.listBoxRules.SelectedIndex = Math.Min(this.listBoxRules.Items.Count - 1, selected);
            }
            this.textBoxLogs.Text = this.currentRule.ToString();
            this.RulesCovered = new Dictionary<RuleInfo, int>[this.solver.Height, this.solver.Width];
            for (var i = 0; i < this.solver.Height; i++)
            {
                for (var j = 0; j < this.solver.Width; j++)
                {
                    if (this.RulesCovered[i, j] == null)
                    {
                        this.RulesCovered[i, j] = new Dictionary<RuleInfo, int>();
                    }
                }
            }
            foreach (var item in this.solver.extraRules)
            {
                foreach (var p in item.Target)
                {
                    if (this.RulesCovered[p.X, p.Y].ContainsKey(item.Type))
                    {
                        this.RulesCovered[p.X, p.Y][item.Type]++;
                    }
                    else
                    {
                        this.RulesCovered[p.X, p.Y][item.Type] = 1;
                    }
                }
            }
            this.dataGridView.Refresh();
        }
    }
}
