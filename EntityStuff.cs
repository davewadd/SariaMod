using Microsoft.Xna.Framework;
namespace SariaMod
{
    public abstract class Entity
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 oldPosition;
        public Vector2 oldVelocity;
        public int width;
        public int height;
        public Vector2 Center
        {
            get
            {
                return new Vector2(position.X + (float)(width / 2), position.Y + (float)(height / 2));
            }
            set
            {
                position = new Vector2(value.X - (float)(width / 2), value.Y - (float)(height / 2));
            }
        }
        public float Distance(Vector2 Other)
        {
            return Vector2.Distance(Center, Other);
        }
    }
}