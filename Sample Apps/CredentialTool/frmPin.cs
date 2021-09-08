using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CredentialTool
{
    public partial class frmPin : Form
    {
        string pin = "";

        public string Pin => pin;

        public bool IsValidPin
        {
            get
            {
                if (!string.IsNullOrEmpty(pin) && pin.Length == 6 && int.TryParse(pin, out _))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public frmPin()
        {
            InitializeComponent();

            button1.Enabled = false;
            label2.Visible = true;

            this.AcceptButton = button1;
            this.CancelButton = button2;

            textBox1.TextChanged += TextBox1_TextChanged;
        }

        public static string GetPin()
        {
            var pin = new frmPin();
            pin.ShowDialog();

            if (pin.IsValidPin && pin.DialogResult == DialogResult.OK)
            {
                return pin.Pin;
            }
            else
            {
                return null;
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                button1.Enabled = false;
                return;
            }

            if (int.TryParse(textBox1.Text, out int pin))
            {
                this.pin = textBox1.Text;
            }
            else
            {
                if (textBox1.Text.Length == 1)
                {
                    textBox1.Text = "";
                }
                else
                {
                    textBox1.Text = this.pin.ToString();
                }
            }

            button1.Enabled = IsValidPin;
            label2.Visible = !IsValidPin;
        }
    }
}
