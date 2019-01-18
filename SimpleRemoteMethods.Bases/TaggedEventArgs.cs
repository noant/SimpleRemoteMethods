using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleRemoteMethods.Bases
{
    public class TaggedEventArgs<T> : EventArgs
    {
        public TaggedEventArgs(T target)
        {
            Target = target;
        }

        public T Target { get; }
    }
}
