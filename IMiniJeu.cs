using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompilationMiniJeux
{
    public interface IMiniJeu
    {
        public void Init();
        public void Jouer();
        public void OnKeyDown(object sender, KeyEventArgs e);
        public void OnKeyUp(object sender, KeyEventArgs e);
    }
}
