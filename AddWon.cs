using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Text_Raffle
{
    public partial class frmAddWon : Form
    {
        public string inp = "";
        private bool but = false;

        // Initializes a new instance of the <see cref="frmAddWon"/> class.
        public frmAddWon()
        {
            InitializeComponent();
        }

        // Returns the input from the variable inp to the parent form.
        public string getInput()
        {
            if (!but)
                return "";
            else
                return inp;
        }

        // Adds the input to the won entry list, and closes the form.
        private void btnAdd_Click(object sender, EventArgs e)
        {
            inp = txtAdd.Text;
            but = true;
            this.Hide();
        }
    }
}
