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
    public partial class frmWon : Form
    {
        List<string> won;

        // Initializes a new instance of the <see cref="frmWon"/> class.
        public frmWon()
        {
            InitializeComponent();
        }

        // Updated list of won entries
        public List<string> updateList()
        {
            won = new List<string>();
            foreach (string val in lbWon.Items)
            {
                won.Add(val);
            }
            return won;
        }

        // Loads the list from the parent form to the listbox.
        public void loadList(List<string> won)
        {
            foreach (string entry in won)
            {
                lbWon.Items.Add(entry);
            }
        }

        // Removes an entry from the list.
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (lbWon.SelectedItems.Count > 0)
            {
                lbWon.Items.Remove(lbWon.SelectedItems[0]);
            }
        }

        // Adds an entry to the list.
        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmAddWon awFrm = new frmAddWon();
            awFrm.ShowDialog(this);
            string toAdd = awFrm.getInput();
            if (toAdd != "")
            {
                lbWon.Items.Add(toAdd);
            }
        }
    }
}
