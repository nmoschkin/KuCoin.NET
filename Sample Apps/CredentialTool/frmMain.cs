using KuCoin.NET;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CredentialTool
{
    public partial class frmMain : Form
    {
        CryptoCredentials cred;
        CryptoCredentials futuresCred;

        string pin;

        private bool changed;
        public bool Changed
        {
            get => changed;
            set
            {
                changed = value;
                
                if (changed)
                {
                    if (chkFutures.Checked)
                    {
                        Text = "KuCoin Futures Credentials Editor*";
                    }
                    else
                    {
                        Text = "KuCoin Credentials Editor*";
                    }
                }
                else
                {
                    if (chkFutures.Checked)
                    {
                        Text = "KuCoin Futures Credentials Editor";
                    }
                    else
                    {
                        Text = "KuCoin Credentials Editor";
                    }
                }

                btnDiscard.Enabled = changed;
                btnSave.Enabled = changed;
            }
        }

        public frmMain()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            ToggleShowHide();
            Changed = false;

            txtKey.TextChanged += TxtKey_TextChanged;
            txtSecret.TextChanged += TxtSecret_TextChanged;
            txtPassphrase.TextChanged += TxtPassphrase_TextChanged;
            btnDiscard.Click += BtnDiscard_Click;
            btnSave.Click += BtnSave_Click;
            btnExit.Click += BtnExit_Click;
            btnNewPin.Click += BtnNewPin_Click;
            btnRemove.Click += BtnRemove_Click;
            chkFutures.Click += ChkFutures_Click;
            chkShow.Click += ChkShow_Click;
            
            this.FormClosing += FrmMain_FormClosing;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(this, "Are you sure you want to delete the credentials for this pin?\r\nThis cannot be undone!", "Delete Credentials?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.No)
            {
                return;
            }

            CryptoCredentials.DeleteCredentials(pin);
            
            cred = CryptoCredentials.LoadFromStorage(Seed, pin);
            futuresCred = (CryptoCredentials)cred.AttachedAccount;

            Changed = false;
            ToggleFutures();
        }

        private void BtnNewPin_Click(object sender, EventArgs e)
        {
            if (Changed)
            {
                var res = MessageBox.Show(this, "You have unsaved changes. Switch anyway?", "Switch Credentials", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == DialogResult.No)
                {
                    return;
                }
            }

            var pin = frmPin.GetPin();

            if (pin != null)
            {
                cred = CryptoCredentials.LoadFromStorage(Seed, pin);
                futuresCred = (CryptoCredentials)cred.AttachedAccount;
                this.pin = pin;
                Changed = false;
                ToggleFutures();
            }
            else
            {
                return;
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(this, "Save Changes?", "Save Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.No)
            {
                return;
            }

            try
            {

                if (chkFutures.Checked)
                {
                    futuresCred.Key = txtKey.Text;
                    futuresCred.Secret = txtSecret.Text;
                    futuresCred.Passphrase = txtPassphrase.Text;
                }
                else
                {
                    cred.Key = txtKey.Text;
                    cred.Secret = txtSecret.Text;
                    cred.Passphrase = txtPassphrase.Text;
                }

                cred.SaveToStorage(pin);
                Changed = false;
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDiscard_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(this, "Are you sure you want to discard your changes?", "Discard Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (res == DialogResult.No)
            {
                return;
            }

            Changed = false;
            ToggleFutures();
        }

        private void ToggleFutures()
        {
            if (Changed)
            {
                var res = MessageBox.Show(this, "You have unsaved changes. Switch anyway?", "Switch Credentials", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == DialogResult.No)
                {
                    return;
                }
            }

            if (chkFutures.Checked)
            {
                txtKey.Text = futuresCred.GetKey();
                txtSecret.Text = futuresCred.GetSecret();
                txtPassphrase.Text = futuresCred.GetPassphrase();
            }
            else
            {
                txtKey.Text = cred.GetKey();
                txtSecret.Text = cred.GetSecret();
                txtPassphrase.Text = cred.GetPassphrase();
            }

            Changed = false;
        }

        private void ToggleShowHide()
        {
            if (chkShow.Checked)
            {
                txtKey.PasswordChar = (char)0;
                txtSecret.PasswordChar = (char)0;
                txtPassphrase.PasswordChar = (char)0;
            }
            else
            {
                txtKey.PasswordChar = '*';
                txtSecret.PasswordChar = '*';
                txtPassphrase.PasswordChar = '*';
            }
        }

        private void ChkShow_Click(object sender, EventArgs e)
        {
            ToggleShowHide();
        }

        private void ChkFutures_Click(object sender, EventArgs e)
        {
            ToggleFutures();
            Changed = changed;

        }

        private void TxtPassphrase_TextChanged(object sender, EventArgs e)
        {
            Changed = true;
        }

        private void TxtSecret_TextChanged(object sender, EventArgs e)
        {
            Changed = true;
        }

        private void TxtKey_TextChanged(object sender, EventArgs e)
        {
            Changed = true;
        }

        public static string Hvcyp
        {
            get
            {
                return CryptoCredentials.GetHvcyp().ToString("d");
            }
            set
            {
                var g = Guid.Parse(value);
                CryptoCredentials.GetHvcyp(g);
            }
        }

        public static Guid Seed
        {
            get => Guid.Parse(Hvcyp);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Changed)
            {
                var res = MessageBox.Show(this, "You have unsaved changes. Exit anyway?", "Exit App", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (res == DialogResult.Yes)
                {
                    Environment.Exit(0);
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            var pin = frmPin.GetPin();

            if (pin != null)
            {
                cred = CryptoCredentials.LoadFromStorage(Seed, pin);
                futuresCred = (CryptoCredentials)cred.AttachedAccount;
                this.pin = pin;
                ToggleFutures();
            }
            else
            {
                Environment.Exit(0);
            }

        }
    }
}
