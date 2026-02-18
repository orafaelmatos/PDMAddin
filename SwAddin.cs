using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Runtime.InteropServices;


namespace PDMAddin
{
    [Guid("D2234567-E89B-12D3-A456-426614174001")]
    [ComVisible(true)]
    public class SwAddin : ISwAddin
    {
        private ISldWorks _swApp;

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            _swApp = (ISldWorks)ThisSW;

            _swApp.SendMsgToUser2(
                "PDM Addin Loaded Successfully",
                (int)swMessageBoxIcon_e.swMbInformation,
                (int)swMessageBoxBtn_e.swMbOk);

            return true;
        }

        public bool DisconnectFromSW()
        {
            return true;
        }

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            string keyname = $@"SOFTWARE\SolidWorks\Addins\{{{t.GUID.ToString().ToUpper()}}}";
            using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(keyname))
            {
                rk.SetValue(null, "Gestor Addin");
                rk.SetValue("Description", "Vault and Changes Manager");
                rk.SetValue("LoadAtStartup", 1);
            }

            string startupKey = $@"SOFTWARE\SolidWorks\AddinsStartup\{{{t.GUID.ToString().ToUpper()}}}";
            using (RegistryKey rk = Registry.LocalMachine.CreateSubKey(startupKey))
            {
                rk.SetValue(null, 1);
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            Registry.LocalMachine.DeleteSubKey($@"SOFTWARE\SolidWorks\Addins\{{{t.GUID.ToString().ToUpper()}}}", false);
            Registry.LocalMachine.DeleteSubKey($@"SOFTWARE\SolidWorks\AddinsStartup\{{{t.GUID.ToString().ToUpper()}}}", false);
        }
    }

}
