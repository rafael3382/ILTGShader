using Game;
using Engine;
using Engine.Graphics;
using System.IO;
using System.Linq;
using System;

namespace ILTG 
{
    public static class ShaderReader 
    {
        public static string GetShader(string ShaderName)
         {
#if DEVMODE
            return new StreamReader(Storage.OpenFile(Storage.CombinePaths(ModsManager.ExternelPath, "/_MyMods/ILTGShader-Reborn/Packing/Assets/ILTG/", ShaderName),OpenFileMode.Read)).ReadToEnd();
#else
            string shaderCode = ContentManager.Get<string>("ILTG/"+Storage.GetFileNameWithoutExtension(ShaderName), ShaderName.EndsWith(".vsh") ? ".vsh" : ".psh");

            if (IsDesktop)
            {
                shaderCode = shaderCode
                    .Replace("highp ", "")
                    .Replace("mediump ", "")
                    .Replace("lowp ", "")
                    .Replace("#extension GL_OES_standard_derivatives : enable", "");
           }
           return shaderCode;
#endif
        }
        
        public static Shader UseShader(string shaderName, params ShaderMacro[] shaderMacros)
        {
            return new Shader(
                GetShader(shaderName+".vsh", ILTG.Settings.ToMacros()),
                GetShader(shaderName+".psh", ILTG.Settings.ToMacros()),
                shaderMacros
            );
        }
        
        public static string GetShader(string ShaderName, ShaderMacro[] shaderMacros)
        {
            string code = "";
            
            // Macro Code
            foreach (ShaderMacro shaderMacro in shaderMacros)
                code += "#define " + shaderMacro.Name + " " + shaderMacro.Value + "\n";
            
            // Fix Lines
            code += "#line 1\n";
            
            // Shader Code
            code += GetShader(ShaderName);
            
            return code;      
        }
        
        public static Texture2D GetImage(string ImageName)
        {
            return ContentManager.Get<Texture2D>("ILTG/Textures/"+ImageName);
        }
        
        
        public static bool IsDesktop
        {
            get
            {
                return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Mono.Android") == null;
            }
        }
    }
}