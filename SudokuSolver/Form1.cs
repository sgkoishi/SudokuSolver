using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Chireiden.SudokuSolver
{
    public partial class Form1 : Form
    {
        private readonly Solver solver = new Solver();

        public Form1()
        {
            this.InitializeComponent();
            this.Text += " v" + Assembly.GetExecutingAssembly().GetName().Version;
            this.buttonStart.Focus();
            this.dataGridView.RowHeadersVisible = this.dataGridView.ColumnHeadersVisible = false;
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
            this.dataGridView.KeyUp += this.DataGridView_KeyUp;
            this.dataGridView.CellEnter += this.DataGridView_CellEnter;
            this.comboBoxRule.Items.Clear();
            this.comboBoxRule.Items.AddRange(RuleTypeHelper.GetAll());
            this.comboBoxRule.SelectedIndex = 0;
            this.pictureBox1.LoadCompleted += this.PictureBox1_LoadCompleted;
            this.pictureBox1.LoadAsync("https://www.sgkoishi.app/img/koishi.png");
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
            if (e.Control && e.KeyValue == (int) Keys.V) // Ctrl + V
            {
                var input = Clipboard.GetText().Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("\t", "");
                if (input.Length == this.solver.Width * this.solver.Height)
                {
                    for (var i = 0; i < input.Length; i++)
                    {
                        int.TryParse(input[i].ToString(), out var value);
                        this.SetElement(i / this.solver.Width, i % this.solver.Width, value);
                    }
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
                    foreach (DataGridViewTextBoxCell item in this.dataGridView.SelectedCells)
                    {
                        this.SetElement(item.RowIndex, item.ColumnIndex, 0);
                    }
                }
                else
                {
                    this.SetElement(this.dataGridView.CurrentCell.RowIndex, this.dataGridView.CurrentCell.ColumnIndex, number);
                }
            }
        }

        public Rule currentRule = new Rule();
        public void SetElement(int x, int y, int number)
        {
            this.solver.resultArray[x, y] = number;
            this.dataGridView.Rows[x].Cells[y].Value = number == 0 ? "" : number.ToString();
        }
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (this.currentRule.Valid())
            {
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

        private void UpdateListBox()
        {
            this.currentRule.Type = RuleTypeHelper.Get((string) this.comboBoxRule.SelectedItem);
            this.listBoxRules.Items.Clear();
            this.listBoxRules.Items.AddRange(this.solver.extraRules.Select(i => i.ToString()).Cast<object>().ToArray());
            this.textBoxLogs.Text = this.currentRule.ToString();
        }

        private void ButtonStart_Click(object sender, EventArgs e)
        {
            var startTime = DateTime.Now;
            new Thread(() =>
            {
                var result = this.solver.Solve();
                if (result == null)
                {
                    this.labelLog.Invoke(new MethodInvoker(() => this.labelLog.Text = $"SudokuNotSolved!"));
                    this.buttonStart.Invoke(new MethodInvoker(() => this.buttonStart.Enabled = true));
                    return;
                }
                for (var i = 0; i < this.solver.Height; i++)
                {
                    for (var j = 0; j < this.solver.Width; j++)
                    {
                        this.SetElement(i, j, int.Parse(result[i, j]));
                    }
                }
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
            if (string.IsNullOrWhiteSpace(s))
            {
                this.currentRule.Extra = 0;
            }
            else if (!int.TryParse(s, out var value))
            {
                e.Cancel = true;
            }
            else
            {
                this.currentRule.Extra = value;
            }
        }
    }
}
