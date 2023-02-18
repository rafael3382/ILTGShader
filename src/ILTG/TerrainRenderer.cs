using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;

namespace ILTG
{
	public class TerrainRenderer : Game.TerrainRenderer
	{
		public struct ShaderChunk
		{
			public Texture2D TopmostBlocks;

			public Point2 Coords;

			public int ModificationCount;
		}

		public static bool VerticalShadows = true;

		public Dictionary<Point2, ShaderChunk> ShaderChunks = new Dictionary<Point2, ShaderChunk>();

		public Dictionary<Point2, ShaderChunk> OldShaderChunks = new Dictionary<Point2, ShaderChunk>();

		public Texture2D shaderTexture;

		public bool Setted;

		private Shader m_shadowShader;

		private Texture2D depthMap;

		private Texture2D normalMap;

		private Texture2D EmptyScreen;

		public static bool is_reflection;

		public DynamicArray<TerrainChunk> m_shadowChunksToDraw = new DynamicArray<TerrainChunk>();

		public SamplerState m_samplerState = new SamplerState
		{
			AddressModeU = TextureAddressMode.Clamp,
			AddressModeV = TextureAddressMode.Clamp,
			FilterMode = TextureFilterMode.Point,
			MaxLod = 0f
		};

		public SamplerState m_samplerStateMips = new SamplerState
		{
			AddressModeU = TextureAddressMode.Clamp,
			AddressModeV = TextureAddressMode.Clamp,
			FilterMode = TextureFilterMode.PointMipLinear,
			MaxLod = 4f
		};

		public SamplerState m_reflectionSampler = new SamplerState
		{
			AddressModeU = TextureAddressMode.Clamp,
			AddressModeV = TextureAddressMode.Clamp,
			FilterMode = TextureFilterMode.Point,
			MaxLod = 25f
		};

		public static DynamicArray<int> m_tmpIndices = new DynamicArray<int>();

		public static bool DrawChunksMap;

		public static int ChunksDrawn;

		public static int ChunkDrawCalls;

		public static int ChunkTrianglesDrawn;

		public UpdateOrder UpdateOrder => UpdateOrder.Terrain;

		public void Update(float dt)
		{
		}

		public void RegenerateTopmost(TerrainChunk terrainChunk)
		{
			if (!VerticalShadows || (ShaderChunks.ContainsKey(terrainChunk.Coords) && terrainChunk.ModificationCounter == ShaderChunks[terrainChunk.Coords].ModificationCount))
			{
				return;
			}
			Image image = GenerateHeightMap(terrainChunk);
			ShaderChunk value = default(ShaderChunk);
			if (!OldShaderChunks.ContainsKey(terrainChunk.Coords))
			{
				value.TopmostBlocks = Texture2D.Load(image);
			}
			else
			{
				OldShaderChunks[terrainChunk.Coords].TopmostBlocks.SetData(0, image.Pixels);
				value.TopmostBlocks = OldShaderChunks[terrainChunk.Coords].TopmostBlocks;
			}
			value.ModificationCount = terrainChunk.ModificationCounter;
			value.Coords = terrainChunk.Coords;
			if (ShaderChunks.ContainsKey(terrainChunk.Coords))
			{
				ShaderChunks[terrainChunk.Coords] = value;
			}
			else
			{
				ShaderChunks.Add(terrainChunk.Coords, value);
			}
		}
		
		public Image GenerateChunkMap(TerrainChunk terrainChunk)
		{
		    Image image = new Image(512, 16);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < 128; k += 4)
					{
						int num = Terrain.ExtractContents(terrainChunk.GetCellValueFast(TerrainChunk.CalculateCellIndex(i, k, j)));
						int num2 = Terrain.ExtractContents(terrainChunk.GetCellValueFast(TerrainChunk.CalculateCellIndex(i, k + 1, j)));
						int num3 = Terrain.ExtractContents(terrainChunk.GetCellValueFast(TerrainChunk.CalculateCellIndex(i, k + 2, j)));
						int num4 = Terrain.ExtractContents(terrainChunk.GetCellValueFast(TerrainChunk.CalculateCellIndex(i, k + 3, j)));
						if (num > 255) num = 0;
						if (num2 > 255) num2 = 0;
						if (num3 > 255) num3 = 0;
						if (num4 > 255) num4 = 0;
						image.SetPixel(i + k * 16 / 4, j, new Color((byte)num, (byte)num2, (byte)num3, (byte)num4));
					}
				}
			}
			return image;
		}
		
		public Image GenerateHeightMap(TerrainChunk terrainChunk)
		{
		    Image image = new Image(16, 16);
			for (int l = 0; l < 16; l++)
			{
				for (int m = 0; m < 16; m++)
				{
					int num5 = terrainChunk.GetTopHeightFast(l, m);
					int num6 = Terrain.ExtractContents(terrainChunk.GetCellValueFast(TerrainChunk.CalculateCellIndex(l, num5, m)));
					if (num6 > 255)
					{
						num6 = 0;
					}
					int num7 = 0;
					if (BlocksManager.Blocks[num6] is FourLedBlock)
					{
						num7 = FourLedBlock.GetColor(Terrain.ExtractData(terrainChunk.GetCellValueFast(TerrainChunk.CalculateCellIndex(l, num5, m)))) + 1;
					}
					image.SetPixel(l, m, new Color((byte)num5, (byte)num6, (byte)num7, (byte)0));
				}
			}
			return image;
		}

		public void FastRefresh(TerrainChunk[] Chunks)
		{
			for (int i = 0; i < Chunks.Length; i++)
			{
				TerrainChunk terrainChunk = Chunks[i];
				if (OldShaderChunks.ContainsKey(terrainChunk.Coords))
				{
					if (OldShaderChunks[terrainChunk.Coords].ModificationCount < terrainChunk.ModificationCounter)
					{
						RegenerateTopmost(terrainChunk);
					}
					else
					{
						ShaderChunks.Add(terrainChunk.Coords, OldShaderChunks[terrainChunk.Coords]);
					}
				}
				else
				{
					RegenerateTopmost(Chunks[i]);
				}
			}
		}

		public void UpdateShaderChunks()
		{
			OldShaderChunks = new Dictionary<Point2, ShaderChunk>(ShaderChunks);
			ShaderChunks.Clear();
			TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
			if (allocatedChunks.Length != OldShaderChunks.Count)
			{
			    OldShaderChunks.Clear();
				FastRefresh(allocatedChunks);
				return;
			}
			foreach (TerrainChunk terrainChunk in allocatedChunks)
			{
				if (!OldShaderChunks.ContainsKey(terrainChunk.Coords))
				{
					RegenerateTopmost(terrainChunk);
				}
				else if (OldShaderChunks[terrainChunk.Coords].ModificationCount < terrainChunk.ModificationCounter || terrainChunk.NewGeometryData)
				{
					RegenerateTopmost(terrainChunk);
				}
				else
				{
					ShaderChunks.Add(terrainChunk.Coords, OldShaderChunks[terrainChunk.Coords]);
				}
			}
			OldShaderChunks.Clear();
		}

		public TerrainRenderer(SubsystemTerrain subsystemTerrain) : base(subsystemTerrain)
		{
			Display.DeviceReset += Display_DeviceReset;
			try
			{
			    OpaqueShader = ShaderReader.UseShader("Opaque");
			    AlphatestedShader = ShaderReader.UseShader("AlphaTested");
			    TransparentShader = ShaderReader.UseShader("Transparent");
			    m_shadowShader = ShaderReader.UseShader("Opaque");
			}
			catch (Exception ex)
			{
			    Log.Error(ex.Message);
			    throw;
            }
            // Get Textures
			shaderTexture = ShaderReader.GetImage("shader_tex");
			depthMap = ShaderReader.GetImage("DepthMap");
			normalMap = ShaderReader.GetImage("NormalMap");
			
			// Set Textures
			OpaqueShader.GetParameter("u_depthMap", true).SetValue(depthMap);
			OpaqueShader.GetParameter("u_normalMap", true).SetValue(normalMap);
			OpaqueShader.GetParameter("u_depthMapState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			OpaqueShader.GetParameter("u_normalMapState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
		}

		public void PrepareForDrawing(Camera camera)
		{
			if (VerticalShadows)
			{
				try
				{
					UpdateShaderChunks();
				}
				catch
				{
				}
			}
			base.PrepareForDrawing(camera);
		}

		public void DrawOpaque(Camera camera)
		{
			int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
			Vector3 viewPosition = camera.ViewPosition;
			Vector3 vector = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(vector - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			Display.BlendState = BlendState.Opaque;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			OpaqueShader.GetParameter("u_origin", true).SetValue(vector.XZ);
			OpaqueShader.GetParameter("u_viewProjectionMatrix", true).SetValue(value);
			OpaqueShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
			OpaqueShader.GetParameter("u_samplerState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			OpaqueShader.GetParameter("u_fogYMultiplier", true).SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
			OpaqueShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			OpaqueShader.GetParameter("u_lightningStrikeBrightness", true).SetValue(m_subsystemSky.m_lightningStrikeBrightness);
			OpaqueShader.GetParameter("u_time", true).SetValue(m_subsystemSky.m_subsystemTimeOfDay.TimeOfDay);
			OpaqueShader.GetParameter("u_waterDepth", true).SetValue(m_subsystemSky.ViewUnderWaterDepth);
			try
			{
				OpaqueShader.GetParameter("u_screenSize", true).SetValue(new Vector2(Display.Viewport.Width, Display.Viewport.Height));
			}
			catch
			{
			}
			if (camera is BasePerspectiveCamera)
			{
				try
				{
					OpaqueShader.GetParameter("u_viewDir", true).SetValue(((BasePerspectiveCamera)camera).ViewDirection);
				}
				catch
				{
				}
			}
			OpaqueShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			ShaderParameter parameter = OpaqueShader.GetParameter("u_fogStartInvLength");
			for (int i = 0; i < m_chunksToDraw.Count; i++)
			{
				TerrainChunk terrainChunk = m_chunksToDraw[i];
				if (VerticalShadows)
				{
					try
					{
						ShaderChunk shaderChunk = ShaderChunks[terrainChunk.Coords];
						OpaqueShader.GetParameter("u_worldTopSampler", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
						OpaqueShader.GetParameter("u_worldTopMap", true).SetValue(shaderChunk.TopmostBlocks);
					}
					catch
					{
					}
				}
				float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
				float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
				parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
				int num3 = 16;
				if (viewPosition.Z > terrainChunk.BoundingBox.Min.Z)
				{
					num3 |= 1;
				}
				if (viewPosition.X > terrainChunk.BoundingBox.Min.X)
				{
					num3 |= 2;
				}
				if (viewPosition.Z < terrainChunk.BoundingBox.Max.Z)
				{
					num3 |= 4;
				}
				if (viewPosition.X < terrainChunk.BoundingBox.Max.X)
				{
					num3 |= 8;
				}
				DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk, num3);
				ChunksDrawn++;
			}
		}

		public void DrawAlphaTested(Camera camera)
		{
			int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
			Vector3 viewPosition = camera.ViewPosition;
			Vector3 vector = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(vector - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			Display.BlendState = BlendState.Opaque;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			AlphatestedShader.GetParameter("u_origin", true).SetValue(vector.XZ);
			AlphatestedShader.GetParameter("u_viewProjectionMatrix", true).SetValue(value);
			AlphatestedShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
			AlphatestedShader.GetParameter("u_samplerState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			AlphatestedShader.GetParameter("u_fogYMultiplier", true).SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
			AlphatestedShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			AlphatestedShader.GetParameter("u_time", true).SetValue(m_subsystemSky.m_subsystemTimeOfDay.TimeOfDay);
			AlphatestedShader.GetParameter("u_waterDepth", true).SetValue(m_subsystemSky.ViewUnderWaterDepth);
			AlphatestedShader.GetParameter("u_lightningStrikeBrightness", true).SetValue(m_subsystemSky.m_lightningStrikeBrightness);
			AlphatestedShader.GetParameter("u_screenSize", true).SetValue(new Vector2(Display.Viewport.Width, Display.Viewport.Height));
			ComponentCreatureModel componentCreatureModel = camera.GameWidget.Target.ComponentCreatureModel;
			Vector3 value2 = (componentCreatureModel.m_componentCreature.ComponentBody.Rotation * Quaternion.CreateFromYawPitchRoll(0f - componentCreatureModel.m_componentCreature.ComponentLocomotion.LookAngles.X, componentCreatureModel.m_componentCreature.ComponentLocomotion.LookAngles.Y, 0f)).ToYawPitchRoll();
			AlphatestedShader.GetParameter("u_viewDir", true).SetValue(value2);
			ShaderParameter parameter = AlphatestedShader.GetParameter("u_fogStartInvLength");
			for (int i = 0; i < m_chunksToDraw.Count; i++)
			{
				TerrainChunk terrainChunk = m_chunksToDraw[i];
				float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
				float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
				parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
				int subsetsMask = 32;
				DrawTerrainChunkGeometrySubsets(AlphatestedShader, terrainChunk, subsetsMask);
			}
		}

		public void DrawTransparent(Camera camera)
		{
			int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
			Vector3 viewPosition = camera.ViewPosition;
			Vector3 vector = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
			Matrix worldViewMatrix = Matrix.CreateTranslation(vector - viewPosition) * camera.ViewMatrix.OrientationMatrix;
			Display.BlendState = BlendState.AlphaBlend;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = ((m_subsystemSky.ViewUnderWaterDepth > 0f) ? RasterizerState.CullClockwiseScissor : RasterizerState.CullCounterClockwiseScissor);
			TransparentShader.GetParameter("u_origin", true).SetValue(vector.XZ);
			TransparentShader.GetParameter("u_viewMatrix", true).SetValue(worldViewMatrix);
            TransparentShader.GetParameter("u_projectionMatrix", true).SetValue(camera.ProjectionMatrix);
            TransparentShader.GetParameter("u_projectionMatrixInverted", true).SetValue(Matrix.Invert(camera.ProjectionMatrix));
			TransparentShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
			TransparentShader.GetParameter("u_fogYMultiplier", true).SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
			TransparentShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			try
			{
				TransparentShader.GetParameter("u_time", true).SetValue(m_subsystemSky.m_subsystemTimeOfDay.TimeOfDay);
				TransparentShader.GetParameter("u_waterDepth", true).SetValue(m_subsystemSky.ViewUnderWaterDepth);
			}
			catch
			{
			}
			TransparentShader.GetParameter("u_screen", true).SetValue(Postprocessing.Screen);
			TransparentShader.GetParameter("u_screenSampler", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			if (camera is BasePerspectiveCamera baseCamera)
			{
				TransparentShader.GetParameter("u_viewDir", true).SetValue(baseCamera.ViewDirection);
			}
			TransparentShader.GetParameter("u_screenSize", true).SetValue(new Vector2(Display.Viewport.Width, Display.Viewport.Height));
			TransparentShader.GetParameter("u_shaderTex", true).SetValue(shaderTexture);
			TransparentShader.GetParameter("u_shaderTexSampler", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			ShaderParameter parameter = TransparentShader.GetParameter("u_fogStartInvLength");
			for (int i = 0; i < m_chunksToDraw.Count; i++)
			{
				TerrainChunk terrainChunk = m_chunksToDraw[i];
				float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
				float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
				parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
				DrawTerrainChunkGeometrySubsets(TransparentShader, terrainChunk, 64, ApplyTexture: false);
			}
		}

		public void Dispose()
		{
			depthMap.Dispose();
			shaderTexture.Dispose();
			normalMap.Dispose();
			m_chunksToDraw.Clear();
			ShaderChunks.Clear();
			OldShaderChunks.Clear();
			//Utilities.Dispose(ref SubsystemReflections.Screen);
			TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
			foreach (TerrainChunk terrainChunk in allocatedChunks)
			{
				DisposeTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
			}
			Display.DeviceReset -= Display_DeviceReset;
	    }
	}
}
