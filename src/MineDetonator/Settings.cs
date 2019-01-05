using System.Windows.Forms;
using SharpDX;
using PoeHUD.Plugins;
using PoeHUD.Hud.Settings;

namespace MineDetonator
{
    using Newtonsoft.Json;

    //All properties and public fields of this class will be saved to file
	public class Settings : SettingsBase
	{
		public Settings()
		{
			Enable = true;
		}

	    [Menu("Detonate Key")]
	    public HotkeyNode DetonateKey { get; set; } = new HotkeyNode(Keys.D);

	    [Menu("Detonate Delay")]
	    public RangeNode<int> DetonateDelay { get; set; } = new RangeNode<int>(300, 100, 2000);

		[Menu("Detonate Dist", 1)]
		public RangeNode<float> DetonateDist { get; set; } = new RangeNode<float>(75, 10, 200);

	    [Menu("Current detected area percent (from skill stats)", 2, 1), JsonIgnore]
	    public TextNode CurrentAreaPct { get; set; } = new TextNode("%NoData%");

	    [Menu("Debug: Last trigger reason"), JsonIgnore]
	    public TextNode TriggerReason { get; set; } = new TextNode();

	    [Menu("Filter empty action (Shaper only!)")]
	    public ToggleNode FilterNullAction { get; set; } = new ToggleNode(false);

	    [Menu("Debug"), JsonIgnore]
	    public ToggleNode Debug { get; set; } = new ToggleNode(false);
	}
}
