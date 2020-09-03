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
    class Heros : Personnage
    {
        public Clock TimerPrincipal, TimerScrollingAuto, TimerAnimeBaffe;

        //Animation attaque
        public int JaugeDeMana;
        public const int MAXMANA = 10;
        public bool PeutUtiliserLeMana;

        //Variable permettant d' ajuster le saut
        public int VitesseDuSaut;
        public int VitesseHorizontal;
        public bool EstEntrainDeSauter;

        //Affichage de la jauge de mana
        Text Jauge;
        CircleShape CercleMana;

        //Bruitage
        public Sound item;
        public Sound jump, powerup;
        public Heros(Texture texture, IntRect AspectInitial, Vector2f TaillePerso, Vector2f positionInitial, Niveau level, int speed = 2): base(texture,AspectInitial,TaillePerso,positionInitial,level,viemax:10)
        {

            PeutUtiliserLeMana = false;
            TimerPrincipal = new Clock();
            ConstanteGravitationnelle = 2;
            EstEntrainDeSauter = false;
            TimerScrollingAuto = new Clock();
            TimerAnimeBaffe = new Clock();
            JaugeDeMana = 0;

            CercleMana = new CircleShape(Mana.CercleDeFeu);
            CercleMana.Position = new Vector2f(10, 10);
            CercleMana.Radius = 10;
            Jauge = new Text("x " + JaugeDeMana,new Font("font.ttf"));
            Jauge.CharacterSize = 20;
            Jauge.Position = new Vector2f(10 + CercleMana.GetGlobalBounds().Width + 2, 10);

            //Bruitage
            item = new Sound(new SoundBuffer("item.wav"));
            jump = new Sound(new SoundBuffer("jump.wav"));
            powerup = new Sound(new SoundBuffer("powerup.wav"));
            item.Loop = false;
            jump.Loop = false;
            powerup.Loop = false;

        }
        public override void Draw(RenderTarget target, RenderStates state)
        {

            if(TimerPrincipal.ElapsedTime.AsSeconds() >= 0.1)
            {
                Input();
                // Vérifiaction des collisions avec objet du décor
                CollisionObect();
                CollisionEnnemi();
                if (EstEntrainDeSauter)
                    Saute();
                else
                    Graviter();
                if(MettreAJourAnimation)
                {
                    AnimationDeplacement.X++;
                    if (AnimationDeplacement.X * TaillePerso.X >= Image.Size.X)
                        AnimationDeplacement.X = 0;
                }
                TimerPrincipal.Restart();
            }

            if(TimerScrollingAuto.ElapsedTime.AsSeconds() >= 0.01)
            {
                ScrollingAutomatique();
                TimerScrollingAuto.Restart();
            }
            DeplacerPersonnage();
            AspectPhysique.TextureRect = new IntRect(AnimationDeplacement.X * (int)TaillePerso.X, AnimationDeplacement.Y * (int)TaillePerso.Y, (int)TaillePerso.X, (int)TaillePerso.Y);
            
            base.Draw(target, state);
            target.Draw(CercleMana);
            target.Draw(Jauge);
        }
        public void CollisionObect()
        {
            for(int i = 0; i < level.Ressources.Length; i++)
            {
                if (level.Ressources[i] != null && AspectPhysique.GetGlobalBounds().Intersects(level.Ressources[i].GetBoiteEnglobante()))
                {
                    if(level.Ressources[i] is BonbonGARGOU)
                    {
                        item.Play();
                        BonbonGARGOU tmp = (BonbonGARGOU)level.Ressources[i];
                        level.Ressources[i] = null;
                        if (JaugeDeMana < MAXMANA && PeutUtiliserLeMana)
                        {
                            JaugeDeMana++;
                            Jauge.DisplayedString = "x " + JaugeDeMana;
                        }
                        else if (NombreDeVie < NOMBREVIEMAX)
                            Revie();
                        else if(JaugeDeMana < MAXMANA)
                        {
                            JaugeDeMana++;
                            Jauge.DisplayedString = "x " + JaugeDeMana;
                        }
                    }
                    else if(level.Ressources[i] is LanterneMortel)
                    {
                        Toucher();
                    }
                    else if(level.Ressources[i] is Cle)
                    {
                        item.Play();
                        Cle tmp = (Cle)level.Ressources[i];
                        tmp.OuvrirMonCoffre();
                        PeutUtiliserLeMana = true;
                        level.Ressources[i] = null;
                   }
                    else if(level.Ressources[i] is Portail)
                    {
                        level.FinNiveau = true;
                    }
                }
            }
        }
        public void CollisionEnnemi()
        {
            for(int i = 0; i < level.Gardiens.Length; i++)
            {


                if (level.Gardiens[i] != null)
                {
                    if (AspectPhysique.GetGlobalBounds().Intersects(level.Gardiens[i].AspectPhysique.GetGlobalBounds()))
                    {
                        this.Toucher();
                    }
                    if(Mana.SeDeplace && Mana.CercleDeFeu.GetGlobalBounds().Intersects(level.Gardiens[i].AspectPhysique.GetGlobalBounds()))
                    {
                        level.Gardiens[i].Toucher();
                        Mana.SeDeplace = false;
                        if(level.Gardiens[i].NombreDeVie == 0)
                        {
                            level.Gardiens[i].Mort();
                            level.Gardiens[i] = null;
                        }
                    }


                }
            }
        }
        public void Input()
        {
            if(Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                if (level.Commencer && EstEntrainDeSauter)
                {
                    VitesseHorizontal = -VitesseDeplacement;
                }
                else if(level.Commencer)
                {
                    VecteurDeplacement = new Vector2f(0, 0);
                    AnimationDeplacement.Y = (int)Direction.Left;
                    VecteurDeplacement.X = -VitesseDeplacement;
                    MettreAJourAnimation = true;
                }
                else
                {
                    level.Pause.ModifierNumero(-1);
                }
            }
            else if(Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                if (level.Commencer && EstEntrainDeSauter)
                {
                    VitesseHorizontal = VitesseDeplacement;
                }
                else if(level.Commencer)
                {
                    VecteurDeplacement = new Vector2f(0, 0);
                    AnimationDeplacement.Y = (int)Direction.Right;
                    VecteurDeplacement.X = VitesseDeplacement;
                    MettreAJourAnimation = true;
                }
                else
                {
                    level.Pause.ModifierNumero(1);
                }
            }
           
            if(level.Commencer && Keyboard.IsKeyPressed(Keyboard.Key.Space) && EstEntrainDeSauter == false)
            {
                jump.Play();
                EstEntrainDeSauter = true;
                VitesseHorizontal = 0;
                VitesseDuSaut = -6;
            }

            if(level.Commencer && Keyboard.IsKeyPressed(Keyboard.Key.G) && PeutUtiliserLeMana && JaugeDeMana > 0)
            {
                powerup.Play();
                Mana.SeDeplace = true;
                if (AnimationDeplacement.Y == (int)Direction.Right)
                    Mana.direction = Direction.Right;
                else if (AnimationDeplacement.Y == (int)Direction.Left)
                    Mana.direction = Direction.Left;
                //Mana.CercleDeFeu.Position = AspectPhysique.Position;
                Mana.MovePositionAbsolue(-Mana.PositionAbsolue + PositionAbsolue, level);
                Mana.TimerDeplacement.Restart();
                JaugeDeMana--;
                Jauge.DisplayedString = "x " + JaugeDeMana;
            }

            if(Keyboard.IsKeyPressed(Keyboard.Key.Return))
            {
                level.Pause.ExecuteChoix();
            }
        }
        public void ArreterAnimation(object sender, KeyEventArgs e)
        {
            MettreAJourAnimation = false;
            switch(e.Code)
            {
                case Keyboard.Key.Left:
                    VecteurDeplacement.X = 0;
                    break;
                case Keyboard.Key.Right:
                    VecteurDeplacement.X = 0;
                    break;
            }
        }
        public void Saute()
        {
            VecteurDeplacement.X = VitesseHorizontal;
            VecteurDeplacement.Y = VitesseDuSaut;

            if(DeplacerPersonnage() == false)
            {
                if(VitesseDuSaut < 0)
                {
                    VitesseDuSaut = 0;
                }
                else
                {
                    EstEntrainDeSauter = false;
                    VecteurDeplacement.X = 0;
                }
            }
            VitesseDuSaut += ConstanteGravitationnelle;
        }
        public void ScrollingAutomatique()
        {
            float x = PositionAbsolue.X + TaillePerso.X / 2 - level.TailleFenetre.X  / 2;
            float y = PositionAbsolue.Y + TaillePerso.Y / 2 - level.TailleFenetre.Y / 2;

            if (x < 0)
                x = 0;
            else if (x + level.TailleFenetre.X > level.Monde.TileMap.GetLength(0) * level.Monde.TailleTuile.X)
                x = level.Monde.TileMap.GetLength(0) * level.Monde.TailleTuile.X - level.TailleFenetre.X;

            if (y < 0)
                y = 0;
            else if (y + level.TailleFenetre.Y > level.Monde.TileMap.GetLength(1) * level.Monde.TailleTuile.Y)
                y = level.Monde.TileMap.GetLength(1) * level.Monde.TailleTuile.Y - level.TailleFenetre.Y;

            level.Monde.ScrollingSFML = new View(new FloatRect(x, y, level.TailleFenetre.X, level.TailleFenetre.Y));
            level.PositionnerLesRessources();
        }

    }
}
