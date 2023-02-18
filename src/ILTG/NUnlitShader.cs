using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace ILTG
{
	public class NUnlitShader : Shader
	{
		public SubsystemTimeOfDay m_subsystemTimeOfDay;

		public ShaderParameter m_worldViewProjectionMatrixParameter;

		public ShaderParameter m_textureParameter;

		public ShaderParameter m_samplerStateParameter;

		public ShaderParameter m_colorParameter;

		public ShaderParameter m_alphaThresholdParameter;

		public readonly ShaderTransforms Transforms;

		public Texture2D Texture
		{
			set
			{
				m_textureParameter.SetValue(value);
			}
		}

		public SamplerState SamplerState
		{
			set
			{
				m_samplerStateParameter.SetValue(value);
			}
		}

		public Vector4 Color
		{
			set
			{
				m_colorParameter.SetValue(value);
			}
		}

		public float AlphaThreshold
		{
			set
			{
				m_alphaThresholdParameter.SetValue(value);
			}
		}

		public NUnlitShader(bool useVertexColor, bool useTexture, bool useAlphaThreshold)
			: base(ShaderReader.GetShader("Unlit.vsh"), ShaderReader.GetShader("Unlit.psh"), PrepareShaderMacros(useVertexColor, useTexture, useAlphaThreshold))
		{
			m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
			m_textureParameter = GetParameter("u_texture", allowNull: true);
			m_samplerStateParameter = GetParameter("u_samplerState", allowNull: true);
			m_colorParameter = GetParameter("u_color", allowNull: true);
			m_alphaThresholdParameter = GetParameter("u_alphaThreshold", allowNull: true);
			Transforms = new ShaderTransforms(1);
			Color = Vector4.One;
		}

		public void SetTimeOfDay(SubsystemTimeOfDay p_subsystemTimeOfDay)
		{
			m_subsystemTimeOfDay = p_subsystemTimeOfDay;
		}

		protected override void PrepareForDrawingOverride()
		{
			Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
			m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
			if (m_subsystemTimeOfDay != null)
			{
				try
				{
					GetParameter("u_time").SetValue(m_subsystemTimeOfDay.TimeOfDay);
				}
				catch
				{
				}
			}
		}

		public static ShaderMacro[] PrepareShaderMacros(bool useVertexColor, bool useTexture, bool useAlphaThreshold)
		{
			List<ShaderMacro> list = new List<ShaderMacro>();
			if (useVertexColor)
			{
				list.Add(new ShaderMacro("USE_VERTEXCOLOR"));
			}
			if (useTexture)
			{
				list.Add(new ShaderMacro("USE_TEXTURE"));
			}
			if (useAlphaThreshold)
			{
				list.Add(new ShaderMacro("USE_ALPHATHRESHOLD"));
			}
			return list.ToArray();
		}
	}
}
