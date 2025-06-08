using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace VKRepro.UnsetTexture
{
    public class Camera(float aspectRatio)
    {
        private const float CamMoveSpeed = 2.0f;

        public Matrix View;
        public Matrix Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 3, aspectRatio, 0.1f, 100f);

        public Vector3 CameraPos = new(3f);
        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            Vector2 camMove = Vector2.Zero;

            var scaledMoveSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * CamMoveSpeed;
            if (keyboardState.IsKeyDown(Keys.Left)) camMove.X -= scaledMoveSpeed;
            if (keyboardState.IsKeyDown(Keys.Right)) camMove.X += scaledMoveSpeed;
            if (keyboardState.IsKeyDown(Keys.Up)) camMove.Y -= scaledMoveSpeed;
            if (keyboardState.IsKeyDown(Keys.Down)) camMove.Y += scaledMoveSpeed;

            var cameraLeft = Vector3.Normalize(Vector3.Cross(Vector3.Up, CameraPos));

            var rotate = Quaternion.CreateFromAxisAngle(Vector3.Up, camMove.X)
                * Quaternion.CreateFromAxisAngle(cameraLeft, camMove.Y);

            CameraPos = Vector3.Transform(CameraPos, rotate);

            View = Matrix.CreateLookAt(CameraPos, Vector3.Zero, Vector3.Up);
        }
    }
}
