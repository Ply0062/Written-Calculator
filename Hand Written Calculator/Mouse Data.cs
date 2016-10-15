using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Hand_Written_Calculator
{
    class Mouse_Data:IComparable
    {
        public string Label;
        bool Combined = false;
        public List<Point> Points;                              //The points collected in the data
        public bool Finished_Collecting;                        //Indicate that the data has already finished collecting
        public Point Center;
        public Rect Boundary;
        public int Strokes;
        public bool Calculated;
        public List<Mouse_Data> AnyOther;
        public List<Mouse_Data> Denominator;
        public Mouse_Data()
        {
            Points = new List<Point>();
            Finished_Collecting = false;
            Label = null;
            Strokes = 0;
            Calculated = false;
        }

        public Mouse_Data(string S)
        {
            Label = S;
            Points = new List<Point>();
            Finished_Collecting = true;
            Strokes = 0;
            Calculated = false;
        }

        public Mouse_Data(Mouse_Data mouse_Data)
        {
            Points = new List<Point>();
            Label = mouse_Data.Label;
            foreach (Point p in mouse_Data.Points)
                Points.Add(p);
            Finished_Collecting = mouse_Data.Finished_Collecting;
            Center = mouse_Data.Center;
            Boundary = mouse_Data.Boundary;
            Strokes = mouse_Data.Strokes;
            Calculated = mouse_Data.Calculated;
        }

        public void Finish_Collecting()
        {
            Delete_Same_Point();
            if (Points.Count > 1)
            {
                Finished_Collecting = true;
                Redivide();
                Get_Boundary_And_Center();
            }
            Strokes++;
#if _DEBUG
            DrawCenter();
#endif
        }

#if _DEBUG
        private void DrawCenter()
        {
            Line L1, L2;
            L1 = new Line();
            L2 = new Line();
            L1.X1 = Center.X - 10;
            L1.X2 = Center.X + 10;
            L1.Y1 = L1.Y2 = Center.Y;
            L2.X1 = L2.X2 = Center.X;
            L2.Y1 = Center.Y - 10;
            L2.Y2 = Center.Y + 10;
            L1.Stroke = L2.Stroke = Brushes.Red;
            MainWindow.canvas.Children.Add(L1);
            MainWindow.canvas.Children.Add(L2);
        }
#endif
        public bool IsAbove(Mouse_Data d)
        {
            return (Center.Y < d.Center.Y && Center.X > d.Boundary.Left && Center.X < d.Boundary.Right && Center.Y > d.Center.Y - d.Boundary.Width * 1.5);
        }

        public bool IsInside(Mouse_Data d)
        {
            return (d.Boundary.Contains(Center)) && (Boundary.Top > d.Boundary.Top);
        }

        public bool IsUnder(Mouse_Data d)
        {
            return (Center.Y > d.Center.Y && Center.X > d.Boundary.Left && Center.X < d.Boundary.Right);
        }

        public void Add_Point(Point P)
        {
            Points.Add(P);
        }


        public bool Intersect(Mouse_Data Next)
        {
            Trace.Assert(Next.Finished_Collecting && Finished_Collecting);
            for(int i = 0;i < MainWindow.DIVISION - 1;i++)
            {
                for(int j = 1;j < MainWindow.DIVISION - 1;j++)
                {
                    if (Intersect_Point(Points[i], Points[i + 1], Next.Points[j], Next.Points[j + 1]))
                        return true;
                }
            }
            return false;
        }

        public void Union(Mouse_Data Next)                          //Simply Connect the two Datas
        {
            Trace.Assert(Next.Finished_Collecting);
            foreach(Point P in Next.Points)
            {
                Points.Add(P);
            }
            Finish_Collecting();
        }

        internal bool IsOnRightTopOf(Mouse_Data mouse_Data)
        {
              return (Boundary.Left > (mouse_Data.Center.X)) && (Boundary.Bottom < mouse_Data.Center.Y) && (Boundary.Bottom - Boundary.Top < mouse_Data.Boundary.Bottom - mouse_Data.Boundary.Top) && (Boundary.Top < (mouse_Data.Boundary.Top + mouse_Data.Center.Y) / 2);
        }

        public bool inRect(Mouse_Data data)
        {
            Trace.Assert(data.Finished_Collecting && Finished_Collecting);
            return Boundary.Contains(data.Center);
        }

        public Point Last
        {
            get
            {
                return Points[Points.Count - 1];
            }
        }

        public int Count
        {
            get
            {
                return Points.Count;
            }
        }

        /*----------------------------------------------------------Start of the private section----------------------------------------------------------*/




        private void Delete_Same_Point()                            //Delete points identical to each other due to user input
        {
            for(int i = 1;i < Points.Count;i++)
            {
                if (Points[i] == Points[i - 1] || Points[i].X == double.NaN)
                    Points.Remove(Points[i]);
            }
        }

        private void Get_Boundary_And_Center()
        {
            Trace.Assert(Finished_Collecting);
            double Xmin = double.MaxValue, Xmax = double.MinValue, Ymin = double.MaxValue, Ymax = double.MinValue;
            foreach (Point P in Points)
            {
                Xmin = Math.Min(Xmin, P.X);
                Xmax = Math.Max(Xmax, P.X);
                Ymin = Math.Min(Ymin, P.Y);
                Ymax = Math.Max(Ymax, P.Y);
            }
            Boundary = new Rect(Xmin, Ymin, Xmax - Xmin, Ymax - Ymin);
            Center = new Point((Xmax + Xmin) / 2, (Ymax + Ymin) / 2);
        }

        public int CompareTo(object D)
        {
            Mouse_Data d = D as Mouse_Data;
            return Boundary.Left.CompareTo(d.Boundary.Left);
        }

        private void Redivide()                                                      //Redivide the points into DIVISION number of equally spaced points
        {
            List<Point> New_Points = new List<Point>();
            double Step_Size = Get_Total_Distance() / (MainWindow.DIVISION - 1);
            Step_Size -= 0.1 * 32 / MainWindow.DIVISION;
            New_Points.Add(Points[0]);
            double D = 0;
            double d = 0;
            Point TP = new Point();
            for (int i = 1; i < Points.Count && New_Points.Count < MainWindow.DIVISION; i++)
            {
                Point p1 = (Point)Points[i - 1], p2 = (Point)Points[i];
                d = Distance(p1, p2);
                if (D + d >= Step_Size)
                {
                    TP.X = p1.X + (p2.X - p1.X) * (Step_Size - D) / d;
                    TP.Y = p1.Y + (p2.Y - p1.Y) * (Step_Size - D) / d;
                    D = 0;
                    Points.Insert(i, TP);
                    New_Points.Add(TP);
                }
                else
                    D += d;
            }
            Points = New_Points;
            Trace.Assert(Points.Count == MainWindow.DIVISION);
        }

        private double Get_Total_Distance()                                     //The total distance of the gesture
        {
            double D = new double();
            for (int i = 1; i < Points.Count; i++)
            {
                D += Distance((Point)Points[i - 1], (Point)Points[i]);
            }
            return D;
        }

        public static double Distance(Point A, Point B)                         //The distace between two points
        {
            return Math.Sqrt((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y));
        }

        private bool Intersect_Point(Point a1, Point a2, Point b1, Point b2)
        {
            double A1, B1, A2, B2;
            return ((((b2.X - a1.X) * (B1 = (a2.Y - a1.Y))) - ((A1 = (a2.X - a1.X)) * (b2.Y - a1.Y))) * (((b1.X - a1.X) * B1) - (A1 * (b1.Y - a1.Y))) <= 0) &&
                   ((((a2.X - b1.X) * (B2 = (b2.Y - b1.Y))) - ((A2 = (b2.X - b1.X)) * (a2.Y - b1.Y))) * (((a1.X - b1.X) * B2) - (A2 * (a1.Y - b1.Y))) <= 0);
        }

        public void Combine(List<Mouse_Data> datas, List<Mouse_Data> denominator = null)
        {
            if (Combined)
                return;
            Combined = true;
            AnyOther = new List<Mouse_Data>();
            AnyOther = datas;
            if(denominator != null)
            {
                Trace.Assert(Label == "divide");
                Denominator = new List<Mouse_Data>();
                Denominator = denominator;
            }
            else
            {
                Trace.Assert(Label != "divide");
            }
        }

        public string ToFormula()
        {
            switch(Label)
            {
                case "square root":
                    return "(√" + String_Calc.Add_Front_End_Parenthese(String_Calc.ToFormula(AnyOther)) + ")";
                case "divide":
                    return String_Calc.Add_Front_End_Parenthese(String_Calc.ToFormula(AnyOther)) + "/" + String_Calc.Add_Front_End_Parenthese(String_Calc.ToFormula(Denominator));
                case "^":
                    return "^" + String_Calc.Add_Front_End_Parenthese(String_Calc.ToFormula(AnyOther));
                default:
                    return "" + MainWindow.DataType[Label];
            }
        }
    }
}
