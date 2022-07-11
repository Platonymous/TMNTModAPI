using System;

namespace ModLoader.Events
{
    public class ContextSwitchedEventArgs
    {
        public string NewContext { get; private set; }

        internal Action<string> switchContextAction;

        public ContextSwitchedEventArgs(string newContext, Action<string> switchContext)
        {
            NewContext = newContext;
            switchContextAction = switchContext;
        }

        public void SwitchContext(string context)
        {
            switchContextAction(context);
        }
    }
}
