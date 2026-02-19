using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using GestorAddin.UI;

namespace PDMAddin
{
    public partial class TaskpaneHost : UserControl
    {
        private ElementHost _elementHost;

        public TaskpaneHost()
        {
            InitializeComponent();

            if (System.Windows.Application.Current == null)
            {
                new GestorAddin.UI.App { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
            }

            this.BackColor = Color.White;

            _elementHost = new ElementHost();
            _elementHost.Dock = DockStyle.Fill;
            _elementHost.BackColor = Color.White;
            _elementHost.Visible = true;

            this.Controls.Add(_elementHost);
            _elementHost.Child = new MainWindow();
        }
    }
}
