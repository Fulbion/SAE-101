using System.Numerics;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CompilationMiniJeux.MiniJeuPacMan
{
    public class Fantome
    {
        public Rectangle Sprite { get; set; }
        public Vector2 Position { get; set; }
        public Direction Direction { get; set; }

        private Random random = new Random();
        private Direction[] directions = new Direction[] { Direction.Haut, Direction.Bas, Direction.Gauche, Direction.Droite };

        public Fantome(Vector2 position, Vector2 taille, Brush color)
        {
            this.Position = position;
            this.Direction = directions[random.Next(directions.Length)];
            this.Sprite = new Rectangle();
            this.Sprite.Width = taille.X;
            this.Sprite.Height = taille.Y;
            this.Sprite.Fill = color;
        }

        public void DirectionAleatoire()
        {
            Direction[] opposee = new Direction[] { Direction.Bas, Direction.Haut, Direction.Droite, Direction.Gauche };

            int indexAleatoire = random.Next(directions.Length);
            Direction dirChoisie = directions[indexAleatoire];
            while (dirChoisie == opposee[indexAleatoire])
            {
                indexAleatoire = random.Next(directions.Length);
                dirChoisie = directions[indexAleatoire];
            }

            this.Direction = dirChoisie;
        }
    }
}
