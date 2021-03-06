﻿using Microsoft.Z3;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chireiden.SudokuSolver
{
    public class Solver
    {
        public int MinValue = 1;
        public int MaxValue = 9;
        public int SquareWidth = 3;
        public int SquareHeight = 3;
        public int SquareCountWid = 3;
        public int SquareCountHei = 3;
        public int Height = 9;
        public int Width = 9;
        private readonly Context _ctx;
        public IntExpr[,] array;
        public int[,] resultArray;
        public ReadOnlyCollection<BoolExpr> _rules;
        public List<Rule> extraRules;

        public Solver()
        {
            this._ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
            this.array = new IntExpr[this.Height, this.Width];
            this.resultArray = new int[this.Height, this.Width];
            this.extraRules = new List<Rule>();
            for (var i = 0; i < this.Height; i++)
            {
                for (var j = 0; j < this.Width; j++)
                {
                    this.array[i, j] = (IntExpr) this._ctx.MkConst(this._ctx.MkSymbol("x_" + (i + 1) + "_" + (j + 1)), this._ctx.IntSort);
                }
            }

            var list = new List<BoolExpr>();
            for (var i = 0; i < this.Height; i++)
            {
                for (var j = 0; j < this.Width; j++)
                {
                    // In range check
                    list.Add(this._ctx.MkAnd(this._ctx.MkLe(this._ctx.MkInt(this.MinValue), this.array[i, j]), this._ctx.MkLe(this.array[i, j], this._ctx.MkInt(this.MaxValue))));
                }
            }

            for (var i = 0; i < this.Height; i++)
            {
                var row = new IntExpr[this.Height];
                for (var j = 0; j < this.Width; j++)
                {
                    row[j] = this.array[i, j];
                }
                // Row
                list.Add(this._ctx.MkDistinct(row));
            }

            for (var j = 0; j < this.Width; j++)
            {
                var column = new IntExpr[this.Height];
                for (var i = 0; i < this.Height; i++)
                {
                    column[i] = this.array[i, j];
                }
                // Column
                list.Add(this._ctx.MkDistinct(column));
            }

            for (var squarei = 0; squarei < this.SquareCountWid; squarei++)
            {
                for (var squarej = 0; squarej < this.SquareCountHei; squarej++)
                {
                    var square = new IntExpr[this.SquareWidth * this.SquareHeight];
                    for (var i = 0; i < this.SquareWidth; i++)
                    {
                        for (var j = 0; j < this.SquareHeight; j++)
                        {
                            square[(this.SquareWidth * i) + j] = this.array[(this.SquareWidth * squarei) + i, (this.SquareHeight * squarej) + j];
                        }
                    }
                    // Square
                    list.Add(this._ctx.MkDistinct(square));
                }
            }
            this._rules = list.AsReadOnly();
        }

        public string[,] Solve()
        {
            var list = new List<BoolExpr>();
            for (var i = 0; i < this.Height; i++)
            {
                for (var j = 0; j < this.Width; j++)
                {
                    // Match origin array
                    // sudoku[i, j] == 0 ? true : sudoku[i, j] == array[i, j]
                    list.Add((BoolExpr) this._ctx.MkITE(this._ctx.MkEq(this._ctx.MkInt(this.resultArray[i, j]), this._ctx.MkInt(0)),
                        this._ctx.MkTrue(),
                        this._ctx.MkEq(this.array[i, j], this._ctx.MkInt(this.resultArray[i, j]))));
                }
            }

            foreach (var item in this.extraRules)
            {
                if (!item.Valid())
                {
                    break;
                }
                var cells = item.Target.Select(p => this.array[p.X, p.Y]).ToList();
                switch (item.Type)
                {
                    case RuleInfo.None:
                    {
                        break;
                    }
                    case RuleInfo.GT:
                    {
                        list.Add(this._ctx.MkGt(cells[0], cells[1]));
                        break;
                    }
                    case RuleInfo.Consecutive:
                    {
                        list.Add(this._ctx.MkOr(this._ctx.MkGt(cells[0], cells[1]), this._ctx.MkLe(cells[0], cells[1])));
                        break;
                    }
                    case RuleInfo.V:
                    {
                        list.Add(this._ctx.MkEq(this._ctx.MkAdd(cells[0], cells[1]), this._ctx.MkInt(5)));
                        break;
                    }
                    case RuleInfo.X:
                    {
                        list.Add(this._ctx.MkEq(this._ctx.MkAdd(cells[0], cells[1]), this._ctx.MkInt(10)));
                        break;
                    }
                    case RuleInfo.Killer:
                    {
                        list.Add(this._ctx.MkEq(this._ctx.MkAdd(cells), this._ctx.MkInt(item.Extra)));
                        break;
                    }
                    case RuleInfo.Diagonals:
                    {
                        var left = new IntExpr[this.Width];
                        var right = new IntExpr[this.Width];
                        for (var i = 0; i < this.Width; i++)
                        {
                            left[i] = this.array[i, i];
                            right[i] = this.array[i, this.Width - i - 1];
                        }
                        list.Add(this._ctx.MkDistinct(left));
                        list.Add(this._ctx.MkDistinct(right));
                        break;
                    }
                    case RuleInfo.Windoku:
                    {
                        foreach (var squarei in new int[] {1, 5})
                        {
                            foreach (var squarej in new int[] { 1, 5 })
                            {
                                var square = new IntExpr[this.SquareWidth * this.SquareHeight];
                                for (var i = 0; i < this.SquareWidth; i++)
                                {
                                    for (var j = 0; j < this.SquareHeight; j++)
                                    {
                                        square[(this.SquareWidth * i) + j] = this.array[squarei + i, squarej + j];
                                    }
                                }
                                // Square
                                list.Add(this._ctx.MkDistinct(square));
                            }
                        }
                        break;
                    }
                    case RuleInfo.Even:
                    {
                        foreach (var cell in cells)
                        {
                            list.Add(this._ctx.MkEq(this._ctx.MkMod(cell, this._ctx.MkInt(2)), this._ctx.MkInt(0)));
                        }
                        break;
                    }
                    case RuleInfo.Odd:
                    {
                        foreach (var cell in cells)
                        {
                            list.Add(this._ctx.MkEq(this._ctx.MkMod(cell, this._ctx.MkInt(2)), this._ctx.MkInt(1)));
                        }
                        break;
                    }
                    case RuleInfo.Mirror:
                    {
                        for (var i = 0; i < this.SquareHeight; i++)
                        {
                            for (var j = 0; j < this.SquareWidth; j++)
                            {
                                list.Add(this._ctx.MkEq(this.array[i, j], this.array[this.Height - i - 1, this.Width - j - 1]));
                                list.Add(this._ctx.MkEq(this.array[i, this.Width - j - 1], this.array[this.Height - i - 1, j]));
                            }
                        }
                        break;
                    }
                }
            }

            var s = this._ctx.MkSolver();
            s.Assert(this._ctx.MkAnd(this._rules));
            s.Assert(this._ctx.MkAnd(list));
            var result = new string[9, 9];
            if (s.Check() == Status.SATISFIABLE)
            {
                var m = s.Model;
                for (var i = 0; i < this.Height; i++)
                {
                    for (var j = 0; j < this.Width; j++)
                    {
                        result[i, j] = m.Evaluate(this.array[i, j]).ToString();
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
