using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Chireiden.SudokuSolver
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class TargetCountAttribute : Attribute
    {
        public TargetCountAttribute(int minCount)
        {
            this.MaxCount = this.MinCount = minCount;
        }

        public int MinCount { get; }
        public int MaxCount { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class NotImplementedAttribute : Attribute
    {
    }

    public static class RuleTypeHelper
    {
        public class RuleTypeItem
        {
            public string Name;
            public int MinCount;
            public int MaxCount;
            public bool Implemented;
            public RuleType Value;
        }

        private static readonly FieldInfo[] _ruleTypes;
        private static readonly Dictionary<string, RuleTypeItem> _lookupTable;

        static RuleTypeHelper()
        {
            _ruleTypes = typeof(RuleType).GetFields(BindingFlags.Static | BindingFlags.Public);
            _lookupTable = new Dictionary<string, RuleTypeItem>();
            foreach (var i in _ruleTypes)
            {
                var item = new RuleTypeItem
                {
                    Implemented = !i.GetCustomAttributes<NotImplementedAttribute>().Any(),
                    Name = i.Name,
                    Value = (RuleType) Enum.Parse(typeof(RuleType), i.Name)
                };
                var targetCount = i.GetCustomAttributes<TargetCountAttribute>().ToList();
                if (targetCount.Count > 0)
                {
                    var tc = targetCount[0];
                    item.MinCount = tc.MinCount;
                    item.MaxCount = tc.MaxCount;
                }
                _lookupTable[i.Name] = item;
            }
        }

        public static string[] GetAll()
        {
            return _lookupTable.Where(f => f.Value.Implemented).Select(f => f.Key).ToArray();
        }

        public static RuleType Get(string name)
        {
            return _lookupTable[name].Value;
        }

        public static bool TargetCorrect(Rule rule)
        {
            if (rule.Type == RuleType.None)
            {
                return false;
            }
            var item = _lookupTable[rule.Type.ToString()];
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
        [NotImplemented]
        None,

        /// <summary>
        /// Two main diagonal contains unique numbers.
        /// </summary>
        [TargetCount(0)]
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
        [TargetCount(0), NotImplemented]
        Windoku,

        /// <summary>
        /// One cell is greater than other.
        /// </summary>
        [TargetCount(2)]
        GT,

        /// <summary>
        /// Two cells are consecutive.
        /// </summary>
        [TargetCount(2)]
        Consecutive,

        /// <summary>
        /// Sum of given region = given number.
        /// </summary>
        [TargetCount(1, MaxCount = 9)]
        Killer,

        /// <summary>
        /// Sum of two cells = 5
        /// </summary>
        [TargetCount(2)]
        V,

        /// <summary>
        /// Sum of two cells = 10
        /// </summary>
        [TargetCount(2)]
        X,

        /// <summary>
        /// Sum of each row/column in square is divisible by 3.
        /// </summary>
        [TargetCount(0), NotImplemented]
        DivisibleByThree,

        /// <summary>
        /// Any two adjacent cells must be consecutive.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotImplemented]
        Touchy,

        /// <summary>
        /// Sum of any two adjacent cells can not be 10.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotImplemented]
        NoTen,

        /// <summary>
        /// Target cells are even.
        /// </summary>
        [TargetCount(1, MaxCount = 36), NotImplemented]
        Even,

        /// <summary>
        /// Target cells are odd.
        /// </summary>
        [TargetCount(1, MaxCount = 45), NotImplemented]
        Odd,

        /// <summary>
        /// Any 2x2 region contains both even and odd.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotImplemented]
        Quadro,

        /// <summary>
        /// Any two 9s can not be in same diagonal.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotImplemented]
        Queen,

        /// <summary>
        /// Target cells are the average of vertical adjacent cells.
        /// Low priority.
        /// </summary>
        [TargetCount(1, MaxCount = 7), NotImplemented]
        AverageV,

        /// <summary>
        /// Target cells are the average of horizontal adjacent cells.
        /// Low priority.
        /// </summary>
        [TargetCount(1, MaxCount = 7), NotImplemented]
        AverageH,

        /// <summary>
        /// The squares in the corner are mirror to the opposite one.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotImplemented]
        Mirror,

        /// <summary>
        /// Given 1x4, 2x3, 3x2, 4x1 arrays. They will be placed in the sudoku without touching each other.
        /// Low priority.
        /// </summary>
        [TargetCount(1, MaxCount = 4), NotImplemented]
        Battleship,

        /// <summary>
        /// Target cells are greater than adjacent cells.
        /// Low priority.
        /// </summary>
        [TargetCount(1), NotImplemented]
        Fortress,

        /// <summary>
        /// Target cells are smaller than other cells in the square.
        /// Low priority.
        /// </summary>
        [TargetCount(1), NotImplemented]
        Stripes,

        /// <summary>
        /// Two main diagonal contains only 3 different numbers.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotImplemented]
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
        [TargetCount(0), NotImplemented]
        Snail,

        /// <summary>
        /// Same product for cross numbers
        /// Low priority.
        /// </summary>
        [TargetCount(4), NotImplemented]
        EqualProduct,

        [NotImplemented]
        NoEvenNeighbours,

        [NotImplemented]
        OddSum,

        [NotImplemented]
        EvenSum,

        [NotImplemented]
        MultiplicationTable,

        [NotImplemented]
        Figure,

        [NotImplemented]
        Sequence,

        [NotImplemented]
        NoTouch,

        [NotImplemented]
        Pandigital,

        [NotImplemented]
        MagicSquare,

        [NotImplemented]
        External,

        [NotImplemented]
        Blackout,

        [NotImplemented]
        ProductFrame,

        [NotImplemented]
        Quadruple,

        [NotImplemented]
        Triplesum,

        [NotImplemented]
        Kropki,

        [NotImplemented]
        Skyscraper,

        [NotImplemented]
        Arithmetic,

        [NotImplemented]
        Coded,

        [NotImplemented]
        EqualSum,

        [NotImplemented]
        Palindrome,

        [NotImplemented]
        Quadmax,

        [NotImplemented]
        SubFrame,

        [NotImplemented]
        NoKnightStep,

        [NotImplemented]
        Trio
    }
}
