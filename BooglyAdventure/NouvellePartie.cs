using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML;
using SFML.System;
using SFML.Graphics;
using SFML.Window;
using SFML.Audio;

namespace BooglyAdventure
{
    class NouvellePartie : Transformable, Drawable
    {
        public int NiveauActuel;
        public readonly int NOMBREDENIVEAU;
        public Niveau LeNiveau;
        public Heros LeHeros;
        public bool QuitterLaPartie;
        public Vector2u TailleFenetre;
        private RenderWindow window;
        private int[] TuileVide;
        public Music Ambiance;

        public NouvellePartie(RenderWindow win, int NombreDeNiveau, params int[] tuileVide)
        {
            window = win;
            NOMBREDENIVEAU = NombreDeNiveau;
            NiveauActuel = 1;
            this.TailleFenetre = win.Size;
            QuitterLaPartie = false;

            TuileVide = tuileVide;

            Recommencer();
        }

        public void Recommencer()
        {
            if (Ambiance != null)
            {
                Ambiance.Stop();
                Ambiance = null;
            }
            Image bbi = new Image("bonbon.png");
            bbi.CreateMaskFromColor(Color.White);
            Sprite bb = new Sprite(new Texture(bbi));

            LeNiveau = new Niveau("level" + NiveauActuel + ".txt", TailleFenetre, bb, TuileVide[NiveauActuel - 1]);
            LeHeros = new Heros(new Texture("hero.png"), new IntRect(0, 0, 32, 32), new Vector2f(32, 32), LeNiveau.PositionInitHero, LeNiveau);
            LeNiveau.GetHeros(LeHeros);

            if (NiveauActuel > 1)
                LeHeros.PeutUtiliserLeMana = true;

            Ambiance = new Music("music" + NiveauActuel + ".wav");
            Ambiance.Loop = true;
            Ambiance.Play();
            window.KeyReleased += new EventHandler<KeyEventArgs>(LeHeros.ArreterAnimation);
        }

        public void ChangerDeNiveau()
        {
            NiveauActuel++;
            if (NiveauActuel > NOMBREDENIVEAU)
                QuitterLaPartie = true;

            if (!QuitterLaPartie)
                Recommencer();
        }

        public void Draw(RenderTarget target, RenderStates state)
        {
            if (LeHeros.NombreDeVie == 0)
            {
                Recommencer();
            }

            if (LeNiveau.QuitterLaPartie)
            {
                QuitterLaPartie = true;
                Ambiance.Stop();
            }

            if (LeNiveau.FinNiveau && LeNiveau.TousCoffreOuvert() && (NiveauActuel == 1 || NiveauActuel == 3))
                ChangerDeNiveau();
            else if ((NiveauActuel != 1 && NiveauActuel != 3) && LeNiveau.FinNiveau)
                ChangerDeNiveau();

            // Afficher le niveau et le heros
            target.Draw(LeNiveau);
            target.Draw(LeHeros);
        }
    }
}
