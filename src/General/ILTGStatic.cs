using Game;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace ILTG
{
    internal static class ILTG
    {
        internal static ILTGSettings Settings
        {
            get
            {
                if (ScreenCaptureManager.m_captureRequested)
                    return ScreenCaptureSettings;
               if (WorldSettings != null) return WorldSettings;
               return GlobalSettings;
            }
       }
       
       internal static ILTGSettings GlobalSettings = new ILTGSettings();
       
       internal static ILTGSettings WorldSettings = null;
       
       internal static ILTGSettings ScreenCaptureSettings = new ILTGSettings();
       
       public static Dictionary<string, double> Statistics = new Dictionary<string, double>()
       {
           {"Block Vertices Modifier", 0.0},
           {"Post-processing", 0.0},
           {"Sky Rendering", 0.0},
           {"Water Rendering", 0.0},
           {"Entities Rendering", 0.0},
           {"Entity Shadow Rendering", 0.0},
           {"Startup", 0.0}
       };
       
        public static string str(float f) => f.ToString("0.00000000").TrimEnd('0');
        
        public static ShaderMacro[] ToMacros(this ILTGSettings settings)
        {
            List<ShaderMacro> macros = new List<ShaderMacro>();
            foreach (PropertyInfo prop in settings.GetType().GetProperties())
            {
                string simpleName = prop.PropertyType.FullName;
                if (!(simpleName.StartsWith("Engine.") || simpleName.StartsWith("System.") || simpleName.StartsWith("ILTG.")))
                    continue;
                simpleName = simpleName.Split('.').Last().
                Replace("Single", "float").
                Replace("Int32", "int").
                Replace("Int64", "long").
                Replace("Boolean", "bool").
                Replace("String", "string");
                
                object pValue = prop.GetValue(settings);
                
                switch (simpleName)
                {
                    case "float":
                        macros.Add(new ShaderMacro("cfg_"+prop.Name, str((float) pValue)));
                        break;
                    
                    case "int":
                        macros.Add(new ShaderMacro("cfg_"+prop.Name, pValue.ToString()));
                        break;
                    
                    case "bool":
                        if ((bool) pValue)
                          macros.Add(new ShaderMacro("cfg_"+prop.Name));
                        break;
                        
                    case "string":
                        string value = pValue as string;
                        value = value.Replace(" ", "_").ToLowerInvariant();
                        
                        macros.Add(new ShaderMacro("cfg_"+prop.Name + "_" + value));
                        break;
                     
                    case "Vector2":
                        Vector2 vec2 = (Vector2) pValue;
                        macros.Add(new ShaderMacro("cfg_"+prop.Name, $"vec2({str(vec2.X)}, {str(vec2.Y)})"));
                        break;
                        
                    case "Vector3":
                        Vector3 vec3 = (Vector3) pValue;
                        macros.Add(new ShaderMacro("cfg_"+prop.Name, $"vec3({str(vec3.X)}, {str(vec3.Y)}, {str(vec3.Z)})"));
                        break;
                        
                    case "Vector4":
                        Vector4 vec4 = (Vector4) pValue;
                        vec4.X = (float) typeof(Vector4).GetField("X").GetValue(vec4);
                        vec4.Y = (float) typeof(Vector4).GetField("Y").GetValue(vec4);
                        vec4.Z = (float) typeof(Vector4).GetField("Z").GetValue(vec4);
                        vec4.W = (float) typeof(Vector4).GetField("W").GetValue(vec4);
                        
                        macros.Add(new ShaderMacro("cfg_"+prop.Name, $"vec4({str(vec4.X)}, {str(vec4.Y)}, {str(vec4.Z)}, {str(vec4.W)})"));
                        break;
                        
                    case "Color":
                        Vector4 col4 = new Vector4((Color) pValue);
                        col4.X = (float) typeof(Vector4).GetField("X").GetValue(col4);
                        col4.Y = (float) typeof(Vector4).GetField("Y").GetValue(col4);
                        col4.Z = (float) typeof(Vector4).GetField("Z").GetValue(col4);
                        col4.W = (float) typeof(Vector4).GetField("W").GetValue(col4);
                        
                        macros.Add(new ShaderMacro("cfg_"+prop.Name, $"vec4({str(col4.X)}, {str(col4.Y)}, {str(col4.Z)}, {str(col4.W)})"));
                        break;
                }
            }
            return macros.ToArray();
        }
    }
}