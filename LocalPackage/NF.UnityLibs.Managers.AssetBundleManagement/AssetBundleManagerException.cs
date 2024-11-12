using System;

namespace NF.UnityLibs.Managers.AssetBundleManagement
{
    public class AssetBundleManagerException : Exception
    {
        public override string StackTrace { get; }
        public E_EXCEPTION_KIND ExceptionKind { get; }

        public AssetBundleManagerException(E_EXCEPTION_KIND kind, string msg = "") : base(msg)
        {
            ExceptionKind = kind;
            string st = Environment.StackTrace;
            StackTrace = st.Substring(st.IndexOf('\n', st.IndexOf('\n') + 1) + 1);
        }
    }
}
