using System.Windows.Forms;

namespace myCTI2
{
    public partial class InputBox : Form
    {
        private const char passwordChar = '*';

        public InputBox(string prompt, string title, string defaultresponse, bool usePasswordChar = false)
        {
            InitializeComponent();
            if (usePasswordChar)
                textBox1.PasswordChar = passwordChar;
            this.CancelButton = buttonAbbrechen;    // Wird ausgelöst, wenn ESC gedrückt wird
            this.AcceptButton = buttonOK;    // wird bei OK ausgelöst
            label1.Text = prompt;
            textBox1.Text = defaultresponse;
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        // eingegebener Text nach OK, "" nach Abbrechen
        public string Prompt { get; private set; } = "";

        private void buttonOK_Click(System.Object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Prompt = textBox1.Text;
            this.Close();
        }
        private void buttonAbbrechen_Click(System.Object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Prompt = "";
            this.Close();
        }

    }
}
