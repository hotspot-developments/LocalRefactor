using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HotspotDevelopments.LocalRefactor
{
    public partial class NameForm : Form
    {
        bool alreadyFocused = false;

        public NameForm()
        {
            InitializeComponent();            
        }

        public string Label
        {
            get { return lblName.Text; }
            set { lblName.Text = value; }
        }

        public string EnteredName
        {
            get { return txtName.Text; }
            set
            {
                txtName.Text = value; 
            }
        }

        private void NameForm_Shown(object sender, EventArgs e)
        {
            txtName.Focus();
        }

        void txtName_Leave(object sender, EventArgs e)
        {
            alreadyFocused = false;
        }


        void txtName_GotFocus(object sender, EventArgs e)
        {
            // Select all text only if the mouse isn't down.
            // This makes tabbing to the textbox give focus.
            if (MouseButtons == MouseButtons.None)
            {
                this.txtName.SelectAll();
                alreadyFocused = true;
            }
        }

        void txtName_MouseUp(object sender, MouseEventArgs e)
        {
            // Web browsers like Google Chrome select the text on mouse up.
            // They only do it if the textbox isn't already focused,
            // and if the user hasn't selected all text.
            if (!alreadyFocused && this.txtName.SelectionLength == 0)
            {
                alreadyFocused = true;
                this.txtName.SelectAll();
            }
        }
    }
}
