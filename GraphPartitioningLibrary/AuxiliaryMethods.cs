using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPartitioningLibrary
{
    public class AuxiliaryMethods
    {
        /// <summary>
        /// расставляет все элементы массива случайным образом
        /// </summary>
        /// <param name="arr">массив для перестановки</param>
        public static void RandomShuffle<T>(T[] arr)
        {
            Random random = new Random();
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                Swap(ref arr[i], ref arr[random.Next(i)]);                
            }
        }

        /// <summary>
        /// меняет две переменные местами
        /// </summary>
        /// <typeparam name="T">тип переменной</typeparam>
        /// <param name="a1">первая переменная</param>
        /// <param name="a2">вторая переменная</param>
        public static void Swap<T>(ref T a1, ref T a2)
        {
            T tmp = a1;
            a1 = a2;
            a2 = tmp;
        }
    }
}
