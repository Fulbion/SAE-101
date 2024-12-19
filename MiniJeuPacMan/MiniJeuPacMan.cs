using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace CompilationMiniJeux.MiniJeuPacMan
{
    public class MiniJeuPacMan : IMiniJeu
    {
        public static readonly int LARGEUR_TILE = 20;
        public static readonly int HAUTEUR_TILE = 20;
        public static readonly int TAILLE_BONBONS = 5;
        public static readonly int TAILLE_SUPER_BONBONS = 10;
        public static readonly int DUREEMINIJEU = 10;
        public static readonly int DECALAGEYALABEL = 25;

        public Random random = new Random();
        public Pac pacquemane;
        public List<Fantome> fantomes;
        public Direction interdit = Direction.Arret;
        public static readonly int PAS = 2;

        public static readonly string[] carte =
        {
            "###############################################################",
            "#........f....................###........f....................#",
            "#.#######.###########.#######.###.#######.###########.#######.#",
            "#.#######.###########.#######.###.#######.###########.#######.#",
            "#.............................###.............................#",
            "###############.#############.###.##############.##############",
            "###############.#############.###.##############.##############",
            "###############.#############.###.##############.##############",
            "#.............................###.............................#",
            "#.#############.#############.###.##############.############.#",
            "#.#############.#############.....##############.############.#",
            "#...........f##.#############.###............f##.############.#",
            "#.##########.##.#############.###.###########.##.############.#",
            "#.##########.##.#############.###.###########.##.############.#",
            "#.##########.##.#############.###.###########.##.############.#",
            "#.............................................................#",
            "#.###########################################################.#",
            "#.###########################################################.#",
            "#.##.......................................................##.#",
            "#.##.##.#######.######.########.########.######.#######.##.##.#",
            "#.##.##.#######.######.########.########.######.#######.##.##.#",
            "#.##.##................        P        ................##.##.#",
            "#.##.#################.#####.##.##.#####.#################.##.#",
            "#.##.#################.#####.##.##.#####.#################.##.#",
            "#.##f......            ...................................f##.#",
            "#.##.##.####### ###### ##.#####.#####.##.######.##########.##.#",
            "#.##.##....    f###### ##.#####.#####.##.######.##########.##.#",
            "#.##.##########.######...................######...f.....##.##.#",
            "#.##.##########.######.#################.######.##.####.##.##.#",
            "#.##.##########.######.#################.######.##......##.##.#",
            "#.##.##................#################........#######.##.##.#",
            "#.##.##.##############.#################.######.#######.##.##.#",
            "#.......##############...................######...............#",
            "###############################################################",
        };

        public Rectangle[,] carteMurs;
        public Rectangle[,] carteBonbons;
        public Rectangle[,] carteSuperBonbons;
        public int nbBonbons, nbBonbonsAuDepart;

        private MainWindow mainWindow;
        private Label afficherChronometre;

        public MiniJeuPacMan(MainWindow fenetreParent)
        {
            mainWindow = fenetreParent;
        }

        public void Init()
        {
            mainWindow.timer.Stop();
            mainWindow.timer.Stop();

            mainWindow.CanvasPrincipal.Children.Clear();

            mainWindow.tempsRestant = DUREEMINIJEU;

            Rectangle rectangleNoir = new Rectangle
            {
                Fill = Brushes.Black, 
                Width = mainWindow.CanvasPrincipal.ActualWidth,   
                Height = mainWindow.CanvasPrincipal.ActualHeight  
            };
            mainWindow.CanvasPrincipal.Children.Add(rectangleNoir);

            InitBonbons();
            InitCarte();

            afficherChronometre = new Label
            {
                Content = mainWindow.tempsRestant,
                Foreground = Brushes.White,
                FontSize = 30,
            };
            Canvas.SetLeft(afficherChronometre, mainWindow.Width/2- DECALAGEYALABEL);
            Canvas.SetTop(afficherChronometre, 0);
            mainWindow.CanvasPrincipal.Children.Add(afficherChronometre);

            mainWindow.timer.Start();
            mainWindow.timer.Start();
        }

        private void InitCarte()
        {
            fantomes = new List<Fantome>();
            carteMurs = new Rectangle[carte.Length, carte[0].Length];
            for (int i = 0; i < carte.Length; i++)
            {
                for (int j = 0; j < carte[i].Length; j++)
                {
                    if (carte[i][j] == '#')
                    {
                        carteMurs[i, j] = new Rectangle();
                        carteMurs[i, j].Fill = Brushes.Blue;
                        carteMurs[i, j].Width = LARGEUR_TILE;
                        carteMurs[i, j].Height = HAUTEUR_TILE;
                        mainWindow.CanvasPrincipal.Children.Add(carteMurs[i, j]);
                        Canvas.SetTop(carteMurs[i, j], i * HAUTEUR_TILE);
                        Canvas.SetLeft(carteMurs[i, j], j * LARGEUR_TILE);
                    }

                    else if (carte[i][j] == 'P')
                    {
                        pacquemane = new Pac(new Vector2((float)(j * HAUTEUR_TILE), (float)(i * LARGEUR_TILE)), new Vector2(LARGEUR_TILE - 5, HAUTEUR_TILE - 5));
                        mainWindow.CanvasPrincipal.Children.Add(pacquemane.Sprite);
                    }

                    else if (carte[i][j] == 'f')
                    {
                        byte[] couleur = new byte[] { (byte)this.random.Next(), (byte)this.random.Next(), (byte)this.random.Next() };
                        fantomes.Add(new Fantome(new Vector2((float)(j * HAUTEUR_TILE), (float)(i * LARGEUR_TILE)), new Vector2(LARGEUR_TILE - 5, HAUTEUR_TILE - 5), new SolidColorBrush(Color.FromRgb(couleur[0], couleur[1], couleur[2]))));
                        mainWindow.CanvasPrincipal.Children.Add(fantomes.Last().Sprite);
                    }
                }
            }
        }
        private void InitBonbons()
        {
            nbBonbons = 0;
            carteBonbons = new Rectangle[carte.Length, carte[0].Length];
            carteSuperBonbons = new Rectangle[carte.Length, carte[0].Length];
            for (int i = 0; i < carte.Length; i++)
            {
                for (int j = 0; j < carte[i].Length; j++)
                {
                    if (carte[i][j] == '.')
                    {
                        nbBonbons++;
                        carteBonbons[i, j] = new Rectangle();
                        carteBonbons[i, j].Fill = Brushes.White;
                        carteBonbons[i, j].Width = TAILLE_BONBONS;
                        carteBonbons[i, j].Height = TAILLE_BONBONS;
                        mainWindow.CanvasPrincipal.Children.Add(carteBonbons[i, j]);
                        Canvas.SetTop(carteBonbons[i, j], i * HAUTEUR_TILE + 1.5 * TAILLE_BONBONS);
                        Canvas.SetLeft(carteBonbons[i, j], j * LARGEUR_TILE + 1.5 * TAILLE_BONBONS);
                    }

                    else if (carte[i][j] == 'o')
                    {
                        carteSuperBonbons[i, j] = new Rectangle();
                        carteSuperBonbons[i, j].Fill = Brushes.White;
                        carteSuperBonbons[i, j].Width = TAILLE_SUPER_BONBONS;
                        carteSuperBonbons[i, j].Height = TAILLE_SUPER_BONBONS;
                        mainWindow.CanvasPrincipal.Children.Add(carteSuperBonbons[i, j]);
                        Canvas.SetTop(carteSuperBonbons[i, j], i * HAUTEUR_TILE + TAILLE_BONBONS);
                        Canvas.SetLeft(carteSuperBonbons[i, j], j * LARGEUR_TILE + TAILLE_BONBONS);
                    }
                }
            }
            nbBonbonsAuDepart = nbBonbons;
        }

        private bool Intersection(Rect r1, Rectangle r2)
        {
            int x1 = (int)r1.X;
            int y1 = (int)r1.Y;
            int w1 = (int)r1.Width;
            int h1 = (int)r1.Height;

            int x2 = (int)Canvas.GetLeft(r2);
            int y2 = (int)Canvas.GetTop(r2);
            int w2 = (int)r2.Width;
            int h2 = (int)r2.Height;

            if (x1 + w1 <= x2 || x2 + w2 <= x1 || y1 + h1 <= y2 || y2 + h2 <= y1)
            {
                return false;
            }

            return true;
        }

        public void Jouer()
        {
            #region Déplacements + Collision

            Vector2[] nouvellePosition =
            {
                new Vector2(pacquemane.Position.X, pacquemane.Position.Y),
                new Vector2(pacquemane.Position.X - PAS, pacquemane.Position.Y),
                new Vector2(pacquemane.Position.X, pacquemane.Position.Y + PAS),
                new Vector2(pacquemane.Position.X, pacquemane.Position.Y - PAS),
                new Vector2(pacquemane.Position.X + PAS, pacquemane.Position.Y)
            };

            afficherChronometre.Content = mainWindow.tempsRestant;

            bool collision = false;
            for (int i = 0; i < carteMurs.GetLength(0); i++)
            {
                for (int j = 0; j < carteMurs.GetLength(1); j++)
                {
                    if (carte[i][j] == '#')
                    {
                        Rect hitbox = new Rect(nouvellePosition[(int)pacquemane.Direction].X, nouvellePosition[(int)pacquemane.Direction].Y, pacquemane.Sprite.Width, pacquemane.Sprite.Height);
                        if (collision || Intersection(hitbox, carteMurs[i, j]))
                        {
                            collision = true;
                        }
                    }

                    if (carte[i][j] == '.')
                    {
                        Rect hitbox = new Rect(nouvellePosition[(int)pacquemane.Direction].X, nouvellePosition[(int)pacquemane.Direction].Y, pacquemane.Sprite.Width, pacquemane.Sprite.Height);
                        if (Intersection(hitbox, carteBonbons[i, j]))
                        {
                            Canvas.SetLeft(carteBonbons[i, j], 0);
                            Canvas.SetTop(carteBonbons[i, j], 0);
                            carteBonbons[i, j].Width = 0;
                            carteBonbons[i, j].Height = 0;
                            nbBonbons--;
                        }
                    }
                }
            }

            if (collision)
            {
                pacquemane.Direction = Direction.Arret;
            }
            else
            {
                pacquemane.Position = nouvellePosition[(int)pacquemane.Direction];

                if (pacquemane.Position.X < (int)-pacquemane.Sprite.Width)
                    pacquemane.Position = new Vector2((int)mainWindow.Width, pacquemane.Position.Y);

                else if (pacquemane.Position.X > (int)mainWindow.Width)
                    pacquemane.Position = new Vector2((int)-pacquemane.Sprite.Width, pacquemane.Position.Y);
            }

            Canvas.SetLeft(pacquemane.Sprite, pacquemane.Position.X);
            Canvas.SetTop(pacquemane.Sprite, pacquemane.Position.Y);
            #endregion

            #region Gestion fantômes

            foreach (Fantome fantome in fantomes)
            {
                nouvellePosition = new Vector2[]
                {
                    new Vector2(fantome.Position.X, fantome.Position.Y),
                    new Vector2(fantome.Position.X - PAS, fantome.Position.Y),
                    new Vector2(fantome.Position.X, fantome.Position.Y + PAS),
                    new Vector2(fantome.Position.X, fantome.Position.Y - PAS),
                    new Vector2(fantome.Position.X + PAS, fantome.Position.Y)
                };

                bool peutBouger = true;

                for (int i = 0; i < carteMurs.GetLength(0); i++)
                {
                    for (int j = 0; j < carteMurs.GetLength(1); j++)
                    {
                        if (carte[i][j] == '#')
                        {
                            Rect hitbox = new Rect(nouvellePosition[(int)fantome.Direction].X, nouvellePosition[(int)fantome.Direction].Y, fantome.Sprite.Width, fantome.Sprite.Height);
                            if (!peutBouger || Intersection(hitbox, carteMurs[i, j]))
                            {
                                peutBouger = false;
                                fantome.DirectionAleatoire();
                                break;
                            }
                        }
                    }
                }

                if (peutBouger)
                {
                    fantome.Position = nouvellePosition[(int)fantome.Direction];
                    Canvas.SetLeft(fantome.Sprite, fantome.Position.X);
                    Canvas.SetTop(fantome.Sprite, fantome.Position.Y);
                }
            }
            #endregion

            VerifieGagne();
            VerifieGameOver();

        }

        private void VerifieGameOver()
        {
            foreach (Fantome fantome in fantomes)
            {
                Rect hitbox = new Rect(pacquemane.Position.X, pacquemane.Position.Y, pacquemane.Sprite.Width, pacquemane.Sprite.Height);
                if (Intersection(hitbox, fantome.Sprite))
                {
                    mainWindow.ecranIntermediaire.InitEcranIntermediaire(0, false);
                }

            }
        }

        private void VerifieGagne()
        {
            if (nbBonbons == 0 || mainWindow.tempsRestant <= 0)
            {
                for (int i = 0; i < carteMurs.GetLength(0); i++)
                    for (int j = 0; j < carteMurs.GetLength(1); j++)
                        if (carte[i][j] == '#')
                            carteMurs[i, j].Fill = Brushes.White;
                mainWindow.ecranIntermediaire.InitEcranIntermediaire(nbBonbonsAuDepart - nbBonbons, true);
            }
        }
        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (interdit != Direction.Gauche)
                        pacquemane.Direction = Direction.Gauche;
                    break;

                case Key.Down:
                    if (interdit != Direction.Bas)
                        pacquemane.Direction = Direction.Bas;
                    break;

                case Key.Up:
                    if (interdit != Direction.Haut)
                        pacquemane.Direction = Direction.Haut;
                    break;

                case Key.Right:
                    if (interdit != Direction.Droite)
                        pacquemane.Direction = Direction.Droite;
                    break;
            }
        }

        public void OnKeyUp(object sender, KeyEventArgs e) { }
    }
}
