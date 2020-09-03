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
    class RessourceObjet : Transformable, Drawable
    {
        public Vector2f PositionAbsolue { get; set; }
        public Vector2f PositionRelative { get; set; }
        public Vector2f Size { get; set; }
        public RessourceObjet(Sprite Forme)
        {
            this.Forme = Forme;
            Size = new Vector2f(Forme.TextureRect.Width,Forme.TextureRect.Height);
        }
        public virtual void Draw(RenderTarget target, RenderStates state)
        {
            target.Draw(Forme);
        }
        public virtual void SetPosition(Vector2f pos)
        {
            Forme.Position = pos;
            PositionRelative = pos;
        }

        public FloatRect GetBoiteEnglobante()
        {
            return Forme.GetGlobalBounds();
        }
        protected Sprite Forme;
    }

    class BonbonGARGOU : RessourceObjet
    {
        public BonbonGARGOU(Sprite sp): base(sp)
        {
        }
    }

    class LanterneMortel : RessourceObjet
    {
        public LanterneMortel(Sprite sp): base(sp)
        {
        }
    }

    class Cle : RessourceObjet
    {
        public Cle(Sprite sp, Coffre coffre): base(sp)
        {
            this.MonCoffre = coffre;
        }
        public void OuvrirMonCoffre()
        {
            MonCoffre.Ouvert = true;
            MonCoffre.ChangerForme();
        }
        private Coffre MonCoffre;
    }

    class Coffre : RessourceObjet
    {

        public bool Ouvert;
        public Coffre(Sprite sp, Sprite CoffreOuvert): base(sp)
        {

            this.Ouvert = false;
            this.CoffreOuvert = CoffreOuvert;
            CoffreOuvert.Position = Forme.Position;
        }
        public void ChangerForme()
        {
            Forme = CoffreOuvert;
        }

        public override void SetPosition(Vector2f pos)
        {
            CoffreOuvert.Position = Forme.Position = pos;
        }
        private Sprite CoffreOuvert;
    }

    class Portail : RessourceObjet
    {
        public Portail(Sprite sp): base(sp)
        {
        }

        public override void Draw(RenderTarget target, RenderStates state)
        {
            base.Draw(target, state);
        }
    }
}
