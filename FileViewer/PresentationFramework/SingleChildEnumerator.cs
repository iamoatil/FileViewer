using System;
using System.Collections;

namespace System.Windows
{
    internal class SingleChildEnumerator : IEnumerator
    {
        private int _index = -1;

        private int _count;

        private object _child;

        object IEnumerator.Current
        {
            get
            {
                if (this._index != 0)
                {
                    return null;
                }
                return this._child;
            }
        }

        internal SingleChildEnumerator(object Child)
        {
            this._child = Child;
            this._count = ((Child == null) ? 0 : 1);
        }

        bool IEnumerator.MoveNext()
        {
            this._index++;
            return this._index < this._count;
        }

        void IEnumerator.Reset()
        {
            this._index = -1;
        }
    }
}
