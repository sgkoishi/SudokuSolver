﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Chireiden.SudokuSolver
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class RuleInfoAttribute : Attribute
    {
        private int maxCount;

        public int MinCount { get; set; }

        public int MaxCount
        {
            get => Math.Max(this.maxCount, this.MinCount);
            set => this.maxCount = value;
        }

        public bool NotImplemented { get; set; }
        public bool GlobalUnique { get; set; }
        public bool Outside { get; set; }
    }

    public static class RuleTypeHelper
    {
        public class RuleTypeItem
        {
            public string Name;
            public int MinCount;
            public int MaxCount;
            public bool Implemented;
            public bool Unique;
            public bool Outside;
            public RuleType Value;
        }

        private static readonly FieldInfo[] _ruleTypes;
        public static readonly Dictionary<string, RuleTypeItem> Table;

        static RuleTypeHelper()
        {
            _ruleTypes = typeof(RuleType).GetFields(BindingFlags.Static | BindingFlags.Public);
            Table = new Dictionary<string, RuleTypeItem>();
            foreach (var i in _ruleTypes)
            {
                var item = new RuleTypeItem
                {
                    Name = i.Name,
                    Value = (RuleType) Enum.Parse(typeof(RuleType), i.Name)
                };
                var targetCount = i.GetCustomAttributes<RuleInfoAttribute>().ToList();
                if (targetCount.Count > 0)
                {
                    var tc = targetCount[0];
                    item.MinCount = tc.MinCount;
                    item.MaxCount = tc.MaxCount;
                    item.Implemented = !tc.NotImplemented;
                    item.Outside = tc.Outside;
                    item.Unique = tc.GlobalUnique;
                }
                Table[i.Name] = item;
            }
        }

        public static string[] GetAll()
        {
            return Table.Where(f => f.Value.Implemented).Select(f => f.Key).ToArray();
        }

        public static RuleType Get(string name)
        {
            return Table[name].Value;
        }

        public static bool TargetCorrect(Rule rule)
        {
            if (rule.Type == RuleType.None)
            {
                return false;
            }
            var item = Table[rule.Type.ToString()];
            if (rule.Target.Count >= item.MinCount && rule.Target.Count <= item.MaxCount)
            {
                return true;
            }
            if (item.MaxCount == 0)
            {
                rule.Target.Clear();
                return true;
            }
            return false;
        }
    }

    public enum RuleType
    {
        [RuleInfo(NotImplemented = true)]
        None,

        /// <summary>
        /// Two main diagonal contains unique numbers.
        /// </summary>
        [RuleInfo(GlobalUnique = true)]
        Diagonals,

        /// <summary>
        /// Extra square.
        /// ---------
        /// -xxx-xxx-
        /// -xxx-xxx-
        /// -xxx-xxx-
        /// ---------
        /// -xxx-xxx-
        /// -xxx-xxx-
        /// -xxx-xxx-
        /// ---------
        /// </summary>
        [RuleInfo(GlobalUnique = true)]
        Windoku,

        /// <summary>
        /// One cell is greater than other.
        /// </summary>
        [RuleInfo(MinCount = 2)]
        GT,

        /// <summary>
        /// Two cells are consecutive.
        /// </summary>
        [RuleInfo(MinCount = 2)]
        Consecutive,

        /// <summary>
        /// Sum of given region = given number.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 9)]
        Killer,

        /// <summary>
        /// Sum of two cells = 5
        /// </summary>
        [RuleInfo(MinCount = 2)]
        V,

        /// <summary>
        /// Sum of two cells = 10
        /// </summary>
        [RuleInfo(MinCount = 2)]
        X,

        /// <summary>
        /// Sum of each row/column in square is divisible by 3.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        DivisibleByThree,

        /// <summary>
        /// Any two adjacent cells must be consecutive.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        Touchy,

        /// <summary>
        /// Sum of any two adjacent cells can not be 10.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        NoTen,

        /// <summary>
        /// Target cells are even.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 36)]
        Even,

        /// <summary>
        /// Target cells are odd.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 45)]
        Odd,

        /// <summary>
        /// Any 2x2 region contains both even and odd.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        Quadro,

        /// <summary>
        /// Any two 9s can not be in same diagonal.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        Queen,

        /// <summary>
        /// Target cells are the average of vertical adjacent cells.
        /// Low priority.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 7, NotImplemented = true)]
        AverageV,

        /// <summary>
        /// Target cells are the average of horizontal adjacent cells.
        /// Low priority.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 7, NotImplemented = true)]
        AverageH,

        /// <summary>
        /// The squares in the corner are mirror to the opposite one.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true)]
        Mirror,

        /// <summary>
        /// Given 1x4, 2x3, 3x2, 4x1 arrays. They will be placed in the sudoku without touching each other.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        Battleship,

        /// <summary>
        /// Target cells are greater than adjacent cells.
        /// Low priority.
        /// </summary>
        [RuleInfo(MinCount = 1, NotImplemented = true)]
        Fortress,

        /// <summary>
        /// Target cells are smaller than other cells in the square.
        /// Low priority.
        /// </summary>
        [RuleInfo(MinCount = 1, NotImplemented = true)]
        Stripes,

        /// <summary>
        /// Two main diagonal contains only 3 different numbers.
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        AntiDiagonal,

        /// <summary>
        /// No consecutive in snail path.
        /// -->-->--|
        /// -------||
        /// |-----||v
        /// ||---||||
        /// |||--||||
        /// ||||--||v
        /// |||----||
        /// ||------|
        /// |--<--<--
        /// Low priority.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        Snail,

        /// <summary>
        /// Same product for cross numbers
        /// Low priority.
        /// </summary>
        [RuleInfo(MinCount = 4, NotImplemented = true)]
        EqualProduct,

        /// <summary>
        /// Even numbers cannot be adjacent to each other.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        NoEvenNeighbours,

        /// <summary>
        /// Sum of target cells is odd number.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 9, NotImplemented = true)]
        OddSum,

        /// <summary>
        /// Sum of target cells is even number.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 9, NotImplemented = true)]
        EvenSum,

        /// <summary>
        /// A * B = CD
        /// </summary>
        [RuleInfo(MinCount = 4, NotImplemented = true)]
        MultiplicationTable,

        /// <summary>
        /// All figures (rotated and/or mirrored) of each shape contain the same set of digits.
        /// <see cref="http://rohanrao.blogspot.com/2009/08/rules-of-figure-sudoku.html"/>
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 9, NotImplemented = true)]
        Figure,

        /// <summary>
        /// Target cells are in order, sequential, even or odd.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 9, NotImplemented = true)]
        Sequence,

        /// <summary>
        /// Target cells are in order, sequential, even or odd.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        NoTouch,

        /// <summary>
        /// A1A2A3 + A4A5A6 = A7A8A9
        /// A5B5C5 + D5E5F5 = G5H5I5
        /// etc
        /// </summary>
        [RuleInfo(MinCount = 9, NotImplemented = true)]
        Pandigital,

        /// <summary>
        /// Target squares have same number in their diagonals.
        /// </summary>
        [RuleInfo(MinCount = 9, NotImplemented = true)]
        MagicSquare,

        /// <summary>
        /// <see cref="http://rohanrao.blogspot.com/2009/06/rules-of-external-sudoku.html"/>
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        External,

        /// <summary>
        /// Numbers outside indicate the digit that is not present in corresponding row or column.
        /// </summary>
        [RuleInfo(MinCount = 9, NotImplemented = true)]
        Missing,

        /// <summary>
        /// Numbers with arrows outside the grid indicate the sum of the numbers in the corresponding direction.
        /// </summary>
        [RuleInfo(GlobalUnique = true, NotImplemented = true)]
        LittleKiller,

        /// <summary>
        /// Target cells don't contain number.
        /// </summary>
        [RuleInfo(MinCount = 1, MaxCount = 80, NotImplemented = true)]
        Blackout,

        /// <summary>
        /// Numbers outside indicate the product of first three numbers in corresponding row or column.
        /// </summary>
        [RuleInfo(MinCount = 3, NotImplemented = true)]
        ProductFrame,

        /// <summary>
        /// Numbers outside indicate the product of first three numbers in corresponding row or column.
        /// </summary>
        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Quadruple,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Triplesum,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Kropki,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Skyscraper,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Arithmetic,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Coded,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        EqualSum,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Palindrome,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Quadmax,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        SubFrame,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        NoKnightStep,

        [RuleInfo(MinCount = 0, NotImplemented = true)]
        Trio
    }
}
