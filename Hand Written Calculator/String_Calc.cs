using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hand_Written_Calculator
{
    class String_Calc
    {
        static int ID = 0;
        static public string ToFormula(List<Mouse_Data> Datas)
        {
            if (Datas.Count == 0)
                return "";
            //Change all "horizontalline" to "minus" or "divide"
            Determine_Horizonal_Line(ref Datas);
            //The return formula
            string S = "";
            //All the formulas including the squareroot sign
            List<string> Sqrt = new List<string>();
            //All the formulas including division
            List<string> Div = new List<string>();
            //All the formulas including the power sign
            List<string> Power = new List<string>();
            //Sort by their X value
            Datas.Sort();
            for (int i = 0;i < Datas.Count;i++)
            {
                Mouse_Data D = Datas[i];
                //Add all the squareroot formulas
                if (D.Label == "square root")
                {
                    List<Mouse_Data> Sqrt_Data = new List<Mouse_Data>();
                    for (int j = 0;j < Datas.Count;j++)
                    {
                        Mouse_Data O = Datas[j];
                        if (O.IsInside(D))
                        {
                            Sqrt_Data.Add(O);
                            Datas.Remove(O);
                            j--;
                        }
                    }
                    D.Combine(Sqrt_Data);
                }
                //Add all the division formulas
                if (i >= Datas.Count - 1)
                    continue;
                List<Mouse_Data> Numerator = new List<Mouse_Data>();
                List<Mouse_Data> Denominator = new List<Mouse_Data>();
                if (D.Label == "divide")
                {
                    for (int j = 0;j < Datas.Count;j++)
                    {
                        Mouse_Data O = Datas[j];
                        if(O.IsAbove(D))
                        {
                            Numerator.Add(O);
                            Datas.Remove(O);
                            j--;
                        }
                        if(O.IsUnder(D))
                        {
                            Denominator.Add(O);
                            Datas.Remove(O);
                            j--;
                        }
                    }
                    D.Combine(Numerator, Denominator);
                }
                //Add all the power formulas
                if (i >= Datas.Count - 1)
                    continue;
                if(Datas[i + 1].IsOnRightTopOf(Datas[i]) && (char.IsDigit(Datas[i].Label[0]) || (Datas[i].Label == "square root")))
                {
                    List<Mouse_Data> Power_Data = new List<Mouse_Data>();
                    while(i < Datas.Count - 1 && Datas[i + 1].IsOnRightTopOf(Datas[i]))
                    {
                        Power_Data.Add(Datas[i + 1]);
                        Datas.RemoveAt(i + 1);
                    }
                    Mouse_Data _Temp = new Mouse_Data("^");
                    _Temp.Combine(Power_Data);
                    Datas.Insert(i + 1, _Temp);
                }
            }
            //Now build the string!
            foreach(Mouse_Data _d in Datas)
            {
                S += _d.ToFormula();
            }
            Add_Abbreviated_Sign(ref S);
            Add_parentheses(ref S, 0, S.Length - 1);
            Debug.Print(S);
            return S;
        }

        private static void Add_Abbreviated_Sign(ref string s)
        {
            for(int i = 0;i < s.Length - 2;i++)
            {
                if(char.IsDigit(s[i]) && s[i + 1] == '(')
                {
                    s = s.Insert(i + 1, "*");
                }
            }
            return;
        }

        private static void Determine_Horizonal_Line(ref List<Mouse_Data> datas)
        {
            for(int i = 0;i < datas.Count;i++)
            {
                if(datas[i].Label == "horizontal line")
                {
                    foreach(Mouse_Data O in datas)
                    {
                        if(O.IsAbove(datas[i]) && Math.Abs(O.Center.X - datas[i].Center.X) < datas[i].Boundary.Width )
                        {
                            datas[i].Label = "divide";
                            break;
                        }
                        datas[i].Label = "minus";
                    }
                }
            }
        }

        static int Add_parentheses(ref string Formula, int start, int end)
        {
            int Added = 0;
            int i = start, j = start;
            for (; i <= end; i++)
            {
                if (Formula[i] == '(')
                {
                    int Next_end = i;
                    Jump_Till_Out(Formula, ref Next_end);
                    Next_end--;
                    int A = Add_parentheses(ref Formula, i + 1, Next_end);
                    end += A;
                    Added += A;
                    Jump_Till_Out(Formula, ref i);
                }
            }
            j = start;
            i = start;
            while (i <= end)
            {
                if (Formula[i] == '^')
                {
                    i++;
                    if (Formula[i] == '(')
                        Jump_Till_Out(Formula, ref i);
                    if (char.IsDigit(Formula[i]))
                        Jump_Till_End_Number(Formula, ref i);
             
                    Formula = Formula.Insert(j, "(");
                    i++;
                    Formula = Formula.Insert(i, ")");
                    end += 2;
                    Added += 2;
                }
                else if (Formula[i] == '+' || Formula[i] == '-' || Formula[i] == '*' || Formula[i] == '/')
                {
                    i++;
                    j = i;
                }
                else if (Formula[i] == '(')
                {
                    Jump_Till_Out(Formula, ref i);
                    i++;
                }
                else if (char.IsDigit(Formula[i]))
                {
                    Jump_Till_End_Number(Formula, ref i);
                }
                else
                {
                    i++;
                }
            }

            i = j = start;
            while (i <= end)
            {
                if (Formula[i] == '*' || Formula[i] == '/')
                {
                    i++;
                    if (Formula[i] == '(')
                        Jump_Till_Out(Formula, ref i);
                    if (char.IsDigit(Formula[i]))
                        Jump_Till_End_Number(Formula, ref i);

                    Formula = Formula.Insert(j, "(");
                    i++;
                    Formula = Formula.Insert(i, ")");
                    end += 2;
                    Added += 2;

                }
                else if (Formula[i] == '+' || Formula[i] == '-')
                {
                    i++;
                    j = i;
                }
                else if (Formula[i] == '(')
                {
                    Jump_Till_Out(Formula, ref i);
                    i++;
                }
                else if (char.IsDigit(Formula[i]))
                {
                    Jump_Till_End_Number(Formula, ref i);
                }
                else
                {
                    i++;
                }
            }

            i = j = start;
            while (i <= end)
            {
                if (Formula[i] == '+' || Formula[i] == '-')
                {
                    i++;
                    if (Formula[i] == '(')
                        Jump_Till_Out(Formula, ref i);
                    if (char.IsDigit(Formula[i]))
                        Jump_Till_End_Number(Formula, ref i);
                    Formula = Formula.Insert(j, "(");
                    i++;
                    Formula = Formula.Insert(i, ")");
                    end += 2;
                    Added += 2;
                }
                else if (Formula[i] == '(')
                {
                    Jump_Till_Out(Formula, ref i);
                    i++;
                }
                else if (char.IsDigit(Formula[i]))
                {
                    Jump_Till_End_Number(Formula, ref i);
                }
                else
                {
                    i++;
                }
            }
            if (Formula[start] == '(')
            {
                i = start;
                Jump_Till_Out(Formula, ref i);
                if (i == end)
                {
                    Formula = Formula.Remove(start, 1);
                    end--;
                    Formula = Formula.Remove(end, 1);
                    end--;
                    Added -= 2;
                }
            }
            return Added;
        }

        public static string Add_Front_End_Parenthese(string F)
        {
            foreach(char C in F)
            {
                if (!char.IsDigit(C))
                    return "(" + F + ")";
            }
            return F;
        }

        static public void Jump_Till_Out(string Formula, ref int i)
        {
            int PCount = 1;
            while (PCount > 0)
            {
                i++;
                if (Formula[i] == '(')
                {
                    PCount++;
                }
                if (Formula[i] == ')')
                {
                    PCount--;
                }
            }
        }

        static public void Jump_Till_End_Number(string Formula, ref int i)
        {
            for (; i < Formula.Length && char.IsDigit(Formula[i]); i++);
        }
    }
}
