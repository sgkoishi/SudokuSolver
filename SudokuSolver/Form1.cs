﻿using System;
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
            foreach (var fieldInfo in typeof(RuleType).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!fieldInfo.GetCustomAttributes<NotIImplementedAttribute>().Any())
                {
                    this.comboBoxRule.Items.Add(fieldInfo.Name);
                }
            }
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
            var number = 0;
            if (e.KeyValue >= 48 && e.KeyValue <= 57)
            {
                number = e.KeyValue - 48;
            }
            else if (e.KeyValue >= 96 && e.KeyValue <= 105)
            {
                number = e.KeyValue - 96;
            }
            this.dataGridView.CurrentCell.Value = number > 0 ? number.ToString() : "";
            this.solver.resultArray[this.dataGridView.CurrentCellAddress.Y, this.dataGridView.CurrentCellAddress.X] = number;
        }

        public Rule currentRule = new Rule();

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
            this.currentRule.Type = (RuleType) Enum.Parse(typeof(RuleType), (string) this.comboBoxRule.SelectedItem);
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
                for (var i = 0; i < this.solver.Height; i++)
                {
                    for (var j = 0; j < this.solver.Width; j++)
                    {
                        this.dataGridView.Rows[i].Cells[j].Value = result[i, j];
                    }
                }
                this.buttonStart.Enabled = true;
                var endTime = DateTime.Now;
                var take = endTime - startTime;
                this.labelLog.Text = $"[{endTime:hh:mm:ss}] Done in {take:mm':'ss':'fff}.";
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
