using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Chireiden.SudokuSolver
{
    public class Rule
    {
        public RuleType Type;
        public List<Point> Target = new List<Point>();
        public int Extra;

        public bool Valid()
        {
            this.Target = this.Target.Distinct().ToList();
            switch (this.Type)
            {
                case RuleType.None:
                    return false;
                case RuleType.GT:
                case RuleType.Consecutive:
                case RuleType.V:
                case RuleType.X:
                    return this.Target.Count == 2
                        && (this.Target[0].X == this.Target[1].X || this.Target[0].Y == this.Target[1].Y)
                        && (Math.Abs(this.Target[0].X - this.Target[1].X) == 1 || Math.Abs(this.Target[0].Y - this.Target[1].Y) == 1);
                case RuleType.Killer:
                    return this.Target.Count >= 1 && this.Extra > 0;
                default:
                    throw new InvalidOperationException("RuleType undefined");
            }
        }

        public override string ToString()
        {
            return Enum.GetName(typeof(RuleType), this.Type) + " " + string.Join(" ", this.Target.Select(i => ((char) (i.X + 65)).ToString() + (i.Y + 1))) + (this.Extra != 0 ? " " + this.Extra : "");
        }
    }
}
