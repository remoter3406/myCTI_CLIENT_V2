using System.Windows.Forms;

namespace myCTI2
{
    public partial class InputBox : Form
    {
        public InputBox(string prompt, string title, string defaultresponse, bool usePasswordChar = false)
        {
            InitializeComponent();
            if (usePasswordChar)
                textBox1.PasswordChar = PasswordChar;
            this.CancelButton = buttonAbbrechen;    // Wird ausgelöst, wenn ESC gedrückt wird
            this.AcceptButton = buttonOK;    // wird bei OK ausgelöst
            label1.Text = prompt;
            textBox1.Text = defaultresponse;
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        private string _prompt;
        public string Prompt
        {
            get
            {
                return _prompt;
            }
            set
            {
                _prompt = value;
            }
        }

        private bool _usepasswordchar;
        public bool UsePassWordChar
        {
            get
            {
                return _usepasswordchar;
            }
            set
            {
                _usepasswordchar = value;
            }
        }

        private char _passwordChar = '*';
        public char PasswordChar
        {
            get
            {
                return _passwordChar;
            }
            set
            {
                _passwordChar = value;
            }
        }

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

