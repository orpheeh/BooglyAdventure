using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace BooglyAdventure
{
    class Program
    {
        static NouvellePartie GestionPartie;
        static RenderWindow window;
        static Menu_Principal LeMenu;
        static void Main(string[] args)
        { 
            window = new RenderWindow(new VideoMode(960, 640), "Boogly", Styles.Titlebar);
            window.Closed += delegate { window.Close(); };
            window.KeyPressed += new EventHandler<KeyEventArgs>(Key);
            window.SetFramerateLimit(60);

            Sprite Fond = new Sprite(new Texture("fond_menu.png"));

            //Menu principale
            LeMenu = new Menu_Principal(50, new Vector2f(50, 300), new ChoixMenu("Nouvelle partie", NewGame), new ChoixMenu("Quitter", Quit));
            LeMenu.ChangePosition(new Vector2f(50, 300));

            Text Titre = new Text("Sweet Dream", new Font("font.ttf"));
            Titre.CharacterSize *= 3;
            Titre.Position = new Vector2f((window.Size.X - Titre.GetGlobalBounds().Width) / 2, (window.Size.Y - Titre.GetGlobalBounds().Height) / 2 - 200);

            GestionPartie = null;

            while(window.IsOpen)
            {
                window.DispatchEvents();

                if (GestionPartie != null && GestionPartie.QuitterLaPartie)
                    GestionPartie = null;

                    window.Clear();

                    window.Draw(Fond);
                    if (GestionPartie != null)
                        window.Draw(GestionPartie);
                    else
                    {
                        window.Draw(Titre);
                        window.Draw(LeMenu);
                    }
                    window.Display();
          
            }
        }

        public static void Key(object sender, KeyEventArgs e)
        {
            if(GestionPartie == null)
            {
                if(e.Code == Keyboard.Key.Up)
                {
                    LeMenu.ModifierNumero(-1);
                }
                else if(e.Code == Keyboard.Key.Down)
                {
                    LeMenu.ModifierNumero(1);
                }
                else if(e.Code == Keyboard.Key.Return)
                {
                    LeMenu.ExecuteChoix();
                }
            }
        }

        public static void NewGame()
        {
            if (GestionPartie == null)
                 GestionPartie = new NouvellePartie(window, 3, 8, 8, 8);
        }

        public static void Quit()
        {
            window.Close();
        }
    }

    class Menu_Principal : Menu
    {
        public Menu_Principal(int EspaceEnChoix, Vector2f pos, params ChoixMenu[] para): base(EspaceEnChoix, pos, para)
        {
            for (int i = 0; i < TextChoix.Length; i++)
            {
                TextChoix[i].Color = Color.Red;
                TextChoix[i].CharacterSize *= 2;
            }
            Puce.FillColor = Color.Red;
        }

        public override void ChangePosition(Vector2f pos)
        {
            for (int i = 0; i < TextChoix.Length; i++)
            {
                TextChoix[i].Position = pos + new Vector2f(0, (Espace + TextChoix[i].GetGlobalBounds().Height) * i);
            }
            Puce.Position = TextChoix[NumerohoixActuel].Position - new Vector2f(5 + Puce.GetGlobalBounds().Width, -10);

        }
        
    }
}
