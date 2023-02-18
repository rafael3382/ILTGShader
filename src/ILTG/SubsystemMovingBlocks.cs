using Game;
using TemplatesDatabase;

namespace ILTG
{
	public class SubsystemMovingBlocks : Game.SubsystemMovingBlocks, IUpdateable, IDrawable
	{
		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_shader = ShaderReader.UseShader("AlphaTested");
		}

		public new void Draw(Camera camera, int drawOrder)
		{
			m_shader.GetParameter("u_time").SetValue(m_subsystemSky.m_subsystemTimeOfDay.TimeOfDay);
			m_shader.GetParameter("u_waterDepth").SetValue(m_subsystemSky.ViewUnderWaterDepth);
			base.Draw(camera, drawOrder);
		}
	}
}
