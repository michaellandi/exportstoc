/*
 * This file is part of ExportsToC++.
 *
 * ExportsToC++ is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ExportsToC++ is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ExportsToC__
{
    public partial class frmName : Form
    {
        public frmName()
        {
            InitializeComponent();
        }

        public string FileName 
        {
            get
            {
                return txtDLL.Text.Trim();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            txtDLL.Text = "";
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}