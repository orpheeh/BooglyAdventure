using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SFML;
using SFML.System;
using SFML.Graphics;
using SFML.Window;
using SFML.Audio;

namespace BooglyAdventure
{
    class Niveau : Transformable, Drawable
    {
        public Map Monde { get; private set; }
        public RessourceObjet[] Ressources { get; set; }
        public Ennemi[] Gardiens { get; set; }
        public Vector2u TailleFenetre { get; private set; }
        public static Vector2i NombreDeTuileTileset;
        public Sprite SpriteBonbon;
        public Heros LeHeros;
        public Vector2f PositionInitHero;
        public bool FinNiveau;

        //Instruction du debut de partie
        public Text[] LigneDInstruction;
        public Vector2f PositionInstruction;
        public bool Commencer;
        public RectangleShape ArrierePlanDuText;

        //Menu de commande
        public delegate void Command();
        public Menu Pause;
        public bool QuitterLaPartie;


        public Niveau(string fichier, Vector2u TailleFenetre, Sprite bonbon = null,int tuileVide = 8)
        {
            FichierNiveau = new FileStream(fichier, FileMode.Open);
            this.TailleFenetre = TailleFenetre;
            SpriteBonbon = bonbon;
            Commencer = false;
            PositionInstruction = new Vector2f(20, 200);
            ChargerNiveau();

            ChoixMenu continuer = new ChoixMenu("Continuer", FContinuer);
            ChoixMenu quit = new ChoixMenu("Quitter", Quitter);
            Pause = new Menu(100, new Vector2f(0,0), continuer, quit);

            ArrierePlanDuText = new RectangleShape(new Vector2f(LigneDInstruction[0].GetGlobalBounds().Width + 20, (LigneDInstruction[0].GetGlobalBounds().Height + 10) * LigneDInstruction.Length + 20 + Pause.TextChoix[0].GetGlobalBounds().Height + 20));
            ArrierePlanDuText.Position = new Vector2f((TailleFenetre.X - ArrierePlanDuText.Size.X) / 2, (TailleFenetre.Y - ArrierePlanDuText.Size.Y) / 2);

            PositionInstruction = ArrierePlanDuText.Position + new Vector2f(10, 10);
            for (int i = 0; i < LigneDInstruction.Length; i++)
            {
                LigneDInstruction[i].Position = PositionInstruction + new Vector2f(0, i * LigneDInstruction[i].GetGlobalBounds().Height + 20);
            }

            Pause.ChangePosition(LigneDInstruction[LigneDInstruction.Length - 1].Position + new Vector2f(ArrierePlanDuText.Size.X / 2 - Pause.TextChoix[0].GetGlobalBounds().Width - Pause.Espace/2 - LigneDInstruction[LigneDInstruction.Length - 1].Position.X, LigneDInstruction[LigneDInstruction.Length - 1].GetGlobalBounds().Height + 20));
            
            ArrierePlanDuText.FillColor = new Color(121, 121, 121);
            FinNiveau = false;

            Monde.TuileVide = tuileVide;
    
        }

        public void ChargerNiveau()
        {
            StreamReader Flux = new StreamReader(FichierNiveau);
            Flux.ReadLine();
            string NomTileset = Flux.ReadLine();
            Tampon = 0;
            NombreDeTuileTileset = new Vector2i(lireEntier(Flux), lireEntier(Flux));
            TileProp[] ProprieteDesTuiles = new TileProp[NombreDeTuileTileset.X * NombreDeTuileTileset.Y];

            for(int i = 0; i < ProprieteDesTuiles.Length; i++)
            {
                string ligne;
                if ((ligne = Flux.ReadLine()).Contains("plein"))
                {
                    ProprieteDesTuiles[i].Plein = true;
                }
                else
                    ProprieteDesTuiles[i].Plein = false;
            }

            Flux.ReadLine();
            Tampon = 0;
            int[,] Schema = new int[lireEntier(Flux), lireEntier(Flux)];

            for (int i = 0; i < Schema.GetLength(1); i++)
            {
                for (int j = 0; j < Schema.GetLength(0); j++)
                {
                    Schema[j, i] = lireEntier(Flux);
                }
            }

            //Initialisation de la Map
            Monde = new Map(ProprieteDesTuiles, Schema, TailleFenetre, NomTileset);

            //Chargement des ennemis du niveau
            Flux.ReadLine();
            Tampon = 0;
            Gardiens = new Ennemi[lireEntier(Flux)];
            for (int i = 0; i < Gardiens.Length; i++)
            {
                Flux.ReadLine();
                string image = Flux.ReadLine();
                int[] coord = new int[4];

                Tampon = 0;
                for (int j = 0; j < coord.Length; j++)
                    coord[j] = lireEntier(Flux);

                Flux.ReadLine();
                Tampon = 0;
                Vector2f taille = new Vector2f(lireEntier(Flux), lireEntier(Flux));

                Flux.ReadLine();
                Tampon = 0;
                Vector2f pos = new Vector2f(lireEntier(Flux), lireEntier(Flux));

                Flux.ReadLine();
                Tampon = 0;
                int vitesse = lireEntier(Flux);

                Flux.ReadLine();
                Tampon = 0;
                int BB = lireEntier(Flux);
                Console.WriteLine(BB);

                Flux.ReadLine();
                int distance = lireEntier(Flux);

                Flux.ReadLine();
                int vie = lireEntier(Flux);

                Gardiens[i] = new Ennemi(new Texture(image), new IntRect(coord[0], coord[1], coord[2], coord[3]), taille, pos, this, vitesse, BB, vie, distance);
            }

            //Chargement des ressources
            Flux.ReadLine();
            Tampon = 0;
            Ressources = new RessourceObjet[lireEntier(Flux)];

            for (int i = 0; i < Ressources.Length; i++)
            {
                Flux.ReadLine();        //Lire le numero
                string TypeDeRessource = Flux.ReadLine();
                string Image = Flux.ReadLine();
                int[] Coordonne = new int[4];
                string ImageCoffreOuvert = "";
                int[] CoordonneCoffreOuvert = new int[4];
                int IndicePourAgregat = 0;

                Tampon = 0;
                for (int j = 0; j < Coordonne.Length; j++)
                    Coordonne[j] = lireEntier(Flux);

                if (TypeDeRessource == "coffre")
                {
                    ImageCoffreOuvert = Flux.ReadLine();
                    CoordonneCoffreOuvert = new int[4];

                    Tampon = 0;
                    for (int j = 0; j < CoordonneCoffreOuvert.Length; j++)
                        CoordonneCoffreOuvert[j] = lireEntier(Flux);
                }
                else if (TypeDeRessource == "cle")
                {
                    
                    IndicePourAgregat = lireEntier(Flux);
                }

                Flux.ReadLine();
                Tampon = 0;
                Vector2f pos = new Vector2f(lireEntier(Flux), lireEntier(Flux));
                Console.WriteLine("Position ressource " + pos);
                switch (TypeDeRessource)
                {
                    case "bonbon":
                        Sprite tmp = new Sprite(new Texture(Image, new IntRect(Coordonne[0], Coordonne[1], Coordonne[2], Coordonne[3])));
                        Ressources[i] = new BonbonGARGOU(tmp);
                        break;
                    case "lanterne":
                        Ressources[i] = new LanterneMortel(new Sprite(new Texture(Image, new IntRect(Coordonne[0], Coordonne[1], Coordonne[2], Coordonne[3]))));
                        break;
                    case "portail":
                        Ressources[i] = new Portail(new Sprite(new Texture(Image, new IntRect(Coordonne[0], Coordonne[1], Coordonne[2], Coordonne[3]))));
                        break;
                    case "cle":
                        Ressources[i] = new Cle(new Sprite(new Texture(Image, new IntRect(Coordonne[0], Coordonne[1], Coordonne[2], Coordonne[3]))), (Coffre)Ressources[IndicePourAgregat]);
                        break;
                    case "coffre":
                        Sprite Ouvert = new Sprite(new Texture(ImageCoffreOuvert, new IntRect(CoordonneCoffreOuvert[0], CoordonneCoffreOuvert[1], CoordonneCoffreOuvert[2], CoordonneCoffreOuvert[3])));
                        Ressources[i] = new Coffre(new Sprite(new Texture(Image, new IntRect(Coordonne[0], Coordonne[1], Coordonne[2], Coordonne[3]))), Ouvert);
                        break;
                }

                Ressources[i].SetPosition(pos);
                Ressources[i].PositionAbsolue = pos;
            }

            //Lecture des instructions du niveau
            Flux.ReadLine();
            Tampon = 0;
            LigneDInstruction = new Text[lireEntier(Flux)];

            for (int i = 0; i < LigneDInstruction.Length; i++ )
            {
                LigneDInstruction[i] = new Text(Flux.ReadLine(), new Font("font1.ttf"));
                LigneDInstruction[i].CharacterSize = 20;
                LigneDInstruction[i].Position = PositionInstruction + new Vector2f(0, i * LigneDInstruction[i].GetGlobalBounds().Height + 20);
            }

            Flux.ReadLine();
            Tampon = 0;
            PositionInitHero = new Vector2f(lireEntier(Flux), lireEntier(Flux));
                //Fermeture du fichier
                Flux.Close();
        }
        public void Draw(RenderTarget target, RenderStates state)
        {

            for (int i = 0; i < Gardiens.Length && Commencer; i++)
            {
                if(Gardiens[i] != null)
                {
                    Gardiens[i].UtiliserMana(LeHeros);
                    if (Gardiens[i].Mana.SeDeplace &&
                       Gardiens[i].Mana.CercleDeFeu.GetGlobalBounds().Intersects(LeHeros.AspectPhysique.GetGlobalBounds()))
                    {
                        LeHeros.Toucher();
                        Gardiens[i].Mana.SeDeplace = false;
                    }
                }
            }

            target.Draw(Monde);
            for (int i = 0; i < Ressources.Length; i++)
                if (Ressources[i] != null && !(Ressources[i] is Coffre) && Ressources[i].PositionRelative.X + Ressources[i].Size.X >= 0 && Ressources[i].PositionRelative.Y + Ressources[i].Size.Y >= 0)
                    target.Draw(Ressources[i]);
            for (int i = 0; i < Gardiens.Length && Commencer; i++ )
                if (Gardiens[i] != null)
                    target.Draw(Gardiens[i]);

            if (Commencer == false)
            {
                target.Draw(ArrierePlanDuText);
                target.Draw(Pause);
            }
            for (int i = 0; i < LigneDInstruction.Length && Commencer == false; i++)
                target.Draw(LigneDInstruction[i]);
        }

        public void PositionnerLesRessources()
        {
            for(int i = 0; i < Ressources.Length; i++)
            {
                if(Ressources[i] != null)
                    Ressources[i].SetPosition(Ressources[i].PositionAbsolue - new Vector2f(Monde.ScrollingSFML.Center.X - Monde.ScrollingSFML.Size.X / 2, Monde.ScrollingSFML.Center.Y - Monde.ScrollingSFML.Size.Y / 2));
            }
        }
        // Permet de Récupérer des nombres entiers dans un fichier texte
        public static int Tampon = 0;
        public static int lireEntier(StreamReader st)
        {
            int c = 0, pick;
            string Num = "";
            bool ok = false;

            Num += Tampon;
            while (Int32.TryParse(Num, out pick))
            {
                ok = true;
                if (Char.IsDigit((char)(c = st.Read())))
                    Tampon = Int32.Parse((char)c + "");
                Num += (char)c;
                c = pick;
            }
            return ok ? c : Tampon;
        }
        public void GetHeros(Heros h) { LeHeros = h; LeHeros.PositionAbsolue = PositionInitHero; }
        
        public bool TousCoffreOuvert()
        {
            bool rep = true;
            for(int i = 0; Ressources != null && i < Ressources.Length && rep; i++)
                if(Ressources[i] is Coffre)
                {
                    Coffre tmp = (Coffre)Ressources[i];
                    if (tmp.Ouvert == false)
                        rep = false;
                }
            return rep;
        }

        public void FContinuer()
        {
            Commencer = !Commencer;
        }
        public void Quitter()
        {
            QuitterLaPartie = true;
            Console.WriteLine("Quitter le niveau");
        }
        private FileStream FichierNiveau;
    }

    struct ChoixMenu
    {
        public string Action;
        public Niveau.Command Fonction;

        public ChoixMenu(string action, Niveau.Command com)
        {
            Action = action;
            Fonction = com;
        }
    }

    class Menu : Transformable, Drawable
    {
        public ChoixMenu[] Choix;
        public int NumerohoixActuel;
        public int Espace;
        protected CircleShape Puce;
        public Text[] TextChoix;
        public Menu(int EspaceEnChoix, Vector2f pos, params ChoixMenu[] para)
        {
            Choix = para;
            NumerohoixActuel = 0;
            Espace = EspaceEnChoix;

            TextChoix = new Text[Choix.Length];
            for(int i = 0; i < TextChoix.Length; i++)
            {
                TextChoix[i] = new Text(Choix[i].Action, new Font("font.ttf"));
                TextChoix[i].CharacterSize = 20;
                if(i > 0)
                    TextChoix[i].Position = pos + new Vector2f(i * TextChoix[i-1].GetGlobalBounds().Width + Espace, 0);
                else
                    TextChoix[i].Position = pos;
                TextChoix[i].Color = Color.Black;
            }

            Puce = new CircleShape(5);
            Puce.FillColor = new Color(0,0,0);
            Puce.Position = TextChoix[NumerohoixActuel].Position - new Vector2f(5 + Puce.GetGlobalBounds().Width, -10);
        }

        public void ModifierNumero(int dl)
        {
            NumerohoixActuel += dl;
            if (NumerohoixActuel > Choix.Length - 1)
                NumerohoixActuel = Choix.Length - 1;
            else if (NumerohoixActuel < 0)
                NumerohoixActuel = 0;

            Puce.Position = TextChoix[NumerohoixActuel].Position - new Vector2f(5 + Puce.GetGlobalBounds().Width, -10);

        }
        public void ExecuteChoix()
        {
            Choix[NumerohoixActuel].Fonction();
        }
        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(Puce);
            for (int i = 0; i < TextChoix.Length; i++)
                target.Draw(TextChoix[i]);
        }
        public virtual void ChangePosition(Vector2f pos)
        {
            for (int i = 0; i < TextChoix.Length; i++)
            {
                if (i > 0)
                    TextChoix[i].Position = pos + new Vector2f(i * TextChoix[i - 1].GetGlobalBounds().Width + Espace, 0);
                else
                    TextChoix[i].Position = pos;
            }
            Puce.Position = TextChoix[NumerohoixActuel].Position - new Vector2f(5 + Puce.GetGlobalBounds().Width, -10);
        }
    }
}
