using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GraphPartitioningLibrary;

namespace GraphPartitioning
{
    public partial class RandomGraphForm : Form
    {
        MainForm mainForm;

        public RandomGraphForm(MainForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
        }

        private void RandomGraphForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void RandomGraphForm_Load(object sender, EventArgs e)
        {
            numericUpDown2.Value = (int)(Math.Round((double)25 / 100 / ((double)numericUpDown1.Value / 15) * 100));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.InitialState();
            numericUpDown4.Value = Math.Max(numericUpDown4.Value, numericUpDown3.Value);
            mainForm.graph = Graph.RandomGraph((int)numericUpDown1.Value, (int)numericUpDown3.Value,
                (int)numericUpDown4.Value + 1, (int)numericUpDown2.Value);
            this.Hide();
            mainForm.DrawGraphOnPictureBox1();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown4.Value = Math.Max(numericUpDown4.Value, numericUpDown3.Value);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown4.Value = Math.Max(numericUpDown4.Value, numericUpDown3.Value);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            double n = (double)numericUpDown1.Value;
            numericUpDown2.Value = Math.Min((int)(Math.Round((double)20 / 100 / (Math.Pow(n, 0.9) / 15) * 100)), 100);
        }
    }
}
