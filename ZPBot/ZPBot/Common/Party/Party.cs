using System.Collections.Generic;
using System.Linq;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Party
{
    internal class Party
    {
        public uint Id { get; protected set; }
        public uint MasterId { get; set; }
        public List<PartyMember> PartyMembers { get; protected set; }

        public Party(uint id, uint masterId, List<PartyMember> partyMembers)
        {
            Id = id;
            MasterId = masterId;
            PartyMembers = partyMembers;
        }

        public void UpdateHpMp(uint worldId, byte hpMpInfo)
        {
            foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.WorldId == worldId))
                partyMember.HpMpInfo = hpMpInfo;
        }

        public void UpdatePosition(uint worldId, GamePosition inGamePosition)
        {
            foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.WorldId == worldId))
                partyMember.InGamePosition = inGamePosition;
        }

        public void Add(PartyMember partyMember) => PartyMembers.Add(partyMember);

        public void Remove(uint worldId)
        {
            foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.WorldId == worldId))
            {
                PartyMembers.Remove(partyMember);
                break;
            }
        }

        public bool PlayerInParty(string charname) => PartyMembers.Any(player => player.Charname == charname);
    }
}
