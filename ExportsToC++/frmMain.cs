/*
 * ExportsToC++
 * Version 1.0
 * Copyright © 2008 Michael Landi
 *
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
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ExportsToC__
{
    public partial class frmMain : Form
    {
        private const string FORMSTITLE     =   "ExportsToC++";
        private const string SEARCHPATH     =   @"%programfiles%\Microsoft Visual Studio 8\Common7\IDE;%programfiles%\Microsoft Visual Studio 9.0\Common7\IDE";
        private const string DEFAULTPTH     =   "dumpbin.exe";
        private const string STARTSTRNG     =   "ordinal hint RVA      name";
        private const string STOPSTRING     =   "Summary";

        private string  _strCurrent;
        private string  _strDumpbinPath;

        public frmMain(string[] args)
        {
            _strCurrent = "";
            _strDumpbinPath = DEFAULTPTH;
            InitializeComponent();

            horizontalToolStripMenuItem_Click(this, EventArgs.Empty);

            if (!DoesDumpbinExist())
            {
                MessageBox.Show(this, "\"Dumpbin.exe\" could not be located on this system.  Please add the file's directory to your environmental PATH variable.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            if (args.Length == 1)
                DumpFile(args[0]);
            
        }

        private string AdditionalPaths
        {
            get
            {
                return SEARCHPATH.Replace("%programfiles%", Environment.GetEnvironmentVariable("ProgramFiles"));
            }
        }

        private void GenerateCPPCode()
        {
            if (_strCurrent == "" || txtExported.Text == "")
            {
                DialogResult dResult = MessageBox.Show(this, "You have not opened an executable file.  Would you like to now?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dResult == DialogResult.Yes)
                    openToolStripMenuItem_Click(this, EventArgs.Empty);

                return;
            }

            /*
             * Prompt for a filename.
             */
            frmName frmPromptFile = new frmName();
            frmPromptFile.ShowDialog(this);

            if (frmPromptFile.FileName == "")
                return;
            else if (!txtExported.Text.Contains(STARTSTRNG))
            {
                MessageBox.Show(this, "The selected file does not export any functions.  Are you missing a definition file?", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string strBuffer;
            string[] strSplit;
            int intStart = txtExported.Text.IndexOf(STARTSTRNG);
            int intFinish = txtExported.Text.LastIndexOf(STOPSTRING);

            //Clear existing content.
            txtCode.Text = "";

            //Get a substing.
            strBuffer = txtExported.Text.Substring(intStart + STARTSTRNG.Length, intFinish - (intStart + STARTSTRNG.Length)).Replace(Environment.NewLine, "\n");

            /*
             * Generate the file header.
             */
            txtCode.Text = "#include \"stdafx.h\"";
            txtCode.Text += Environment.NewLine;
            txtCode.Text += "#include <iostream>";
            txtCode.Text += Environment.NewLine;
            txtCode.Text += "#include <windows.h>";
            txtCode.Text += Environment.NewLine;
            txtCode.Text += Environment.NewLine;
            txtCode.Text += "using namespace std;";
            txtCode.Text += Environment.NewLine;

            //Split the text into an array seperated by lines.
            strSplit = strBuffer.Split('\n');

            /*
             * Generate each exports statement.
             */
            for (int i = 0; i < strSplit.Length; i++)
            {
                if (strSplit[i].Trim() != "")
                {
                    //Strip all multi-space spaces.
                    while (strSplit[i].Contains("  "))
                        strSplit[i] = strSplit[i].Replace("  ", " ");

                    string[] strPart = strSplit[i].Trim().Split(' ');

                    if (strPart.Length == 4)
                    {
                        txtCode.Text += Environment.NewLine;
                        txtCode.Text += "#pragma comment (linker, \"/export:" + strPart[3] + "=";
                        txtCode.Text += frmPromptFile.FileName.ToLower().Replace(".dll", "") + ".";
                        txtCode.Text += strPart[3] + ",@" + strPart[0] + "\")";
                    }
                    else
                    {
                        txtCode.Text += Environment.NewLine;
                        txtCode.Text += "#pragma comment (linker, \"/export:" + strPart[2] + "=";
                        txtCode.Text += frmPromptFile.FileName.ToLower().Replace(".dll", "") + ".";
                        txtCode.Text += strPart[2] + ",@" + strPart[0] + "\")";
                    }
                }
            }

            /*
             * Generate entry point function.
             */
            txtCode.Text += Environment.NewLine;
            txtCode.Text += Environment.NewLine + "BOOL WINAPI DllMain(HINSTANCE hInst,DWORD reason,LPVOID)";
            txtCode.Text += Environment.NewLine + "{";
            txtCode.Text += Environment.NewLine + "\treturn true;";
            txtCode.Text += Environment.NewLine + "}";
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateCPPCode();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofDialog.Filter = "DLL Files|*.dll";

            DialogResult dResult = ofDialog.ShowDialog(this);

            if (dResult == DialogResult.Cancel)
                return;

            if (!File.Exists(ofDialog.FileName))
                return;

            DumpFile(ofDialog.FileName);
        }

        private void DumpFile(FileInfo fInfo)
        {
            if (fInfo.Exists)
                DumpFile(fInfo.FullName);
            else
                MessageBox.Show(this, "The specified file does not exist.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DumpFile(string file)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = _strDumpbinPath;
                p.StartInfo.EnvironmentVariables["PATH"] += ";" + AdditionalPaths;
                p.StartInfo.Arguments = "/EXPORTS " + file;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                txtExported.Text = p.StandardOutput.ReadToEnd().Trim();

                p.WaitForExit();

                _strCurrent = file;
                this.Text = FORMSTITLE + " - [" + file + "]";
            }
            catch (Exception a)
            {
                MessageBox.Show(this, "An error occurred: " + a.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                _strCurrent = "";
                txtExported.Text = "";
            }
        }

        private bool DoesDumpbinExist()
        {
            bool doesExist = false;

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "dumpbin.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.EnvironmentVariables["PATH"] += ";" + AdditionalPaths;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                doesExist = true;
            }
            catch
            {
                doesExist = false;
            }

            if (!doesExist)
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Microsoft Visual Studio 9.0\VC\bin\dumpbin.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + AdditionalPaths;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();

                    doesExist = true;
                    _strDumpbinPath = p.StartInfo.FileName;
                }
                catch
                {
                    doesExist = false;
                }
            }

            if (!doesExist)
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Microsoft Visual Studio 8\VC\bin\dumpbin.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.EnvironmentVariables["PATH"] += ";" + AdditionalPaths;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();

                    doesExist = true;
                    _strDumpbinPath = p.StartInfo.FileName;
                }
                catch
                {
                    doesExist = false;
                }
            }

            return doesExist;
        }

        private void tsCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtCode.Text);
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SplitCont.Orientation = Orientation.Horizontal;
            SplitCont.SplitterDistance = (int)(.35 * this.Height);
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SplitCont.Orientation = Orientation.Vertical;
            SplitCont.SplitterDistance = (int)(.35 * this.Width);
        }

        private void tsSave_Click(object sender, EventArgs e)
        {
            sfDialog.Filter = "C++ Source File|*.cpp";

            DialogResult dResult = sfDialog.ShowDialog(this);

            if (dResult == DialogResult.Cancel)
                return;

            try
            {
                StreamWriter sWriter = new StreamWriter(sfDialog.FileName);
                sWriter.Write(txtCode.Text);
                sWriter.Close();
            }
            catch (Exception a)
            {
                MessageBox.Show(this, "An error occurred: " + a.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog(this);
        }
    }
}