
/*
 * The Implementation of the "Penny Pincher Data" class
 * The class contains a list of vectors and the label that correspond to it 
 * The class implements the reading, constuction, and other operations to the data
 * 
 * 
 * Copyright Pan Leyan 2016
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hand_Written_Calculator
{
    class Penny_Pincher_Data
    {
        List<Vector> Vectors;
        public int Strokes;
        string Label;

        public void Set_Label(string label)
        {
            Label = label;
        }

        public string Get_Label()
        {
            return Label;
        }

        public List<Vector> Get_Vectors()
        {
            return Vectors;
        }

        public Penny_Pincher_Data()
        {
            Vectors = new List<Vector>();
            Label = "";
            Strokes = 0;
        }

        public Penny_Pincher_Data(Mouse_Data Data)
        {
            Vectors = new List<Vector>();
            Tranform_from_Mouse_Data(Data);
        }

        public Penny_Pincher_Data(List<Vector> vectors)
        {
            Vectors = vectors;
        }

        public void Construct_From_Stream(BinaryReader sr)
        {
            Set_Label(sr.ReadString());
            int cnt = sr.ReadInt32();
            Vector V = new Vector();
            Strokes = sr.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                V.X = sr.ReadDouble();
                V.Y = sr.ReadDouble();
                Vectors.Add(V);
            }
        }

        public void Tranform_from_Mouse_Data(Mouse_Data Data)
        {
            Trace.Assert(Data.Finished_Collecting);
            for(int i = 1;i < Data.Count;i++)
            {
                Vectors.Add(Data.Points[i] - Data.Points[i - 1]);
            }
            Label = Data.Label;
            Strokes = Data.Strokes;
            Trace.Assert(Vectors.Count == MainWindow.DIVISION - 1);
        }
        /*-------------------------------------------Private Section--------------------------------------------*/

        private void init()
        {
            Label = null;
            Vectors = new List<Vector>();
        }
    }
}
