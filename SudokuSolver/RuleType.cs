using System;

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
    public sealed class NotIImplementedAttribute : Attribute
    {
    }

    public enum RuleType
    {
        [NotIImplemented]
        None,

        /// <summary>
        /// Two main diagonal contains unique numbers.
        /// </summary>
        [TargetCount(0), NotIImplemented]
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
        [TargetCount(0), NotIImplemented]
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
        [TargetCount(0), NotIImplemented]
        DivisibleByThree,

        /// <summary>
        /// Any two adjacent cells must be consecutive.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotIImplemented]
        Touchy,

        /// <summary>
        /// Sum of any two adjacent cells can not be 10.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotIImplemented]
        NoTen,

        /// <summary>
        /// Target cells are even.
        /// </summary>
        [TargetCount(1, MaxCount = 36), NotIImplemented]
        Even,

        /// <summary>
        /// Target cells are odd.
        /// </summary>
        [TargetCount(1, MaxCount = 45), NotIImplemented]
        Odd,

        /// <summary>
        /// Any 2x2 region contains both even and odd.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotIImplemented]
        Quadro,

        /// <summary>
        /// Any two 9s can not be in same diagonal.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotIImplemented]
        Queen,

        /// <summary>
        /// Target cells are the average of vertical adjacent cells.
        /// Low priority.
        /// </summary>
        [TargetCount(1, MaxCount = 7), NotIImplemented]
        AverageV,

        /// <summary>
        /// Target cells are the average of horizontal adjacent cells.
        /// Low priority.
        /// </summary>
        [TargetCount(1, MaxCount = 7), NotIImplemented]
        AverageH,

        /// <summary>
        /// The squares in the corner are mirror to the opposite one.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotIImplemented]
        Mirror,

        /// <summary>
        /// Given 1x4, 2x3, 3x2, 4x1 arrays. They will be placed in the sudoku without touching each other.
        /// Low priority.
        /// </summary>
        [TargetCount(1, MaxCount = 4), NotIImplemented]
        Battleship,

        /// <summary>
        /// Target cells are greater than adjacent cells.
        /// Low priority.
        /// </summary>
        [TargetCount(1), NotIImplemented]
        Fortress,

        /// <summary>
        /// Target cells are smaller than other cells in the square.
        /// Low priority.
        /// </summary>
        [TargetCount(1), NotIImplemented]
        Stripes,

        /// <summary>
        /// Two main diagonal contains only 3 different numbers.
        /// Low priority.
        /// </summary>
        [TargetCount(0), NotIImplemented]
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
        [TargetCount(0), NotIImplemented]
        Snail,

        /// <summary>
        /// Same product for cross numbers
        /// Low priority.
        /// </summary>
        [TargetCount(4), NotIImplemented]
        EqualProduct,

        [NotIImplemented]
        NoEvenNeighbours,

        [NotIImplemented]
        OddSum,

        [NotIImplemented]
        EvenSum,

        [NotIImplemented]
        MultiplicationTable,

        [NotIImplemented]
        Figure,

        [NotIImplemented]
        Sequence,

        [NotIImplemented]
        NoTouch,

        [NotIImplemented]
        Pandigital,

        [NotIImplemented]
        MagicSquare,

        [NotIImplemented]
        External,

        [NotIImplemented]
        Blackout,

        [NotIImplemented]
        ProductFrame,

        [NotIImplemented]
        Quadruple,

        [NotIImplemented]
        Triplesum,

        [NotIImplemented]
        Kropki,

        [NotIImplemented]
        Skyscraper,

        [NotIImplemented]
        Arithmetic,

        [NotIImplemented]
        Coded,

        [NotIImplemented]
        EqualSum,

        [NotIImplemented]
        Palindrome,

        [NotIImplemented]
        Quadmax,

        [NotIImplemented]
        SubFrame,

        [NotIImplemented]
        NoKnightStep,

        [NotIImplemented]
        Trio
    }
}
