﻿using ZPBot.SilkroadSecurityApi;

// ReSharper disable once CheckNamespace
namespace ZPBot.Common
{
    internal partial class GlobalManager
    {
        private static void SkillCast(Packet packet)
        {
            packet.ReadUInt8(); //type 1 = Start 2 = Stop
            var queue = packet.ReadUInt8();

            Game.AllowCast = queue <= 1;
        }

        private void SkillRegister(Packet packet)
        {
            var worldId = packet.ReadUInt32();

            if (worldId == Player.WorldId)
            {
                var skillId = packet.ReadUInt32();
                var skillWorldId = packet.ReadUInt32();

                var skill = Silkroad.GetSkillById(skillId);
                skill.WorldId = skillWorldId;
                SkillManager.RegBuff(skill);
            }
        }

        private void SkillUnRegister(Packet packet)
        {
            var amount = packet.ReadUInt8();

            for (var i = 0; i < amount; i++)
            {
                var worldId = packet.ReadUInt32();
                SkillManager.UnRegBuff(worldId);
            }
        }

        private void UpdateSkill(Packet packet)
        {
            var newSkill = packet.ReadUInt8();
            if (newSkill != 1) return;

            var skillId = packet.ReadUInt32();
            SkillManager.Update(Silkroad.GetSkillById(skillId));
        }
    }
}
