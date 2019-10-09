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
using System.IO;
using System.Drawing.Imaging;

namespace GraphPartitioning
{
    public partial class MainForm : Form
    {
        Bitmap bmp;
        Graphics graphics;
        int pictureBox1StartLeft, pictureBox1StartTop;
        public Graph graph;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MinimumSize = this.Size;
            randomGraphForm = new RandomGraphForm(this);
            panel1.AutoScroll = true;
            panel1.VerticalScroll.Enabled = true;
            panel1.HorizontalScroll.Enabled = true;
            pictureBox1StartLeft = pictureBox1.Left;
            pictureBox1StartTop = pictureBox1.Top;
            bmp = new Bitmap(pictureBox1.Width - 2, pictureBox1.Height - 2);
            graphics = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;
            graph = new Graph(new int[0, 0]);
        }

        int startPositionX, startPositionY;

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
            if (button2.BackColor == Color.DarkRed)
                button2.BackColor = Color.DarkGreen;
            else
                button2.BackColor = Color.DarkRed;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            graph.NextStepOfForceBasedAlgtorithm();
            graphics.Clear(pictureBox1.BackColor);
            graph.DrawGraph(graphics, bmp.Size);
            pictureBox1.Refresh();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            AlignPictureBox1(0, 0);
            if (WindowState == FormWindowState.Minimized || graph == null)
                return;
            if (bmp != null)
                bmp.Dispose();
            if (graphics != null)
                graphics.Dispose();
            bmp = new Bitmap(pictureBox1.Width - 2, pictureBox1.Height - 2);
            pictureBox1.Image = bmp;
            graphics = Graphics.FromImage(bmp);
            graph.DrawGraph(graphics, bmp.Size);
            pictureBox1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (graph == null || !graph.IsInitialized())
            {
                MessageBox.Show("First create or upload a graph", "Incorrect action");
                return;
            }
            if (button3.BackColor == Color.RoyalBlue)
            {
                button3.BackColor = Color.DarkGoldenrod;
                button3.Text = "TWO SUBGRAPHS";
                graph.IsDivided = true;
                textBox1.Visible = true;
                textBox1.Text = $"Crossing cost: {graph.couuntCrossingCost()}";
                button1.Enabled = true;
            }
            else
            {
                button3.BackColor = Color.RoyalBlue;
                button3.Text = "SINGLE GRAPH";
                textBox1.Visible = false;
                graph.IsDivided = false;
                button1.Enabled = false;
            }
            DrawGraphOnPictureBox1();
        }
        

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int oldTop = pictureBox1.Top, oldLeft = pictureBox1.Left;
            int oldWidth = pictureBox1.Width, oldHeight = pictureBox1.Height;
            pictureBox1.Width = panel1.Width - 10;
            pictureBox1.Height = panel1.Height - 10;
            //int xShift 
            pictureBox1.Width = (int)(pictureBox1.Width * (double)trackBar1.Value / trackBar1.Minimum);
            pictureBox1.Height = (int)(pictureBox1.Height * (double)trackBar1.Value / trackBar1.Minimum);
            if (trackBar1.Value == 1)
            {
                pictureBox1.Left = (panel1.Width - pictureBox1.Width) / 2 - 2;
                pictureBox1.Top = (panel1.Height - pictureBox1.Height) / 2 - 2;
            }
            else
            {
                pictureBox1.Left = oldLeft + (oldWidth - pictureBox1.Width) / 2;
                pictureBox1.Top = oldTop + (oldHeight - pictureBox1.Height) / 2;
            }
            AlignPictureBox1(0, 0);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            graph.MovableVertex = -1;
        }
        

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            if (graph.VertexMooving(e.X - startPositionX, e.Y - startPositionY))
            {
                graphics.Clear(pictureBox1.BackColor);
                graph.DrawGraph(graphics, bmp.Size);
                pictureBox1.Refresh();
                startPositionX = e.X;
                startPositionY = e.Y;
                return;
            }
            AlignPictureBox1(e.X - startPositionX, e.Y - startPositionY);
        }
        

        RandomGraphForm randomGraphForm;
        private void button4_Click(object sender, EventArgs e)
        {
            randomGraphForm.Show();
            randomGraphForm.BringToFront();
        }

        public void InitialState()
        {
            button3.BackColor = Color.RoyalBlue;
            button3.Text = "SINGLE GRAPH";
            textBox1.Visible = false;
            checkBox1.Checked = false;
            button1.Enabled = false;
        }

        public void DrawGraphOnPictureBox1()
        {
            graphics.Clear(pictureBox1.BackColor);
            graph.DrawGraph(graphics, bmp.Size);
            pictureBox1.Refresh();
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (randomGraphForm != null && randomGraphForm.Visible)
                randomGraphForm.BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            graph.KernighanLinAlgorithm();
            DrawGraphOnPictureBox1();
            textBox1.Text = $"Crossing cost: {graph.couuntCrossingCost()}";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                graph.showWeights = true;
            else
                graph.showWeights = false;
            DrawGraphOnPictureBox1();
        }
        
        OpenFileDialog openFile = new OpenFileDialog();
        private void weightMatrixInTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    string line = "";
                    StreamReader sr = new StreamReader(openFile.FileName);
                    int[,] matrix;
                    line = sr.ReadLine();
                    int n = line.Trim().Split().Length;
                    if (n > 100 || n < 3)
                        throw new Exception();
                    matrix = new int[n, n];
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            matrix[i, j] = matrix[j, i] = int.Parse(line.Trim().Split()[j]);
                        }
                        matrix[i, i] = 0;
                        line = sr.ReadLine();
                    }
                    graph = new Graph(matrix);
                    DrawGraphOnPictureBox1();
                    InitialState();
                }
            }
            catch
            {
                MessageBox.Show("Failed to read matrix", "Incorrect data");
            }

        }

        private void WMatrix_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = "WeightMatrix.txt";
            save.Filter = "Text File | *.txt";
            if (save.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(save.OpenFile());
                int[,] matrix = graph.GetWeightMatrixCopy();
                string s = "";
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(0); j++)
                    {
                        s += matrix[i, j].ToString() + " ";
                    }
                    writer.WriteLine(s);
                    s = "";
                }
                writer.Dispose();
                writer.Close();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (graph.IsDivided)
            {
                graph.RandomSplitInHalf();
                textBox1.Text = $"Crossing cost: {graph.couuntCrossingCost()}";
            }
            else
                graph.RandomDispositionInCenter();
            DrawGraphOnPictureBox1();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The program was written as a coursework by a student of the BSE185 group, Yadav Navin.",
                "About program");
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Images|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }
                pictureBox1.Image.Save(sfd.FileName, format);
            }
        }

        private void AlignPictureBox1(int xPlus, int yPlus)
        {
            int newX = pictureBox1.Left + xPlus;
            int newY = pictureBox1.Top + yPlus;
            newX = Math.Min(newX, 3);
            newX = Math.Max(newX, panel1.Width - 7 - pictureBox1.Width);
            newY = Math.Min(newY, 3);
            newY = Math.Max(newY, panel1.Height - 7 - pictureBox1.Height);
            pictureBox1.Left = newX;
            pictureBox1.Top = newY;
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            graph.MouseDownOnVertex(e.X, e.Y);
            startPositionX = e.X;
            startPositionY = e.Y;
        }
    }
}
