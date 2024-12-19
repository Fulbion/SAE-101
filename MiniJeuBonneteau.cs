using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace CompilationMiniJeux
{
    internal class MiniJeuBonneteau : IMiniJeu
    {
        private Random random = new Random();
        private Image[] rectangles;
        private Image rJoueur;
        private Image bonbons;

        private DispatcherTimer echangeChronometre;
        private DispatcherTimer sequenceChronometre;
        private DispatcherTimer timerAnimation;
        private int indexFrame = 0;

        private List<Tuple<Image, Image>> sequenceDeCoups;
        private int indiceTemps;
        private double r1InitialX, r1InitialY, r2InitialX, r2InitialY;
        private double r1DeltaX, r1DeltaY, r2DeltaX, r2DeltaY;
        public int etat;

        private static readonly double PAS_ECHANGE = 0.2;
        private static readonly double PAS_JOUEUR = 3;
        private static readonly int NBGOBELETSINITIALISES = 4;
        private static readonly int NBBONBONSGAGNES = 8;
        public static readonly int AUDESSUS = 10;
        private static int[,] POSGOBELETS = {
                                                { 200, 100 },
                                                { 540, 100 },
                                                { 200, 270 },
                                                { 540, 270 }
                                            };

        private bool gauche = false, bas = false, haut = false, droite = false;

        private MainWindow mainWindow;

        public MiniJeuBonneteau(MainWindow parentWindow)
        {
            mainWindow = parentWindow;

            echangeChronometre = new DispatcherTimer();
            echangeChronometre.Interval = TimeSpan.FromMilliseconds(16.67);
            echangeChronometre.Tick += EchangeChronometre_Tick;

            sequenceChronometre = new DispatcherTimer();
            sequenceChronometre.Interval = TimeSpan.FromMilliseconds(500);
            sequenceChronometre.Tick += SequenceChronometre_Tick;
        }


        private void AnimationJoueur(object? sender, EventArgs e)
        {
            indexFrame = (indexFrame + 1) % 4;

            if (bas)
                rJoueur.Source = new BitmapImage(new Uri("pack://application:,,,/images/joueur/bas/tile00" + indexFrame + ".png"));

            else if (haut)
                rJoueur.Source = new BitmapImage(new Uri("pack://application:,,,/images/joueur/haut/tile00" + indexFrame + ".png"));

            else if (droite)
                rJoueur.Source = new BitmapImage(new Uri("pack://application:,,,/images/joueur/droite/tile00" + indexFrame + ".png"));

            else if (gauche)
                rJoueur.Source = new BitmapImage(new Uri("pack://application:,,,/images/joueur/gauche/tile00" + indexFrame + ".png"));
        }

        public void Init()
        {
            mainWindow.timer.Stop();
            mainWindow.chronometre.Stop();
            echangeChronometre.Stop();
            sequenceChronometre.Stop();

            mainWindow.CanvasPrincipal.Children.Clear();

            indiceTemps = -2;
            etat = 0;

            rJoueur = new Image
            {
                Width = 40,
                Height = 40,
            };
            Canvas.SetLeft(rJoueur, 380);
            Canvas.SetTop(rJoueur, 197);
            Panel.SetZIndex(rJoueur, AUDESSUS);
            mainWindow.CanvasPrincipal.Children.Add(rJoueur);

            GenererSequenceDeCoups();

            mainWindow.timer.Start();
            mainWindow.chronometre.Start();
            sequenceChronometre.Start();
        }

        private void GenererSequenceDeCoups()
        {
            rectangles = new Image[NBGOBELETSINITIALISES];

            for (int i = 0; i < rectangles.Length; i++)
            {
                rectangles[i] = new Image
                {
                    Width = 60,
                    Height = 80
                };
                Canvas.SetLeft(rectangles[i], POSGOBELETS[i, 0]);
                Canvas.SetTop(rectangles[i], POSGOBELETS[i, 1]);
                rectangles[i].Source = new BitmapImage(new Uri("pack://application:,,,/images/gobelet.png"));
                mainWindow.CanvasPrincipal.Children.Add(rectangles[i]);
            }

            sequenceDeCoups = new List<Tuple<Image, Image>>();

            for (int i = 0; i < 15; i++)
            {
                int _1, _2;
                _1 = random.Next(rectangles.Length);

                do
                {
                    _2 = random.Next(rectangles.Length);
                } while (_1 == _2);

                Image temp = rectangles[_1];
                rectangles[_1] = rectangles[_2];
                rectangles[_2] = temp;

                sequenceDeCoups.Add(new Tuple<Image, Image>(rectangles[_1], rectangles[_2]));
            }
        }
        private void SequenceChronometre_Tick(object? sender, EventArgs e)
        {
            indiceTemps++;

            if (indiceTemps < 0)
            {
                bonbons = new Image
                {
                    Width = 50,
                    Height = 50
                };
                bonbons.Source = new BitmapImage(new Uri("pack://application:,,,/images/sac-bonbons.png"));
                bonbons.Stretch = Stretch.Fill;
                Canvas.SetLeft(bonbons, Canvas.GetLeft(rectangles[0]));
                Canvas.SetTop(bonbons, Canvas.GetTop(rectangles[0]) + rectangles[0].Height - bonbons.Height);
                mainWindow.CanvasPrincipal.Children.Add(bonbons);

                Canvas.SetTop(rectangles[0], Canvas.GetTop(rectangles[0]) - 50);
            }

            else if (indiceTemps == 0)
            {
                Canvas.SetTop(rectangles[0], Canvas.GetTop(rectangles[0]) + 50);
                mainWindow.CanvasPrincipal.Children.Remove(bonbons);
            }

            else if (indiceTemps < sequenceDeCoups.Count)
            {
                Tuple<Image, Image> coup = sequenceDeCoups[indiceTemps];
                Echange(coup.Item1, coup.Item2);
            }
            else
            {
                sequenceChronometre.Stop();

                rJoueur.Source = new BitmapImage(new Uri("pack://application:,,,/images/joueur/bas/tile000.png"));
                etat = 1;

                timerAnimation = new DispatcherTimer();
                timerAnimation.Interval = TimeSpan.FromMilliseconds(100);
                timerAnimation.Tick += AnimationJoueur;
                timerAnimation.Start();
            }

        }

        private void Echange(Image r1, Image r2)
        {
            r1InitialX = Canvas.GetLeft(r1);
            r1InitialY = Canvas.GetTop(r1);
            r2InitialX = Canvas.GetLeft(r2);
            r2InitialY = Canvas.GetTop(r2);

            r1DeltaX = r2InitialX - r1InitialX;
            r1DeltaY = r2InitialY - r1InitialY;
            r2DeltaX = r1InitialX - r2InitialX;
            r2DeltaY = r1InitialY - r2InitialY;

            echangeChronometre.Tag = new List<Image> { r1, r2 };

            echangeChronometre.Start();
        }

        private void EchangeChronometre_Tick(object? sender, EventArgs e)
        {
            List<Image> list = (List<Image>)echangeChronometre.Tag;
            Image r1 = list[0];
            Image r2 = list[1];

            double moveX1 = PAS_ECHANGE * r1DeltaX;
            double moveY1 = PAS_ECHANGE * r1DeltaY;
            double moveX2 = PAS_ECHANGE * r2DeltaX;
            double moveY2 = PAS_ECHANGE * r2DeltaY;

            Canvas.SetLeft(r1, Canvas.GetLeft(r1) + moveX1);
            Canvas.SetTop(r1, Canvas.GetTop(r1) + moveY1);
            Canvas.SetLeft(r2, Canvas.GetLeft(r2) + moveX2);
            Canvas.SetTop(r2, Canvas.GetTop(r2) + moveY2);

            if (Math.Abs(Canvas.GetLeft(r1) - r2InitialX) < 1 && Math.Abs(Canvas.GetTop(r1) - r2InitialY) < 1)
            {
                echangeChronometre.Stop();
                Canvas.SetLeft(r1, r2InitialX);
                Canvas.SetTop(r1, r2InitialY);
                Canvas.SetLeft(r2, r1InitialX);
                Canvas.SetTop(r2, r1InitialY);
            }
        }

        public void Jouer()
        {
            Vector2 position = new Vector2((int)Canvas.GetLeft(rJoueur), (int)Canvas.GetTop(rJoueur));

            if (gauche) position.X -= (float)PAS_JOUEUR;
            if (bas) position.Y += (float)PAS_JOUEUR;
            if (haut) position.Y -= (float)PAS_JOUEUR;
            if (droite) position.X += (float)PAS_JOUEUR;

            if (position.X < 0) position.X = 0;
            if (position.X > mainWindow.CanvasPrincipal.Width - rJoueur.Width) position.X = (int)(mainWindow.CanvasPrincipal.Width - rJoueur.Width);
            if (position.Y < 0) position.Y = 0;
            if (position.Y > mainWindow.CanvasPrincipal.Height - rJoueur.Height) position.Y = (int)(mainWindow.CanvasPrincipal.Width - rJoueur.Height);

            // Rect hitbox = new Rect((float)Canvas.GetLeft(rJoueur), (float)Canvas.GetTop(rJoueur), (float)rJoueur.Width, (float)rJoueur.Height);
            // foreach (Image rectangle in rectangles)
            // {
            //     Rect hitbox2 = new Rect((float)Canvas.GetLeft(rectangle), (float)Canvas.GetTop(rectangle), (float)rectangle.Width, (float)rectangle.Height);
            //     if (hitbox.IntersectsWith(hitbox2))
            //     {
            //         rectangle.Stroke = Brushes.Lime;
            //     }
            // 
            //     else
            //     {
            //         rectangle.Stroke = Brushes.Black;
            //     }
            // }

            Canvas.SetLeft(rJoueur, position.X);
            Canvas.SetTop(rJoueur, position.Y);
        }

        private void Verifier()
        {
            Rect hitbox = new Rect((float)Canvas.GetLeft(rJoueur), (float)Canvas.GetTop(rJoueur), (float)rJoueur.Width, (float)rJoueur.Height);

            foreach (Image rectangle in rectangles)
            {
                Rect hitbox2 = new Rect((float)Canvas.GetLeft(rectangle), (float)Canvas.GetTop(rectangle), (float)rectangle.Width, (float)rectangle.Height);

                if (hitbox.IntersectsWith(hitbox2))
                {
                    if (rectangle == rectangles[0])
                        mainWindow.ecranIntermediaire.InitEcranIntermediaire(NBBONBONSGAGNES, true);
                    else
                        mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);

                    gauche = false;
                    droite = false;
                    haut = false;
                    bas = false;

                    return;
                }
            }
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (etat == 1)
            {
                if (e.Key == Key.Enter)
                {
                    Verifier();
                }

                if (e.Key == Key.Down)
                {
                    bas = true;
                }

                if (e.Key == Key.Right)
                {
                    droite = true;
                }

                if (e.Key == Key.Up)
                {
                    haut = true;
                }

                if (e.Key == Key.Left)
                {
                    gauche = true;
                }
            }
        }
        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (etat == 1)
            {
                if (e.Key == Key.Down)
                {
                    bas = false;
                }

                if (e.Key == Key.Right)
                {
                    droite = false;
                }

                if (e.Key == Key.Up)
                {
                    haut = false;
                }

                if (e.Key == Key.Left)
                {
                    gauche = false;
                }
            }
        }
    }
}

