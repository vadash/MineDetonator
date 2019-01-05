using PoeHUD.Plugins;

namespace MineDetonator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PoeHUD.Models;
    using PoeHUD.Models.Enums;
    using PoeHUD.Poe;
    using PoeHUD.Poe.Components;
    using SharpDX;

    public class Core : BaseSettingsPlugin<Settings>
    {
        public Core()
        {
            PluginName = "Mines Detonator";
        }

        private DateTime LastDetonTime;

        public override void Render()
        {
            if (!GameController.InGame)
                return;
            var actor = GameController.Player.GetComponent<Actor>();
            var deployedObjects = actor.DeployedObjects.Where(x => x.Entity != null && x.Entity.Path == "Metadata/MiscellaneousObjects/RemoteMine").ToList();

            var realRange = Settings.DetonateDist.Value;
            var CGSkill = actor.ActorSkills.Find(x => x.Name == "GlacialCascade");
            if (CGSkill != null)
            {
                if (CGSkill.Stats.TryGetValue(GameStat.TotalSkillAreaOfEffectPctIncludingFinal, out var areaPct))
                {
                    realRange = realRange + realRange * areaPct / 100f;
                    var text = (100 + areaPct) + "%";
                    Settings.CurrentAreaPct.Value = $"Skill dist: {text} ({realRange}). "; 
                }
                else
                {
                    Settings.CurrentAreaPct.Value = "100%";
                }
            }
            else
            {
                Settings.CurrentAreaPct.Value = "Skill GlacialCascade not found!";
            }


            if (deployedObjects.Count == 0)
            {
                return;
            }

            var playerPos = deployedObjects.Last().Entity.PositionedComp.GridPos;// GameController.Player.PositionedComp.GridPos;

            Monsters = Monsters.Where(x => x.IsAlive).ToList();

            var nearMonsters = Monsters.Where(x => x != null &&
	            !x.GetComponent<Life>().HasBuff("hidden_monster") && 
	            !x.GetComponent<Life>().HasBuff("avarius_statue_buff") && 
				!x.GetComponent<Life>().HasBuff("hidden_monster_disable_minions") &&
                FilterNullAction(x.GetComponent<Actor>()) &&
				x.GetComponent<Actor>().CurrentAction?.Skill?.Name != "AtziriSummonDemons" &&
				x.GetComponent<Actor>().CurrentAction?.Skill?.Id != 728 &&//Lab?
	            Vector2.Distance(playerPos, x.PositionedComp.GridPos) < realRange).ToList();

            if (nearMonsters.Count == 0)
                return;

	        Settings.TriggerReason = "Path: " + nearMonsters[0].Path;

            if(Settings.Debug.Value)
			    LogMessage($"Ents: {nearMonsters.Count}. Last: {nearMonsters[0].Path}", 2);

            if ((DateTime.Now - LastDetonTime).TotalMilliseconds > Settings.DetonateDelay.Value)
            {
                if (deployedObjects.Any(x => x.Entity != null && x.Entity.IsValid && x.Entity.GetComponent<Stats>().StatDictionary[(int) GameStat.CannotDie] == 0))
                {
                    LastDetonTime = DateTime.Now;
                    Keyboard.KeyPress(Settings.DetonateKey.Value);
                }
            }
        }

        private List<EntityWrapper> Monsters = new List<EntityWrapper>();

        #region Overrides of BasePlugin

        public override void EntityAdded(EntityWrapper entityWrapper)
        {
            if (entityWrapper == null)
                return;

            if (!entityWrapper.HasComponent<Monster>())
                return;

            if (!entityWrapper.IsAlive)
                return;

            if (!entityWrapper.IsHostile)
                return;

	        if (entityWrapper.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/BetrayalTaserNet") ||
	            entityWrapper.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/BetrayalUpgrades/UnholyRelic") ||
	            entityWrapper.Path.StartsWith("Metadata/Monsters/LeagueBetrayal/BetrayalUpgrades/BetrayalDaemonSummonUnholyRelic"))
		        return;

            Monsters.Add(entityWrapper);
        }

        private bool FilterNullAction(Actor actor)
        {
            if (Settings.FilterNullAction.Value)
                return actor.CurrentAction != null;

            return true;
        }

        public override void EntityRemoved(EntityWrapper entityWrapper)
        {
            if (entityWrapper == null)
                return;

            if (!entityWrapper.HasComponent<Monster>())
                return;

            Monsters.Remove(entityWrapper);
        }

        #endregion
    }
}
