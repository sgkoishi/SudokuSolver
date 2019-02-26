using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;
namespace SudokuSolver
{
    public static class Solver
    {
        public static int MinValue = 1;
        public static int MaxValue = 9;
        public static int SquareWidth = 3;
        public static int SquareHeight = 3;
        public static int SquareCountWid = 3;
        public static int SquareCountHei = 3;
        public static int Height = 9;
        public static int Width = 9;
        public static void Solve()
        {
            var ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });
            var array = new IntExpr[Height, Width];
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    array[i, j] = (IntExpr) ctx.MkConst(ctx.MkSymbol("x_" + (i + 1) + "_" + (j + 1)), ctx.IntSort);
                }
            }
            var list = new List<BoolExpr>();
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    // In range check
                    list.Add(ctx.MkAnd(ctx.MkLe(ctx.MkInt(MinValue), array[i, j]), ctx.MkLe(array[i, j], ctx.MkInt(MaxValue))));
                }
            }
            for (var i = 0; i < Height; i++)
            {
                var row = new IntExpr[Height];
                for (var j = 0; j < Width; j++)
                {
                    row[j] = array[i, j];
                }
                // Row
                list.Add(ctx.MkDistinct(row));
            }
            for (var j = 0; j < Width; j++)
            {
                var column = new IntExpr[Height];
                for (var i = 0; i < Height; i++)
                {
                    column[i] = array[i, j];
                }
                // Column
                list.Add(ctx.MkDistinct(column));
            }
            for (var squarei = 0; squarei < SquareCountWid; squarei++)
            {
                for (var squarej = 0; squarej < SquareCountHei; squarej++)
                {
                    var square = new IntExpr[SquareWidth * SquareHeight];
                    for (var i = 0; i < SquareWidth; i++)
                    {
                        for (var j = 0; j < SquareHeight; j++)
                        {
                            square[(SquareWidth * i) + j] = array[(SquareWidth * squarei) + i, (SquareHeight * squarej) + j];
                        }
                    }
                    // Square
                    list.Add(ctx.MkDistinct(square));
                }
            }

            // sudoku instance, we use '0' for empty cells
            int[,] sudoku = {{0,0,0,0,9,4,0,3,0},
                             {0,0,0,5,1,0,0,0,7},
                             {0,8,9,0,0,0,0,4,0},
                             {0,0,0,0,0,0,2,0,8},
                             {0,6,0,2,0,1,0,5,0},
                             {1,0,2,0,0,0,0,0,0},
                             {0,7,0,0,0,0,5,2,0},
                             {9,0,0,0,6,5,0,0,0},
                             {0,4,0,9,7,0,0,0,0}};

            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    // Match origin array
                    // sudoku[i, j] == 0 ? true : sudoku[i, j] == array[i, j]
                    list.Add((BoolExpr) ctx.MkITE(ctx.MkEq(ctx.MkInt(sudoku[i, j]), ctx.MkInt(0)),
                        ctx.MkTrue(),
                        ctx.MkEq(array[i, j], ctx.MkInt(sudoku[i, j]))));
                }
            }

            var s = ctx.MkSolver();
            s.Assert(ctx.MkAnd(list));

            if (s.Check() == Status.SATISFIABLE)
            {
                var m = s.Model;
                Console.WriteLine("Sudoku solution:");
                for (var i = 0; i < Height; i++)
                {
                    for (var j = 0; j < Width; j++)
                    {
                        Console.Write(" " + m.Evaluate(array[i, j]));
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Failed to solve sudoku");
                throw new Exception("SudokuNotSolved");
            }
        }
    }
}
