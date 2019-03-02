using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Chireiden.SudokuSolver
{
    public class Rule
    {
        public RuleInfo Type;
        public List<Point> Target = new List<Point>();
        public int Extra;

        public bool Valid()
        {
            this.Target = this.Target.Distinct().ToList();
            if (!RuleInfoHelper.TargetCorrect(this))
            {
                return false;
            }
            if (this.Type == RuleInfo.Killer && this.Extra <= 0)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return this.Type + " " + string.Join(" ", this.Target.Select(i => ((char) (i.X + 65)).ToString() + (i.Y + 1))) + (this.Extra != 0 ? " " + this.Extra : "");
        }
    }
}
