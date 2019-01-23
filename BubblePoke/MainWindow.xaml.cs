using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BubblePoke
{
    public enum UserAction {NotDefined, TryAgain, PlayNext, Quit }

    public enum BAction {Add, Remove, RemoveAll, MarkBall, UnMarkBall }

    public partial class MainWindow : Window
    {
        public static int Dim { get; set; } = 10;
        public static int CCount { get; set; } = 8;

        private RoundButton[,] BallButtons = null;

        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                LevelTB.Text = $"Level: {_level}";
            }
        }

        private BGame Game = new BGame
        {
            DIM = Dim,
            MaxColorsCount = CCount,
        };
        private Grid grid = null;

        private readonly object LockObj = new object();

        public MainWindow()
        {
            InitializeComponent();
            CreateBoard();

            Game.BoardChanged += (o, e) => BallAction(e.BallAction, e.Ball);
            Game.ScoreChanged += (o, e) => ScoreTB.Text = o.ToString();
            Game.MaxScoreChanged += (o, e) => HighScoreTB.Text = "Highscore: " + o.ToString();
            Game.ToWinChanged += (o, e) => ToWinTB.Text = $"To win: {(int)((int)o * (Game.TutorMode ? 1 : 0.8))}";
            Game.UndoBTStateChanged += (o, e) => UndoBT.IsEnabled = (bool)o;
            Game.GameEnded += (o, e) => GameEnded();

            Level = 1;
            Game.Start();            
        }

        private void BallAction(BAction ba, BState b = null)
        {
            switch (ba)
            {
                case BAction.Add:
                    var bt = BallButtons[b.X, b.Y];
                    bt.Value = b.Value;
                    bt.Background = SR.BColors[b.Value][0];
                    bt.MouseOverBackground = SR.BColors[b.Value][1];
                    bt.PressedBackground = SR.BColors[b.Value][2];
                    bt.Visibility = Visibility.Visible;
                    break;
                case BAction.Remove:
                    BallButtons[b.X, b.Y].Visibility = Visibility.Hidden;
                    break;
                case BAction.RemoveAll:
                    foreach (var bl in Game.Balls)
                        BallButtons[bl.X, bl.Y].Visibility = Visibility.Hidden;
                    break;
                case BAction.MarkBall:
                    bt = BallButtons[b.X, b.Y];
                    bt.Content = "T";
                    break;
                case BAction.UnMarkBall:
                    bt = BallButtons[b.X, b.Y];
                    bt.Content = "";
                    break;
                default:
                    break;
            }
        }

        private void Ball_Click(object sender, RoutedEventArgs e)
        {
            var ball = sender as RoundButton;
            Game.BSelected(new BState(ball.X, ball.Y, ball.Value));
            //MessageBox.Show(ball.X + " " + ball.Y);
        }

        private void GameEnded()
        {
            int Star = 0;
            int score = Game.Score;
            int towin = Game.ToWin;

            if (score >= towin * 0.95)
                Star = 3;
            else if (score >= towin * 0.90)
                Star = 2;
            else if (score >= towin * 0.85)
                Star = 1;

            var stat = new List<int>()
            {
                score,
                (int)(towin * 0.8),
                Star
            };

            var res = new ResultDialog(stat)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            res.ShowDialog();

            if (res.DialogResult == true)
            {
                switch (res.Answer)
                {
                    case UserAction.NotDefined:
                        break;
                    case UserAction.TryAgain:
                        Game.Restart();
                        break;
                    case UserAction.PlayNext:
                        Level++;
                        Game.Start();
                        break;
                    case UserAction.Quit:
                        Close();
                        break;
                    default:
                        break;
                }
            }
        }

        private void CreateBoard()
        {
            grid = new Grid
            {
                Margin = new Thickness(6)
            };

            for (int i = 0; i < Dim; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }

            MainGrid.Children.Add(grid);
            Grid.SetRow(grid, 1);

            BallButtons = new RoundButton[Dim, Dim];

            for (int i = 0; i < Dim; i++)
                for (int j = 0; j < Dim; j++)
                {
                    var ball = new RoundButton()
                    {
                        Value = 0,
                        Content = "",
                        CornerRadius = new CornerRadius(30),
                        X = i,
                        Y = j,
                        Visibility =  Visibility.Hidden,
                        FontSize = 20,
                        Foreground = SR.BColors[0][0]
                    };
                    ball.Click += Ball_Click;

                    BallButtons[i, j] = ball;

                    grid.Children.Add(ball);
                    Grid.SetRow(ball, j);
                    Grid.SetColumn(ball, i);
                }
        }

        private void UndoBT_Click(object sender, RoutedEventArgs e)
        {
            Game.TryUndo();
        }
    }

    public class BGame
    {
        public int DIM { get; set; }
        private int ColorsCount;
        public int MaxColorsCount { get; set; }
        public List<BState> Balls { get; set; }
        public Stack<BoardState> Steps = new Stack<BoardState>();
        private BoardState BallsInitial { get; set; }

        private Random RND = new Random();

        public bool BindToBoard { get; set; } = true;

        public event EventHandler<BoardChangedEventArgs> BoardChanged = null;
        public event EventHandler ScoreChanged = null;
        public event EventHandler MaxScoreChanged = null;
        public event EventHandler ToWinChanged = null;
        public event EventHandler UndoBTStateChanged = null;
        public event EventHandler GameEnded = null;

        private int _score;
        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                if (BindToBoard)
                    ScoreChanged?.Invoke(_score, new EventArgs());
                if (_score > HighScore)
                    HighScore = _score;
            }
        }

        private int _highScore;
        public int HighScore
        {
            get { return _highScore; }
            set
            {
                _highScore = value;
                if (BindToBoard)
                    MaxScoreChanged?.Invoke(_highScore, new EventArgs());
            }
        }

        private int _toWin;
        public int ToWin
        {
            get { return _toWin; }
            set
            {
                _toWin = value;
                if (BindToBoard)
                    ToWinChanged?.Invoke(_toWin, new EventArgs());
            }
        }

        private readonly object LockObj = new object();

        #region Simulation
        public bool TutorMode { get; set; } = false;
        public int NTry { get; set; } = 500;
        private bool GameEnd = false;
        private Queue<BState> BestSteps = new Queue<BState>();
        private List<BState> _bsteps = new List<BState>();
        private BState LastHit = null;
        #endregion
        public bool CustomBalls { get; set; } = false;

        public void Start()
        {
            Steps.Clear();
            Balls = new List<BState>();
            ColorsCount = RND.Next(3, MaxColorsCount + 1);
            SetBalls();
            Score = 0;
            if (BindToBoard)
                UndoBTStateChanged?.Invoke(false, new EventArgs());

            Simulate();
            Restart();

            MarkBestStep();

            GC.Collect();
        }

        private void MarkBestStep()
        {
            if (BindToBoard && TutorMode)
            {
                if (LastHit != null)
                    BoardChanged?.Invoke(LastHit, new BoardChangedEventArgs { Ball = LastHit, BallAction = BAction.UnMarkBall });
                if (BestSteps.Count > 0)
                {
                    LastHit = BestSteps.Dequeue();
                    BoardChanged?.Invoke(LastHit, new BoardChangedEventArgs { Ball = LastHit, BallAction = BAction.MarkBall });
                }
            }
        }

        internal void Restart()
        {
            Balls.Clear();
            Steps.Clear();
            SetBoard(BallsInitial);
            if (BindToBoard)
                UndoBTStateChanged?.Invoke(false, new EventArgs());
        }

        private void SetBalls()
        {
            //Custom Set
            #region 2___
            var CB = new string[]
            {
                "2111220222",
                "0102020122",
                "1110102101",
                "1122001100",
                "1122120100",
                "1002222000",
                "0111121101",
                "2222111111",
                "1201200001",
                "2010022002",
            };
            #endregion

            #region 2446
            CB = new string[]
            {
                "2221212112",
                "1122201122",
                "1220210000",
                "2011000210",
                "1111120221",
                "2101022001",
                "2002121021",
                "1002122210",
                "2200012212",
                "2222001102",
            };
            #endregion
            //

            var bs = new BoardState();

            for (int i = 0; i < DIM; i++)
                for (int j = 0; j < DIM; j++)
                    bs.Balls.Add(new BState(i, j, CustomBalls ? CB[j][i] - 48 :
                        RND.Next(ColorsCount)));

            Score = 0;
            SetBoard(bs);
            BallsInitial = GetBoardState();
        }

        private BoardState GetBoardState()
        {
            return new BoardState
            {
                Balls = Balls.Select(b => new BState(b.X, b.Y, b.Value)).ToList(),
                Score = Score,
            };
        }

        public void SetBoard(BoardState bs)
        {
            foreach (var b in bs.Balls)
                BallAction(BAction.Add, b);
            Score = bs.Score;
        }

        private void BallAction(BAction ba, BState b = null)
        {
            if (BindToBoard)
                BoardChanged?.Invoke(b, new BoardChangedEventArgs { Ball = b, BallAction = ba });

            switch (ba)
            {
                case BAction.Add:
                    Balls.Add(b);
                    break;
                case BAction.Remove:
                    Balls.Remove(b);
                    break;
                case BAction.RemoveAll:
                    Balls.Clear();
                    break;
                default:
                    break;
            }
        }

        public void BSelected(BState ball)
        {
            var CurrentCluster = GetCluster(ball);

            int ClusterSize = CurrentCluster.Count;

            if (ClusterSize > 1)
            {
                if (TutorMode)
                    _bsteps.Add(ball);

                if (BindToBoard)
                {
                    Steps.Push(GetBoardState());
                    UndoBTStateChanged?.Invoke(Steps.Count > 0, new EventArgs());
                }

                Score += ClusterSize * (ClusterSize - 1);

                // Delete Cluster
                foreach (var b in CurrentCluster)
                    BallAction(BAction.Remove, b);

                #region Vertical Fall
                var vfb = Balls.Where(b => b.Y < DIM - 1 && !Balls.Any(s => s.X == b.X && s.Y == b.Y + 1));
                var vbls = vfb.ToList();
                while (vbls.Count > 0)
                {
                    foreach (var b in vbls)
                    {
                        BallAction(BAction.Remove, b);
                        BallAction(BAction.Add, new BState(b.X, b.Y + 1, b.Value));
                    }
                    vbls = vfb.ToList();
                }
                #endregion

                #region Horizontal Shift
                var hb = Balls.Where(b => b.Y == DIM - 1 && b.X < DIM - 1 && !Balls.Any(s => s.X == b.X + 1 && s.Y == b.Y));
                var hbls = hb.ToList();
                while (hbls.Count > 0)
                {
                    var hshb = Balls.Where(b => b.X <= hbls[0].X).OrderByDescending(b => b.X);
                    var hshbls = hshb.ToList();
                    foreach (var b in hshbls)
                    {
                        BallAction(BAction.Remove, b);
                        BallAction(BAction.Add, new BState(b.X + 1, b.Y, b.Value));
                    }
                    hbls = hb.ToList();
                }
                #endregion

                if (BindToBoard && TutorMode)
                    MarkBestStep();

                CheckForEndOfGame();
            }
        }

        private void CheckForEndOfGame()
        {
            foreach (var b in Balls)
                if (GetCluster(b).Count > 1)
                    return;

            GameEnd = true;

            if (BindToBoard)
                GameEnded?.Invoke(true, new EventArgs());
        }

        private List<BState> GetCluster(BState b)
        {
            List<BState> Cluster = new List<BState>();
            GetCluster(b, Cluster);
            return Cluster;
        }

        private void GetCluster(BState b, List<BState> cluster)
        {
            if (cluster.Contains(b))
                return;
            else
                cluster.Add(b);

            BState n = Balls.FirstOrDefault(e => e.X == b.X - 1 && e.Y == b.Y);
            if (n != null && n.Value == b.Value)
                GetCluster(n, cluster);

            n = Balls.FirstOrDefault(e => e.X == b.X + 1 && e.Y == b.Y);
            if (n != null && n.Value == b.Value)
                GetCluster(n, cluster);

            n = Balls.FirstOrDefault(e => e.X == b.X && e.Y == b.Y - 1);
            if (n != null && n.Value == b.Value)
                GetCluster(n, cluster);

            n = Balls.FirstOrDefault(e => e.X == b.X && e.Y == b.Y + 1);
            if (n != null && n.Value == b.Value)
                GetCluster(n, cluster);
        }

        public void TryUndo()
        {
            lock (LockObj)
            {
                if (Steps.Count > 0)
                {
                    BallAction(BAction.RemoveAll);
                    SetBoard(Steps.Pop());
                    if (BindToBoard)
                        UndoBTStateChanged?.Invoke(Steps.Count > 0, new EventArgs());
                    GC.Collect();
                }
            }
        }

        private void Simulate()
        {
            int MaxToWin = 0;
            BindToBoard = false;

            for (int n = 0; n < NTry; n++)
            {
                GameEnd = false;
                Restart();
                _bsteps.Clear();
                while (!GameEnd)
                {
                    var b = Balls[RND.Next(Balls.Count)];
                    BSelected(b);
                }

                if (Score > MaxToWin)
                {
                    MaxToWin = Score;
                    BestSteps = new Queue<BState>(_bsteps);
                }
            }

            BindToBoard = true;
            ToWin = MaxToWin;
            HighScore = Score = 0;
        }
    }

    public class BoardChangedEventArgs : EventArgs
    {
        public BState Ball { get; set; }
        public BAction BallAction { get; set; }
    }

    public class BState : IEquatable<BState>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Value { get; set; }

        public BState(int _x, int _y, int _val)
        {
            X = _x;
            Y = _y;
            Value = _val;
        }

        public BState() { }

        public bool Equals(BState other)
        {
            return X == other.X && Y == other.Y && Value == other.Value;
        }
    }

    public class BoardState
    {
        public List<BState> Balls { get; set; }
        public int Score { get; set; }

        public BoardState()
        {
            Balls = new List<BState>();
            Score = 0;
        }
    }
}
