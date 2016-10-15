using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hand_Written_Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int DIVISION = 32;
        public const string FileName = "Hand Written Calculator.data";
        private static Dictionary<string, char> dataType;
        public static Canvas canvas;

        public MainWindow()
        {
            Trace.Assert(File.Exists(FileName));
            InitializeComponent();
            Screen = new List<Mouse_Data>();
            Recognizer = new Penny_Pincher(FileName);
            init_dict();
            canvas = MainCanvas;
        }



        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Temp = new Mouse_Data();
            Point P = e.GetPosition(this);
            Temp.Add_Point(P);
            Mouse_Down = true;
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!Mouse_Down)
            {
                return;
            }
            Mouse_Down = false;
            //---------------------------------Add the last point to the canvas and screen---------------------------------
            Point P = e.GetPosition(this);
            Draw_Point(P);
            Temp.Add_Point(P);
            //---------------------------------Do the finished work--------------------------------------------------------
            Temp.Finish_Collecting();
            if (!Temp.Finished_Collecting)
                return;
            //--------------------------check if the current gesture should be unioned with the last gesture-----------------------
            if(Screen.Count != 0)
            {
                Mouse_Data REF = Screen[Screen.Count - 1];
                if ((Screen[Screen.Count - 1].Intersect(Temp) || (Screen[Screen.Count - 1].Boundary.Contains(Temp.Center)) && Recognizer.Recognize(ref REF) != "square root"))
                {
                    Mouse_Data T = Screen[Screen.Count - 1];
                    T.Union(Temp);
                    Screen[Screen.Count - 1] = T;
                }
                else
                {
                    RecLast();
                    if (Screen.Last().Label == "horizontal line")
                    {
                        Mouse_Data D = new Mouse_Data(Screen.Last());
                        D.Union(Temp);
                        if (Recognizer.Recognize(ref Temp) == "horizontal line" && Recognizer.Recognize(ref D, false) == "equals")
                        {
                            Screen[Screen.Count - 1] = D;
                            double R = Calculate_Value();
                            Debug.Print(R.ToString());
                            #region Output_Answer
                            Label AnswerBox = new Label();
                            AnswerBox.Content = R.ToString();
                            double L, T;
                            AnswerBox.Margin = new Thickness(L = Temp.Boundary.X + Temp.Boundary.Width + 10, T = ((Temp.Boundary.Y + Temp.Boundary.Height + D.Boundary.Y + D.Boundary.Height) / 2 - 50), 0, MainCanvas.Height - T - 20);
                            AnswerBox.FontSize = 50;
                            MainCanvas.Children.Add(AnswerBox);
                            #endregion
                        }
                        else
                        {
                            Screen.Add(Temp);
                        }
                    }
                    else
                    {
                        Screen.Add(Temp);
                    }
                }
            }
            else
            {
                Screen.Add(Temp);
            }

        }



        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse_Down)
            {
                Point P = e.GetPosition(this);
                Draw_Point(P);
                Temp.Add_Point(P);
            }
        }

        /* Not self generated Section */

        private List<Mouse_Data> Screen;
        private Mouse_Data Temp;
        private bool Mouse_Down;

        // The Recognizer
        Penny_Pincher Recognizer;

        internal static Dictionary<string, char> DataType
        {
            get
            {
                return dataType;
            }

            set
            {
                dataType = value;
            }
        }


        private void Draw_Point(Point p)
        {
            Line L = new Line();
            L.Stroke = Brushes.Black;
            L.X1 = Temp.Last.X;
            L.X2 = p.X;
            L.Y1 = Temp.Last.Y;
            L.Y2 = p.Y;
            MainCanvas.Children.Add(L);
        }

        private void RecLast()
        {
            Mouse_Data M = Screen.Last();
            Recognizer.Recognize(ref M);
            Screen[Screen.Count - 1] = M;
        }
        /*
         *---------------------------One of the most important functions of the program----------------------------------------
         * Called when the equal sign is written
         */
        private double Calculate_Value()
        {
            List<Mouse_Data> L = new List<Mouse_Data>();
            for(int i = 0;i < Screen.Count;i++)
            {
                Mouse_Data D = Screen[i];
                if(D.Label != "equals" && !D.Calculated)
                {
                    L.Add(D);
                }
                Screen[i].Calculated = true;
            }
            Math_Node Tree = new Math_Node(L);
            return Tree.Calculate();
        }

        private void init_dict()
        {
            DataType = new Dictionary<string, char>();
            for (int i = 0; i < 10; i++)
            {
                DataType["" + i] = (char)(i + '0');
            }
            DataType["plus"] = '+';
            DataType["minus"] = /*DataType["horizontal line"]*/ '-';
            DataType["multiply"] = '*';
            DataType["divide"] = '/';
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Clear();
            Screen.Clear();
        }
    }
}
