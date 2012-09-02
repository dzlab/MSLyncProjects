using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LyncPBX
{
    public class UcmaObject
    {
        public static void InvokeDelegates(MulticastDelegate mDelegate, object sender, EventArgs e)
        {
            if (mDelegate == null)
                return;

            Delegate[] delegates = mDelegate.GetInvocationList();
            if (delegates == null)
                return;

            // Invoke delegates within their threads
            foreach (Delegate _delegate in delegates)
            {
                if (_delegate.Target.GetType().ContainsGenericParameters)
                {
                    Console.WriteLine("Cannot invoke event handler on a generic type.");
                    return;
                }

                object[] contextAndArgs = { sender, e };
                ISynchronizeInvoke syncInvoke = _delegate.Target as ISynchronizeInvoke;
                {
                    if (syncInvoke != null)
                    {
                        // This case applies to the situation when Delegate.Target is a 
                        // Control with an open window handle.
                        syncInvoke.Invoke(_delegate, contextAndArgs);
                    }
                    else
                    {
                        _delegate.DynamicInvoke(contextAndArgs);
                    }
                }
            }
        }
        public static void RaiseEvent(MulticastDelegate mDelegate, object sender, EventArgs e)
        {
            InvokeDelegates(mDelegate, sender, e);
        }

        public static void Trace(string msg)
        {
            Console.WriteLine(msg);
        }
        public static string NewLine { get { return Environment.NewLine; } }
        public static string Tab { get { return "\t"; } }
    }


}
