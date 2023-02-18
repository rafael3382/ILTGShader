using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;
using Game;


namespace ILTG
{
    public class PostprocessingLoader : ModLoader, IDrawable
    {
        public static PostprocessingLoader It;
        
        public int[] DrawOrders => new int[2]
        {
            int.MinValue,
			1250
        };
        
        
        public override void __ModInitialize()
        {
            It = this;
            ModsManager.RegisterHook("OnProjectLoaded", this);
        }
        
        public override void OnProjectLoaded(Project project)
        {
            try
            {
                SubsystemDrawing subsystemDrawing = project.FindSubsystem<SubsystemDrawing>();
                subsystemDrawing.AddDrawable(this);
                
                Postprocessing.Load(project);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        
        public void Draw(Camera camera, int drawOrder)
        {
            try
            {
                Postprocessing.Draw(camera, drawOrder);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
