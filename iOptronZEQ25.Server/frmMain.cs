using System.Windows.Forms;

namespace ASCOM.iOptronZEQ25
{
    public partial class frmMain : Form
    {
        private delegate void SetTextCallback(string text);

        public frmMain()
        {
            InitializeComponent();
        }
    }
}