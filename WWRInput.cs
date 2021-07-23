using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDFObjects
{
    public partial class WWR_Input : Form
    {
        public float WWR;
        List<Label> labels;
        List<NumericUpDown> vals;
        List<IDFObjects.Surface> walls;
        public WWR_Input(string zone, List<IDFObjects.Surface> walls)
        {
            InitializeComponent();

            labels = new List<Label>()
            {
                label1, label2, label3, label4, label5, label6, label7, label8, label9,
                label10, label11, label12, label13, label14, label15, label16
            };
            vals = new List<NumericUpDown>()
            {
                numericUpDown1, numericUpDown2, numericUpDown3, numericUpDown4, numericUpDown5, numericUpDown6, numericUpDown7, numericUpDown8, numericUpDown9,
                numericUpDown10, numericUpDown11, numericUpDown12, numericUpDown13, numericUpDown14, numericUpDown15, numericUpDown16
            };
            labels.ForEach(l => l.Visible = false);
            vals.ForEach(l => l.Visible = false);

            Height = Math.Max(70 + walls.Count * 30, 200);
            this.walls = walls;
            Text = string.Format("WWR for {0}", zone);
            for(int n =0; n<walls.Count; n++)
            {
                labels[n].Visible = true;
                vals[n].Visible = true;
                labels[n].Text = walls[n].Name;
            }
               
        }
        public void AssociateWithCorrespondingWalls(List<IDFObjects.Surface> walls) 
        {
            for (int n = 0; n < walls.Count; n++)
            {
                walls[n].WWR = (float)vals[n].Value;
            }
        }
        private void WWR_Input_Load(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < walls.Count; n++)
            {
               walls[n].WWR = (float)vals[n].Value;
            }
            Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
