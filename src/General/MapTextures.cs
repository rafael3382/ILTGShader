using System;
using Engine;
using Engine.Audio;
using System.Reflection;
using OpenTK.Audio.OpenAL;

namespace ILTG
{
	public static class MapTextures
	{
	    public static SubsystemTerrain m_subsystemTerrain;
	    
		public static void Load(Project project)
		{
			m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>();
		}

		public static float ToEnginePitch(float pitch)
		{
			return MathUtils.Pow(2f, pitch);
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
	}
}
