using System;
using System.Collections;

namespace MS.Internal.Controls
{
    internal class EmptyEnumerator : IEnumerator
    {
        private static IEnumerator _instance;

        public static IEnumerator Instance
        {
            get
            {
                if (EmptyEnumerator._instance == null)
                {
                    EmptyEnumerator._instance = new EmptyEnumerator();
                }
                return EmptyEnumerator._instance;
            }
        }

        public object Current
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        private EmptyEnumerator()
        {
        }

        public void Reset()
        {
        }

        public bool MoveNext()
        {
            return false;
        }
    }
}
