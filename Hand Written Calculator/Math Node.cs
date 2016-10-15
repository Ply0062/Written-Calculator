using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Hand_Written_Calculator
{

    class Math_Node
    {
        Math_Node Left, Right;
        char Operation;
        bool Root_Node;
        double Value;

        public Math_Node()
        {
            Operation = ' ';
            Root_Node = false;
            Value = 0;
        }

        public Math_Node(List<Mouse_Data> Datas)
        {
            Operation = ' ';
            Root_Node = false;
            Value = 0;
            Build_Tree(String_Calc.ToFormula(Datas));
        }

        public Math_Node(Math_Node math_Node)
        {
            Operation = math_Node.Operation;
            Root_Node = math_Node.Root_Node;
            Value = math_Node.Value;
            if (math_Node.Left != null)
                Left = new Math_Node(math_Node.Left);
            if (math_Node.Right != null)
                Right = new Math_Node(math_Node.Right);
        }

        public Math_Node(double i)
        {
            Value = i;
            Root_Node = true;
        }

        public double Calculate()
        {
            if(Root_Node)
            {
                return Value;
            }
            if(Left == null)
            {
                return Perform_Action(Right.Calculate());
            }
            if(Right == null)
            {
                return Perform_Action(Left.Calculate());
            }
            return Perform_Action(Left.Calculate(), Right.Calculate());
        }

        private double Perform_Action(double v)
        {
            if (Operation == '√')
                return Math.Sqrt(v);
            return 0;
        }

        private double Perform_Action(double v1, double v2)
        {
            if(Operation == '+')
            {
                return v1 + v2;
            }

            if (Operation == '-')
            {
                return v1 - v2;
            }

            if(Operation == '*')
            {
                return v1 * v2;
            }

            if(Operation == '/')
            {
                return v1 / v2;
            }

            if(Operation == '^')
            {
                return Math.Pow((double)v1, (double)v2);
            }
            return 0;
        }

        private void Build_Tree(string Formula, int start = 0, int end = -1)
        {
            //end = -1 means that the end is at default value
            if (end == -1)
                end = Formula.Length - 1;
            //Iterator
            int i = start;
            //IsDigit: Whether the Left Node should be built as Number node
            //IsP: Whether the Left Node should be built by recursion
            bool IsDigit = false, IsP = false;
            //Left Node is number
            if(IsDigit = char.IsDigit(Formula[i]))
            {
                String_Calc.Jump_Till_End_Number(Formula, ref i);
            }
            //Left Node is new formula
            else if(IsP = (Formula[i] == '('))
            {
                String_Calc.Jump_Till_Out(Formula, ref i);
                i++;
            }
            //The Formula only consist of a number
            if(i > end)
            {
                Value = Extract_Number(Formula, start);
                Root_Node = true;
                return;
            }
            if(Formula[i] == '(')
            {
                Left = new Math_Node();
                Right = new Math_Node();
                //default to multply
                Left.Build_Tree(Formula, start, i - 1);
                int j = i;
                String_Calc.Jump_Till_Out(Formula, ref j);
                Right.Build_Tree(Formula, i + 1, j - 1);
                Operation = '*';
                return;
            }
            //It is now assumed that the next character is the current Operator
            Operation = Formula[i];
            //Build the Left Node
            if (IsDigit)
                Left = new Math_Node(Extract_Number(Formula, start));
            else if(IsP)
            {
                Left = new Math_Node();
                Left.Build_Tree(Formula, start + 1, i - 2);
            }
            //Now goto the Right Node
            i++;
            //The Right Node is a number
            if(char.IsDigit(Formula[i]))
            {
                Right = new Math_Node(Extract_Number(Formula, i));
            }
            //The Right Node is a new formula
            else if(Formula[i] == '(')
            {
                Right = new Math_Node();
                Right.Build_Tree(Formula, i + 1, end - 1);
            }
        }

        private double Extract_Number(string S, int start)
        {
            double N = 0;
            for(int i = start;i < S.Length && char.IsDigit(S[i]);i++)
            {
                N = N * 10 + (char)(S[i] - '0');
            }
            return N;
        }
    }
}
