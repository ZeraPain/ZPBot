using System.Collections.Generic;
using System.Linq;
using ZPBot.Common.Resources;

namespace ZPBot.Common.Party
{
    internal class Party
    {
        public uint Id { get; protected set; }
        public uint MasterId { get; set; }
        public byte PartyType { get; protected set; }
        public List<PartyMember> PartyMembers { get; protected set; }

        public Party(uint id, uint masterId, byte partyType, List<PartyMember> partyMembers)
        {
            Id = id;
            MasterId = masterId;
            PartyType = partyType;
            PartyMembers = partyMembers;
        }

        public void UpdateHpMp(uint accountId, byte hpMpInfo)
        {
            foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.AccountId == accountId))
                partyMember.HpMpInfo = hpMpInfo;
        }

        public void UpdatePosition(uint accountId, GamePosition inGamePosition)
        {
            foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.AccountId == accountId))
                partyMember.InGamePosition = inGamePosition;
        }

        public void Add(PartyMember partyMember) => PartyMembers.Add(partyMember);

        public void Remove(uint accountId)
        {
            foreach (var partyMember in PartyMembers.Where(partyMember => partyMember.AccountId == accountId))
            {
                PartyMembers.Remove(partyMember);
                break;
            }
        }

        public bool PlayerInParty(string charname) => PartyMembers.Any(player => player.Charname == charname);

        public bool PlayerInParty(uint accountId) => PartyMembers.Any(player => player.AccountId == accountId);

        public bool IsAutoShare() => PartyType % 4 == 2 || PartyType % 4 == 3;
    }
}
