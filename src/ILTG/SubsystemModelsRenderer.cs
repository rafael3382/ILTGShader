using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using TemplatesDatabase;

namespace ILTG
{
	public class SubsystemModelsRenderer : Game.SubsystemModelsRenderer, IDrawable
	{
		public static NModelShader m_NshaderOpaque;

		public static NModelShader m_NshaderAlphaTested;

		public Vector3 sunPos;

		public new SubsystemTimeOfDay m_subsystemTimeOfDay;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemShadows = base.Project.FindSubsystem<SubsystemShadows>(throwOnError: true);
			m_subsystemTimeOfDay = base.Project.FindSubsystem<SubsystemTimeOfDay>(throwOnError: true);
			int value = valuesDictionary.GetValue("maxInstancesCount", 14);
			m_NshaderOpaque = new NModelShader(useAlphaThreshold: false, value);
			m_NshaderAlphaTested = new NModelShader(useAlphaThreshold: true, value);
		}

		public new void Draw(Camera camera, int drawOrder)
		{
			float x = 2f * m_subsystemSky.m_subsystemTimeOfDay.TimeOfDay * MathUtils.PI + MathUtils.PI;
			sunPos = new Vector3
			{
				X = 0f - MathUtils.Sin(x),
				Y = 0f - MathUtils.Cos(x),
				Z = 0f
			};
			if (drawOrder == m_drawOrders[0])
			{
				ModelsDrawn = 0;
				List<ModelData>[] modelsToDraw = m_modelsToDraw;
				for (int i = 0; i < modelsToDraw.Length; i++)
				{
					modelsToDraw[i].Clear();
				}
				m_modelsToPrepare.Clear();
				foreach (ModelData value in m_componentModels.Values)
				{
					if (value.ComponentModel.Model != null)
					{
						value.ComponentModel.CalculateIsVisible(camera);
						if (value.ComponentModel.IsVisibleForCamera)
						{
							m_modelsToPrepare.Add(value);
						}
					}
				}
				m_modelsToPrepare.Sort();
				{
					foreach (ModelData item in m_modelsToPrepare)
					{
						PrepareModel(item, camera);
						m_modelsToDraw[(int)item.ComponentModel.RenderingMode].Add(item);
					}
					return;
				}
			}
			if (!Game.SubsystemModelsRenderer.DisableDrawingModels)
			{
				if (drawOrder == m_drawOrders[1])
				{
					Display.DepthStencilState = DepthStencilState.Default;
					Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
					Display.BlendState = BlendState.Opaque;
					NDrawModels(camera, m_modelsToDraw[0], null);
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					NDrawModels(camera, m_modelsToDraw[1], 0f);
					Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
					m_primitivesRenderer.Flush(camera.ProjectionMatrix, clearAfterFlush: true, 0);
				}
				else if (drawOrder == m_drawOrders[2])
				{
					Display.DepthStencilState = DepthStencilState.Default;
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					Display.BlendState = BlendState.AlphaBlend;
					NDrawModels(camera, m_modelsToDraw[2], null);
				}
				else if (drawOrder == m_drawOrders[3])
				{
					Display.DepthStencilState = DepthStencilState.Default;
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					Display.BlendState = BlendState.AlphaBlend;
					NDrawModels(camera, m_modelsToDraw[3], null);
					m_primitivesRenderer.Flush(camera.ProjectionMatrix);
				}
			}
			else
			{
				m_primitivesRenderer.Clear();
			}
		}

		public void NDrawModels(Camera camera, List<ModelData> modelsData, float? alphaThreshold)
		{
			NDrawInstancedModels(camera, modelsData, alphaThreshold);
			NDrawModelsExtras(camera, modelsData);
		}

		public void NDrawModelsExtras(Camera camera, List<ModelData> modelsData)
		{
			foreach (ModelData modelsDatum in modelsData)
			{
				if (modelsDatum.ComponentBody != null && modelsDatum.ComponentModel.CastsShadow)
				{
					Vector3 shadowPosition = modelsDatum.ComponentBody.Position + new Vector3(0f, 0.1f, 0f);
					BoundingBox boundingBox = modelsDatum.ComponentBody.BoundingBox;
					float shadowDiameter = 2.25f * (boundingBox.Max.X - boundingBox.Min.X);
					m_subsystemShadows.QueueShadow(camera, shadowPosition, shadowDiameter, modelsDatum.ComponentModel.Opacity ?? 1f);
				}
				modelsDatum.ComponentModel.DrawExtras(camera);
			}
		}

		public void NDrawInstancedModels(Camera camera, List<ModelData> modelsData, float? alphaThreshold)
		{
			NModelShader nModelShader = (alphaThreshold.HasValue ? m_NshaderAlphaTested : m_NshaderOpaque);
			nModelShader.GetParameter("u_time").SetValue(m_subsystemSky.m_subsystemTimeOfDay.TimeOfDay);
			nModelShader.LightDirection1 = 1.25f * Vector3.TransformNormal(sunPos, camera.ViewMatrix);
			nModelShader.LightDirection2 = -Vector3.TransformNormal(LightingManager.DirectionToLight2, camera.ViewMatrix);
			nModelShader.FogColor = new Vector3(m_subsystemSky.ViewFogColor);
			nModelShader.FogStartInvLength = new Vector2(m_subsystemSky.ViewFogRange.X, 1f / (m_subsystemSky.ViewFogRange.Y - m_subsystemSky.ViewFogRange.X));
			nModelShader.FogYMultiplier = m_subsystemSky.VisibilityRangeYMultiplier;
			nModelShader.WorldUp = Vector3.TransformNormal(Vector3.UnitY, camera.ViewMatrix);
			nModelShader.Transforms.View = Matrix.Identity;
			nModelShader.Transforms.Projection = camera.ProjectionMatrix;
			nModelShader.SamplerState = SamplerState.PointClamp;
			if (alphaThreshold.HasValue)
			{
				nModelShader.AlphaThreshold = alphaThreshold.Value;
			}
			foreach (ModelData modelsDatum in modelsData)
			{
				ComponentModel componentModel = modelsDatum.ComponentModel;
				Vector3 vector = (componentModel.DiffuseColor.HasValue ? componentModel.DiffuseColor.Value : Vector3.One);
				float num = (componentModel.Opacity.HasValue ? componentModel.Opacity.Value : 1f);
				nModelShader.InstancesCount = componentModel.AbsoluteBoneTransformsForCamera.Length;
				nModelShader.MaterialColor = new Vector4(vector * num, num);
				nModelShader.EmissionColor = (componentModel.EmissionColor.HasValue ? componentModel.EmissionColor.Value : Vector4.Zero);
				nModelShader.AmbientLightColor = new Vector3(LightingManager.LightAmbient * modelsDatum.Light);
				nModelShader.DiffuseLightColor1 = new Vector3(modelsDatum.Light);
				nModelShader.DiffuseLightColor2 = new Vector3(modelsDatum.Light);
				nModelShader.Texture = componentModel.TextureOverride;
				Array.Copy(componentModel.AbsoluteBoneTransformsForCamera, nModelShader.Transforms.World, componentModel.AbsoluteBoneTransformsForCamera.Length);
				InstancedModelData instancedModelData = InstancedModelsManager.GetInstancedModelData(componentModel.Model, componentModel.MeshDrawOrders);
				Display.DrawIndexed(PrimitiveType.TriangleList, nModelShader, instancedModelData.VertexBuffer, instancedModelData.IndexBuffer, 0, instancedModelData.IndexBuffer.IndicesCount);
				ModelsDrawn++;
			}
		}

		public Matrix CalculateBaseProjectionMatrix(Point2 ViewSize)
		{
			float num = 90f;
			float num2 = 0.9f;
			float num3 = ViewSize.X / ViewSize.Y;
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

		public Vector3? get_shadow(Camera camera, Vector3 shadowPosition, float shadowDiameter, float alpha)
		{
			return null;
		}
	}
}
