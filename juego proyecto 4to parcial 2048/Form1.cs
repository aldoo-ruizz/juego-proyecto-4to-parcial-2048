using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace juego_proyecto_4to_parcial_2048
{
    public partial class Form1 : Form
    {
        private const int SIZE = 4;
        private int[,] board = new int[SIZE, SIZE];
        private Random rnd = new Random();
        private int score = 0;
        private int elapsedSeconds = 0;
        private bool gameOver = false;
        private bool gameWon = false;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // opcional: cargar imagen en pictureBoxLogo si se desea
            // pictureBoxLogo.Image = ...;
            StartGame();
        }

        private void StartGame()
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    board[r, c] = 0;

            score = 0;
            elapsedSeconds = 0;
            gameOver = false;
            gameWon = false;
            lblResult.Text = "";
            lblScore.Text = "Puntos: 0";
            lblTime.Text = "Tiempo: 00:00";
            timerGame.Start();
            SpawnTile();
            SpawnTile();
            panelGrid.Invalidate();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void timerGame_Tick(object sender, EventArgs e)
        {
            if (gameOver) return;
            elapsedSeconds++;
            lblTime.Text = "Tiempo: " + TimeSpan.FromSeconds(elapsedSeconds).ToString(@"mm\:ss");
        }

        private void panelGrid_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(panelGrid.BackColor);
            int w = panelGrid.Width;
            int h = panelGrid.Height;
            int margin = 16;
            int tileSpacing = 16;
            int tileSize = (Math.Min(w, h) - 2 * margin - (SIZE - 1) * tileSpacing) / SIZE;
            var rect = new Rectangle(margin, margin, tileSize, tileSize);

            for (int r = 0; r < SIZE; r++)
            {
                for (int c = 0; c < SIZE; c++)
                {
                    int x = margin + c * (tileSize + tileSpacing);
                    int y = margin + r * (tileSize + tileSpacing);
                    var tileRect = new Rectangle(x, y, tileSize, tileSize);

                    int val = board[r, c];
                    Color back = GetTileColor(val);
                    using (var b = new SolidBrush(back))
                        g.FillRectangle(b, tileRect);
                    using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                        g.DrawRectangle(pen, tileRect);

                    if (val != 0)
                    {
                        string s = val.ToString();
                        var fontSize = val < 100 ? 28 : val < 1000 ? 24 : 18;
                        using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
                        using (var brush = new SolidBrush(val <= 4 ? Color.FromArgb(119, 110, 101) : Color.White))
                        {
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(s, font, brush, tileRect, sf);
                        }
                    }
                }
            }
        }

        private Color GetTileColor(int val)
        {
            switch (val)
            {
                case 0: return Color.FromArgb(205, 193, 180);
                case 2: return Color.FromArgb(238, 228, 218);
                case 4: return Color.FromArgb(237, 224, 200);
                case 8: return Color.FromArgb(242, 177, 121);
                case 16: return Color.FromArgb(245, 149, 99);
                case 32: return Color.FromArgb(246, 124, 95);
                case 64: return Color.FromArgb(246, 94, 59);
                case 128: return Color.FromArgb(237, 207, 114);
                case 256: return Color.FromArgb(237, 204, 97);
                case 512: return Color.FromArgb(237, 200, 80);
                case 1024: return Color.FromArgb(237, 197, 63);
                case 2048: return Color.FromArgb(237, 194, 46);
                default: return Color.FromArgb(60, 58, 50);
            }
        }

        private void SpawnTile()
        {
            var empties = new List<Tuple<int, int>>();
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    if (board[r, c] == 0)
                        empties.Add(Tuple.Create(r, c));

            if (empties.Count == 0) return;

            var choice = empties[rnd.Next(empties.Count)];
            board[choice.Item1, choice.Item2] = rnd.NextDouble() < 0.9 ? 2 : 4;
        }

        // Centraliza el manejo de teclas para flechas (y otras si se añaden).
        private bool HandleMove(Keys key)
        {
            if (gameOver || gameWon) return false;

            bool moved = false;
            Keys k = key & Keys.KeyCode;

            if (k == Keys.Left)
                moved = MoveLeft();
            else if (k == Keys.Right)
                moved = MoveRight();
            else if (k == Keys.Up)
                moved = MoveUp();
            else if (k == Keys.Down)
                moved = MoveDown();

            if (moved)
            {
                SpawnTile();
                lblScore.Text = "Puntos: " + score;
                panelGrid.Invalidate();

                if (CheckWin())
                {
                    gameWon = true;
                    timerGame.Stop();
                    lblResult.Text = "¡Felicidades!\nHas alcanzado 2048";
                    MessageBox.Show("¡Has ganado! Puntaje: " + score, "Victoria", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (!CanMove())
                {
                    gameOver = true;
                    timerGame.Stop();
                    lblResult.Text = "Juego terminado";
                    MessageBox.Show("No hay movimientos posibles. Has perdido.\nPuntaje: " + score, "Derrota", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            return moved;
        }

        // Captura las flechas incluso cuando un control interno tiene el foco.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HandleMove(keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // siguen llamando aquí por compatibilidad, pero ProcessCmdKey cubre la mayoría de casos.
            if (HandleMove(e.KeyCode))
                e.Handled = true;
        }

        private bool CheckWin()
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    if (board[r, c] == 2048)
                        return true;
            return false;
        }

        private bool CanMove()
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                {
                    if (board[r, c] == 0) return true;
                    if (c + 1 < SIZE && board[r, c] == board[r, c + 1]) return true;
                    if (r + 1 < SIZE && board[r, c] == board[r + 1, c]) return true;
                }
            return false;
        }

        private bool MoveLeft()
        {
            bool moved = false;
            for (int r = 0; r < SIZE; r++)
            {
                int[] row = new int[SIZE];
                for (int c = 0; c < SIZE; c++) row[c] = board[r, c];
                int[] compressed = CompressAndMerge(row, out bool changed, out int gained);
                if (changed)
                {
                    moved = true;
                    score += gained;
                    for (int c = 0; c < SIZE; c++) board[r, c] = compressed[c];
                }
            }
            return moved;
        }

        private bool MoveRight()
        {
            bool moved = false;
            for (int r = 0; r < SIZE; r++)
            {
                int[] row = new int[SIZE];
                for (int c = 0; c < SIZE; c++) row[c] = board[r, SIZE - 1 - c];
                int[] compressed = CompressAndMerge(row, out bool changed, out int gained);
                if (changed)
                {
                    moved = true;
                    score += gained;
                    for (int c = 0; c < SIZE; c++) board[r, SIZE - 1 - c] = compressed[c];
                }
            }
            return moved;
        }

        private bool MoveUp()
        {
            bool moved = false;
            for (int c = 0; c < SIZE; c++)
            {
                int[] col = new int[SIZE];
                for (int r = 0; r < SIZE; r++) col[r] = board[r, c];
                int[] compressed = CompressAndMerge(col, out bool changed, out int gained);
                if (changed)
                {
                    moved = true;
                    score += gained;
                    for (int r = 0; r < SIZE; r++) board[r, c] = compressed[r];
                }
            }
            return moved;
        }

        private bool MoveDown()
        {
            bool moved = false;
            for (int c = 0; c < SIZE; c++)
            {
                int[] col = new int[SIZE];
                for (int r = 0; r < SIZE; r++) col[r] = board[SIZE - 1 - r, c];
                int[] compressed = CompressAndMerge(col, out bool changed, out int gained);
                if (changed)
                {
                    moved = true;
                    score += gained;
                    for (int r = 0; r < SIZE; r++) board[SIZE - 1 - r, c] = compressed[r];
                }
            }
            return moved;
        }

        // Comprime la línea hacia la izquierda y fusiona iguales.
        private int[] CompressAndMerge(int[] line, out bool changed, out int gained)
        {
            changed = false;
            gained = 0;
            var list = new List<int>();
            foreach (var v in line) if (v != 0) list.Add(v);

            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i] == list[i + 1])
                {
                    list[i] *= 2;
                    gained += list[i];
                    list.RemoveAt(i + 1);
                    changed = true;
                }
            }

            var result = new int[SIZE];
            for (int i = 0; i < list.Count; i++) result[i] = list[i];

            // detecta si hubo movimiento (por posicion o por cambio en valores)
            for (int i = 0; i < SIZE; i++)
            {
                if (result[i] != line[i])
                    changed = true;
            }

            return result;
        }
    }
}
