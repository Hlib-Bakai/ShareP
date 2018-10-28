﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP.Forms
{
    public partial class FormViewer : Form
    {
        public int curentViewerSlide = 0;
        public bool following = true;
        

        public FormViewer()
        {
            InitializeComponent();
            loadingCircle1.NumberSpoke = 36;
        }
        
        public void LoadSlide(int index, bool byButton = false)
        {
            if (!byButton && !following)
            {
                timerBlinkButton.Enabled = true;
                return;
            }

            if (Connection.CurrentGroup.navigation == GroupNavigation.FollowOnly)
            {
                buttonPrevious.Hide();
                buttonNext.Hide();
            }
            else if (Connection.CurrentGroup.navigation == GroupNavigation.Backwards)
            {
                if (index > 1)
                    buttonPrevious.Show();
                else
                    buttonPrevious.Hide();

                if (index < Connection.CurrentPresentation.CurrentSlide)
                    buttonNext.Show();
                else
                    buttonNext.Hide();
            }
            else
            {
                if (index < Connection.CurrentPresentation.SlidesTotal)
                    buttonNext.Show();
                else
                    buttonNext.Hide();

                if (index > 1)
                    buttonPrevious.Show();
                else
                    buttonPrevious.Hide();
            }

            curentViewerSlide = index;
            SetCurrentSlideImage();
            labelCount.Text = String.Format("Slide: {0}/{1}", curentViewerSlide, Connection.CurrentPresentation.SlidesTotal);
        }

        public void SetCurrentSlideImage()
        {
            pictureBox1.Image = null;
            string fileName = Helper.GetCurrentFolder() + @"downloaded\" + (curentViewerSlide.ToString()) + ".jpg";
            bool fileExists = File.Exists(fileName);
            if (fileExists)
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(fileName);
                }
                catch
                {
                    timerRetryLoad.Enabled = true;
                    panelLoading.Show();
                    return;
                }
                panelLoading.Hide();
                if (timerRetryLoad.Enabled)
                    timerRetryLoad.Enabled = false;
            }
            else
            {
                panelLoading.Show();
                timerRetryLoad.Enabled = true;
            }
        }
        

        private void NextSlide()
        {
            if (Connection.CurrentGroup.navigation == GroupNavigation.FollowOnly)
                return;

            if (Connection.CurrentGroup.navigation == GroupNavigation.Backwards)
            {
                if (curentViewerSlide == Connection.CurrentPresentation.SlidesTotal)
                    return;

                if (curentViewerSlide >= Connection.CurrentPresentation.CurrentSlide)
                    return;
            }
            else if (Connection.CurrentGroup.navigation == GroupNavigation.BothDirections)
            {
                if (curentViewerSlide == Connection.CurrentPresentation.SlidesTotal)
                    return;
            }
            following = false;
            LoadSlide(curentViewerSlide + 1, true);
        }

        private void PreviousSlide()
        {
            if (Connection.CurrentGroup.navigation == GroupNavigation.FollowOnly)
                return;
            if (Connection.CurrentGroup.navigation == GroupNavigation.Backwards)
            {
                if (curentViewerSlide == 1)
                    return;
            }
            else if (Connection.CurrentGroup.navigation == GroupNavigation.BothDirections)
            {
                if (curentViewerSlide == 1)
                    return;
            }
            following = false;
            LoadSlide(curentViewerSlide - 1, true);

        }

        private void Follow()
        {
            following = true;
            LoadSlide(Connection.CurrentPresentation.CurrentSlide, true);

            buttonFollow.ForeColor = Color.FromArgb(0, 162, 232);
            timerBlinkButton.Enabled = false;
        }

        private void CloseViewer()
        {
            FormAlert formAlert = new FormAlert("Confirm exit", "Exit viewer?");
            if (formAlert.ShowDialog() == DialogResult.OK)
            {
                this.Close();
            }
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            PreviousSlide();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            NextSlide();
        }

        private void FormViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                PreviousSlide();
            else if (e.KeyCode == Keys.Right)
                NextSlide();
            else if (e.KeyCode == Keys.Space)
                Follow();
            else if (e.KeyCode == Keys.Escape)
                CloseViewer();
        }

        private void buttonFollow_Click(object sender, EventArgs e)
        {
            Follow();
        }

        private void FormViewer_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                e.IsInputKey = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SetCurrentSlideImage();
        }

        private bool BlinkeGreen = true;
        private void timerBlinkButton_Tick(object sender, EventArgs e)
        {
            if (BlinkeGreen)
                buttonFollow.ForeColor = Color.FromArgb(0, 192, 0);
            else
                buttonFollow.ForeColor = Color.FromArgb(0, 162, 232);
            BlinkeGreen = !BlinkeGreen;
        }
    }
}