using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace PDMAddin
{
    [Guid("D2234567-E89B-12D3-A456-426614174001")]
    [ComVisible(true)]
    [ProgId("PDMAddin.SwAddin")]
    public class SwAddin : ISwAddin
    {
        private ISldWorks _swApp;

        private int _cookie;

        private TaskpaneView _taskpaneView;
        private TaskpaneHost _taskpaneHost;

        static SwAddin()
        {
            System.Windows.Media.RenderOptions.ProcessRenderMode =
                System.Windows.Interop.RenderMode.SoftwareOnly;

            System.AppContext.SetSwitch(
                "Switch.System.Windows.Media.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable",
                true);
        }

        private void CreateTaskpane()
        {
            string iconPath = @"C:\PDMAddin\icon.bmp"; // 20x20 BMP

            _taskpaneView = _swApp.CreateTaskpaneView2(iconPath, "Gestor");

            _taskpaneHost = new TaskpaneHost();

            _taskpaneView.DisplayWindowFromHandle(_taskpaneHost.Handle.ToInt32());
        }

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            _swApp = (ISldWorks)ThisSW;
            _cookie = Cookie;

            _swApp.SetAddinCallbackInfo2(0, this, _cookie);

            CreateTaskpane();

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
