using System;
using Engine;
using Engine.Graphics;
using Game;
using TemplatesDatabase;

namespace ILTG
{
	public class SubsystemTerrain : Game.SubsystemTerrain, IDrawable, IUpdateable
	{
		public static RenderTarget2D ShadowTex = new RenderTarget2D(1024, 1024, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);

		public TerrainRenderer NTerrainRenderer;

		public override void Load(ValuesDictionary valuesDictionary)
		{
		    base.Load(valuesDictionary);
		    try
		    {
			   NTerrainRenderer = new TerrainRenderer(this);
			   base.TerrainRenderer = new Game.TerrainRenderer(this);
			}
			catch (Exception ex)
			{
			    Log.Error(ex);
		        throw;
	        }
			Game.SubsystemTerrain.TerrainRenderingEnabled = true;
		}

		public new void Update(float dt)
		{
			base.Update(dt);
			NTerrainRenderer.Update(dt);
		}

		public Matrix CalculateBaseProjectionMatrix(Vector2 wh)
		{
			float num = 90f;
			float num2 = 1f;
			if (SettingsManager.ViewAngleMode == ViewAngleMode.Narrow)
			{
				num2 = 0.8f;
			}
			else if (SettingsManager.ViewAngleMode == ViewAngleMode.Normal)
			{
				num2 = 0.9f;
			}
			float num3 = wh.X / wh.Y;
			float num4 = MathUtils.Min(num * num3, num);
			float num5 = num4 * num3;
			if (num5 < 90f)
			{
				num4 *= 90f / num5;
			}
			else if (num5 > 175f)
			{
				num4 *= 175f / num5;
			}
			return Matrix.CreatePerspectiveFieldOfView(MathUtils.DegToRad(num4 * num2), num3, 0.1f, 2048f);
		}

		public new void Draw(Camera camera, int drawOrder)
		{
			if (!Game.SubsystemTerrain.TerrainRenderingEnabled)
			{
				return;
			}
			if (drawOrder == Game.SubsystemTerrain.m_drawOrders[0])
			{
				base.TerrainUpdater.PrepareForDrawing(camera);
				NTerrainRenderer.PrepareForDrawing(camera);
				try
				{
					NTerrainRenderer.DrawOpaque(camera);
					return;
				}
				catch (Exception ex)
				{
					Game.SubsystemTerrain.TerrainRenderingEnabled = false;
					DialogsManager.ShowDialog(null, new MessageDialog("Unhandled Exception", ex.ToString(), LanguageControl.Get("Usual", "ok"), null, null));
					return;
				}
			}
			if (drawOrder == Game.SubsystemTerrain.m_drawOrders[1])
			{
				try
				{
					NTerrainRenderer.DrawAlphaTested(camera);
					NTerrainRenderer.DrawTransparent(camera);
				}
				catch (Exception ex)
				{
					Game.SubsystemTerrain.TerrainRenderingEnabled = false;
					DialogsManager.ShowDialog(null, new MessageDialog("Unhandled Exception", ex.ToString(), LanguageControl.Get("Usual", "ok"), null, null));
				}
			}
		}
	}
}
