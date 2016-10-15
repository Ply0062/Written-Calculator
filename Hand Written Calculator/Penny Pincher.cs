/*
 * The implementation of the "Penny Pincher" Recognizer
 * Implements the reading of the data from a data file and the recognization process
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hand_Written_Calculator
{
    class Penny_Pincher
    {
        public bool Loaded;


        /*
         * Constructor
         */

        public Penny_Pincher()
        {
            Loaded = false;
            Datas = new List<Penny_Pincher_Data>();
        }

        public Penny_Pincher(string File)
        {
            FileStream F = new FileStream(File, FileMode.Open);
            BinaryReader br = new BinaryReader(F);
            Datas = new List<Penny_Pincher_Data>();
            Construct_From_Stream(br);
            br.Close();
        }
        /*
         * The loading of the collected data from the data file
         */
        public void Construct_From_Stream(BinaryReader sr)
        {
            Loaded = true;
            int cnt = sr.ReadInt32();
            Penny_Pincher_Data s;
            for (int i = 0; i < cnt; i++)
            {
                s = new Penny_Pincher_Data();
                s.Construct_From_Stream(sr);
                Datas.Add(s);
            }
        }
        /*
         *  Recognize a Mouse_Data type gesture
         */
        public string Recognize(ref Mouse_Data s, bool Consider_Stroke = true)
        {
            Penny_Pincher_Data P = new Penny_Pincher_Data(s);
            s.Label = Recognize(ref P, Consider_Stroke);
            return s.Label;
        }

        /*
         *  Recognize a Penny_Pincher_Data type Gesture
         */

        public string Recognize(ref Penny_Pincher_Data Data, bool Consider_Stroke = true)
        {
            double MaxS = double.MinValue;
            double T;
            string L = "";
            foreach(Penny_Pincher_Data D in Datas)
            {
                if(MaxS < (T = Calculate_Score(Data, D, Consider_Stroke)))
                {
                    MaxS = T;
                    L = D.Get_Label();
                }
            }
            return L;
        }
        #region Private

        private List<Penny_Pincher_Data> Datas;

        private double Calculate_Score(Penny_Pincher_Data G, Penny_Pincher_Data D, bool Consider_Stroke = true)
        {
            double S = 0;
            List<Vector> V1 = G.Get_Vectors(), V2 = D.Get_Vectors();
            if (V1.Count != 0 && V2.Count != 0 && (!Consider_Stroke || G.Strokes == D.Strokes))
                for (int i = 0; i < MainWindow.DIVISION - 1; i++)
                {
                    S += V1[i] * V2[i];
                }
            else
                return double.MinValue;
            return S;
        }
        #endregion
    }
}
