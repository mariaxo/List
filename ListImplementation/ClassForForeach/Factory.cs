using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ClassForForeach
{
    /// <summary>
    /// this class does not implement the IEnumerable interface, but still can be used in a foreach statement
    /// </summary>
    /// 

    //foreach works if the class has GetEnumerator() method, iterator used
    class Factory
    {
        Product[] _products;

        public Factory(Product[] products)
        {
            _products = products;
        }
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < _products.Length; i++)
            {
                yield return _products[i];
            }  
        }
    }
}
