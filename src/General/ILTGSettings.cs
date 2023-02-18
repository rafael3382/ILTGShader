using Engine;
using Engine.Graphics;
using System.Reflection;
using System.Collections.Generic;
namespace ILTG
{
    public enum SettingsFor
    {
        Global,
        Screen_Capture,
        World
    }
    public class ILTGSettings
    {
        public Dictionary<string,object> BeforeDeactivation = null;
        public bool ShaderActivated { get; set; } = true;
        
        public bool BlockEffect { get; set; } = true;
        
        public bool BlockOrangeEffect { get; set; } = true;
        
        public bool BlockGammaContrast { get; set; } = true;
        
        public bool ToneMapping { get; set; } = false;
        
        public bool BlockVerticesModifier { get; set; } = true;
        
        public bool TorchLight { get; set; } = true;
        
        public bool DawnDuskLight { get; set; } = true;
        
        public bool GalaxyLight { get; set; } = false;
        
        public bool Postprocessing { get; set; } = true;
        
        public bool WaterActivated { get; set; } = true;
        
        public bool WaterReflections { get; set; } = true;
        
        public bool WaterRefraction { get; set; } = false;
        
        public bool WaterTexture { get; set; } = true;
        
        public bool WaterWaving { get; set; } = true;
        
        public float WaterOpacity { get; set; } = 0.5f;
        
        public float WaterReflectionOpacity { get; set; } = 0.6f;
        
        public bool RainReflections { get; set; } = true;
        
        public bool SkyGalaxy { get; set; } = true;
        
        public bool SkyClouds { get; set; } = true;
        
        public bool BestResults { get; set; } = false;
        
        public bool EntityShActivated { get; set; } = true;
        
        public bool DynamicDiffuse { get; set; } = true;
        
        public bool EntityShadows { get; set; } = true;
        
        public float EntityShadowOpacity { get; set; } = 0.18f;
        
        public Color BlockTone { get; set; } = new Color(0.83f, 0.754f, 0.75f);
        public float BlockExtraBrightness { get; set; } = 2.2f;
        
        public Color TorchColor { get; set; } = new Color(255, 193, 131);
        public float TorchExtraBrightness { get; set; } = 2.2f;
        
        public Color DawnDuskColor { get; set; } = new Color(1.0f, 0.6f, 0.2f);
        public float DawnDuskExtraBrightness { get; set; } = 2f;
        
        public Color RainColor { get; set; } = new Color(0.8f, 0.9f, 1f);
        
        public Color GalaxyColor { get; set; } = new Color(25, 23, 85);
        public float GalaxyExtraBrightness { get; set; } = 5.4f;
    }
}