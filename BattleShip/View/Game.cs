﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BattleShip.Controller;
using BattleShip.Model;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BattleShip
{
    [Serializable]
    partial class Game : Form
    {
        bool GameStarted;
        public static bool isFinished;
        public bool Turn;
        public static int score;
        public State state;
        PlayerController player;
        ComputerController computer;
        Point startedPosition;
        Point shotPosition;
        public static bool MuteClicked { get; set; }
        
        public Game()
        {
            DoubleBuffered = true;
            Turn = true;
            InitializeComponent();
            player = new PlayerController();
            isFinished = false;
            score = 0;
            computer = new ComputerController();
            dgvPlayer.DoubleBuffered(true);
            dgvComputer.DoubleBuffered(true);
            GameStarted = false;
            MuteClicked = false;
            ShowPlayerView();
            ShowComputerView();
  
        }

        public void UpdateState()
        {
            player = state.Player;
            computer = state.Computer;
            ShowPlayerView();
            ShowComputerView();
            player.UpdateMissed(dgvPlayer);
            computer.UpdateMissed(dgvComputer);
            score = state.Score;
            Turn = state.Turn;
            btnRandomize.Visible = false;
            btnStart.Text = "Resume";
            dgvPlayer.Enabled = false;
        }

        public void ShowPlayerView()
        {
            player.SetGridView(dgvPlayer);
            player.ShowShips(dgvPlayer);
        }

        public void ShowComputerView()
        {
            computer.SetGridView(dgvComputer);
            computer.ShowShips(dgvComputer);
        }

        private void dgvPlayer_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            player.Select(new Point { X = e.RowIndex, Y = e.ColumnIndex });
            if (player.selected != null)
            {
                startedPosition = player.selected.Cells[0].Positon;
            }
        }

        private void dgvPlayer_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            Point position = new Point { X = e.RowIndex, Y = e.ColumnIndex };

            if (player.selected != null)
            {
                player.selected.AddPositions(position);
                if (player.SearchShip())
                {
                    Cursor.Current = Cursors.No;
                    double opacity = 0.6;
                    player.selected.Cells.ForEach(cell => cell.Opacity((float)opacity));
                    ShowPlayerView();
                    return;
                }
                ShowPlayerView();
            }
            if (player.SearchShip(position))
            {
                Cursor.Current = Cursors.SizeAll;
                if (player.selected != null)
                    player.selected.Cells.ForEach(cell => cell.Opacity(1));
                ShowPlayerView();
            }
        }

        private void dgvPlayer_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (player.selected != null)
            {
                if (player.SearchShip())
                {
                    player.selected.AddPositions(startedPosition);
                    ShowPlayerView();
                }
            }
            player.UnSelect();
        }

        private void dgvPlayer_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Point newPosition = new Point { X = e.RowIndex, Y = e.ColumnIndex };
            player.Select(newPosition);
            Point oldPosition = new Point();
            if (player.selected != null)
            {
                oldPosition = player.selected.Cells[0].Positon;
            }

            if (player.selected != null)
            {
                player.selected.ChangePosition(newPosition);
                if (player.SearchShip())
                {
                    player.selected.ChangePosition(oldPosition);
                }
            }
            ShowPlayerView();
            player.UnSelect();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            player.DisableCells(dgvPlayer);
            player.ShowShips(dgvPlayer);
            ComputerTimer.Start();
            System.Media.SoundPlayer song = new System.Media.SoundPlayer(Properties.Resources.start);
            song.Play();
            if (MuteClicked)
                song.Stop();
            else
            {
                song.Play();
            }
            GameStarted = true;
            dgvComputer.Enabled = true;
            btnRandomize.Enabled = false;
            label1.Hide();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            player.EnableCells(dgvPlayer);
            player.ShowShips(dgvPlayer);
            GameStarted = false;
            dgvComputer.Enabled = false;
        }

        private void btnShoot_Click(object sender, EventArgs e)
        {
            newGame();
            //player.Shoot(dgvPlayer);
            //player.ShowShips(dgvPlayer);
           // saveFile(Microsoft.VisualBasic.Interaction.InputBox("Highscore!", "Save your Highscore", "Name", 500, 250), 1200);

        }

        private void ComputerTimer_Tick(object sender, EventArgs e)
        {
            label2.Text = Turn ? "Your turn" : "Bot's turn";
            lblScore.Text = score.ToString();
            if (score < 0)
            {
                lblScore.ForeColor = Color.Red;

            }
            else if (score > 0)
            {

                lblScore.ForeColor = Color.Green;


            }
            if (!Turn)
            {
                dgvComputer.Enabled = false;
                ShootTimer.Start();
            }
            if (Turn)
            {
                dgvComputer.Enabled = true;
                ShootTimer.Stop();
            }

            if (computer.Won())
            {
                isFinished = true;

                ComputerTimer.Interval = 999999999;
                ShootTimer.Interval = 999999999;
                ShootTimer.Stop();
               
                ShootTimer.Enabled = false;
                //ShowPlayerView();
                //ShowComputerView();
                ComputerTimer.Enabled = false;
                dgvComputer.Enabled = false;
                ShootTimer.Enabled = false;

                if (score > 0)
                {
                    saveFile(Microsoft.VisualBasic.Interaction.InputBox("Highscore!", "Save your Highscore", "Name", 500, 250), score);
                }
                
                ComputerTimer.Dispose();
                if (MessageBox.Show("YOU WON! Do you want to play a new game?", "VICTORY",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    newGame();
                }
            }
            if (player.Won())
            {
                isFinished = true;
                ComputerTimer.Enabled = false;
                dgvComputer.Enabled = false;
                ComputerTimer.Interval = 999999999;
                ShootTimer.Interval = 999999999;
                ShootTimer.Stop();
                // ShowPlayerView();
                //ShowComputerView();
                dgvComputer.Enabled = false;
                ShootTimer.Dispose();
                ComputerTimer.Dispose();
                if (MessageBox.Show("YOU WON! Do you want to play a new game?", "VICTORY", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    newGame();
                }
            }
        }

        private void dgvComputer_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (GameStarted)
            {
                shotPosition = new Point { X = e.RowIndex, Y = e.ColumnIndex };
                if (computer.Shoot(shotPosition, dgvComputer))
                {
                    Turn = false;
                    dgvComputer.Enabled = false;
                }
                computer.ShowShips(dgvComputer);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            computer.Shoot(new Point { X = random.Next(0, 12), Y = random.Next(0, 12) }, dgvComputer);
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            ShootTimer.Interval = random.Next(1000, 2000);
            player.Shoot(dgvPlayer);
            player.ShowShips(dgvPlayer);
            Turn = !player.found;
            lblScore.Text = score.ToString();
            if (score < 0)
            {
                score = 0;  

            }
            if (score > 0)
            {
                lblScore.ForeColor = Color.Green;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            player.Random();
            ShowPlayerView();
        }

        private void dgvComputer_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (!startedPosition.IsEmpty && dgvComputer.Rows[startedPosition.X].Cells[startedPosition.Y].Style.BackColor == Color.DimGray)
            {
                dgvComputer.Rows[startedPosition.X].Cells[startedPosition.Y].Style.BackColor = Color.Transparent;
            }
            Point position = new Point { X = e.RowIndex, Y = e.ColumnIndex };

            if (computer.positions.Exists(point => point.Equals(position)))
            {
                dgvComputer.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.DimGray;
                startedPosition = position;
                Cursor.Current = Cursors.Hand;
            }
            else
            {
                startedPosition = Point.Empty;
             
            }
            if(!startedPosition.IsEmpty && (e.ColumnIndex!=0|| e.RowIndex != 0))
            {
                dgvComputer.Rows[0].Cells[0].Style.BackColor = Color.Transparent;
            }
            

        }

        private void dgvPlayer_MouseLeave(object sender, EventArgs e)
        {
            if (player.selected != null)
            {
                if (player.SearchShip())
                {
                    player.selected.AddPositions(startedPosition);
                    ShowPlayerView();
                }
            }
            player.UnSelect();
        }

        private void dgvComputer_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (!startedPosition.IsEmpty && dgvComputer.Rows[startedPosition.X].Cells[startedPosition.Y].Style.BackColor == Color.DimGray)
            {
                //dgvComputer[0, 0].Style.BackColor = Color.Transparent;
                dgvComputer.Rows[startedPosition.X].Cells[startedPosition.Y].Style.BackColor = Color.Transparent;
            }
        }

        private void saveFile(String name, int Score)
        {
            using (System.IO.StreamWriter file =
         new System.IO.StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\highscores.csv", true))
            {
                file.WriteLine(name + ";" + Score.ToString());
            }
            //System.IO.File.WriteAllText(Application.StartupPath + "\\highscores.txt", name + ";" + Score.ToString());
        }

        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            state = new State();
            state.Computer = computer;
            state.Player = player;
            state.Score = score;
            state.Turn = Turn;
            ComputerTimer.Stop();
            ShootTimer.Stop();
            DialogResult = DialogResult.Cancel;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            MuteClicked = true;
            label4.Hide();
            label5.Show();
            label4.BringToFront();
            
        }

        private void Game_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {
            MuteClicked = false;
            label5.Hide();
            label4.Show();

        }
        private void newGame()
        {
            Game form = new Game();
            this.Hide();
            if (form.ShowDialog() == DialogResult.Cancel)
            {
                this.Close();
                form.Close();
            }

        }

        private void label4_MouseHover(object sender, EventArgs e)
        {
            
        }
    }
}
