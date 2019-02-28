using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {
        private readonly Solver solver = new Solver();

        public Form1()
        {
            this.InitializeComponent();
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
        }

        private void DataGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            this.currentRule.Target = this.dataGridView.SelectedCells.Cast<DataGridViewCell>().Select(i => new Point(i.ColumnIndex, i.RowIndex)).Reverse().ToList();
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
            this.listBoxRules.Items.Clear();
            this.listBoxRules.Items.AddRange(this.solver.extraRules.Select(i => i.ToString()).Cast<object>().ToArray());
            this.textBoxLogs.Text = this.currentRule.ToString();
        }
    }
}
