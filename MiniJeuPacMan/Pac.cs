using System.Numerics;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CompilationMiniJeux.MiniJeuPacMan
{   public class Pac
    {
        public Rectangle Sprite { get; set; }
        public Vector2 Position { get; set; }
        public Direction Direction { get; set; }

        public Pac(Vector2 position, Vector2 taille)
        {
            this.Position = position;
            this.Direction = Direction.Arret;
            this.Sprite = new Rectangle();
            this.Sprite.Width = taille.X;
            this.Sprite.Height = taille.Y;
            this.Sprite.Fill = Brushes.Yellow;
        }

    }
}
