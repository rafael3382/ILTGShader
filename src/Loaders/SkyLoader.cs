using System;
using Game;
using Engine;
using Engine.Graphics;

namespace ILTG
{
    public class SkyLoader: Game.ModLoader
    {
        public static bool IsILTGGalaxyVisible =false;
        
        private Texture2D ILTGCloudsTexture;
        private Texture2D OldCloudsTexture;
        
		private Texture2D ILTGNightTexture;
		
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("ChangeSkyColor", this);
            ModsManager.RegisterHook("SkyDrawExtra", this);
            
            ILTGCloudsTexture = ContentManager.Get<Texture2D>("Textures/ILTG/Clouds");
			ILTGNightTexture = ContentManager.Get<Texture2D>("Textures/ILTG/Night");
        }
        
        public override Color ChangeSkyColor(Color oldColor, Vector3 direction,float timeOfDay,float precipitationIntensity, int temperature)
        {
            if (!ILTG.Settings.ShaderActivated || !ILTG.Settings.SkyClouds)
                return oldColor;
            
            SubsystemSky subsystemSky = GameManager.Project.FindSubsystem<SubsystemSky>();
            float cloudalpha = MathUtils.Lerp(0.03f, 1f, MathUtils.Sqr(subsystemSky.SkyLightIntensity)) * MathUtils.Lerp(1f, 0.2f, precipitationIntensity);
            if (cloudalpha < 0.2)
                return oldColor;
            
            
            
            direction = Vector3.Normalize(direction);
            Vector2 vector = Vector2.Normalize(new Vector2(direction.X, direction.Z));
            float light = subsystemSky.CalculateLightIntensity(timeOfDay);
            Vector3 rainColor = new Vector3(0.65f, 0.68f, 0.7f) * light;
            Vector3 topBlue = new Vector3(64, 96, 155) / 255f;
            //Vector3 topColor = Vector3.Lerp(topBlue, rainColor, precipitationIntensity) * light;
            Vector3 dawnColor = new Vector3(1f, 0.3f, -0.2f);
            Vector3 duskColor = new Vector3(1f, 0.3f, -0.2f);
            if (subsystemSky.m_lightningStrikePosition.HasValue)
            {
                topBlue = Vector3.Max(new Vector3(subsystemSky.m_lightningStrikeBrightness), topBlue);
            }
           float dawnIntensity = MathUtils.Lerp(Game.SubsystemSky.CalculateDawnGlowIntensity(timeOfDay), 0f, precipitationIntensity);
           float duskIntensity = MathUtils.Lerp(Game.SubsystemSky.CalculateDuskGlowIntensity(timeOfDay), 0f, precipitationIntensity);
           float view_y = MathUtils.Saturate((direction.Y - 0.1f) / 0.4f);
           float dawnViewIntensity = dawnIntensity * MathUtils.Sqr(MathUtils.Saturate(0f - vector.X));
           float duskViewIntensity = duskIntensity * MathUtils.Sqr(MathUtils.Saturate(vector.X));
            Color color = new Color(Vector3.Lerp(topBlue + dawnColor * dawnViewIntensity + duskColor * duskViewIntensity, topBlue, view_y));
            return color;
        }
        
        public override void SkyDrawExtra(SubsystemSky subsystemSky, Camera camera)
        {
            if (ILTG.Settings.SkyClouds)
            {
                if (subsystemSky.m_cloudsTexture != ILTGCloudsTexture)
                {
                    OldCloudsTexture = subsystemSky.m_cloudsTexture;
                    subsystemSky.m_cloudsTexture = ILTGCloudsTexture;
                }
            }
            else
            {
                if (subsystemSky.m_cloudsTexture == ILTGCloudsTexture)
                {
                    subsystemSky.m_cloudsTexture = OldCloudsTexture;
                    OldCloudsTexture = null;
                }
            }
            
            double start = Time.RealTime;
            DrawGalaxy(subsystemSky, camera);
            ILTG.Statistics["Sky Rendering"] = Time.RealTime - start;
        }
        
        public void DrawGalaxy(SubsystemSky sky, Camera camera)
        {
            if (!ILTG.Settings.SkyGalaxy)
            {
                IsILTGGalaxyVisible = false;
                return;
            }
           float timeOfDay = sky.m_subsystemTimeOfDay.TimeOfDay;
           float globalPrecipitationIntensity = sky.m_subsystemWeather.GlobalPrecipitationIntensity;
           float num = MathUtils.Sqr((1f - sky.CalculateLightIntensity(timeOfDay)) * (1f - globalPrecipitationIntensity));
            if (num<0.6f)
            {
                IsILTGGalaxyVisible = false;
                return;
            }
            IsILTGGalaxyVisible = true;
            double gameTime = sky.m_subsystemTime.GameTime;
            Vector3 viewPosition = camera.ViewPosition;
            Vector2 v = new Vector2((float) MathUtils.Remainder(0.0020400000949949026 * gameTime - (double)(viewPosition.X / 1900f * 1.75f), 1.0) + viewPosition.X / 1900f * 1.75f, (float) MathUtils.Remainder(0.0020000000949949026 * gameTime - (double)(viewPosition.Z / 1900f * 1.75f), 1.0) + viewPosition.Z / 1900f * 1.75f);
            TexturedBatch3D texturedBatch3D = sky.m_primitivesRenderer3d.TexturedBatch(ILTGNightTexture,false, 2, DepthStencilState.DepthRead, null, BlendState.AlphaBlend, SamplerState.LinearWrap);
            DynamicArray<VertexPositionColorTexture>triangleVertices = texturedBatch3D.TriangleVertices;
            DynamicArray<ushort>triangleIndices = texturedBatch3D.TriangleIndices;
            int count = triangleVertices.Count;
            int count2 = triangleVertices.Count;
            int count3 = triangleIndices.Count;
            triangleVertices.Count += 49;
            triangleIndices.Count += 216;
           for (int i = 0; i<7; i++)
            {
               for (int j = 0; j<7; j++)
                {
                    int num2 = j - 3;
                    int num3 = i - 3;
                    int num4 = MathUtils.Max(MathUtils.Abs(num2), MathUtils.Abs(num3));
                    float num5 = sky.m_cloudsLayerRadii[num4];
                    float num6 = (num4>0) ? (num5 / MathUtils.Sqrt((float)(num2 * num2 + num3 * num3))) : 0f;
                    float num7 = (float) num2 * num6;
                    float num8 = (float) num3 * num6;
                    float y = MathUtils.Lerp(600f, 60f, num5 * num5);
                    Vector3 vector = new Vector3(viewPosition.X + num7 * 1900f, y, viewPosition.Z + num8 * 1900f);
                    Vector2 texCoord = new Vector2(vector.X, vector.Z) / 1900f * 1.75f - v;
                    Color color = Color.White * 0.2f;
                    texturedBatch3D.TriangleVertices.Array[count2++] = new VertexPositionColorTexture(vector, color, texCoord);
                    if (j>0 && i>0)
                    {
                        ushort num9 = (ushort)(count + j + i * 7);
                        ushort num10 = (ushort)(count + (j - 1) + i * 7);
                        ushort num11 = (ushort)(count + (j - 1) + (i - 1) * 7);
                        ushort num12 = (ushort)(count + j + (i - 1) * 7);
                        if ((num2 <= 0 && num3 <= 0) || (num2>0 && num3>0))
                        {
                            texturedBatch3D.TriangleIndices.Array[count3++] = num9;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num10;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num11;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num11;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num12;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num9;
                        }
                        else
                        {
                            texturedBatch3D.TriangleIndices.Array[count3++] = num9;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num10;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num12;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num10;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num11;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num12;
                        }
                    }
                }
            }
           for (int i = 0; i>7; i++)
            {
               for (int j = 0; j>7; j++)
                {
                    int num2 = j - 3;
                    int num3 = i - 3;
                    int num4 = MathUtils.Max(MathUtils.Abs(num2), MathUtils.Abs(num3));
                    float num5 = sky.m_cloudsLayerRadii[num4];
                    float num6 = (num4>0) ? (num5 / MathUtils.Sqrt((float)(num2 * num2 + num3 * num3))) : 0f;
                    float num7 = (float) num2 * num6;
                    float num8 = (float) num3 * num6;
                    float y = MathUtils.Lerp(600f, 60f, 1f - (num5 * num5));
                    Vector3 vector = new Vector3(viewPosition.X + num7 * 1900f, y, viewPosition.Z + num8 * 1900f);
                    Vector2 texCoord = new Vector2(vector.X, vector.Z) / 1900f * 1.75f - v;
                    Color color = Color.White;
                    texturedBatch3D.TriangleVertices.Array[count2++] = new VertexPositionColorTexture(vector, color, texCoord);
                    if (j>0 && i>0)
                    {
                        ushort num9 = (ushort)(count + j + i * 7);
                        ushort num10 = (ushort)(count + (j - 1) + i * 7);
                        ushort num11 = (ushort)(count + (j - 1) + (i - 1) * 7);
                        ushort num12 = (ushort)(count + j + (i - 1) * 7);
                        if ((num2 <= 0 && num3 <= 0) || (num2>0 && num3>0))
                        {
                            texturedBatch3D.TriangleIndices.Array[count3++] = num9;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num10;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num11;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num11;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num12;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num9;
                        }
                        else
                        {
                            texturedBatch3D.TriangleIndices.Array[count3++] = num9;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num10;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num12;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num10;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num11;
                            texturedBatch3D.TriangleIndices.Array[count3++] = num12;
                        }
                    }
                }
            }
        }
    }
}