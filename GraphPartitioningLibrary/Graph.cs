using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace GraphPartitioningLibrary
{
    public class Graph
    {
        const float halfShare = 0.45f; // Доля, которую занимает одна из частей графа
        const int vertexRadius = 8; // Радиус вершины при рисовании
        public bool showWeights = false;
        int[,] weightsMatrix; // Матрица весов (для невзеш. графа любое ребро им. вес = 1)
        Vector[] vertexLocation; // Координаты всех вершин графа
        Vector[] singleVertexLocation;
        Vector[] splitVertexLocation;
        int widthOfPicture, heightOfPicture; // Ширина и высота картинки, на которой рисуется граф
        private int NumVertieces { get => weightsMatrix.GetLength(0); } // Количество вершин (по матрице весов)
        bool isDivided = false;
        public bool IsDivided
        {
            get => isDivided;
            set
            {
                isDivided = value;
                if (value)
                {
                    singleVertexLocation = (Vector[])vertexLocation.Clone();
                    if (splitVertexLocation != null)
                    {
                        vertexLocation = splitVertexLocation;
                        splitVertexLocation = null;
                        return;
                    }
                    else
                    {
                        RandomSplitInHalf();
                    }
                }
                else
                {
                    splitVertexLocation = (Vector[])vertexLocation.Clone();
                    vertexLocation = singleVertexLocation;
                    singleVertexLocation = null;
                }
            }
        }
        bool[] isOnLeftPart; // i-й элемент равен true, если вершина в левой части разбиения, false, если нет
        int[] numVertexComponent; // Номер компоненты связности вершины, не учитывая ребра между разбиением 
                                  //(используется, чтоб разные компоненты связности не взаимодействовали при визуализации)
        static Random random = new Random(); // Генератор случайных числе для использования в методах

        public bool IsInitialized() => vertexLocation != null;

        public Graph(int[,] weightsMatrix)
        {
            this.weightsMatrix = weightsMatrix;
        }

        public static Graph RandomGraph(int n, int minEdgeWeight, int maxEdgeWeight, int edgeProbabPercent)
        {
            int[,] a = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (i == j)
                        continue;
                    if (random.Next(100) < edgeProbabPercent)
                    {
                        a[i, j] = a[j, i] = random.Next(minEdgeWeight, maxEdgeWeight);
                    }
                }
            }
            LeadToOneConnectedComponent(a, minEdgeWeight, maxEdgeWeight);
            return new Graph(a);
        }



        public void RandomDispositionInCenter()
        {
            const double centerShare = 0.15;
            //widthOfPicture = bmpSize.Width;
            //heightOfPicture = bmpSize.Height;
            int xRightShift = (int)((1 - halfShare) * widthOfPicture);
            int numOfVertiecesInCurrentPart = NumVertieces;
            int width = (int)(widthOfPicture * (isDivided ? halfShare : 1) * centerShare),
                height = (int)(heightOfPicture * centerShare);
            if (numOfVertiecesInCurrentPart == 0)
                return;
            int x, y;
            x = (int)Math.Ceiling(Math.Sqrt((double)width * numOfVertiecesInCurrentPart / height));
            y = numOfVertiecesInCurrentPart / x + (numOfVertiecesInCurrentPart % x == 0 ? 0 : 1);
            int WidthOffset = (int)((widthOfPicture * (isDivided ? halfShare : 1) - width) / 2),
                HeightOfset = (int)((heightOfPicture - height) / 2);
            if (vertexLocation == null)
                vertexLocation = new Vector[NumVertieces];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (i * y + j >= numOfVertiecesInCurrentPart)
                        break;

                    vertexLocation[i * y + j] = new Vector(WidthOffset + width / x * i + width / (2 * x) + random.Next(-5, 5),
                                                 HeightOfset + height / y * j + height / (2 * y) + random.Next(-5, 5));
                    if (IsDivided && !isOnLeftPart[i * y + j])
                        vertexLocation[i * y + j].X += xRightShift;
                }
            }
            if (!IsDivided)
                AuxiliaryMethods.RandomShuffle(vertexLocation);
        }

        

        /// <summary>
        /// делает случайное разбиение на равные части
        /// </summary>
        public void RandomSplitInHalf()
        {
            int[] vertexList = new int[NumVertieces];
            for (int i = 0; i < NumVertieces; i++)
                vertexList[i] = i;
            AuxiliaryMethods.RandomShuffle(vertexList);
            isOnLeftPart = new bool[NumVertieces];
            for (int i = 0; i < NumVertieces / 2; i++)
            {
                isOnLeftPart[vertexList[i]] = true;
            }
            for (int i = NumVertieces / 2; i < NumVertieces; i++)
            {
                isOnLeftPart[vertexList[i]] = false;
            }
            RandomDispositionInCenter();
            numerateVertexComponents();
        }

        public void numerateVertexComponents()
        {            // Нумерация компонент связности с использованием обхода в ширину
            numVertexComponent = new int[NumVertieces];
            int[] used = new int[NumVertieces];
            for (int i = 0; i < NumVertieces; i++)
            {
                if (used[i] == 1)
                    throw new Exception();
                if (used[i] != 0)
                    continue;
                Queue<int> queue = new Queue<int>();
                queue.Enqueue(i);
                numVertexComponent[i] = i;
                while (queue.Count != 0)
                {
                    int current = queue.Dequeue();
                    used[current] = 2;
                    for (int j = 0; j < NumVertieces; j++)
                    {
                        if (used[j] == 0 && isOnLeftPart[current] == isOnLeftPart[j] && weightsMatrix[current, j] != 0)
                        {
                            used[j] = 1;
                            queue.Enqueue(j);
                            numVertexComponent[j] = i;
                        }
                    }
                }
            }
        }

        public void DrawGraph(Graphics graphics, Size bmpSize)
        {
            if (NumVertieces == 0)
                return;
            int bmpWidth = bmpSize.Width, bmpHeight = bmpSize.Height;
            if (widthOfPicture == 0 || heightOfPicture == 0)
            {
                widthOfPicture = bmpWidth;
                heightOfPicture = bmpHeight;
                RandomDispositionInCenter();
            }
            else if (widthOfPicture != bmpWidth || heightOfPicture != bmpHeight)
            {
                for (int i = 0; i < NumVertieces; i++)
                {
                    vertexLocation[i].X = vertexLocation[i].X * (double)bmpWidth / widthOfPicture;
                    vertexLocation[i].Y = vertexLocation[i].Y * (double)bmpHeight / heightOfPicture;
                    if (splitVertexLocation != null)
                    {
                        splitVertexLocation[i].Y = splitVertexLocation[i].Y * (double)bmpHeight / heightOfPicture;
                        splitVertexLocation[i].X = splitVertexLocation[i].X * (double)bmpWidth / widthOfPicture;
                    }
                    if (singleVertexLocation != null)
                    {
                        singleVertexLocation[i].X = singleVertexLocation[i].X * (double)bmpWidth / widthOfPicture;
                        singleVertexLocation[i].Y = singleVertexLocation[i].Y * (double)bmpHeight / heightOfPicture;
                    }
                }
                widthOfPicture = bmpWidth;
                heightOfPicture = bmpHeight;
            }
            if (!IsDivided && vertexLocation == null)
            {
                vertexLocation = singleVertexLocation;
                singleVertexLocation = null;
            }
            if (IsDivided && vertexLocation == null)
            {
                vertexLocation = splitVertexLocation;
                splitVertexLocation = null;
            }
            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 50, 50, 60));
            Pen pen = new Pen(brush, 4);
            if (IsDivided)
            {
                pen.Color = Color.DarkRed;
                graphics.DrawLine(pen, widthOfPicture * halfShare, 0, widthOfPicture * halfShare, heightOfPicture);
                graphics.DrawLine(pen, widthOfPicture * (1 - halfShare), 0, widthOfPicture * (1 - halfShare), heightOfPicture);
            }
            for (int i = 0; i < NumVertieces; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (weightsMatrix[i, j] != 0)
                    {
                        if (IsDivided && isOnLeftPart[i] != isOnLeftPart[j])
                            pen.Color = Color.DimGray;
                        else
                            pen.Color = Color.Black;
                        graphics.DrawLine(pen, (float)vertexLocation[i].X, (float)vertexLocation[i].Y,
                                          (float)vertexLocation[j].X, (float)vertexLocation[j].Y);
                    }

                }
            }
            brush.Color = Color.Black;
            foreach (Vector point in vertexLocation)
            {
                graphics.FillEllipse(brush, (float)(bmpWidth / widthOfPicture * point.X - vertexRadius),
                    (float)(bmpHeight / heightOfPicture * point.Y - vertexRadius), 2 * vertexRadius, 2 * vertexRadius);
            }
            if (showWeights)
            {
                for (int i = 0; i < NumVertieces; i++)
                    for (int j = 0; j < NumVertieces; j++)
                    {
                        if (weightsMatrix[i, j] == 0)
                            continue;
                        Vector mid = (vertexLocation[i] + vertexLocation[j]) / 2;
                        Font myFont = new Font("Arial", 15, FontStyle.Bold);
                        if (!IsDivided || IsDivided && isOnLeftPart[i] == isOnLeftPart[j])
                            graphics.DrawString($"{weightsMatrix[i, j]}", myFont, Brushes.Green, new Point((int)mid.X, (int)mid.Y));
                        else if (IsDivided)
                            graphics.DrawString($"{weightsMatrix[i, j]}", myFont, Brushes.Red, new Point((int)mid.X, (int)mid.Y));


                    }
            }
        }

        Vector[] speeds;
        public void RandomSpeeds()
        {
            double k = (double)Math.Min(widthOfPicture * (IsDivided ? halfShare : 1), heightOfPicture) / 800;
            speeds = new Vector[NumVertieces];
            for (int i = 0; i < NumVertieces; i++)
            {
                double speed = random.Next(10, 80) * k;
                double angle = random.Next(0, (int)(2 * Math.PI * 1e8)) / 1e8;
                speeds[i] = new Vector((Math.Cos(angle) * speed), (Math.Sin(angle) * speed));
            }

        }

        /// <summary>
        /// Изменяет положение графа, считая что прошло t (const) секунд
        /// </summary>
        public void NextStepOfForceBasedAlgtorithm()
        {
            int widthOfDrawingArea = widthOfPicture;
            if (IsDivided)
                widthOfDrawingArea = (int)(widthOfDrawingArea * halfShare);
            int shift = (int)((1 - halfShare) * widthOfPicture); //сдвиг до левой границы правой части рисования = сдвиг до правой границы левой части
            double k = Math.Sqrt(widthOfDrawingArea * heightOfPicture) / 800;
            //double k = (double)Math.Min(widthOfPicture, heightOfPicture) / 800;
            const double t = 0.07;
            double spring = 3 * Math.Pow(NumVertieces, 0.7);
            double springLength = Math.Sqrt(widthOfDrawingArea * heightOfPicture) * 300 / Math.Pow(NumVertieces, 6);
            //double springLength = Math.Min(widthOfPicture, heightOfPicture) * 300 / Math.Pow(n, 6);
            double squareOfElectricCharge = 1000 * Math.Pow(NumVertieces, 0.5) * Math.Pow(k, 1.5);
            double friction = 2.5 * Math.Pow(NumVertieces, 0.2);
            double borderFriction = 500;
            if (speeds == null)
                RandomSpeeds();
            for (int i = 0; i < NumVertieces; i++)
            {
                if (i == MovableVertex)
                    continue;
                vertexLocation[i] += speeds[i] * t;
                vertexLocation[i].X = Math.Max(vertexLocation[i].X, vertexRadius + (IsDivided && !isOnLeftPart[i] ? shift : 0));
                vertexLocation[i].X = Math.Min(vertexLocation[i].X, widthOfPicture - vertexRadius - (IsDivided && isOnLeftPart[i] ? shift : 0));
                vertexLocation[i].Y = Math.Max(vertexLocation[i].Y, vertexRadius);
                vertexLocation[i].Y = Math.Min(vertexLocation[i].Y, heightOfPicture - vertexRadius);
                if (vertexLocation[i].X == widthOfPicture - vertexRadius - ((IsDivided && isOnLeftPart[i] ? shift : 0)) ||
                    vertexLocation[i].X == vertexRadius + (IsDivided && !isOnLeftPart[i] ? shift : 0))
                {
                    speeds[i].X *= -1;
                }
                if (vertexLocation[i].Y == heightOfPicture - vertexRadius || vertexLocation[i].Y == vertexRadius)
                {
                    speeds[i].Y *= -1;
                }
            }
            for (int i = 0; i < NumVertieces; i++)
            {
                for (int j = 0; j < NumVertieces; j++)
                {
                    if (j == i || IsDivided && (isOnLeftPart[i] != isOnLeftPart[j]))
                        continue;
                    Vector ort = vertexLocation[i] - vertexLocation[j];
                    if (ort.Abs() != 0)
                        ort /= ort.Abs();
                    Vector f = ort;
                    if ((vertexLocation[i] - vertexLocation[j]).Abs() != 0)
                        f /= Math.Pow((vertexLocation[i] - vertexLocation[j]).Abs(), 0.5);
                    f *= squareOfElectricCharge;
                    if (IsDivided && (numVertexComponent[i] != numVertexComponent[j]))
                    {
                        if ((vertexLocation[i] - vertexLocation[j]).Abs() != 0)
                        {
                            f.X /= Math.Pow((vertexLocation[i] - vertexLocation[j]).Abs(), 1.6);
                            f.Y /= Math.Pow((vertexLocation[i] - vertexLocation[j]).Abs(), 1.2);
                        }
                        speeds[i] += f * t;
                    }
                    else
                        speeds[i] += f * t; //прибавляем к скорости (= импульсу) электростатическую силу отталкивания, умноженную на изменение времени
                    if (weightsMatrix[i, j] != 0)
                    {
                        f = ort * spring * (springLength - (vertexLocation[i] - vertexLocation[j]).Abs());
                        //if ((vertexLocation[i] - vertexLocation[j]).Abs() != 0)
                        //    f = ort * spring * Math.Log(springLength / (vertexLocation[i] - vertexLocation[j]).Abs());
                        speeds[i] += f * t; //прибавляем к скорости (= имульсу) силу растяжения / сжатия пружины
                    }
                }
                if (speeds[i].Abs() != 0)
                {
                    //friction /= 5;
                    Vector fFric = -(speeds[i] / speeds[i].Abs()) * t * friction * speeds[i].Abs();
                    //friction *= 5;
                    if (fFric.Abs() > speeds[i].Abs())
                        speeds[i] = new Vector(0, 0);
                    else
                        speeds[i] += fFric;
                }
                if (speeds[i].Y != 0 && (vertexLocation[i].X == vertexRadius + (IsDivided && !isOnLeftPart[i] ? shift : 0)
                    || vertexLocation[i].X == widthOfDrawingArea - vertexRadius - (IsDivided && isOnLeftPart[i] ? shift : 0)))
                {
                    if (speeds[i].Y > 0)
                        speeds[i].Y -= Math.Min(speeds[i].Y, borderFriction * t);
                    else
                        speeds[i].Y += Math.Min(-speeds[i].Y, borderFriction * t);
                }
                if (speeds[i].X != 0 && (vertexLocation[i].Y == vertexRadius || vertexLocation[i].Y == heightOfPicture - vertexRadius))
                {
                    if (speeds[i].X > 0)
                        speeds[i].X -= Math.Min(speeds[i].X, borderFriction * t);
                    else
                        speeds[i].X += Math.Min(-speeds[i].X, borderFriction * t);
                }
            }
        }

        public int couuntCrossingCost()
        {
            if (!isDivided)
                return 0;
            int sum = 0;
            for (int i = 0; i < NumVertieces; i++)
                for (int j = 0; j < i; j++)
                    if (isOnLeftPart[i] != isOnLeftPart[j])
                        sum += weightsMatrix[i, j];
            return sum;
        }

        public void KernighanLinAlgorithm()
        {
            const int INF = 1000000;
            int g_max = -INF;
            do
            {
                int[] D = new int[NumVertieces];
                for (int i = 0; i < NumVertieces; i++)
                    for (int j = 0; j < NumVertieces; j++)
                    {
                        if (isOnLeftPart[i] != isOnLeftPart[j])
                            D[i] += weightsMatrix[i, j];
                        else
                            D[i] -= weightsMatrix[i, j];
                    }
                List<int> gv = new List<int>(), av = new List<int>(), bv = new List<int>();
                bool[] used = new bool[NumVertieces];
                for (int n = 0; n < NumVertieces / 2; n++)
                {
                    int g = -INF;
                    int a = -1, b = -1;
                    for (int i = 0; i < NumVertieces; i++)
                    {
                        if (used[i])
                            continue;
                        for (int j = 0; j < NumVertieces; j++)
                        {
                            if (used[j])
                                continue;
                            if (isOnLeftPart[i] == isOnLeftPart[j] || used[j])
                                continue;
                            if (D[i] + D[j] - 2 * weightsMatrix[i, j] > g)
                            {
                                g = D[i] + D[j] - 2 * weightsMatrix[i, j];
                                a = i;
                                b = j;
                            }
                        }
                    }
                    
                    gv.Add(g);
                    av.Add(a);
                    bv.Add(b);
                    used[a] = true;
                    used[b] = true;

                    D = new int[NumVertieces];
                    for (int i = 0; i < NumVertieces; i++)
                    {
                        if (used[i])
                            continue;
                        for (int j = 0; j < NumVertieces; j++)
                        {
                            if (isOnLeftPart[i] != isOnLeftPart[j])
                                D[i] += weightsMatrix[i, j];
                            else
                                D[i] -= weightsMatrix[i, j];
                        }
                    }
                }
                int k = -1;
                g_max = -INF;
                int sum = 0;
                for (int i = 0; i < gv.Count(); i++)
                {
                    sum += gv[i];
                    if (gv[i] > g_max)
                    {
                        k = i;
                        g_max = gv[i];
                    }
                }
                if (g_max > 0)
                {
                    for (int i = 0; i <= k; i++)
                    {
                        bool tmp = isOnLeftPart[bv[i]];
                        isOnLeftPart[bv[i]] = isOnLeftPart[av[i]];
                        isOnLeftPart[av[i]] = tmp;
                        Vector tmpVector = vertexLocation[bv[i]];
                        vertexLocation[bv[i]] = vertexLocation[av[i]];
                        vertexLocation[av[i]] = tmpVector;
                    }
                }

            } while (g_max > 0);
            numerateVertexComponents();
        }

        public int MovableVertex = -1; // Номер вершины, на которой зажата мышь в текущий момент, -1 если ни одна вершина не перемещается
        /// <summary>
        /// Возращает true, если координаты нажатия мышкой соответствуют одной из вершин и запоминает ее
        /// </summary>
        /// <param name="x">x-координата мышки при нажатии</param>
        /// <param name="y">y-координата мышки при нажатии</param>
        public bool MouseDownOnVertex(int x, int y)
        {
            List<int> suitableVertexies = new List<int>();
            double[] distances = new double[NumVertieces];
            double minDist = vertexRadius + 1;
            for (int i = 0; i < NumVertieces; i++)
            {
                distances[i] = vertexRadius + 1;
                if ((vertexLocation[i] - new Vector(x, y)).Abs() <= vertexRadius)
                {
                    distances[i] = (vertexLocation[i] - new Vector(x, y)).Abs();
                    minDist = Math.Min(minDist, distances[i]);
                }
            }
            if (minDist > vertexRadius)
                return false;
            for (int i = 0; i < NumVertieces; i++)
                if (distances[i] == minDist)
                    suitableVertexies.Add(i);
            Random random = new Random();
            MovableVertex = suitableVertexies[random.Next(suitableVertexies.Count)];
            return true;
        }
        /// <summary>
        /// Двигает вершину в соответствии с действиями пользователя
        /// </summary>
        /// <param name="xPlus">прибавление по координате x</param>
        /// <param name="yPlus">прибавление по координате y</param>
        /// <returns></returns>
        public bool VertexMooving(int xPlus, int yPlus)
        {
            int shift = (int)((1 - halfShare) * widthOfPicture); //сдвиг до левой границы правой части рисования = сдвиг до правой границы левой части
            if (MovableVertex == -1)
                return false;
            ref Vector v = ref vertexLocation[MovableVertex];
            v.X += xPlus;
            v.Y += yPlus;
            v.X = Math.Max(v.X, vertexRadius + (IsDivided && !isOnLeftPart[MovableVertex] ? shift : 0));
            v.X = Math.Min(v.X, widthOfPicture - vertexRadius - (IsDivided && isOnLeftPart[MovableVertex] ? shift : 0));
            v.Y = Math.Max(v.Y, vertexRadius);
            v.Y = Math.Min(v.Y, heightOfPicture - vertexRadius);
            return true;
        }

        public int[,] GetWeightMatrixCopy() => (int[,])weightsMatrix.Clone();

        /// <summary>
        /// Объединяет все компоненты графа в одну, если их больше
        /// </summary>
        /// <param name="weightMatrix">матрица весов графа</param>
        /// <param name="minEdgeWeight">минимально возможный вес ребра</param>
        /// <param name="maxEdgeWeight">максимально возможный вес ребра</param>
        private static void LeadToOneConnectedComponent(int[,] weightMatrix, int minEdgeWeight, int maxEdgeWeight)
        {
            int n = weightMatrix.GetLength(0);
            do
            {
                int[] used = new int[n];
                List<int> theComponent = new List<int>();
                List<int> otherComponent = new List<int>();
                Queue<int> queue = new Queue<int>();
                queue.Enqueue(0);
                while (queue.Count != 0)
                { // Обход в ширину для нахождения всех вершин текущей компоненты связности
                    int u = queue.Dequeue();
                    used[u] = 2;
                    theComponent.Add(u);
                    for (int v = 0; v < n; v++)
                        if ((used[v] == 0) && weightMatrix[u, v] != 0)
                        {
                            queue.Enqueue(v);
                            used[v] = 1;
                        }
                }
                for (int i = 0; i < n; i++)
                    if (used[i] == 0)
                        otherComponent.Add(i);
                if (otherComponent.Count != 0)
                {
                    int t1 = random.Next(theComponent.Count), t2 = random.Next(otherComponent.Count);
                    weightMatrix[theComponent[t1], otherComponent[t2]] =
                        weightMatrix[otherComponent[t2], theComponent[t1]] = random.Next(minEdgeWeight, maxEdgeWeight);

                }
                else return;
            } while (true);

        }
    }
}

