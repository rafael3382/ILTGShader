using Engine;
using Engine.Graphics;
using Game;
using System;
using System.Collections.Generic;
using GameEntitySystem;

namespace ILTG
{
    public static class Postprocessing
    {
	    public static bool IsEnabled = true;
    
	    private static PrimitivesRenderer2D m_primitiveRender;
    
	    public static bool BlockNextDraw = false;
    
	    public static RenderTarget2D Screen;
	
	    public static RenderTarget2D OldRenderTarget;
	    public static Rectangle OldScissorRectangle;
	
	    public static Game.SubsystemSky m_subsystemSky;
        
        public static UnlitShader m_finalShader;
        
        public static SubsystemPlayers m_subsystemPlayers;
        
	    public static void Load(Project project)
	    {
	        IsEnabled = true;
	        double start = Time.RealTime;
	        
	        try
	        {
	            if (Screen == null)
		            Screen = new RenderTarget2D(Display.Viewport.Width, Display.Viewport.Height, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);
		    }
		    catch {}
		    
		    m_primitiveRender = new PrimitivesRenderer2D();
		    m_subsystemSky = project.FindSubsystem<Game.SubsystemSky>();
		    m_subsystemPlayers = project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
		    try
            {
		       m_finalShader = new UnlitShader(ShaderReader.GetShader("Final.vsh", ILTG.Settings.ToMacros()), ShaderReader.GetShader("Final.psh", ILTG.Settings.ToMacros()), useVertexColor: false, useTexture: true, useAlphaThreshold: false);
		    }
		    catch (Exception e)
            {
#if DEBUG
                DialogsManager.ShowDialog(null, new MessageDialog("Shader Error", "Could not compile the post-processing effect on your device, that means the shader won't have reflections on blocks, but still with reflections on water. \n\nSorry, This shader can't be tested in all devices, It usually works better on smartphones, and mainly in the Qualcomm Adreno GPU which is the gpu that the developer uses. \n\nIf you want to report the error, \nGive the developer this information: \n"+e.Message, LanguageControl.Get("Usual", "ok"), null, null));
#else
                DialogsManager.ShowDialog(null, new MessageDialog("Final Shader Error", e.Message, LanguageControl.Get("Usual", "ok"), null, null));
#endif
                IsEnabled = false;
                Log.Error(e);
            }
            ILTG.Statistics["Startup"] += Time.RealTime - start;
	    }
    
	    public static void Draw(Camera camera, int drawOrder)
	    {
		    if (BlockNextDraw || !ILTG.Settings.ShaderActivated || !ILTG.Settings.Postprocessing || !IsEnabled)
		    {
			    return;
		    }
		    
		    ViewWidget viewWidget = camera.GameWidget.ViewWidget;
		
		    ReadOnlyList<ComponentPlayer> players = m_subsystemPlayers.ComponentPlayers;
		    
		    // Force ViewWidget Render Target in Splitscreen, Bugfix
		    if (viewWidget.m_scalingRenderTarget == null && players.Count > 1)
		    {
		        viewWidget.m_globalColorTransform = new Color(255, 255, 255, 254);
		        viewWidget.SetupScalingRenderTarget();
		    }
		    
		   Point2 DesideredScreenSize = viewWidget.ScalingRenderTargetSize ?? new Point2((int) (viewWidget.ActualSize.X * viewWidget.GlobalTransform.Right.Length()), (int) (viewWidget.ActualSize.Y * viewWidget.GlobalTransform.Up.Length()));
		    
		   double renderTargetDuration = 0.0;
		   if (Screen == null || 
                Screen.Width != DesideredScreenSize.X || 
                Screen.Height != DesideredScreenSize.Y
            )
		    {
		        double renderTargetStart = Time.RealTime;
			    if (Screen != null)
			    {
				    Screen.Dispose();
			    }
			    Screen = new RenderTarget2D(DesideredScreenSize.X, DesideredScreenSize.Y, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);
			    renderTargetDuration = Time.RealTime - renderTargetStart;
		    }
		    
		    if (drawOrder == int.MinValue)
		    {
		        OldScissorRectangle = Display.ScissorRectangle;
		        OldRenderTarget = Display.RenderTarget;
		        Display.ScissorRectangle = OldScissorRectangle;
		        
		        Display.RenderTarget = Screen;
		        Display.Clear(Color.Black, 1f);
		    }
		    if (drawOrder == 1250)
		    {
		        double start = Time.RealTime;
		        Display.RenderTarget = OldRenderTarget;
		        Display.ScissorRectangle = OldScissorRectangle;
			    TexturedBatch2D texturedBatch2D = m_primitiveRender.TexturedBatch(Screen);
			    texturedBatch2D.QueueQuad(new Vector2(0f, 0f), new Vector2(DesideredScreenSize), 0f, new Vector2(0f, 0f), new Vector2(1f, 1f), Color.White);
			    if (viewWidget.ScalingRenderTargetSize == null) 
                    texturedBatch2D.TransformTriangles(viewWidget.GlobalTransform, texturedBatch2D.TriangleVertices.Count);
			    Display.DepthStencilState = DepthStencilState.None;
			    Display.RasterizerState = texturedBatch2D.RasterizerState;
			    Display.BlendState = BlendState.Opaque;
			    
			    if (camera is BasePerspectiveCamera)
				    m_finalShader.GetParameter("u_viewDir", true).SetValue(((BasePerspectiveCamera)camera).ViewDirection);
			    m_finalShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			    m_finalShader.GetParameter("u_rainIntensity", true).SetValue(m_subsystemSky.m_subsystemWeather.GlobalPrecipitationIntensity);
			    
			    m_finalShader.Texture = texturedBatch2D.Texture;
			    m_finalShader.SamplerState = texturedBatch2D.SamplerState;
			    m_finalShader.Transforms.World[0] = PrimitivesRenderer2D.ViewportMatrix();
			    texturedBatch2D.FlushWithCurrentStateAndShader(m_finalShader);
			    ILTG.Statistics["Post-processing"] = (Time.RealTime - start) + renderTargetDuration;
		    }
	    }
    }
}