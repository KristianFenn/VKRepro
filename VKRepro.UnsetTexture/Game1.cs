using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace VKRepro.UnsetTexture;

public class Game1 : Game
{
    private const bool _enableNormalMaps = true;
    private const bool _provideNormalMap = false;

    private GraphicsDeviceManager _graphics;
    private Model _sphere;
    private Effect _shader;
    private Camera _camera;
    private Vector3 _lightDirection;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        _graphics.SynchronizeWithVerticalRetrace = true;
    }

    protected override void Initialize()
    {
        _lightDirection = Vector3.Normalize(new Vector3(.2f, .1f, .0f));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sphere = Content.Load<Model>("Sphere");
        var sphereTexture = Content.Load<Texture2D>("Sphere.Color");

        _shader = Content.Load<Effect>("Shader");
        _shader.CurrentTechnique = _shader.Techniques["DrawVertexNormal"];
        _shader.Parameters["Texture"].SetValue(sphereTexture);

        if (_enableNormalMaps)
        {
            _shader.CurrentTechnique = _shader.Techniques["DrawNormalMap"];

            if (_provideNormalMap)
            {
                var sphereNormal = Content.Load<Texture2D>("Sphere.Normal");
                _shader.Parameters["UseNormalMap"].SetValue(1);
                _shader.Parameters["NormalMap"].SetValue(sphereNormal);
            }
        }

        _camera = new Camera(GraphicsDevice.Viewport.AspectRatio);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _camera.Update(gameTime);

        var lightRotate = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver4 * (float)gameTime.ElapsedGameTime.TotalSeconds);
        Vector3.Transform(ref _lightDirection, ref lightRotate, out _lightDirection);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _shader.Parameters["ModelToView"].SetValue(_camera.View);

        var modelToViewNormal = _camera.View;
        modelToViewNormal.Translation = Vector3.Zero;
        modelToViewNormal = Matrix.Transpose(Matrix.Invert(modelToViewNormal));

        _shader.Parameters["ModelToViewNormal"].SetValue(modelToViewNormal);
        _shader.Parameters["LightDirection"].SetValue(Vector3.Transform(_lightDirection, modelToViewNormal));
        _shader.Parameters["Projection"].SetValue(_camera.Projection);

        foreach (var bone in _sphere.Bones)
        {
            foreach (var mesh in _sphere.Meshes.SelectMany(m => m.MeshParts))
            {
                GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                GraphicsDevice.Indices = mesh.IndexBuffer;

                var effect = mesh.Effect as BasicEffect;
                effect.View = _camera.View;
                effect.Projection = _camera.Projection;
                effect.World = Matrix.Identity;

                foreach (var effectPass in _shader.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount);
                }
            }
        }

        base.Draw(gameTime);
    }
}
