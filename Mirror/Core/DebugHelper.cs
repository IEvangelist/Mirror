using System;
using System.Diagnostics;


namespace Mirror.Core
{
    static class DebugHelper
    {
        internal static bool IsNotHandled(Exception ex)
        {
            bool isHandled = true;

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            else
            {
                Debug.WriteLine(ex);
                isHandled = false;
            }

            return isHandled == false;
        }
    }
}