using System;

namespace BinarySearchImplementation
{
    class Program
    {
        public static int BinarySearchIterative(int[] array, int item)
        {
            int low = 0;
            int high = array.Length - 1;
            while(low <= high)
            {
                int guess = (low + high) / 2;
                if (array[guess] == item) return guess;
                else if (array[guess] > item) high = guess - 1;
                else low = guess + 1;
            }
            return -1;
        }

        public static int BinarySearchRecursive(int[] array , int item)
        {
           return BinarySearchRecursiveStep(array, 0, array.Length - 1, item);
        }

        private static int BinarySearchRecursiveStep(int[] array, int low, int high, int item)
        {
            if (low > high) return -1;
            int guess = (low + high) / 2;

                if (array[guess] == item) return guess;
                else if (array[guess] < item) return BinarySearchRecursiveStep(array, guess + 1, high, item);
                else return BinarySearchRecursiveStep(array, low, guess - 1, item);
        }

        static void Main(string[] args)
        {
            
            int[] sampleSortedArray = new int[] { 2,3,5,7,11,13,17,19,23,29,31,37,41 };
            int searchItem = 10;
            
            int resultIterative = BinarySearchIterative(sampleSortedArray, searchItem);
            int resultRecursive = BinarySearchRecursive(sampleSortedArray, searchItem);
            Console.WriteLine("Iterative result is: "+ resultIterative);
            Console.WriteLine("Recursive result is: "+ resultRecursive);
        }

    }
}
