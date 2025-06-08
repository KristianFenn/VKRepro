using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace VKRepro.TextureNotBound;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private Model _sphere;
    private Effect _bufferEffect;
    private Effect _drawEffect;
    private Camera _camera;
    private Vector3 _lightDirection;
    private Matrix _texCoordsToNdc;
    private RenderTarget2D _albedo;
    private RenderTarget2D _normal;
    private RenderTarget2D _depth;
    private RenderTargetBinding[] _deferredBufferBindings;
    private VertexBuffer _fullScreenQuad;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        _graphics.SynchronizeWithVerticalRetrace = true;
    }

    protected override void Initialize()
    {
        _lightDirection = Vector3.Normalize(new Vector3(.2f, .1f,  .0f));
        _texCoordsToNdc = Matrix.CreateScale(2.0f, -2.0f, 1f) * Matrix.CreateTranslation(-1.0f, 1.0f, 0f);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sphere = Content.Load<Model>("Sphere");
        var sphereTexture = Content.Load<Texture2D>("Sphere.Color");
        var sphereNormal = Content.Load<Texture2D>("Sphere.Normal");

        _bufferEffect = Content.Load<Effect>("Buffer");
        _bufferEffect.CurrentTechnique = _bufferEffect.Techniques["DrawBuffers"];
        _bufferEffect.Parameters["Texture"].SetValue(sphereTexture);
        _bufferEffect.Parameters["NormalMap"].SetValue(sphereNormal);

        var viewportSize = GraphicsDevice.Viewport.Bounds;

        _albedo = new RenderTarget2D(GraphicsDevice, viewportSize.Width, viewportSize.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        _normal = new RenderTarget2D(GraphicsDevice, viewportSize.Width, viewportSize.Height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
        _depth = new RenderTarget2D(GraphicsDevice, viewportSize.Width, viewportSize.Height, false, SurfaceFormat.Single, DepthFormat.Depth24);

        _deferredBufferBindings = [
            new RenderTargetBinding(_albedo),
            new RenderTargetBinding(_normal),
            new RenderTargetBinding(_depth),
        ];

        _drawEffect = Content.Load<Effect>("Draw");

        _drawEffect.Parameters["Albedo"].SetValue(_albedo);
        _drawEffect.Parameters["Normal"].SetValue(_normal);
        _drawEffect.Parameters["Depth"].SetValue(_depth);

        _drawEffect.CurrentTechnique = _drawEffect.Techniques["Draw"];

        _fullScreenQuad = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);

        _fullScreenQuad.SetData([
            new VertexPositionTexture(new Vector3(-1,  1,  0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3( 1,  1,  0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, -1,  0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3( 1, -1,  0), new Vector2(1, 1)),
        ]);

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
        _bufferEffect.Parameters["ModelToScreen"].SetValue(_camera.View * _camera.Projection);

        var modelToViewNormal = _camera.View;
        modelToViewNormal.Translation = Vector3.Zero;
        modelToViewNormal = Matrix.Transpose(Matrix.Invert(modelToViewNormal));

        _bufferEffect.Parameters["ModelToViewNormal"].SetValue(modelToViewNormal);

        GraphicsDevice.SetRenderTargets(_deferredBufferBindings);
        GraphicsDevice.Clear(Color.Black);

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

                foreach (var effectPass in _bufferEffect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount);
                }
            }
        }

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.SetVertexBuffer(_fullScreenQuad);
        GraphicsDevice.Indices = null;

        _drawEffect.Parameters["LightDirection"].SetValue(_lightDirection);
        _drawEffect.Parameters["ScreenSpaceToView"].SetValue(_texCoordsToNdc * Matrix.Invert(_camera.Projection));

        _drawEffect.CurrentTechnique.Passes[0].Apply();
        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

        base.Draw(gameTime);
    }
}
